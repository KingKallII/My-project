
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    // Menyimpan urutan tombol yang sedang ditekan
    private List<Key> pressedKeys = new List<Key>();

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

        if (Keyboard.current == null)
        {
            movement = Vector2.zero;
            pressedKeys.Clear();
            return;
        }

        // =====================================
        // DETEKSI TOMBOL BARU DITEKAN
        // =====================================

        if (Keyboard.current.wKey.wasPressedThisFrame)
            RegisterKeyPress(Key.W);

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            RegisterKeyPress(Key.UpArrow);

        if (Keyboard.current.sKey.wasPressedThisFrame)
            RegisterKeyPress(Key.S);

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            RegisterKeyPress(Key.DownArrow);

        if (Keyboard.current.aKey.wasPressedThisFrame)
            RegisterKeyPress(Key.A);

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            RegisterKeyPress(Key.LeftArrow);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            RegisterKeyPress(Key.D);

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            RegisterKeyPress(Key.RightArrow);


        // =====================================
        // DETEKSI TOMBOL DILEPASKAN
        // =====================================

        if (Keyboard.current.wKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.W);

        if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.UpArrow);

        if (Keyboard.current.sKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.S);

        if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.DownArrow);

        if (Keyboard.current.aKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.A);

        if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.LeftArrow);

        if (Keyboard.current.dKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.D);

        if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
            RegisterKeyRelease(Key.RightArrow);


        // =====================================
        // BERSIHKAN INPUT YANG SUDAH DILEPAS
        // =====================================

        pressedKeys.RemoveAll(
            key => !Keyboard.current[key].isPressed
        );


        // =====================================
        // AMBIL TOMBOL TERAKHIR YANG MASIH AKTIF
        // =====================================

        movement = Vector2.zero;

        if (pressedKeys.Count > 0)
        {
            Key activeKey = pressedKeys[pressedKeys.Count - 1];

            switch (activeKey)
            {
                case Key.W:
                case Key.UpArrow:
                    movement = Vector2.up;
                    break;

                case Key.S:
                case Key.DownArrow:
                    movement = Vector2.down;
                    break;

                case Key.A:
                case Key.LeftArrow:
                    movement = Vector2.left;
                    break;

                case Key.D:
                case Key.RightArrow:
                    movement = Vector2.right;
                    break;
            }
        }


        // =====================================
        // UPDATE ARAH DAN ANIMASI
        // =====================================

        if (movement != Vector2.zero)
        {
            if (movement.x < 0)
            {
                lastDirection = FacingDirection.Left;
            }
            else if (movement.x > 0)
            {
                lastDirection = FacingDirection.Right;
            }
            else if (movement.y > 0)
            {
                lastDirection = FacingDirection.Back;
            }
            else if (movement.y < 0)
            {
                lastDirection = FacingDirection.Front;
            }

            UpdateWalkingSprite();
        }
        else
        {
            ResetAnimation();
            SetIdleSprite();
        }
    }


    // =====================================
    // REGISTER TOMBOL
    // =====================================

    void RegisterKeyPress(Key key)
    {
        // Hapus jika sudah ada agar tidak duplikat
        pressedKeys.Remove(key);

        // Masukkan sebagai tombol terbaru
        pressedKeys.Add(key);
    }

    void RegisterKeyRelease(Key key)
    {
        pressedKeys.Remove(key);
    }


    // =====================================
    // PERGERAKAN PLAYER
    // =====================================

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


    // =====================================
    // UPDATE SPRITE BERJALAN
    // =====================================

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
            // W: berjalan membelakangi kamera
            case FacingDirection.Back:
                frame1 = walkingAwayFrame1;
                frame2 = walkingAwayFrame2;

                fallbackSprite = walkingAwaySprite != null
                    ? walkingAwaySprite
                    : walkingBackSprite;
                break;

            // S: berjalan menghadap kamera
            case FacingDirection.Front:
                frame1 = walkingFrontFrame1;
                frame2 = walkingFrontFrame2;

                fallbackSprite = topDownWalkingSprite != null
                    ? topDownWalkingSprite
                    : idleSprite;
                break;

            // A: berjalan ke kiri
            case FacingDirection.Left:
                frame1 = walkingLeftFrame1;
                frame2 = walkingLeftFrame2;

                fallbackSprite = walkingProfile2 != null
                    ? walkingProfile2
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;

            // D: berjalan ke kanan
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


    // =====================================
    // ANIMASI BERJALAN
    // =====================================

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


    // =====================================
    // RESET ANIMASI
    // =====================================

    void ResetAnimation()
    {
        walkAnimationTimer = 0f;
        currentWalkFrame = 0;
        animatedDirection = null;
    }


    // =====================================
    // IDLE SPRITE SETIAP ARAH
    // =====================================

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
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleLeftSprite != null
                    ? idleLeftSprite
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;

            case FacingDirection.Right:
                spriteRenderer.flipX = false;

                spriteRenderer.sprite = idleRightSprite != null
                    ? idleRightSprite
                    : (profileHoodieSprite != null
                        ? profileHoodieSprite
                        : walkingProfile1);
                break;
        }
    }


    // =====================================
    // STOP MOVEMENT
    // =====================================

    public void StopMovement()
    {
        canMove = false;
        movement = Vector2.zero;

        pressedKeys.Clear();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetAnimation();
        SetIdleSprite();
    }


    // =====================================
    // RESUME MOVEMENT
    // =====================================

    public void ResumeMovement()
    {
        canMove = true;
    }
}
