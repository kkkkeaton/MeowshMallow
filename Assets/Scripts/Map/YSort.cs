using UnityEngine;

// 要求物体必须有 SpriteRenderer 组件
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    // 这是一个偏移量，用于微调
    [SerializeField] private int yOffset = 0; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // 核心算法：将 Y 坐标取反并乘以一个系数（例如 100）
        // 乘以 100 是为了拉开间距，避免两个整数坐标太近导致层级闪烁
        // 加上 yOffset 实现微调
        spriteRenderer.sortingOrder = (int)(-transform.position.y * 100) + yOffset;
    }
}
