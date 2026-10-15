using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("游戏组件")]
    [SerializeField] private EnemySpawner enemySpawner;      // 敌人生成器
    [SerializeField] private Animator playerAnimator;        // 玩家动画控制器
    [SerializeField] private TerrainGenerator terrainGenerator; // 地形生成器

    [Header("默认地形")]
    [SerializeField] private GameObject defaultTerrain;      // 默认地形预制体
    [SerializeField] private Vector3 defaultTerrainPosition = Vector3.zero; // 默认地形位置
    private GameObject currentDefaultTerrain; // 当前存在的默认地形实例

    [Header("UI按钮")]
    [SerializeField] private GameObject startButton;         // 开始按钮
    [SerializeField] private GameObject tutorialButton;      // 新手教程按钮
    [SerializeField] private GameObject highScoreButton;     // 最高分按钮
    [SerializeField] private GameObject exitButton;          // 退出游戏按钮

    [Header("教程UI")]
    [SerializeField] private GameObject tutorialPanel;      // 教程面板
    [SerializeField] private GameObject tutorialBackButton; // 教程返回按钮

    [Header("计分系统")]
    [SerializeField] private ScoreManager scoreManager;  // 分数管理器

    [Header("最高分UI")]
    [SerializeField] private GameObject highScorePanel;         // 最高分面板
    [SerializeField] private GameObject highScoreBackButton;    // 最高分返回按钮
    [SerializeField] private TextMeshProUGUI highScoreDisplayText;         // 最高分显示文本

    [Header("死亡界面UI")]
    [SerializeField] private GameObject deathUI;             // 死亡UI面板
    [SerializeField] private GameObject restartButton;           // 重新开始按钮
    [SerializeField] private GameObject returnTitleButton;       // 返回标题按钮

    [Header("游戏状态")]
    [SerializeField] private bool isGameStarted = false;     // 游戏是否已开始

    private static GameManager instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        // 单例初始化
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 自动查找组件（如果没有手动指定）
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner == null)
            {
                Debug.LogWarning("未找到敌人生成器组件");
            }
        }

        if (playerAnimator == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerAnimator = player.GetComponent<Animator>();
            }
        }

        if (terrainGenerator == null)
        {
            terrainGenerator = FindObjectOfType<TerrainGenerator>();
            if (terrainGenerator == null)
            {
                Debug.LogWarning("未找到地形生成器组件");
            }
        }

    }

    void Start()
    {
        // 初始状态：游戏未开始
        SetGameStarted(false);
        startButton.SetActive(true);
        tutorialButton.SetActive(true);
        highScoreButton.SetActive(true);
        exitButton.SetActive(true);

        // 初始化分数系统
        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance;
        }

        // 游戏开始时分数重置
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            scoreManager.StopScoring(); // 初始停止计分
        }

        // 背景音乐在游戏开始时播放
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeBackgroundMusic();
        }
        // 创建默认地形
        CreateDefaultTerrain();

        // 确保游戏开始前所有移动物体的速度为0
        SetAllMovingObjectsSpeed(0f);

        // 确保地形生成器没有在生成
        if (terrainGenerator != null)
        {
            terrainGenerator.StopSpawning();
        }

        // 确保敌人生成器没有在生成
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        Debug.Log("游戏管理器初始化完成，等待开始游戏");
    }

    /// <summary>
    /// 创建默认地形
    /// </summary>
    private void CreateDefaultTerrain()
    {
        if (defaultTerrain != null)
        {
            // 如果已存在默认地形，先销毁
            if (currentDefaultTerrain != null)
            {
                Destroy(currentDefaultTerrain);
            }

            // 创建新的默认地形实例
            currentDefaultTerrain = Instantiate(defaultTerrain, defaultTerrainPosition, Quaternion.identity);

            // 确保默认地形速度为0
            MoveController moveController = currentDefaultTerrain.GetComponent<MoveController>();
            if (moveController != null)
            {
                moveController.SetMoveSpeed(0f);
            }

            Debug.Log("默认地形已创建");
        }
        else
        {
            Debug.LogWarning("未设置默认地形预制体");
        }
    }

    /// <summary>
    /// 重置默认地形
    /// </summary>
    private void ResetDefaultTerrain()
    {

        // 重新创建默认地形
        if (defaultTerrain != null)
        {
            // 创建新的默认地形实例
            currentDefaultTerrain = Instantiate(defaultTerrain, defaultTerrainPosition, Quaternion.identity);

            // 根据游戏状态设置移动速度
            MoveController moveController = currentDefaultTerrain.GetComponent<MoveController>();
            if (moveController != null)
            {
                float targetSpeed = (isGameStarted && Application.isPlaying) ? 3f : 0f;
                moveController.SetMoveSpeed(targetSpeed);
            }

            Debug.Log($"默认地形已重新创建，移动速度: {(moveController != null ? moveController.GetMoveSpeed() : 0)}");
        }
        else
        {
            Debug.LogError("未设置默认地形预制体，无法重置默认地形！");
        }
    }

    private void SetAllMovingObjectsSpeed(float speed)
    {
        // 设置地形生成器中的移动物体速度
        MoveController[] moveControllers = FindObjectsOfType<MoveController>();
        foreach (MoveController moveController in moveControllers)
        {
            moveController.SetMoveSpeed(speed);
        }
    }

    /// <summary>
    /// 开始游戏（由按钮调用）
    /// </summary>
    public void StartGame()
    {
        if (isGameStarted) return;

        Debug.Log("游戏开始！");

        // 设置游戏状态
        SetGameStarted(true);

        // 隐藏菜单按钮
        if (startButton != null)
        {
            startButton.SetActive(false);
            tutorialButton.SetActive(false);
            highScoreButton.SetActive(false);
            exitButton.SetActive(false);
        }
        else
        {
            // 尝试通过GameObject.Find查找按钮
            GameObject button = GameObject.Find("StartButton");
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        // 重置并开始计分
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            scoreManager.StartScoring();
            Debug.Log("计分系统已启动");
        }

        // 开始敌人生成
        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
            Debug.Log("敌人生成已启动");
        }
        else
        {
            Debug.LogError("未找到敌人生成器，无法开始生成敌人！");
        }

        // 开始地形生成
        if (terrainGenerator != null)
        {
            terrainGenerator.StartSpawning();
            Debug.Log("地形生成已启动");
        }
        else
        {
            Debug.LogWarning("未找到地形生成器，地形将不会生成");
        }

        // 设置所有移动物体的速度为3
        SetAllMovingObjectsSpeed(3f);

        // 确保默认地形也开始移动
        if (currentDefaultTerrain != null)
        {
            MoveController moveController = currentDefaultTerrain.GetComponent<MoveController>();
            if (moveController != null && moveController.GetMoveSpeed() == 0f)
            {
                moveController.SetMoveSpeed(3f);
            }
        }

    }

    /// <summary>
    /// 停止游戏
    /// </summary>
    public void StopGame()
    {
        if (!isGameStarted) return;

        Debug.Log("游戏停止");

        SetGameStarted(false);

        // 停止计分
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
            Debug.Log("计分系统已停止");
        }

        // 停止敌人生成
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
            Debug.Log("敌人生成已停止");
        }

        // 停止地形生成
        if (terrainGenerator != null)
        {
            terrainGenerator.StopSpawning();
            Debug.Log("地形生成已停止");
        }

        // 设置所有移动物体的速度为0
        SetAllMovingObjectsSpeed(0f);

    }

    /// <summary>
    /// 玩家死亡时调用
    /// </summary>
    public void OnPlayerDeath()
    {
        if (!isGameStarted) return;

        // 停止游戏
        SetGameStarted(false);

        // 停止计分
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
        }

        // 停止敌人生成
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        // 停止地形生成
        if (terrainGenerator != null)
        {
            terrainGenerator.StopSpawning();
        }

        // 设置所有移动物体的速度为0
        SetAllMovingObjectsSpeed(0f);


        // 显示死亡界面
        if (deathUI != null)
        {
            deathUI.SetActive(true);
        }
    }

    /// <summary>
    /// 设置游戏开始状态
    /// </summary>
    private void SetGameStarted(bool started)
    {
        isGameStarted = started;

        // 更新玩家动画参数
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsStart", isGameStarted);
            Debug.Log($"玩家动画参数 IsStart 设置为: {isGameStarted}");
        }
        else
        {
            Debug.LogWarning("未找到玩家动画控制器，无法设置IsStart参数");
        }
    }

    /// <summary>
    /// 获取游戏是否已开始
    /// </summary>
    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    /// <summary>
    /// 重置游戏（可用于重新开始）
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("重置游戏");

        // 清除所有敌人
        if (enemySpawner != null)
        {
            // 查找并销毁所有敌人
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
        }

        // 重置游戏状态
        StopGame();

        // 重置分数
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }

        // 重置默认地形
        ResetDefaultTerrain();


        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 重置玩家位置到初始位置
            player.transform.position = new Vector3(0, 0, 0);

            // 重置玩家动画状态
            if (playerAnimator != null)
            {
                playerAnimator.Rebind(); // 重置动画器状态
                playerAnimator.SetBool("IsStart", false);
            }
        }

        // 重新启用玩家的碰撞体
        player.ResetColliders(); 

        deathUI.SetActive(false);
        StartGame();
    }

    /// <summary>
    /// 返回标题界面
    /// </summary>
    public void BackToTitle()
    {
        Debug.Log("返回标题界面");
        // 重置分数
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }

        // 停止游戏
        if (isGameStarted)
        {
            SetGameStarted(false);

            // 停止敌人生成
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }
            // 停止地形生成
            if (terrainGenerator != null)
            {
                terrainGenerator.StopSpawning();
            }
            // 清除所有敌人
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
            // 清除所有地形
            if (terrainGenerator != null)
            {
                terrainGenerator.ClearAllTerrains();
            }
        }

        // 重置默认地形
        ResetDefaultTerrain();

        // 重置玩家状态
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 重置玩家位置到初始位置
            player.transform.position = new Vector3(0, 0, 0);

            // 重置玩家碰撞体和死亡状态
            player.ResetColliders();

            // 重置玩家动画状态
            if (playerAnimator != null)
            {
                playerAnimator.Rebind(); // 重置动画器状态
                playerAnimator.SetBool("IsStart", false);
                playerAnimator.SetBool("IsDead", false);
            }

            // 重置刚体速度
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
        // 设置所有移动物体的速度为0
        SetAllMovingObjectsSpeed(0f);

        // 隐藏死亡UI
        if (deathUI != null)
        {
            deathUI.SetActive(false);
        }

        // 显示标题界面按钮
        if (startButton != null)
        {
            startButton.SetActive(true);
            tutorialButton.SetActive(true);
            highScoreButton.SetActive(true);
            exitButton.SetActive(true);
        }

        // 游戏状态为未开始
        isGameStarted = false;

        // 保持背景音乐播放
        if (AudioManager.Instance != null && !isGameStarted)
        {
            // 音乐在标题界面也播放
            AudioManager.Instance.PlayBackgroundMusic();
        }

    }

    /// <summary>
    /// 打开教程面板
    /// </summary>
    public void OpenTutorial()
    {
        Debug.Log("打开教程面板");

        // 隐藏初始按钮
        if (startButton != null)
        {
            startButton.SetActive(false);
            highScoreButton.SetActive(false);
            exitButton.SetActive(false);
            tutorialButton.SetActive(false);
        }

        // 显示教程面板和返回按钮
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        if (tutorialBackButton != null)
        {
            tutorialBackButton.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭教程面板（返回标题界面）
    /// </summary>
    public void CloseTutorial()
    {
        Debug.Log("关闭教程面板");

        // 隐藏教程面板和返回按钮
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (tutorialBackButton != null)
        {
            tutorialBackButton.SetActive(false);
        }

        // 显示初始按钮
        if (startButton != null)
        {
            startButton.SetActive(true);
            tutorialButton.SetActive(true);
            highScoreButton.SetActive(true);
            exitButton.SetActive(true);
        }
    }

    /// <summary>
    /// 打开最高分面板
    /// </summary>
    public void OpenHighScore()
    {
        Debug.Log("打开最高分面板");

        // 隐藏初始按钮
        if (startButton != null)
        {
            startButton.SetActive(false);
            tutorialButton.SetActive(false);
            highScoreButton.SetActive(false);
            exitButton.SetActive(false);
        }

        // 显示最高分面板
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);

            // 更新最高分显示
            UpdateHighScoreDisplay();
        }

        // 显示返回按钮
        if (highScoreBackButton != null)
        {
            highScoreBackButton.SetActive(true);

            // 确保按钮可交互
            Button btn = highScoreBackButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.RemoveListener(CloseHighScore);
                btn.onClick.AddListener(CloseHighScore);
            }
        }
    }

    /// <summary>
    /// 关闭最高分面板（返回标题界面）
    /// </summary>
    public void CloseHighScore()
    {
        Debug.Log("关闭最高分面板");

        // 隐藏最高分面板
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }

        // 隐藏返回按钮
        if (highScoreBackButton != null)
        {
            highScoreBackButton.SetActive(false);
        }

        // 显示初始按钮
        if (startButton != null)
        {
            startButton.SetActive(true);
            tutorialButton.SetActive(true);
            highScoreButton.SetActive(true);
            exitButton.SetActive(true);
        }
    }

    /// <summary>
    /// 更新最高分显示
    /// </summary>
    private void UpdateHighScoreDisplay()
    {
        if (scoreManager != null && highScoreDisplayText != null)
        {
            int currentHighScore = scoreManager.GetHighScore();
            highScoreDisplayText.text = $"BestScore: {currentHighScore}";
        }
        else if (highScoreDisplayText != null)
        {
            highScoreDisplayText.text = "BestScore: 0";
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// 在编辑器中可视化游戏状态（调试用）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // 在Scene视图中显示游戏状态文本
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Game State: {(isGameStarted ? "Running" : "Waiting")}");
#endif
        }
    }
}
