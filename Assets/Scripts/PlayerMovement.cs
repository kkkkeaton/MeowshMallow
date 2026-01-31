using UnityEngine;
using UnityEngine.InputSystem;

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

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private Vector2 _moveInput;
        private Vector2 _currentVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            if (playerInputActions == null)
            {
                Debug.LogError("[PlayerMovement] 未指定 Player Input Actions，请在 Inspector 中拖入 InputSystem_Actions。", this);
                return;
            }

            var playerMap = playerInputActions.FindActionMap("Player", true);
            _moveAction = playerMap.FindAction("Move", true);
        }

        private void OnEnable()
        {
            if (_rb != null)
                _currentVelocity = _rb.linearVelocity;
            if (_moveAction == null) return;
            _moveAction.Enable();
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            if (_moveAction == null) return;
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
            _moveAction.Disable();
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

            Vector2 move = _moveInput;
            if (normalizeMovement && move.sqrMagnitude > 1f)
                move = move.normalized;

            Vector2 targetVelocity = move * moveSpeed;
            float decel = deceleration > 0f ? deceleration : acceleration;
            float rate = move.sqrMagnitude > 0.01f ? acceleration : decel;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = _currentVelocity;
        }
    }
}
