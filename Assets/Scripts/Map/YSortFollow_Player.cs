using UnityEngine;

// 要求物体必须有 SpriteRenderer 组件
[RequireComponent(typeof(SpriteRenderer))]
public class YSortFollow_Player : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer followSpriteRenderer;

    [SerializeField] private string followTransformName;

    bool isInit = false;
    /// <summary>任一层级 parent 为空时为 true，不再尝试获取 followSpriteRenderer。</summary>
    private bool parentChainInvalid;

    // 这是一个偏移量，用于微调
    [SerializeField] private int yOffset = 0;


    private void LateUpdate()
    {
        if (!isInit)
        {
            isInit = true;
            spriteRenderer = GetComponent<SpriteRenderer>();

            Transform p0 = transform.parent;
            Transform p1 = p0 != null ? p0.parent : null;
            Transform p2 = p1 != null ? p1.parent : null;
            if (p0 == null || p1 == null || p2 == null)
                parentChainInvalid = true;
            if (!parentChainInvalid)
            {
                Transform target = p2.Find(followTransformName);
                if (target != null)
                    followSpriteRenderer = target.GetComponent<SpriteRenderer>();
            }
        }
        if (parentChainInvalid || followSpriteRenderer == null)
            return;
        // 核心算法：将 Y 坐标取反并乘以一个系数（例如 100）
        // 乘以 100 是为了拉开间距，避免两个整数坐标太近导致层级闪烁
        // 加上 yOffset 实现微调
        spriteRenderer.sortingOrder = followSpriteRenderer.sortingOrder + yOffset;
    }
}
