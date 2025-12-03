using DG.Tweening.Core.Easing;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private Vector2 walkInput;
    public float jumpPower;
    public LayerMask groundLayerMask;

    [Header("Sprint")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    private bool isSprinting = false;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;
    private Vector2 mouseDelta;
    public bool canLook = true;

    [HideInInspector]
    public Action inventory;
    public Action builder;
    private Rigidbody _rigidbody;
    private InteractionHandler interactionHandler;
    private Equipment equipment;
    private Animator animator;
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        interactionHandler = GetComponent<InteractionHandler>();
        equipment = GetComponent<Equipment>();
    }

    void Start()
    {
        if (GameManager.Instance == null || UIManager.Instance.IsSettingOpen)
            return;
        Cursor.lockState = CursorLockMode.Locked;
        inventory += ToggleCursor;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.CanMove)
            Walk();
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    void Walk()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 dir = transform.forward * walkInput.y + transform.right * walkInput.x;
        dir *= speed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;

        float inputMagnitude = walkInput.magnitude;    // 0~1
        float animSpeed = isSprinting ? Mathf.Lerp(0.5f, 1f, inputMagnitude)
                                      : Mathf.Lerp(0f, 0.5f, inputMagnitude);
        animator.SetFloat("Speed", animSpeed);
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            walkInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            walkInput = Vector2.zero;
        }

    }
    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            isSprinting = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isSprinting = false;
        }
    }
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            _rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) +(transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.8f, groundLayerMask))
            {
                return true;
            }
        }
        return false;
    }

    // Tab 키로 인벤토리창 on/off
    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            UIManager.Instance.ToggleInventory();
            ToggleCursor();
        }
    }

    // ESC 키로 설정창 on/off
    public void OnSettingButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            UIManager.Instance.ToggleSetting();
            ToggleCursor();
        }
    }


    public void OnInteractionInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
            interactionHandler.Interact();
    }

    public void OnAttackInput(InputAction.CallbackContext callbackContext)
    {
        if (UIManager.Instance.IsInventoryOpen || UIManager.Instance.IsSettingOpen)
            return;

        if (callbackContext.phase == InputActionPhase.Started)
        {
            animator.SetTrigger(AttackTrigger);
            equipment.OnAttack();
        }
    }

    public void OnBuilderButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            builder?.Invoke();
        }
    }

    void ToggleCursor()
    {

        bool isLocked = Cursor.lockState == CursorLockMode.Locked;

        if (UIManager.Instance.IsInventoryOpen || UIManager.Instance.IsSettingOpen)
        {
            // ▶ 인벤토리 OR 설정창 열림 상태 : 커서 보이게 + 카메라 회전 끔
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canLook = false;
        }
        else
        {
            // ▶ 인벤토리 OR 설정창 닫힘 상태 : 커서 숨기고 + 다시 카메라 회전 켬
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canLook = true;
        }
    }
}