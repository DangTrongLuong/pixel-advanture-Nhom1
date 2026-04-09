using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;




public class PlayerMoveBtn : MonoBehaviour
{
    [Header("Joystick")]
    public RectTransform sliderBackground;
    public RectTransform circle;

    [Header("Jump Button Visual")]
    public Image jumpButtonImage;
    public Sprite jumpSprite1;
    public Sprite jumpSprite2;

    [Header("Jump Button Area")]
    public RectTransform jumpButtonRect;

    [Header("Player")]
    public PlayerController playerController;

    private Canvas _canvas;
    private Vector2 _circleOrigin;
    private float _halfWidth;

    private int _joystickTouchId = -1;
    private int _jumpTouchId = -1;
    private bool _joystickHeld = false;

    const int MOUSE_ID = -99;

    void Awake()
    {
       
        _canvas = GetComponentInParent<Canvas>();
        _circleOrigin = circle.anchoredPosition;
        _halfWidth = (sliderBackground.rect.width - circle.rect.width) * 0.5f;

        // Default sprite an toàn ngay từ đầu
        if (jumpButtonImage != null && jumpSprite1 != null)
            jumpButtonImage.sprite = jumpSprite1;
    }

    void Start()
    {
        StartCoroutine(InitController());
    }

    IEnumerator InitController()
{
    float timeout = 3f;
    float elapsed = 0f;

    while (elapsed < timeout)
    {
        if (PersistentPlayerManager.instance != null)
            playerController = PersistentPlayerManager.instance.GetSelectedController();

        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.OnMoveInputChanged += SyncCircleToInput;
            Debug.Log("[PlayerMoveBtn] Tìm thấy playerController: " + playerController.name);
            yield break; 
        }

        yield return new WaitForSeconds(0.1f);
        elapsed += 0.1f;
    }

    Debug.LogError("[PlayerMoveBtn] Không tìm thấy PlayerController sau " + timeout + "s!");
}

    void OnEnable()
    {
         EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        if (playerController != null)
            playerController.OnMoveInputChanged -= SyncCircleToInput;
    }

    void Update()
    {
        HandleTouches();
        HandleMouse();
        SyncJumpVisual();
    }

    // ── TOUCH ──────────────────────────────────
    void HandleTouches()
{
    foreach (var t in Touch.activeTouches)
    {
        switch (t.phase)
{
    case TouchPhase.Began:
        BeginContact(t.finger.index, t.screenPosition);
        break;

    case TouchPhase.Moved:
    case TouchPhase.Stationary:
        MoveContact(t.finger.index, t.screenPosition);
        break;

    case TouchPhase.Ended:
    case TouchPhase.Canceled:
        EndContact(t.finger.index);
        break;
}

    }
}


    // ── MOUSE (editor / PC) ────────────────────
    void HandleMouse()
    {
#if UNITY_EDITOR
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0) return;

        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
                BeginContact(MOUSE_ID, mousePos);

            if (Mouse.current.leftButton.isPressed)
                MoveContact(MOUSE_ID, mousePos);

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                EndContact(MOUSE_ID);
        }
#endif
    }

    // ── SHARED LOGIC ───────────────────────────
    void BeginContact(int id, Vector2 screen)
    {
        bool inJump = jumpButtonRect != null
            ? RectTransformUtility.RectangleContainsScreenPoint(jumpButtonRect, screen, GetCamera())
            : screen.x > Screen.width * 0.5f;

        bool inSlider = RectTransformUtility.RectangleContainsScreenPoint(sliderBackground, screen, GetCamera());

        if (inJump && _jumpTouchId == -1)
        {
            _jumpTouchId = id;
            playerController?.MobileJump();
            return;
        }

        if (inSlider && _joystickTouchId == -1)
        {
            _joystickTouchId = id;
            _joystickHeld = true;
            MoveCircle(screen);
        }
    }

    void MoveContact(int id, Vector2 screen)
    {
        if (id == _joystickTouchId) MoveCircle(screen);
    }

    void EndContact(int id)
    {
        if (id == _joystickTouchId) { _joystickTouchId = -1; _joystickHeld = false; ResetCircle(); }
        if (id == _jumpTouchId) _jumpTouchId = -1;
    }

    // ── CIRCLE ─────────────────────────────────
    void MoveCircle(Vector2 screen)
    {
        if (playerController == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderBackground, screen, GetCamera(), out Vector2 local);
        float clamped = Mathf.Clamp(local.x, -_halfWidth, _halfWidth);
        circle.anchoredPosition = new Vector2(_circleOrigin.x + clamped, _circleOrigin.y);

        float direction = Mathf.Approximately(clamped, 0f) ? 0f : Mathf.Sign(clamped);
        playerController.SetMoveInput(direction);
    }

    void ResetCircle()
    {
        circle.anchoredPosition = _circleOrigin;
        playerController?.SetMoveInput(0f);
    }

    void SyncCircleToInput(float value)
    {
        if (_joystickHeld) return;
        float clamped = Mathf.Clamp(value, -1f, 1f);
        circle.anchoredPosition = new Vector2(_circleOrigin.x + clamped * _halfWidth, _circleOrigin.y);
    }

    // ── JUMP VISUAL ────────────────────────────
    void SyncJumpVisual()
    {
        if (jumpButtonImage == null || playerController == null) return;
        jumpButtonImage.sprite = playerController.IsGrounded ? jumpSprite1 : jumpSprite2;
    }

    Camera GetCamera()
{
    if (_canvas == null) return null;
    if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
    return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
}
}