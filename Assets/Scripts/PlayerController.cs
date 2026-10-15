using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 10f;          // 跳跃力度
    [SerializeField] private float jumpGroundOffset = 0.2f;  // 跳跃时向上偏移量

    [Header("重力设置")]
    [SerializeField] private float gravity = -20f;           // 重力加速度
    [SerializeField] private float groundCheckRadius = 0.2f; // 地面检测半径

    [Header("敌人设置")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>(); // 可配置的敌人预制体列表
    [SerializeField] private bool debugAttack = true;        // 是否显示攻击调试信息

    [Header("攻击设置")]
    [SerializeField] private float attackDuration = 0.5f;    // 攻击动画持续时间
    [SerializeField] private float attackCooldown = 0.3f;    // 攻击冷却时间
    [SerializeField] private float attackRange = 1.2f;       // 攻击范围（X轴方向）
    [SerializeField] private float attackHeight = 2.0f;      // 攻击高度（Y轴方向）
    [SerializeField] private float attackOffset = 0.8f;      // 攻击检测点偏移距离

    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;          // 地面层
    [SerializeField] private Transform groundCheckPoint;     // 地面检测点

    [Header("动画")]
    [SerializeField] private Animator animator;              // 动画控制器

    [Header("死亡效果设置")]
    [SerializeField] private ParticleSystem deathParticle;   // 死亡粒子效果
    [SerializeField] private bool disableControlsOnDeath = true; // 死亡时禁用控制

    private Rigidbody2D rb2d;        // 2D刚体
    private bool isGrounded;          // 是否在地面上
    private float verticalVelocity;   // 垂直速度
    private bool isAttacking;         // 是否正在攻击
    private float attackTimer;        // 攻击计时器
    private float attackCooldownTimer; // 攻击冷却计时器
    private bool isJumping;           // 是否正在跳跃上升阶段
    private bool isDead = false;      // 是否死亡

    void Start()
    {
        // 获取组件
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // 配置刚体
        rb2d.gravityScale = 0;        // 禁用内置重力，使用自定义重力
        rb2d.freezeRotation = true;    // 冻结旋转
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation; // 明确冻结X轴和旋转

        // 自动创建地面检测点
        if (groundCheckPoint == null)
        {
            CreateGroundCheckPoint();
        }

        // 确保角色初始位置固定
        Vector3 pos = transform.position;
        pos.x = 0; // 固定X轴位置
        transform.position = pos;

        // 确保角色旋转归零
        transform.rotation = Quaternion.identity;

        // 初始动画状态
        UpdateAnimation();
    }

    void Update()
    {
        // 如果已死亡，不处理任何输入
        if (isDead) return;

        // 检测是否在地面上
        CheckGrounded();

        // 更新动画状态
        UpdateAnimation();

        // 跳跃输入（只有在地面上且不在攻击时才能跳跃）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking)
        {
            Jump();
        }

        // 攻击输入（只有在冷却完成且不在攻击中才能攻击）
        if (Input.GetKeyDown(KeyCode.J) && attackCooldownTimer <= 0 && !isAttacking)
        {
            Attack();
        }

        // 更新攻击计时器
        UpdateAttackTimers();

        // 调试信息
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"空格按下 - 地面状态: {isGrounded}, 攻击状态: {isAttacking}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log($"J键按下 - 冷却: {attackCooldownTimer}, 攻击状态: {isAttacking}");
        }
    }

    void FixedUpdate()
    {
        // 如果已死亡，不处理物理更新
        if (isDead) return;

        // 应用重力
        ApplyGravity();
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("玩家死亡！");

        // 播放死亡音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeathSound();
        }

        GameManager.Instance.OnPlayerDeath();
        // 禁用控制
        if (disableControlsOnDeath)
        {
            // 停止所有移动
            verticalVelocity = 0;
            rb2d.velocity = Vector2.zero;
        }

        // 播放死亡粒子效果
        if (deathParticle != null)
        {
            deathParticle.transform.SetParent(null); // 脱离父对象
            deathParticle.Play();
            Destroy(deathParticle.gameObject, deathParticle.main.duration);
        }

        // 触发死亡动画
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // 禁用所有碰撞体，避免死亡后继续触发碰撞
        DisablePlayerColliders();
    }


    /// <summary>
    /// 重置玩家的碰撞体状态（重新开始游戏时调用）
    /// </summary>
    public void ResetColliders()
    {
        // 重置死亡标志
        isDead = false;

        // 重新启用所有碰撞体
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = true;
            }
        }

        // 重置动画死亡状态
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }

        // 重置垂直速度
        verticalVelocity = 0;

        Debug.Log("玩家碰撞体已重置，死亡状态清除");
    }

    /// <summary>
    /// 禁用玩家的所有碰撞体
    /// </summary>
    private void DisablePlayerColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    /// <summary>
    /// 处理与敌人的碰撞
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果已死亡，不再处理碰撞
        if (isDead) return;

        if (collision.gameObject.CompareTag("Deadline"))//摔落死亡或越屏死亡
        {
            Die();
        }

            // 检查是否碰到敌人
            if (collision.gameObject.CompareTag("Enemy"))
        {
            // 检查是否是从上方踩踏敌人（玩家击杀逻辑）
            bool isStomping = false;

            // 获取碰撞点
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 如果碰撞点法线朝上（玩家从上方踩到敌人）
                if (contact.normal.y > 0.5f)
                {
                    isStomping = true;
                    break;
                }
            }

            if (isStomping)
            {
                // 播放踩踏音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayStompSound();
                }

                // 从上方踩踏：击杀敌人，玩家反弹
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);

                    // 玩家反弹跳跃
                    verticalVelocity = Mathf.Sqrt(jumpForce * 0.7f * -2f * gravity);
                    isJumping = true;
                    isGrounded = false;

                    Debug.Log("踩踏敌人！玩家反弹");
                }
            }
            else
            {
                // 非踩踏碰撞：玩家死亡
                Debug.Log("玩家碰到敌人，死亡！");
                Die();
            }
        }
    }

    /// <summary>
    /// 执行跳跃
    /// </summary>
    private void Jump()
    {
        // 计算跳跃初速度
        verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        isJumping = true;

        // 强制更新地面检测状态为未着地
        isGrounded = false;

        // 立即将角色向上移动一小段距离，确保脱离地面检测
        Vector3 newPosition = transform.position;
        newPosition.y += jumpGroundOffset;
        transform.position = newPosition;

        // 触发跳跃动画
        animator.SetTrigger("jump");

        // 播放跳跃音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }

        Debug.Log($"跳跃！初速度: {verticalVelocity}");

        // 可选：添加跳跃音效
        // AudioManager.PlayJumpSound();
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private void Attack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;

        // 触发攻击动画
        animator.SetTrigger("attack");

        Debug.Log("攻击！");

        // 可选：添加攻击检测（碰撞检测伤害）
        PerformAttackDamage();

        // 播放攻击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAttackSound();
        }
    }

    /// <summary>
    /// 执行攻击伤害检测
    /// </summary>
    private void PerformAttackDamage()
    {
        // 根据角色朝向确定攻击方向
        bool facingRight = transform.localScale.x > 0;
        Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * attackOffset;

        // 创建攻击检测区域
        Vector2 attackSize = new Vector2(attackRange, attackHeight);

        // 检测所有在攻击范围内的碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackOrigin, attackSize, 0);

        bool hitAny = false;
        int hitCount = 0;

        foreach (Collider2D hitCollider in hitColliders)
        {
            // 检查是否击中敌人（通过标签）
            if (hitCollider.CompareTag("Enemy"))
            {
                hitAny = true;
                hitCount++;
                EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(1);
                }
                if (debugAttack)
                {
                    Debug.Log($"击中敌人: {hitCollider.name}");
                }

            }
            
        }

        if (debugAttack)
        {
            if (!hitAny)
            {
                Debug.Log("攻击未击中任何敌人");
            }
            else
            {
                Debug.Log($"攻击击中 {hitCount} 个目标");
            }
        }
    }


    /// <summary>
    /// 应用重力
    /// </summary>
    private void ApplyGravity()
    {
        // 应用重力加速度
        verticalVelocity += gravity * Time.fixedDeltaTime;

        // 限制最大下落速度
        if (verticalVelocity < -30f)
        {
            verticalVelocity = -30f;
        }

        // 更新刚体速度（只有Y轴移动）
        rb2d.velocity = new Vector2(0, verticalVelocity);
    }

    /// <summary>
    /// 检测是否在地面上（使用圆形检测）
    /// </summary>
    private void CheckGrounded()
    {
        // 从地面检测点进行圆形检测
        Collider2D hit = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        bool wasGrounded = isGrounded;
        isGrounded = hit != null;

        // 刚落地时重置垂直速度和跳跃状态
        if (isGrounded && !wasGrounded && verticalVelocity < 0)
        {
            verticalVelocity = 0;
            isJumping = false;
            Debug.Log("落地，回到地面状态");
        }

    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;

        // 更新地面状态参数
        animator.SetBool("IsOnGround", isGrounded);

        // 动画状态逻辑：
        // - IsOnGround = true：播放Run动画（原地奔跑）
        // - IsOnGround = false 且垂直速度 > 0：播放Jump动画
        // - IsOnGround = false 且垂直速度 < 0：播放Fall动画
        // 跳跃和攻击动画会通过trigger触发并打断Run动画

        // 调试动画状态
        if (!isGrounded)
        {
            if (verticalVelocity > 0.1f)
            {
                Debug.Log($"上升中(Jump)，速度: {verticalVelocity:F2}");
            }
            else if (verticalVelocity < -0.1f)
            {
                Debug.Log($"下落中(Fall)，速度: {verticalVelocity:F2}");
            }
        }
        else
        {
            // 在地面上时确保垂直速度为0
            if (verticalVelocity != 0)
            {
                verticalVelocity = 0;
            }
        }
    }

    /// <summary>
    /// 更新攻击计时器
    /// </summary>
    private void UpdateAttackTimers()
    {
        // 更新攻击计时器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
                Debug.Log("攻击结束");
            }
        }

        // 更新冷却计时器
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 自动创建地面检测点
    /// </summary>
    private void CreateGroundCheckPoint()
    {
        GameObject checkPoint = new GameObject("GroundCheck");
        checkPoint.transform.parent = transform;

        // 获取碰撞体范围
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            checkPoint.transform.localPosition = new Vector3(0, -collider.bounds.extents.y + groundCheckRadius, 0);
        }
        else
        {
            checkPoint.transform.localPosition = new Vector3(0, -0.5f, 0);
        }

        groundCheckPoint = checkPoint.transform;
    }

    /// <summary>
    /// 获取是否在地面上（供其他脚本使用）
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// 获取是否正在攻击（供其他脚本使用）
    /// </summary>
    public bool IsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// 获取垂直速度（供其他脚本使用）
    /// </summary>
    public float GetVerticalVelocity()
    {
        return verticalVelocity;
    }

    /// <summary>
    /// 在编辑器中可视化地面检测圆和攻击范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 显示地面检测圆（当选中该物体时显示）
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        else
        {
            // 如果没有指定检测点，尝试在脚本所在位置显示一个示意圆
            Gizmos.color = Color.yellow;
            Vector3 defaultPos = transform.position + Vector3.down * 0.5f;
            Gizmos.DrawWireSphere(defaultPos, groundCheckRadius);
        }

        // 显示攻击范围
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.8f;
        Gizmos.DrawWireCube(attackOrigin, new Vector3(1.2f, 2.0f, 0));
    }
}

