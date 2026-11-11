using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CCTVMazeMinigameManager : MonoBehaviour
{
    public static CCTVMazeMinigameManager Instance;

    [Header("Panel & Monitor Komputer Utama")]
    [Tooltip("Panel latar belakang gelap (full screen) di dalam MazeMinigameCanvas.")]
    public GameObject mazeMinigamePanel;

    [Tooltip("RectTransform gambar Monitor Komputer yang akan dianimasikan naik dari bawah ke atas.")]
    public RectTransform computerMonitorRect;

    [Tooltip("Image tempat gambar Monitor / Labirin Level 1, 2, dan 3 ditampilkan.")]
    public Image mazeDisplayImage;

    [Tooltip("Opsional: Masukkan panel Hotbar agar disembunyikan sementara saat meretas komputer.")]
    public GameObject hotbarPanel;

    [Header("Asset PNG Labirin 3 Stage")]
    [Tooltip("PNG Komputer/Labirin Level 1 (Mudah).")]
    public Sprite level1MazeSprite;

    [Tooltip("PNG Komputer/Labirin Level 2 (Sedang).")]
    public Sprite level2MazeSprite;

    [Tooltip("PNG Komputer/Labirin Level 3 (Sulit).")]
    public Sprite level3MazeSprite;

    [Header("Titik Start & Finish Tiap Level (Objek UI di Atas Port Abu-abu)")]
    public RectTransform startPointLevel1;
    public RectTransform finishPointLevel1;

    public RectTransform startPointLevel2;
    public RectTransform finishPointLevel2;

    public RectTransform startPointLevel3;
    public RectTransform finishPointLevel3;

    [Header("Titik Sinyal / Kursor Player di Dalam Labirin")]
    [Tooltip("Objek UI Image kecil (titik cahaya/sinyal) yang digerakkan pemain di dalam labirin.")]
    public RectTransform playerCursorRect;

    [Tooltip("Kecepatan gerak titik sinyal di dalam labirin (piksel per detik).")]
    public float cursorMoveSpeed = 135f;

    [Tooltip("Jika dicentang, saat menabrak garis hijau akan Gagal & Ulang ke Level 1 (ala GTA V). Jika tidak dicentang, garis hijau hanya menahan titik agar tidak tembus.")]
    public bool resetKeLevel1SaatNabrak = true;

    [Tooltip("Jika dicentang, titik sinyal melaju terus otomatis seperti GTA V Circuit Breaker. Jika tidak dicentang, hanya bergerak saat tombol WASD/Panah ditekan.")]
    public bool continuousGTAStyleMovement = false;

    [Tooltip("Jarak (piksel UI) untuk dianggap sudah menyentuh titik Finish.")]
    public float finishReachRadius = 22f;

    [Tooltip("Radius deteksi tabrakan di sekitar titik sinyal (piksel UI).")]
    public float cursorCollisionRadius = 5f;

    [Header("Sensitivitas Deteksi Warna Dinding Hijau PNG")]
    [Tooltip("Batas minimum kecerahan warna Hijau (G) untuk dianggap sebagai garis dinding labirin (default 0.38).")]
    [Range(0.20f, 0.85f)]
    public float wallGreenThreshold = 0.38f;

    [Tooltip("Selisih minimum warna Hijau dibanding Merah (G - R) agar latar gelap tidak dianggap dinding (default 0.15).")]
    [Range(0.05f, 0.45f)]
    public float wallGreenContrast = 0.15f;

    [Header("Teks Status / Notifikasi di Layar Komputer (Opsional)")]
    public TMP_Text statusText;

    [Header("Pengaturan Animasi Komputer Naik dari Bawah")]
    [Tooltip("Jarak awal komputer di bawah layar sebelum naik ke atas (piksel UI).")]
    public float slideUpStartOffset = -1100f;

    [Tooltip("Durasi animasi komputer naik dari bawah ke tengah layar (detik).")]
    public float slideUpDuration = 0.65f;

    [Tooltip("Kekuatan efek membal (overshoot) saat komputer berhenti di tengah layar.")]
    public float slideUpOvershoot = 1.35f;

    private int currentStage = 1;
    private bool isPlayingMaze = false;
    private bool isTransitioning = false;
    private bool allCCTVsDisabled = false;

    private Vector2 currentMoveDir = Vector2.zero;
    private Vector2 monitorDefaultAnchoredPos;
    private bool defaultPosSaved = false;

    private GameObject currentPlayer;
    private Image panelBgImage;
    private Coroutine activeCoroutine;

    // Cache tekstur yang sudah dikonversi otomatis menjadi Readable di RAM (tanpa perlu centang Read/Write manual)
    private Dictionary<Texture2D, Texture2D> readableTextureCache = new Dictionary<Texture2D, Texture2D>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        AutoFindMissingReferences();
        SaveDefaultMonitorPosition();

        if (mazeMinigamePanel != null)
            mazeMinigamePanel.SetActive(false);
    }

    void Start()
    {
        AutoFindMissingReferences();
        SaveDefaultMonitorPosition();

        // Siapkan salinan tekstur yang bisa dibaca pikselnya secara otomatis
        PrewarmReadableTextures();

        if (mazeMinigamePanel != null && !isPlayingMaze && !isTransitioning)
            mazeMinigamePanel.SetActive(false);
    }

    void PrewarmReadableTextures()
    {
        if (level1MazeSprite != null && level1MazeSprite.texture != null)
            GetReadableTexture(level1MazeSprite.texture);

        if (level2MazeSprite != null && level2MazeSprite.texture != null)
            GetReadableTexture(level2MazeSprite.texture);

        if (level3MazeSprite != null && level3MazeSprite.texture != null)
            GetReadableTexture(level3MazeSprite.texture);
    }

    /// <summary>
    /// Membuat salinan Texture2D yang bisa dibaca pikselnya (GetPixel) secara otomatis melalui RenderTexture,
    /// sehingga deteksi tembok hijau tetap bekerja meskipun Read/Write lupa dicentang di Inspector PNG!
    /// </summary>
    Texture2D GetReadableTexture(Texture2D source)
    {
        if (source == null)
            return null;

        if (source.isReadable)
            return source;

        if (readableTextureCache.TryGetValue(source, out Texture2D cached) && cached != null)
            return cached;

        RenderTexture tmp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readableCopy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableCopy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        readableTextureCache[source] = readableCopy;
        return readableCopy;
    }

    void AutoFindMissingReferences()
    {
        if (mazeMinigamePanel == null)
        {
            GameObject foundPanel = GameObject.Find("MazeMinigamePanel");
            if (foundPanel != null)
                mazeMinigamePanel = foundPanel;
        }

        if (computerMonitorRect == null)
        {
            GameObject foundMonitor = GameObject.Find("ComputerMonitor");
            if (foundMonitor != null)
                computerMonitorRect = foundMonitor.GetComponent<RectTransform>();
        }

        if (mazeDisplayImage == null && computerMonitorRect != null)
        {
            mazeDisplayImage = computerMonitorRect.GetComponent<Image>();
        }

        if (computerMonitorRect != null)
        {
            if (startPointLevel1 == null)
                startPointLevel1 = FindChildRect(computerMonitorRect, "Start_Level1");
            if (finishPointLevel1 == null)
                finishPointLevel1 = FindChildRect(computerMonitorRect, "Finish_Level1");
            if (startPointLevel2 == null)
                startPointLevel2 = FindChildRect(computerMonitorRect, "Start_Level2");
            if (finishPointLevel2 == null)
                finishPointLevel2 = FindChildRect(computerMonitorRect, "Finish_Level2");
            if (startPointLevel3 == null)
                startPointLevel3 = FindChildRect(computerMonitorRect, "Start_Level3");
            if (finishPointLevel3 == null)
                finishPointLevel3 = FindChildRect(computerMonitorRect, "Finish_Level3");
            if (playerCursorRect == null)
                playerCursorRect = FindChildRect(computerMonitorRect, "PlayerCursor");
        }
    }

    RectTransform FindChildRect(Transform parent, string childName)
    {
        Transform t = parent.Find(childName);
        return t != null ? t.GetComponent<RectTransform>() : null;
    }

    void SaveDefaultMonitorPosition()
    {
        if (defaultPosSaved)
            return;

        if (computerMonitorRect == null && mazeDisplayImage != null)
            computerMonitorRect = mazeDisplayImage.rectTransform;

        if (computerMonitorRect != null)
            monitorDefaultAnchoredPos = computerMonitorRect.anchoredPosition;

        if (mazeMinigamePanel != null)
            panelBgImage = mazeMinigamePanel.GetComponent<Image>();

        defaultPosSaved = true;
    }

    public void OpenComputerMaze(GameObject playerObj)
    {
        if (allCCTVsDisabled)
        {
            Debug.Log("Sistem CCTV sudah dimatikan sebelumnya!");
            return;
        }

        SaveDefaultMonitorPosition();
        PrewarmReadableTextures();
        currentPlayer = playerObj;

        if (currentPlayer != null)
        {
            PlayerMovement pm = currentPlayer.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.StopMovement();
        }

        if (hotbarPanel != null)
            hotbarPanel.SetActive(false);

        if (mazeMinigamePanel != null)
            mazeMinigamePanel.SetActive(true);

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(PlayComputerSlideUpAndStart());
    }

    IEnumerator PlayComputerSlideUpAndStart()
    {
        isPlayingMaze = false;
        isTransitioning = true;

        currentStage = 1;
        ApplyStageVisuals(1);

        if (playerCursorRect != null)
            playerCursorRect.gameObject.SetActive(false);

        if (statusText != null)
            statusText.text = "MENGHUBUNGKAN KE SERVER CCTV...";

        Vector2 startPos = monitorDefaultAnchoredPos + new Vector2(0f, slideUpStartOffset);
        Vector2 targetPos = monitorDefaultAnchoredPos;

        if (computerMonitorRect != null)
        {
            computerMonitorRect.anchoredPosition = startPos;
            computerMonitorRect.localScale = new Vector3(0.88f, 0.88f, 1f);
        }

        Color bgTargetColor = panelBgImage != null
            ? new Color(0f, 0f, 0f, 0.78f)
            : Color.clear;

        float elapsed = 0f;
        while (elapsed < slideUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideUpDuration);

            float c1 = slideUpOvershoot;
            float c3 = c1 + 1f;
            float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            if (computerMonitorRect != null)
            {
                computerMonitorRect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, easedT);
                float scale = Mathf.Lerp(0.88f, 1f, Mathf.SmoothStep(0f, 1f, t));
                computerMonitorRect.localScale = new Vector3(scale, scale, 1f);
            }

            if (panelBgImage != null)
            {
                panelBgImage.color = Color.Lerp(Color.clear, bgTargetColor, t);
            }

            yield return null;
        }

        if (computerMonitorRect != null)
        {
            computerMonitorRect.anchoredPosition = targetPos;
            computerMonitorRect.localScale = Vector3.one;
        }

        if (mazeDisplayImage != null)
        {
            mazeDisplayImage.color = new Color(1.3f, 1.3f, 1.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            mazeDisplayImage.color = Color.white;
        }

        StartStage(1);
    }

    void ApplyStageVisuals(int stage)
    {
        if (mazeDisplayImage == null)
            return;

        switch (stage)
        {
            case 1:
                if (level1MazeSprite != null)
                    mazeDisplayImage.sprite = level1MazeSprite;
                break;
            case 2:
                if (level2MazeSprite != null)
                    mazeDisplayImage.sprite = level2MazeSprite;
                break;
            case 3:
                if (level3MazeSprite != null)
                    mazeDisplayImage.sprite = level3MazeSprite;
                break;
        }

        mazeDisplayImage.color = Color.white;
    }

    void StartStage(int stage)
    {
        currentStage = stage;
        ApplyStageVisuals(stage);

        RectTransform startPt = GetStartPoint(stage);

        if (playerCursorRect != null)
        {
            playerCursorRect.gameObject.SetActive(true);
            if (startPt != null)
            {
                playerCursorRect.position = startPt.position;
            }
        }

        currentMoveDir = stage == 3 ? Vector2.down : Vector2.right;

        if (statusText != null)
        {
            string diffName = stage == 1 ? "MUDAH" : (stage == 2 ? "SEDANG" : "SULIT");
            statusText.text = "STAGE " + stage + "/3 (" + diffName + ") - GUNAKAN WASD / PANAH";
        }

        isTransitioning = false;
        isPlayingMaze = true;
    }

    void Update()
    {
        if (mazeMinigamePanel == null || !mazeMinigamePanel.activeSelf)
            return;

        if (Keyboard.current == null)
            return;

        if (!isTransitioning &&
            (Keyboard.current.xKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            activeCoroutine = StartCoroutine(CloseComputerSequence(false));
            return;
        }

        if (!isPlayingMaze || isTransitioning || playerCursorRect == null)
            return;

        HandleCursorMovementAndCollision();
    }

    /// <summary>
    /// Menggerakkan titik sinyal secara bertahap (sub-stepping) agar tidak pernah menembus garis dinding hijau,
    /// menahan titik sinyal tepat di depan dinding, lalu memicu Gagal/Reset ke Level 1 (jika diaktifkan).
    /// </summary>
    void HandleCursorMovementAndCollision()
    {
        Vector2 inputDir = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            inputDir = Vector2.up;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            inputDir = Vector2.down;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputDir = Vector2.left;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputDir = Vector2.right;

        if (continuousGTAStyleMovement)
        {
            if (inputDir != Vector2.zero)
                currentMoveDir = inputDir;
        }
        else
        {
            currentMoveDir = inputDir;
        }

        if (currentMoveDir != Vector2.zero)
        {
            float totalMoveDist = cursorMoveSpeed * Time.deltaTime;
            // Bagi langkah gerak per 1.5 piksel agar titik sinyal tidak pernah melompati garis dinding tipis
            int steps = Mathf.Max(1, Mathf.CeilToInt(totalMoveDist / 1.5f));
            Vector2 stepDelta = currentMoveDir * (totalMoveDist / steps);

            for (int s = 0; s < steps; s++)
            {
                Vector2 prevAnchoredPos = playerCursorRect.anchoredPosition;
                playerCursorRect.anchoredPosition = prevAnchoredPos + stepDelta;

                // Jika langkah ini menyentuh garis dinding hijau:
                if (IsCursorTouchingGreenWall())
                {
                    // Tahan titik sinyal di posisi aman terakhir (sebagai barrier agar tidak tembus!)
                    playerCursorRect.anchoredPosition = prevAnchoredPos;

                    if (resetKeLevel1SaatNabrak)
                    {
                        OnStageFailed();
                    }
                    return;
                }

                // Cek apakah sudah menyentuh titik Finish
                if (HasReachedFinishPoint())
                {
                    OnStageCleared();
                    return;
                }
            }
        }
        else
        {
            if (HasReachedFinishPoint())
            {
                OnStageCleared();
            }
        }
    }

    bool HasReachedFinishPoint()
    {
        RectTransform finishPt = GetFinishPoint(currentStage);
        if (finishPt == null || mazeDisplayImage == null)
            return false;

        float distToFinish = Vector2.Distance(
            mazeDisplayImage.rectTransform.InverseTransformPoint(playerCursorRect.position),
            mazeDisplayImage.rectTransform.InverseTransformPoint(finishPt.position)
        );

        return distToFinish <= finishReachRadius;
    }

    /// <summary>
    /// Membaca warna piksel langsung dari gambar PNG labirin menggunakan salinan tekstur yang otomatis Readable
    /// dan memetakan koordinat berdasarkan activeSprite.rect agar akurat 100% pada komponen UI Image.
    /// </summary>
    bool IsCursorTouchingGreenWall()
    {
        if (mazeDisplayImage == null || mazeDisplayImage.sprite == null || playerCursorRect == null)
            return false;

        Sprite activeSprite = mazeDisplayImage.sprite;
        Texture2D tex = GetReadableTexture(activeSprite.texture);

        if (tex == null)
            return false;

        float rCol = cursorCollisionRadius;
        float diag = rCol * 0.7f;

        // 9 titik sampel di sekeliling kursor (tengah, atas, bawah, kiri, kanan, dan 4 sudut diagonal)
        Vector2[] sampleOffsets = new Vector2[]
        {
            Vector2.zero,
            new Vector2(rCol, 0f),
            new Vector2(-rCol, 0f),
            new Vector2(0f, rCol),
            new Vector2(0f, -rCol),
            new Vector2(diag, diag),
            new Vector2(-diag, diag),
            new Vector2(diag, -diag),
            new Vector2(-diag, -diag)
        };

        RectTransform imgRect = mazeDisplayImage.rectTransform;
        Rect uiRect = imgRect.rect;

        if (uiRect.width <= 0.01f || uiRect.height <= 0.01f)
            return false;

        // Gunakan activeSprite.rect (bukan textureRect) karena UI Image Simple merentangkan rect penuh sprite
        Rect spriteRect = activeSprite.rect;

        for (int i = 0; i < sampleOffsets.Length; i++)
        {
            Vector3 worldSamplePos = playerCursorRect.TransformPoint((Vector3)sampleOffsets[i]);
            Vector2 localPos = imgRect.InverseTransformPoint(worldSamplePos);

            float normX = (localPos.x - uiRect.x) / uiRect.width;
            float normY = (localPos.y - uiRect.y) / uiRect.height;

            if (normX < 0f || normX > 1f || normY < 0f || normY > 1f)
                continue;

            int pixelX = Mathf.Clamp(
                Mathf.RoundToInt(spriteRect.x + normX * spriteRect.width),
                0,
                tex.width - 1
            );
            int pixelY = Mathf.Clamp(
                Mathf.RoundToInt(spriteRect.y + normY * spriteRect.height),
                0,
                tex.height - 1
            );

            Color c = tex.GetPixel(pixelX, pixelY);

            // Abaikan warna abu-abu pada konektor Start/Finish (R, G, B hampir sama)
            bool isGreyPort = Mathf.Abs(c.r - c.g) < 0.08f && Mathf.Abs(c.g - c.b) < 0.08f;
            if (isGreyPort)
                continue;

            // Garis dinding labirin berwarna hijau neon terang
            if (c.a > 0.5f && c.g >= wallGreenThreshold && (c.g - c.r) >= wallGreenContrast)
            {
                return true;
            }
        }

        return false;
    }

    void OnStageCleared()
    {
        isPlayingMaze = false;
        isTransitioning = true;

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(StageClearedRoutine());
    }

    IEnumerator StageClearedRoutine()
    {
        if (currentStage < 3)
        {
            int nextStage = currentStage + 1;

            if (statusText != null)
                statusText.text = "STAGE " + currentStage + " BERHASIL! MEMUAT STAGE " + nextStage + "...";

            if (mazeDisplayImage != null)
            {
                mazeDisplayImage.color = new Color(0.6f, 1f, 0.7f, 1f);
                yield return new WaitForSeconds(0.25f);
                mazeDisplayImage.color = Color.white;
            }

            yield return new WaitForSeconds(0.45f);
            StartStage(nextStage);
        }
        else
        {
            if (statusText != null)
                statusText.text = "RETAS BERHASIL! SELURUH SISTEM CCTV DINONAKTIFKAN!";

            if (playerCursorRect != null)
                playerCursorRect.gameObject.SetActive(false);

            DisableAllCCTVsInScene();

            yield return new WaitForSeconds(1.2f);
            yield return StartCoroutine(CloseComputerSequence(true));
        }
    }

    void OnStageFailed()
    {
        isPlayingMaze = false;
        isTransitioning = true;

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(StageFailedRoutine());
    }

    IEnumerator StageFailedRoutine()
    {
        if (statusText != null)
            statusText.text = "GAGAL! MENABRAK DINDING - KEMBALI KE LEVEL 1!";

        if (mazeDisplayImage != null)
            mazeDisplayImage.color = new Color(1f, 0.45f, 0.45f, 1f);

        if (computerMonitorRect != null)
        {
            float elapsed = 0f;
            float duration = 0.45f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fade = 1f - (elapsed / duration);
                float offsetX = Mathf.Sin(elapsed * 65f) * 14f * fade;
                computerMonitorRect.anchoredPosition = monitorDefaultAnchoredPos + new Vector2(offsetX, 0f);
                yield return null;
            }
            computerMonitorRect.anchoredPosition = monitorDefaultAnchoredPos;
        }
        else
        {
            yield return new WaitForSeconds(0.45f);
        }

        if (mazeDisplayImage != null)
            mazeDisplayImage.color = Color.white;

        yield return new WaitForSeconds(0.35f);

        // Kembali ke Stage 1 (Level 1)
        StartStage(1);
    }

    void DisableAllCCTVsInScene()
    {
        allCCTVsDisabled = true;

        CCTVController[] controllers = FindObjectsByType<CCTVController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null)
            {
                if (controllers[i].visionCone != null)
                    controllers[i].visionCone.gameObject.SetActive(false);

                controllers[i].enabled = false;
            }
        }

        CCTVVision[] visions = FindObjectsByType<CCTVVision>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < visions.Length; i++)
        {
            if (visions[i] != null)
            {
                if (visions[i].visionCone != null)
                    visions[i].visionCone.gameObject.SetActive(false);

                visions[i].enabled = false;
            }
        }

        Debug.Log("SELURUH SISTEM CCTV BERHASIL DIMATIKAN!");
    }

    IEnumerator CloseComputerSequence(bool hackCompleted)
    {
        isPlayingMaze = false;
        isTransitioning = true;

        Vector2 startPos = computerMonitorRect != null
            ? computerMonitorRect.anchoredPosition
            : monitorDefaultAnchoredPos;
        Vector2 endPos = monitorDefaultAnchoredPos + new Vector2(0f, slideUpStartOffset);

        float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = t * t;

            if (computerMonitorRect != null)
                computerMonitorRect.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        if (mazeMinigamePanel != null)
            mazeMinigamePanel.SetActive(false);

        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);

        if (currentPlayer != null)
        {
            PlayerMovement pm = currentPlayer.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.ResumeMovement();
        }

        isTransitioning = false;
    }

    RectTransform GetStartPoint(int stage)
    {
        switch (stage)
        {
            case 1: return startPointLevel1;
            case 2: return startPointLevel2;
            case 3: return startPointLevel3;
            default: return startPointLevel1;
        }
    }

    RectTransform GetFinishPoint(int stage)
    {
        switch (stage)
        {
            case 1: return finishPointLevel1;
            case 2: return finishPointLevel2;
            case 3: return finishPointLevel3;
            default: return finishPointLevel1;
        }
    }

    public bool AreAllCCTVsDisabled()
    {
        return allCCTVsDisabled;
    }
}