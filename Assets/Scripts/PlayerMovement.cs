using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations;

namespace MeowshMallow
{
    /// <summary>
    /// 2D 俯视角玩家基础移动，使用 Unity 新 Input System。
    /// 依赖 Rigidbody2D，请在 Inspector 中指定 InputSystem_Actions 资源。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("输入")]
        [Tooltip("拖入 Assets/InputSystem_Actions.inputactions")]
        [SerializeField] private InputActionAsset playerInputActions;

        [Header("移动")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("为 true 时斜向移动与轴向速度一致；为 false 时摇杆按力度比例移动")]
        [SerializeField] private bool normalizeMovement = true;

        [Header("加速度")]
        [Tooltip("从静止加速到目标速度的速率（单位/秒²），越大起步越快")]
        [SerializeField] private float acceleration = 20f;

        [Tooltip("松手后减速到静止的速率（单位/秒²），越大停下越快；0 表示与加速度相同")]
        [SerializeField] private float deceleration = 25f;

        [Header("暗杀")]
        [Tooltip("与敌人在此距离内按下 Attack 可暗杀（取自 GlobalSetting.ATTACK_RANGE）")]
        private float _assassinationRange => GlobalSetting.ATTACK_RANGE;

        [Header("动画")]
        [Tooltip("Animator 状态名：idle（移动/待机时）、kill（暗杀时）")]
        [SerializeField] private string animIdleState = "Anim_obj_idle";
        [SerializeField] private string animKillState = "Anim_obj_kill";

        [Header("水源")]
        [Tooltip("水源区域的碰撞体 Tag，进入后按交互键可将玩家颜色改为 2")]
        [SerializeField] private string waterSourceTag = "WaterSource";

        [Header("脚步声")]
        [Tooltip("移动时循环随机播放的脚步声列表，可配多个；空则静默")]
        [SerializeField] private AudioClip[] footstepSfxList = System.Array.Empty<AudioClip>();
        [Tooltip("两次播放间隔（秒）")]
        [SerializeField] private float footstepSfxInterval = 0.4f;

        [Header("胜利区域")]
        [Tooltip("胜利区域的碰撞体 Tag，进入后触发游戏胜利")]
        [SerializeField] private string winZoneTag = "WinZone";

        [SerializeField] private AnimationCurve moveSpeedCurve;

        [SerializeField] private float moveCurveOneTime = 1f;
        private float moveCurveTimeNow = 0f;

        private Rigidbody2D _rb;
        private Animator _animator;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _attackAction;
        private MonsterManager _monsterManager;
        private PlayerRangeDetector _rangeDetector;
        private Vector2 _moveInput;
        private Vector2 _currentVelocity;
        private int _waterSourceTriggerCount;
        private float _footstepSfxCooldown;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = transform.Find("PartShow").GetComponent<Animator>();
            _rangeDetector = GetComponent<PlayerRangeDetector>();

            if (playerInputActions == null)
            {
                Debug.LogError("[PlayerMovement] 未指定 Player Input Actions，请在 Inspector 中拖入 InputSystem_Actions。", this);
                return;
            }

            var playerMap = playerInputActions.FindActionMap("Player", true);
            _moveAction = playerMap.FindAction("Move", true);
            _interactAction = playerMap.FindAction("Interact", true);
            _attackAction = playerMap.FindAction("Attack", true);
        }

        private void OnEnable()
        {
            if (_rb != null)
                _currentVelocity = _rb.linearVelocity;
            if (_moveAction != null)
            {
                _moveAction.Enable();
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }
            if (_interactAction != null)
            {
                _interactAction.Enable();
                _interactAction.performed += OnInteractPerformed;
            }
            if (_attackAction != null)
            {
                _attackAction.Enable();
                _attackAction.performed += OnAttackPerformed;
            }
        }

        private void OnDisable()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
                _moveAction.Disable();
            }
            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
                _interactAction.Disable();
            }
            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
                _attackAction.Disable();
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (_rangeDetector == null) return;
            MonsterBase closest = _rangeDetector.GetClosestEnemyInAttackRange();
            if (closest == null) return;

            if (_monsterManager == null)
                _monsterManager = FindObjectOfType<MonsterManager>();
            if (_monsterManager == null && God.Instance != null)
                _monsterManager = God.Instance.Get<MonsterManager>();
            if (_monsterManager != null)
            {
                PlayKillAnimation();
                _monsterManager.Assassinate(closest);
            }
        }

        /// <summary>播放暗杀动画，播完后切回 idle。</summary>
        private void PlayKillAnimation()
        {
            if (_animator == null) return;
            StopAllCoroutines();
            StartCoroutine(PlayKillAndReturnToIdle());
        }

        private IEnumerator PlayKillAndReturnToIdle()
        {
            _animator.Play(animKillState, 0, 0f);
            yield return null;
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(info.length);
            if (_animator != null)
                _animator.Play(animIdleState, 0, 0f);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_waterSourceTriggerCount > 0)
            {
                var player = GetComponent<Player>();
                if (player != null)
                {
                    player.ChangeColor(2);
                    return;
                }
            }
            if (_rangeDetector == null) return;
            PickableItem closest = _rangeDetector.GetClosestPickableInRange();
            if (closest != null)
                closest.DoPickup();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(waterSourceTag) && other.CompareTag(waterSourceTag))
            {
                _waterSourceTriggerCount++;
                Debug.Log($"[PlayerMovement] 进入水源区域，当前水源触发计数: {_waterSourceTriggerCount}");
            }
            if (!string.IsNullOrEmpty(winZoneTag) && other.CompareTag(winZoneTag))
            {
                var process = God.Instance?.Get<GameProcessManager>();
                if (process != null && process.IsPlaying)
                    process.TriggerVictory();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(waterSourceTag) && other.CompareTag(waterSourceTag))
                _waterSourceTriggerCount--;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (_rb == null || _moveAction == null) return;

            moveCurveTimeNow += Time.fixedDeltaTime;
            if (moveCurveTimeNow > moveCurveOneTime)
                moveCurveTimeNow = 0f;


            Vector2 move = _moveInput;
            if (normalizeMovement && move.sqrMagnitude > 1f)
                move = move.normalized;

            if (move.sqrMagnitude < 0.01f)
                moveCurveTimeNow = 0f;


            Vector2 targetVelocity = move * moveSpeed*moveSpeedCurve.Evaluate(moveCurveTimeNow/moveCurveOneTime);
            float decel = deceleration > 0f ? deceleration : acceleration;
            float rate = move.sqrMagnitude > 0.01f ? acceleration : decel;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = _currentVelocity;

            if (move.sqrMagnitude > 0.01f && _currentVelocity.sqrMagnitude > 0.01f)
                TryPlayFootstepSfx();
        }

        private void TryPlayFootstepSfx()
        {
            if (footstepSfxList == null || footstepSfxList.Length == 0 || footstepSfxInterval <= 0f) return;
            _footstepSfxCooldown -= Time.fixedDeltaTime;
            if (_footstepSfxCooldown <= 0f)
            {
                _footstepSfxCooldown = footstepSfxInterval;
                var clip = footstepSfxList[Random.Range(0, footstepSfxList.Length)];
                if (clip != null)
                    God.Instance?.Get<AudioManager>()?.PlaySfx(clip);
            }
        }
    }
}
