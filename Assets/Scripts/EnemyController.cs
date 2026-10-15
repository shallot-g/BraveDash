using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 2f;           // 向左移动速度

    [Header("死亡效果设置")]
    [SerializeField] private float flyForce = 8f;            // 飞出力度
    [SerializeField] private float flyAngle = 45f;           // 飞出角度（度）
    [SerializeField] private float rotationSpeed = 360f;     // 旋转速度（度/秒）
    [SerializeField] private float destroyDelay = 2f;        // 销毁延迟时间
    [SerializeField] private float deathDelay = 0.3f;        // 死亡延迟时间

    [Header("碰撞体设置")]
    [SerializeField] private Collider2D[] collidersToDisable; // 需要禁用的碰撞体

    [Header("视觉效果")]
    [SerializeField] private ParticleSystem deathParticle;    // 死亡粒子效果

    private Rigidbody2D rb2d;
    private bool isDead = false;
    GameObject Deadline;

    void Start()
    {
        Deadline = GameObject.Find("Deadline");

        // 获取组件
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }

        // 如果没有手动指定碰撞体，自动获取所有碰撞体
        if (collidersToDisable == null || collidersToDisable.Length == 0)
        {
            collidersToDisable = GetComponents<Collider2D>();
        }

        // 初始配置刚体
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.gravityScale = 0;
        rb2d.freezeRotation = true;
    }
    void Update()
    {
        // 存活状态下持续向左移动
        if (!isDead)
        {
            MoveLeft();
        }
    }


    /// 向左移动
    private void MoveLeft()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        if (transform.position.x < Deadline.transform.position.x)
            Destroy(gameObject);

    }


    /// 受到攻击
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 延迟执行死亡效果
        StartCoroutine(DelayedDeath());
    }

    /// 死亡处理

    private IEnumerator DelayedDeath()
    {   // 禁用所有碰撞体
        DisableColliders();

        // 等待指定的延迟时间
        yield return new WaitForSeconds(deathDelay);

        // 执行死亡效果
        Die();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name} 死亡");

        // 添加击杀分数
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddKillScore();
        }

        // 播放死亡粒子效果
        if (deathParticle != null)
        {
            deathParticle.transform.SetParent(null); // 脱离父对象
            deathParticle.Play();
            Destroy(deathParticle.gameObject, deathParticle.main.duration);
        }

        // 配置刚体为动态，用于抛物线飞行
        SetupRigidbodyForFlight();

        // 应用飞出力
        ApplyFlyForce();

        // 开始旋转
        StartCoroutine(RotateWhileFlying());

        // 延迟销毁
        Destroy(gameObject, destroyDelay);
    }

  
    /// 禁用所有碰撞体
    private void DisableColliders()
    {
        foreach (Collider2D collider in collidersToDisable)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

  
    /// 配置刚体用于飞行
    private void SetupRigidbodyForFlight()
    {
        if (rb2d != null)
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.gravityScale = 1;
            rb2d.freezeRotation = false;
            rb2d.constraints = RigidbodyConstraints2D.None;
        }
    }

 
    /// 应用飞出力（斜向右上抛物线）
    private void ApplyFlyForce()
    {
        if (rb2d == null) return;

        // 计算方向（向右上方）
        float angleRad = flyAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // 应用冲量
        rb2d.AddForce(direction * flyForce, ForceMode2D.Impulse);

        Debug.Log($"应用飞出力: 方向 {direction}, 力度 {flyForce}");
    }

   
    /// 旋转协程
    private IEnumerator RotateWhileFlying()
    {
        float startTime = Time.time;

        while (Time.time - startTime < destroyDelay)
        {
            // 旋转（顺时针）
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// 设置飞出参数（供外部调用）
    public void SetFlyParameters(float force, float angle, float rotationSpeedValue)
    {
        flyForce = force;
        flyAngle = angle;
        rotationSpeed = rotationSpeedValue;
    }

    
    /// 获取是否死亡
    public bool IsDead()
    {
        return isDead;
    }

    /// 在编辑器中可视化飞出方向
    private void OnDrawGizmosSelected()
    {
        // 显示飞出方向
        Gizmos.color = Color.yellow;
        float angleRad = flyAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)direction * 2f;
        Gizmos.DrawLine(startPos, endPos);

        // 绘制简单的箭头
        Vector3 arrowDirection = (endPos - startPos).normalized;
        Vector3 arrowPerp = new Vector3(-arrowDirection.y, arrowDirection.x, 0);
        Gizmos.DrawLine(endPos, endPos - arrowDirection * 0.3f + arrowPerp * 0.15f);
        Gizmos.DrawLine(endPos, endPos - arrowDirection * 0.3f - arrowPerp * 0.15f);
    }
}
