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

    private Animator animator;

    private InteractionHandler interactionHandler;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        interactionHandler = GetComponent<InteractionHandler>();
    }

    void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsSettingOpen)
            return;
        Cursor.lockState = CursorLockMode.Locked;
        inventory += ToggleCursor;
    }

    void FixedUpdate()
    {
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
    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsSettingOpen)
                return;
            inventory?.Invoke();
        }
    }

    public void OnInteractionInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
            interactionHandler.Interact();
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

        if (isLocked)
        {
            // ▶ 인벤토리 열림 상태 : 커서 보이게 + 카메라 회전 끔
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canLook = false;
        }
        else
        {
            // ▶ 인벤토리 닫힘 상태 : 커서 숨기고 + 다시 카메라 회전 켬
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canLook = true;
        }
    }
}