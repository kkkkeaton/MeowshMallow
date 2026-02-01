using UnityEngine;

public class AnimationRandomStart : MonoBehaviour
{
    [Header("播放速率")]
    [Tooltip("随机速率范围的最小值")]
    [SerializeField] private float speedMin = 0.8f;
    [Tooltip("随机速率范围的最大值")]
    [SerializeField] private float speedMax = 1.2f;

    [Header("开始时刻")]
    [Tooltip("是否在随机归一化时刻(0~1)开始播放当前状态")]
    [SerializeField] private bool randomizeStartTime = true;

    private void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null) return;

        animator.speed = Random.Range(speedMin, speedMax);

        if (randomizeStartTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.shortNameHash, 0, Random.Range(0f, 1f));
        }
    }
}
