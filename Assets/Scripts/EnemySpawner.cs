using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private GameObject[] enemyPrefabs;      // 敌人预制体数组
    [SerializeField] private float spawnInterval = 2f;       // 生成间隔时间
    [SerializeField] private float spawnRangeX = 5f;         // X轴生成范围
    [SerializeField] private float spawnRangeY = 3f;         // Y轴生成范围
    [SerializeField] private bool randomizeInterval = true;  // 是否随机化生成间隔
    [SerializeField] private Vector2 intervalRange = new Vector2(1f, 3f); // 随机间隔范围

    [Header("边界设置")]
    [SerializeField] private bool useSpawnerPosition = true; // 使用生成器位置作为中心点
    [SerializeField] private Vector2 customSpawnCenter = Vector2.zero; // 自定义生成中心点

    private bool isSpawning = true;
    private Coroutine spawnCoroutine;

    void Start()
    {
        StopSpawning();
    }

    /// <summary>
    /// 开始生成敌人
    /// </summary>
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    /// <summary>
    /// 停止生成敌人
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 循环生成敌人协程
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        while (isSpawning)
        {
            SpawnSingleEnemy();

            // 等待下一个生成间隔
            float waitTime = randomizeInterval ?
                Random.Range(intervalRange.x, intervalRange.y) :
                spawnInterval;

            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// 生成单个敌人
    /// </summary>
    private void SpawnSingleEnemy()
    {
        // 检查是否有预制体
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("没有设置敌人预制体！");
            return;
        }

        // 随机选择一个敌人预制体
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 获取生成位置
        Vector3 spawnPosition = GetSpawnPosition();

        // 实例化敌人
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"生成敌人: {enemyPrefab.name} 在位置 {spawnPosition}");
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        Vector3 center;

        if (useSpawnerPosition)
        {
            center = transform.position;
        }
        else
        {
            center = customSpawnCenter;
        }

        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomY = Random.Range(-spawnRangeY, spawnRangeY);

        return new Vector3(10, -1.53f, 0);
    }

    /// <summary>
    /// 立即生成指定数量的敌人
    /// </summary>
    public void SpawnEnemiesNow(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleEnemy();
        }
    }

    /// <summary>
    /// 设置生成参数
    /// </summary>
    public void SetSpawnParameters(float interval, float rangeX, float rangeY)
    {
        spawnInterval = interval;
        spawnRangeX = rangeX;
        spawnRangeY = rangeY;
    }

    /// <summary>
    /// 设置随机间隔范围
    /// </summary>
    public void SetRandomIntervalRange(float min, float max)
    {
        intervalRange = new Vector2(min, max);
    }
}
