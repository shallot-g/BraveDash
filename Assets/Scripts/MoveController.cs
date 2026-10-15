using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool useLocalPosition = false;

    // 运行时速度（可以被外部修改）
    private float currentMoveSpeed;

    // 存储初始位置和状态
    private Vector3 initialPosition;
    private bool isInitialized = false;

    void Awake()
    {
        // 存储初始位置
        if (!isInitialized)
        {
            initialPosition = useLocalPosition ? transform.localPosition : transform.position;
            isInitialized = true;
        }
    }

    void Start()
    {
        // 初始化当前速度为预设速度
        currentMoveSpeed = moveSpeed;

        // 检查游戏是否已经开始，如果没有开始则速度为0
        if (GameManager.Instance != null && !GameManager.Instance.IsGameStarted())
        {
            currentMoveSpeed = 0f;
        }
    }

    void Update()
    {
        Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;

        // 向左移动
        Vector3 newPos = currentPos + Vector3.left * currentMoveSpeed * Time.deltaTime;

        // 检查是否到达目标点
        if (newPos.x <= targetPosition.x)
        {
            // 精确设置到目标位置
            if (useLocalPosition)
            {
                transform.localPosition = targetPosition;
            }
            else
            {
                transform.position = targetPosition;
            }

            // 销毁物体
            Destroy(gameObject);
        }
        else
        {
            // 继续移动
            if (useLocalPosition)
            {
                transform.localPosition = newPos;
            }
            else
            {
                transform.position = newPos;
            }
        }
    }

    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">新的移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        currentMoveSpeed = speed;
        Debug.Log($"{gameObject.name} 移动速度已设置为: {speed}");
    }

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    public float GetMoveSpeed()
    {
        return currentMoveSpeed;
    }

    /// <summary>
    /// 重置为默认速度
    /// </summary>
    public void ResetMoveSpeed()
    {
        currentMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// 重置物体的位置和速度到初始状态
    /// </summary>
    public void ResetToInitialState()
    {
        // 重置位置
        if (useLocalPosition)
        {
            transform.localPosition = initialPosition;
        }
        else
        {
            transform.position = initialPosition;
        }

        // 重置速度
        currentMoveSpeed = 0f;

        // 重新激活物体
        gameObject.SetActive(true);

        Debug.Log($"{gameObject.name} 已重置到初始状态");
    }

    /// <summary>
    /// 获取初始位置
    /// </summary>
    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

}
