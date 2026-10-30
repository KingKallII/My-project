
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Sprite Berjalan (Fallback)")]
    public Sprite idleSprite;
    public Sprite walkingAwaySprite;
    public Sprite walkingBackSprite;
    public Sprite walkingProfile1;
    public Sprite walkingProfile2;
    public Sprite profileHoodieSprite;
    public Sprite overheadWalkingSprite;
    public Sprite sideViewSprite;
    public Sprite topDownWalkingSprite;
    public Sprite walkingStickerSprite;

    [Header("Sprite Idle Setiap Arah")]
    public Sprite idleFrontSprite;
    public Sprite idleBackSprite;
    public Sprite idleLeftSprite;
    public Sprite idleRightSprite;

    [Header("Animasi Berjalan ke Belakang (W)")]
    public Sprite walkingAwayFrame1;
    public Sprite walkingAwayFrame2;

    [Header("Animasi Berjalan ke Depan (S)")]
    public Sprite walkingFrontFrame1;
    public Sprite walkingFrontFrame2;

    [Header("Animasi Berjalan ke Kiri (A)")]
    public Sprite walkingLeftFrame1;
    public Sprite walkingLeftFrame2;

    [Header("Animasi Berjalan ke Kanan (D)")]
    public Sprite walkingRightFrame1;
    public Sprite walkingRightFrame2;

    [Header("Pengaturan Animasi")]
    [Min(0.01f)]
    public float walkAnimationSpeed = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private bool canMove = true;

    private enum FacingDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    private FacingDirection lastDirection = FacingDirection.Front;

    private float walkAnimationTimer;
    private int currentWalkFrame;
    private FacingDirection? animatedDirection = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        SetIdleSprite();
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        movement = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            movement.y += 1;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            movement.y -= 1;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            movement.x -= 1;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            movement.x += 1;
        }

        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirection = movement.x < 0
                    ? FacingDirection.Left
                    : FacingDirection.Right;
            }
            else
            {
                lastDirection = movement.y > 0
                    ? FacingDirection.Back
                    : FacingDirection.Front;
            }

            UpdateWalkingSprite();
        }
        else
        {
            ResetAnimation();
            SetIdleSprite();
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movement * moveSpeed;
    }

    void UpdateWalkingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite frame1 = null;
        Sprite frame2 = null;
        Sprite fallbackSprite = null;

        spriteRenderer.flipX = false;

        switch (lastDirection)
        {
            // W: berjalan membelakangi kamera.
            case FacingDirection.Back:
                frame1 = walkingAwayFrame1;
                frame2 = walkingAwayFrame2;

                fallbackSprite = walkingAwaySprite != null
                    ? walkingAwaySprite
                    : walkingBackSprite;
                break;

            // S: berjalan menghadap kamera.
            case FacingDirection.Front:
                frame1 = walkingFrontFrame1;
                frame2 = walkingFrontFrame2;

                fallbackSprite = topDownWalkingSprite != null
                    ? topDownWalkingSprite
                    : idleSprite;
                break;

            // A: berjalan ke kiri.
            case FacingDirection.Left:
                frame1 = walkingLeftFrame1;
                frame2 = walkingLeftFrame2;

                fallbackSprite = walkingProfile2 != null
                    ? walkingProfile2
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;

            // D: berjalan ke kanan.
            case FacingDirection.Right:
                frame1 = walkingRightFrame1;
                frame2 = walkingRightFrame2;

                fallbackSprite = walkingProfile1 != null
                    ? walkingProfile1
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;
        }

        if (frame1 != null && frame2 != null)
        {
            AnimateFrames(frame1, frame2);
        }
        else
        {
            ResetAnimation();

            if (fallbackSprite != null)
                spriteRenderer.sprite = fallbackSprite;
        }
    }

    void AnimateFrames(Sprite frame1, Sprite frame2)
    {
        if (spriteRenderer == null)
            return;

        if (animatedDirection != lastDirection)
        {
            animatedDirection = lastDirection;
            walkAnimationTimer = 0f;
            currentWalkFrame = 0;

            spriteRenderer.sprite = frame1;
            return;
        }

        walkAnimationTimer += Time.deltaTime;

        if (walkAnimationTimer >= walkAnimationSpeed)
        {
            walkAnimationTimer -= walkAnimationSpeed;
            currentWalkFrame = 1 - currentWalkFrame;

            spriteRenderer.sprite = currentWalkFrame == 0
                ? frame1
                : frame2;
        }
    }

    void ResetAnimation()
    {
        walkAnimationTimer = 0f;
        currentWalkFrame = 0;
        animatedDirection = null;
    }

    void SetIdleSprite()
    {
        if (spriteRenderer == null)
            return;

        switch (lastDirection)
        {
            case FacingDirection.Back:
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleBackSprite != null
                    ? idleBackSprite
                    : (walkingBackSprite != null
                        ? walkingBackSprite
                        : idleSprite);
                break;

            case FacingDirection.Front:
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleFrontSprite != null
                    ? idleFrontSprite
                    : idleSprite;
                break;

            case FacingDirection.Left:
                // Prioritaskan sprite idle kiri.
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleLeftSprite != null
                    ? idleLeftSprite
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;

            case FacingDirection.Right:
                // Pertahankan perilaku idle kanan.
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleRightSprite != null
                    ? idleRightSprite
                    : (profileHoodieSprite != null
                        ? profileHoodieSprite
                        : walkingProfile1);
                break;
        }
    }

    public void StopMovement()
    {
        canMove = false;
        movement = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetAnimation();
        SetIdleSprite();
    }

    public void ResumeMovement()
    {
        canMove = true;
    }
}
