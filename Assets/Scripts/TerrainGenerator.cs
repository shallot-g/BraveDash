using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地形随机生成器 - 支持将地形预制体放入后随机生成地形
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Header("预制体设置")]
    [SerializeField] private GameObject[] prefabsToSpawn; // 要生成的预制体数组

    [Header("生成点设置")]
    [SerializeField] private Vector3 spawnPoint = Vector3.zero; // 固定生成点
    [SerializeField] private bool useRandomXOffset = false; // 是否使用随机X偏移
    [SerializeField] private Vector2 randomXRange = new Vector2(-2f, 2f); // 随机X偏移范围

    [Header("生成设置")]
    [SerializeField] private float spawnInterval = 2f; // 生成间隔（秒）
    [SerializeField] private bool spawnOnStart = false; // 是否在启动时生成（改为false，由GameManager控制）
    [SerializeField] private int maxSpawnCount = -1; // 最大生成数量，-1表示无限生成

    private float timer = 0f;
    private bool isSpawning = false;
    private int currentSpawnedCount = 0;

    void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    void Update()
    {
        if (isSpawning)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;

                // 检查是否达到最大生成数量
                if (maxSpawnCount > 0 && currentSpawnedCount >= maxSpawnCount)
                {
                    StopSpawning();
                    return;
                }

                SpawnOnePrefab();
                currentSpawnedCount++;
            }
        }
    }

    /// <summary>
    /// 开始生成
    /// </summary>
    public void StartSpawning()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogError("没有设置要生成的预制体！");
            return;
        }

        isSpawning = true;
        timer = 0f;
        currentSpawnedCount = 0;
        Debug.Log("地形生成器已启动");
    }

    /// <summary>
    /// 停止生成
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        timer = 0f;
        Debug.Log("地形生成器已停止");
    }

    /// <summary>
    /// 重置生成计数（清空计数，重新开始生成）
    /// </summary>
    public void ResetSpawning()
    {
        currentSpawnedCount = 0;
        if (isSpawning)
        {
            timer = 0f;
        }
    }

    /// <summary>
    /// 清除所有已生成的地形
    /// </summary>
    public void ClearAllTerrains()
    {
        GameObject[] terrains = GameObject.FindGameObjectsWithTag("Terrain");
        foreach (GameObject terrain in terrains)
        {
            Destroy(terrain);
        }
        Debug.Log("已清除所有地形");
    }

    /// <summary>
    /// 生成单个预制体（随机选择）
    /// </summary>
    private void SpawnOnePrefab()
    {
        // 随机选择一个预制体
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefabToSpawn = prefabsToSpawn[randomIndex];

        if (prefabToSpawn != null)
        {
            // 计算生成位置
            Vector3 spawnPos = spawnPoint;

            // 添加随机X偏移
            if (useRandomXOffset)
            {
                float randomX = Random.Range(randomXRange.x, randomXRange.y);
                spawnPos.x += randomX;
            }

            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"预制体数组索引 {randomIndex} 为空！");
        }
    }

    /// <summary>
    /// 获取是否正在生成
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }

    /// <summary>
    /// 设置生成间隔
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }

    // 在编辑器中可视化生成点
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPoint, 0.3f);

        // 可视化随机偏移范围
        if (useRandomXOffset)
        {
            Gizmos.color = Color.yellow;
            Vector3 minPos = spawnPoint + new Vector3(randomXRange.x, 0, 0);
            Vector3 maxPos = spawnPoint + new Vector3(randomXRange.y, 0, 0);
            Gizmos.DrawLine(minPos, maxPos);
        }
    }
}
