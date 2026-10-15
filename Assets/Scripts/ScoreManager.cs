using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ScoreManager : MonoBehaviour
{
    [Header("分数设置")]
    [SerializeField] private int currentScore = 0;           // 当前分数
    [SerializeField] private int scorePerSecond = 100;       // 每秒增加的分数
    [SerializeField] private int scorePerKill = 100;         // 每个敌人的分数

    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI scoreText;              // 分数显示文本（使用Text）
    [SerializeField] private TextMeshProUGUI highScoreText;


    [Header("计分设置")]
    [SerializeField] private bool isScoring = false;         // 是否正在计分
    [SerializeField] private float scoreInterval = 1f;       // 计分间隔时间

    [Header("最高分设置")]
    [SerializeField] private int highScore = 0;              // 历史最高分
    [SerializeField] private bool saveHighScore = true;      // 是否保存最高分
    [SerializeField] private string saveFileName = "highscore.dat"; // 保存文件名

    private float scoreTimer = 0f;                           // 计分计时器
    private static ScoreManager instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScoreManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ScoreManager");
                    instance = go.AddComponent<ScoreManager>();
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
        DontDestroyOnLoad(gameObject); // 场景切换时不销毁

        // 加载最高分
        LoadHighScore();

        // 自动查找 ScoreText
        if (scoreText == null)
        {
            // 查找 TextMeshProUGUI 类型的组件
            scoreText = FindObjectOfType<TextMeshProUGUI>();
            if (scoreText != null)
            {
                Debug.Log("自动找到 ScoreText: " + scoreText.name);
            }
        }
    }

    void Start()
    {
        // 初始化分数显示
        UpdateScoreUI();

        // 开始计分
        StartScoring();
    }

    void Update()
    {
        // 如果正在计分，更新时间计分
        if (isScoring)
        {
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= scoreInterval)
            {
                scoreTimer = 0;
                AddTimeScore();
            }
        }
    }

    /// <summary>
    /// 开始计分
    /// </summary>
    public void StartScoring()
    {
        isScoring = true;
        scoreTimer = 0;
        Debug.Log("开始计分系统");
    }

    /// <summary>
    /// 停止计分
    /// </summary>
    public void StopScoring()
    {
        isScoring = false;
        Debug.Log("停止计分系统");
    }

    /// <summary>
    /// 重置分数
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        Debug.Log("分数已重置");
        UpdateHighScoreUI();
    }

    /// <summary>
    /// 添加时间分数（每秒调用）
    /// </summary>
    private void AddTimeScore()
    {
        if (isScoring)
        {
            AddScore(scorePerSecond, "时间奖励");
        }
    }

    /// <summary>
    /// 添加击杀分数（由敌人死亡时调用）
    /// </summary>
    public void AddKillScore()
    {
        AddScore(scorePerKill, "击杀奖励");
    }

    /// <summary>
    /// 通用添加分数方法
    /// </summary>
    /// <param name="score">添加的分数</param>
    /// <param name="reason">加分原因（用于调试）</param>
    public void AddScore(int score, string reason = "")
    {
        currentScore += score;
        UpdateScoreUI();
        CheckAndUpdateHighScore(); 

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"增加分数: +{score} ({reason})，当前总分: {currentScore}");
        }
        else
        {
            Debug.Log($"增加分数: +{score}，当前总分: {currentScore}");
        }
    }

    /// <summary>
    /// 更新分数显示UI
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    /// <summary>
    /// 检查并更新最高分
    /// </summary>
    private void CheckAndUpdateHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            UpdateHighScoreUI();

            // 保存最高分
            if (saveHighScore)
            {
                SaveHighScore();
            }

            Debug.Log($"新的最高分！: {highScore}");
        }
    }

    /// <summary>
    /// 更新最高分显示UI
    /// </summary>
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"BestScore: {highScore}";
        }
    }

    /// <summary>
    /// 获取当前分数
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// 获取历史最高分
    /// </summary>
    public int GetHighScore()
    {
        return highScore;
    }

    /// <summary>
    /// 设置分数（用于特殊需求）
    /// </summary>
    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreUI();

        CheckAndUpdateHighScore();
    }

    /// <summary>
    /// 保存最高分到文件
    /// </summary>
    private void SaveHighScore()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            string data = highScore.ToString();
            File.WriteAllText(path, data);
            Debug.Log($"最高分已保存: {highScore} 到 {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存最高分失败: {e.Message}");
        }
    }

    /// <summary>
    /// 从文件加载最高分
    /// </summary>
    private void LoadHighScore()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                if (int.TryParse(data, out int loadedScore))
                {
                    highScore = loadedScore;
                    Debug.Log($"加载最高分: {highScore} 从 {path}");
                }
                else
                {
                    Debug.LogWarning("最高分文件格式错误，使用默认值0");
                    highScore = 0;
                }
            }
            else
            {
                Debug.Log("最高分文件不存在，使用默认值0");
                highScore = 0;
            }

            UpdateHighScoreUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载最高分失败: {e.Message}");
            highScore = 0;
        }
    }

}
