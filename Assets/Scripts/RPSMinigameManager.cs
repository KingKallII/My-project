using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RPSMinigameManager : MonoBehaviour
{
    public static RPSMinigameManager Instance;

    public enum RPSChoice
    {
        Kertas = 0,
        Gunting = 1,
        Batu = 2
    }

    [Header("Panel Utama Minigame")]
    [Tooltip("Masukkan Panel utama di dalam MinigameCanvas yang akan dimunculkan saat tertangkap.")]
    public GameObject minigamePanel;

    [Tooltip("Opsional: Masukkan panel Hotbar agar disembunyikan sementara saat minigame berlangsung.")]
    public GameObject hotbarPanel;

    [Header("UI Karakter (Kiri = Player, Kanan = Guard)")]
    public Image playerPortraitImage;
    public Image guardPortraitImage;

    [Header("Asset PNG Karakter (Tempeli PNG dari Figma)")]
    [Tooltip("PNG Player sedang berpikir (memegang dagu).")]
    public Sprite playerNormalSprite;

    [Tooltip("PNG Player ekspresi kaget saat kalah.")]
    public Sprite playerLoseSprite;

    [Tooltip("PNG Guard pasang kuda-kuda normal.")]
    public Sprite guardNormalSprite;

    [Tooltip("PNG Guard ekspresi kaget/kalah saat Player menang.")]
    public Sprite guardLoseSprite;

    [Header("Asset PNG Kartu (Kertas, Gunting, Batu)")]
    [Tooltip("PNG Kartu Kertas (Merah - Telapak Tangan).")]
    public Sprite kertasSprite;

    [Tooltip("PNG Kartu Gunting (Biru - Jari V).")]
    public Sprite guntingSprite;

    [Tooltip("PNG Kartu Batu (Kuning - Kepalan Tangan).")]
    public Sprite batuSprite;

    [Header("Wadah & Tombol 3 Pilihan Player")]
    [Tooltip("GameObject induk tempat 3 tombol kartu Player berada (agar bisa disembunyikan saat adu kartu).")]
    public GameObject choiceButtonsContainer;
    public Button btnKertas;
    public Button btnGunting;
    public Button btnBatu;

    [Header("UI Kartu Hasil Adu (Di Kiri & Kanan Tulisan VS)")]
    [Tooltip("Image kartu pilihan Player yang muncul di sebelah kiri tulisan VS saat diadu.")]
    public Image playerSelectedCardImage;

    [Tooltip("Image kartu pilihan Guard yang muncul di sebelah kanan tulisan VS saat diadu.")]
    public Image guardSelectedCardImage;

    [Header("Teks Judul / Status (Opsional)")]
    [Tooltip("Teks di bagian atas, misalnya 'Pilih Strategi' yang berubah jadi 'Kamu Menang!' / 'Seri!' / 'Kamu Kalah!'.")]
    public TMP_Text statusTitleText;

    [Header("Pengaturan Animasi Pembuka (Intro Bertahap)")]
    [Tooltip("Durasi layar hitam pekat sebelum background muncul (detik).")]
    public float blackScreenDuration = 1.0f;

    [Tooltip("Durasi transisi munculnya background dari layar hitam (detik).")]
    public float backgroundRevealDuration = 0.45f;

    [Tooltip("Jarak luncur karakter Player (dari kiri) dan Guard (dari kanan) dalam piksel UI.")]
    public float characterSlideDistance = 650f;

    [Tooltip("Durasi animasi meluncurnya Player dan Guard dari samping layar (detik).")]
    public float characterSlideDuration = 0.45f;

    [Tooltip("Durasi animasi pop-up tiap kartu pilihan saat muncul satu per satu (detik).")]
    public float cardPopDuration = 0.22f;

    [Tooltip("Jeda waktu antar kemunculan kartu Kertas -> Gunting -> Batu (detik).")]
    public float cardStaggerDelay = 0.18f;

    [Header("Pengaturan Animasi Hasil Adu")]
    [Tooltip("Seberapa jauh gambar yang kalah turun ke bawah (piksel UI) sesuai desain Figma.")]
    public float loseDropOffset = 35f;

    [Tooltip("Kekuatan getaran (shake) saat karakter kaget/kalah.")]
    public float shakeIntensity = 12f;

    [Tooltip("Durasi animasi kaget sebelum minigame selesai (detik).")]
    public float resultDisplayDuration = 1.5f;

    private GameObject currentPlayer;
    private GuardCatch currentGuardCatch;
    private GuardPatrol currentGuardPatrol;

    private Image minigamePanelImage;
    private RectTransform playerPortraitRect;
    private RectTransform guardPortraitRect;
    private Vector2 playerDefaultAnchoredPos;
    private Vector2 guardDefaultAnchoredPos;

    private Vector3 kertasDefaultScale = Vector3.one;
    private Vector3 guntingDefaultScale = Vector3.one;
    private Vector3 batuDefaultScale = Vector3.one;

    private bool defaultTransformsSaved = false;
    private bool isWaitingForChoice = false;
    private Coroutine activeSequenceCoroutine;

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
    }

    void Start()
    {
        SaveDefaultTransforms();

        // Hubungkan klik tombol secara otomatis ke fungsi pemilihan kartu
        if (btnKertas != null)
        {
            btnKertas.onClick.RemoveAllListeners();
            btnKertas.onClick.AddListener(() => OnPlayerChoose(RPSChoice.Kertas));

            if (kertasSprite != null && btnKertas.image != null)
                btnKertas.image.sprite = kertasSprite;
        }

        if (btnGunting != null)
        {
            btnGunting.onClick.RemoveAllListeners();
            btnGunting.onClick.AddListener(() => OnPlayerChoose(RPSChoice.Gunting));

            if (guntingSprite != null && btnGunting.image != null)
                btnGunting.image.sprite = guntingSprite;
        }

        if (btnBatu != null)
        {
            btnBatu.onClick.RemoveAllListeners();
            btnBatu.onClick.AddListener(() => OnPlayerChoose(RPSChoice.Batu));

            if (batuSprite != null && btnBatu.image != null)
                btnBatu.image.sprite = batuSprite;
        }

        if (minigamePanel != null)
            minigamePanel.SetActive(false);
    }

    void SaveDefaultTransforms()
    {
        if (defaultTransformsSaved)
            return;

        if (minigamePanel != null)
            minigamePanelImage = minigamePanel.GetComponent<Image>();

        if (playerPortraitImage != null)
        {
            playerPortraitRect = playerPortraitImage.GetComponent<RectTransform>();
            if (playerPortraitRect != null)
                playerDefaultAnchoredPos = playerPortraitRect.anchoredPosition;
        }

        if (guardPortraitImage != null)
        {
            guardPortraitRect = guardPortraitImage.GetComponent<RectTransform>();
            if (guardPortraitRect != null)
                guardDefaultAnchoredPos = guardPortraitRect.anchoredPosition;
        }

        if (btnKertas != null)
            kertasDefaultScale = btnKertas.transform.localScale;

        if (btnGunting != null)
            guntingDefaultScale = btnGunting.transform.localScale;

        if (btnBatu != null)
            batuDefaultScale = btnBatu.transform.localScale;

        defaultTransformsSaved = true;
    }

    /// <summary>
    /// Dipanggil oleh GuardCatch saat Player tertangkap oleh Guard.
    /// Memulai urutan animasi: Layar Hitam (1 dtk) -> Background -> Karakter dari Samping -> Kartu Satu per Satu.
    /// </summary>
    public void StartMinigame(GameObject playerObj, GuardCatch guardCatch, GuardPatrol guardPatrol)
    {
        SaveDefaultTransforms();

        currentPlayer = playerObj;
        currentGuardCatch = guardCatch;
        currentGuardPatrol = guardPatrol;

        if (hotbarPanel != null)
            hotbarPanel.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        if (activeSequenceCoroutine != null)
            StopCoroutine(activeSequenceCoroutine);

        activeSequenceCoroutine = StartCoroutine(PlayIntroAndSelectionSequence());
    }

    /// <summary>
    /// Urutan animasi pembuka minigame secara bertahap agar terasa natural.
    /// </summary>
    IEnumerator PlayIntroAndSelectionSequence()
    {
        isWaitingForChoice = false;

        // Sembunyikan seluruh elemen anak terlebih dahulu selama layar hitam
        if (playerPortraitImage != null)
            playerPortraitImage.gameObject.SetActive(false);

        if (guardPortraitImage != null)
            guardPortraitImage.gameObject.SetActive(false);

        if (choiceButtonsContainer != null)
            choiceButtonsContainer.SetActive(false);

        if (playerSelectedCardImage != null)
            playerSelectedCardImage.gameObject.SetActive(false);

        if (guardSelectedCardImage != null)
            guardSelectedCardImage.gameObject.SetActive(false);

        if (statusTitleText != null)
        {
            statusTitleText.text = "Pilih Strategi";
            statusTitleText.gameObject.SetActive(false);
        }

        // Pasang sprite pose normal sejak awal
        if (playerPortraitImage != null && playerNormalSprite != null)
            playerPortraitImage.sprite = playerNormalSprite;

        if (guardPortraitImage != null && guardNormalSprite != null)
            guardPortraitImage.sprite = guardNormalSprite;

        // =========================================================================
        // TAHAP 1: LAYAR HITAM SEKITAR 1 DETIK
        // =========================================================================
        if (minigamePanelImage != null)
            minigamePanelImage.color = Color.black;

        yield return new WaitForSeconds(blackScreenDuration);

        // =========================================================================
        // TAHAP 2: MUNCULKAN BACKGROUND TERLEBIH DAHULU (FADE DARI HITAM KE TERANG)
        // =========================================================================
        if (minigamePanelImage != null)
        {
            float elapsedBg = 0f;
            while (elapsedBg < backgroundRevealDuration)
            {
                elapsedBg += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedBg / backgroundRevealDuration);
                minigamePanelImage.color = Color.Lerp(Color.black, Color.white, t);
                yield return null;
            }
            minigamePanelImage.color = Color.white;
        }

        if (statusTitleText != null)
            statusTitleText.gameObject.SetActive(true);

        // Jeda singkat agar background terlihat jelas sebelum karakter masuk
        yield return new WaitForSeconds(0.15f);

        // =========================================================================
        // TAHAP 3: MUNCULKAN PLAYER (DARI KIRI) & GUARD (DARI KANAN)
        // =========================================================================
        Vector2 playerStartPos = playerDefaultAnchoredPos + new Vector2(-characterSlideDistance, 0f);
        Vector2 guardStartPos = guardDefaultAnchoredPos + new Vector2(characterSlideDistance, 0f);

        if (playerPortraitRect != null)
            playerPortraitRect.anchoredPosition = playerStartPos;

        if (guardPortraitRect != null)
            guardPortraitRect.anchoredPosition = guardStartPos;

        if (playerPortraitImage != null)
            playerPortraitImage.gameObject.SetActive(true);

        if (guardPortraitImage != null)
            guardPortraitImage.gameObject.SetActive(true);

        float elapsedSlide = 0f;
        while (elapsedSlide < characterSlideDuration)
        {
            elapsedSlide += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedSlide / characterSlideDuration);
            // Kurva Ease-Out Cubic agar gerakan meluncur cepat di awal dan mengerem halus di akhir
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            if (playerPortraitRect != null)
                playerPortraitRect.anchoredPosition = Vector2.Lerp(playerStartPos, playerDefaultAnchoredPos, easedT);

            if (guardPortraitRect != null)
                guardPortraitRect.anchoredPosition = Vector2.Lerp(guardStartPos, guardDefaultAnchoredPos, easedT);

            yield return null;
        }

        if (playerPortraitRect != null)
            playerPortraitRect.anchoredPosition = playerDefaultAnchoredPos;

        if (guardPortraitRect != null)
            guardPortraitRect.anchoredPosition = guardDefaultAnchoredPos;

        yield return new WaitForSeconds(0.12f);

        // =========================================================================
        // TAHAP 4: MUNCULKAN PILIHAN KARTU SATU PER SATU (KERTAS -> GUNTING -> BATU)
        // =========================================================================
        yield return StartCoroutine(RevealChoiceCardsOneByOne());

        // Setelah semua kartu muncul, izinkan Player memilih kartu
        isWaitingForChoice = true;
    }

    /// <summary>
    /// Menampilkan ke-3 kartu pilihan secara berurutan: Kertas dulu, lalu Gunting, terakhir Batu.
    /// </summary>
    IEnumerator RevealChoiceCardsOneByOne()
    {
        if (choiceButtonsContainer != null)
            choiceButtonsContainer.SetActive(true);

        // Sembunyikan ke-3 tombol terlebih dahulu dengan skala 0
        if (btnKertas != null)
        {
            btnKertas.gameObject.SetActive(false);
            btnKertas.transform.localScale = Vector3.zero;
        }

        if (btnGunting != null)
        {
            btnGunting.gameObject.SetActive(false);
            btnGunting.transform.localScale = Vector3.zero;
        }

        if (btnBatu != null)
        {
            btnBatu.gameObject.SetActive(false);
            btnBatu.transform.localScale = Vector3.zero;
        }

        // 1. Munculkan Kartu Kertas
        if (btnKertas != null)
        {
            btnKertas.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnKertas.transform, kertasDefaultScale));
            yield return new WaitForSeconds(cardStaggerDelay);
        }

        // 2. Munculkan Kartu Gunting
        if (btnGunting != null)
        {
            btnGunting.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnGunting.transform, guntingDefaultScale));
            yield return new WaitForSeconds(cardStaggerDelay);
        }

        // 3. Munculkan Kartu Batu
        if (btnBatu != null)
        {
            btnBatu.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnBatu.transform, batuDefaultScale));
        }
    }

    /// <summary>
    /// Animasi pop-in natural untuk satu tombol kartu (dari skala 0 -> membal sedikit -> ukuran normal).
    /// </summary>
    IEnumerator AnimateSingleButtonPopIn(Transform targetTransform, Vector3 targetScale)
    {
        if (targetTransform == null)
            yield break;

        float elapsed = 0f;
        Vector3 overshootScale = targetScale * 1.12f;

        while (elapsed < cardPopDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cardPopDuration);

            if (t < 0.7f)
            {
                // Membesar dari 0 ke 1.12x
                float subT = t / 0.7f;
                targetTransform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, subT);
            }
            else
            {
                // Kembali halus dari 1.12x ke 1.0x
                float subT = (t - 0.7f) / 0.3f;
                targetTransform.localScale = Vector3.Lerp(overshootScale, targetScale, subT);
            }

            yield return null;
        }

        targetTransform.localScale = targetScale;
    }

    public void OnPlayerChoose(RPSChoice playerChoice)
    {
        if (!isWaitingForChoice)
            return;

        isWaitingForChoice = false;

        // Guard memilih secara acak antara Kertas (0), Gunting (1), atau Batu (2)
        RPSChoice guardChoice = (RPSChoice)Random.Range(0, 3);

        if (activeSequenceCoroutine != null)
            StopCoroutine(activeSequenceCoroutine);

        activeSequenceCoroutine = StartCoroutine(PlayRoundSequence(playerChoice, guardChoice));
    }

    IEnumerator PlayRoundSequence(RPSChoice playerChoice, RPSChoice guardChoice)
    {
        // 1. Sembunyikan 3 tombol pilihan dan tampilkan kartu yang diadu di kiri & kanan "VS"
        if (choiceButtonsContainer != null)
            choiceButtonsContainer.SetActive(false);

        if (playerSelectedCardImage != null)
        {
            playerSelectedCardImage.sprite = GetCardSprite(playerChoice);
            playerSelectedCardImage.gameObject.SetActive(true);
            StartCoroutine(AnimateCardPop(playerSelectedCardImage.rectTransform));
        }

        if (guardSelectedCardImage != null)
        {
            guardSelectedCardImage.sprite = GetCardSprite(guardChoice);
            guardSelectedCardImage.gameObject.SetActive(true);
            StartCoroutine(AnimateCardPop(guardSelectedCardImage.rectTransform));
        }

        // Jeda singkat saat kedua kartu beradu di layar
        yield return new WaitForSeconds(0.4f);

        // 2. Tentukan pemenang: 0 = Seri, 1 = Player Menang, -1 = Player Kalah
        int result = EvaluateWinner(playerChoice, guardChoice);

        if (result == 0)
        {
            // SERI -> Ulangi pemilihan kartu dengan kemunculan kartu satu per satu
            if (statusTitleText != null)
                statusTitleText.text = "Seri! Pilih Lagi!";

            yield return new WaitForSeconds(1.0f);

            if (statusTitleText != null)
                statusTitleText.text = "Pilih Strategi";

            if (playerSelectedCardImage != null)
                playerSelectedCardImage.gameObject.SetActive(false);

            if (guardSelectedCardImage != null)
                guardSelectedCardImage.gameObject.SetActive(false);

            yield return StartCoroutine(RevealChoiceCardsOneByOne());
            isWaitingForChoice = true;
            yield break;
        }
        else if (result == 1)
        {
            // PLAYER MENANG (Guard ganti gambar kaget, bergetar & turun sedikit)
            if (statusTitleText != null)
                statusTitleText.text = "Kamu Menang!";

            if (guardPortraitImage != null && guardLoseSprite != null)
                guardPortraitImage.sprite = guardLoseSprite;

            if (guardPortraitRect != null)
            {
                yield return StartCoroutine(
                    AnimateLoseReaction(guardPortraitRect, guardDefaultAnchoredPos)
                );
            }
            else
            {
                yield return new WaitForSeconds(resultDisplayDuration);
            }

            FinishMinigamePlayerWin();
        }
        else
        {
            // PLAYER KALAH (Player ganti gambar kaget, bergetar & turun sedikit)
            if (statusTitleText != null)
                statusTitleText.text = "Kamu Kalah!";

            if (playerPortraitImage != null && playerLoseSprite != null)
                playerPortraitImage.sprite = playerLoseSprite;

            if (playerPortraitRect != null)
            {
                yield return StartCoroutine(
                    AnimateLoseReaction(playerPortraitRect, playerDefaultAnchoredPos)
                );
            }
            else
            {
                yield return new WaitForSeconds(resultDisplayDuration);
            }

            FinishMinigamePlayerLose();
        }
    }

    /// <summary>
    /// Efek animasi pop-up saat kartu pilihan muncul di samping tulisan VS.
    /// </summary>
    IEnumerator AnimateCardPop(RectTransform cardRect)
    {
        if (cardRect == null)
            yield break;

        Vector3 targetScale = Vector3.one;
        cardRect.localScale = Vector3.one * 0.5f;

        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(0.5f, 1.08f, t);
            cardRect.localScale = Vector3.one * scale;
            yield return null;
        }

        cardRect.localScale = targetScale;
    }

    /// <summary>
    /// Animasi karakter yang kalah: bergetar ke kiri-kanan sambil turun sedikit (sesuai mockup Figma).
    /// </summary>
    IEnumerator AnimateLoseReaction(RectTransform targetRect, Vector2 startPos)
    {
        float elapsed = 0f;
        Vector2 endPos = startPos + new Vector2(0f, -loseDropOffset);

        while (elapsed < resultDisplayDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / 0.35f);

            Vector2 currentBasePos = Vector2.Lerp(startPos, endPos, progress);

            float shakeFade = Mathf.Clamp01(1f - (elapsed / 0.6f));
            float offsetX = Mathf.Sin(elapsed * 55f) * shakeIntensity * shakeFade;

            targetRect.anchoredPosition = currentBasePos + new Vector2(offsetX, 0f);
            yield return null;
        }

        targetRect.anchoredPosition = endPos;
    }

    void FinishMinigamePlayerWin()
    {
        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);

        if (currentPlayer != null)
        {
            PlayerMovement pm = currentPlayer.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.ResumeMovement();
        }

        if (currentGuardPatrol != null)
        {
            currentGuardPatrol.KnockOutGuard();
        }

        if (currentGuardCatch != null)
        {
            currentGuardCatch.OnMinigamePlayerWon();
        }

        Debug.Log("MINIGAME MENANG! GUARD PINGSAN!");
    }

    void FinishMinigamePlayerLose()
    {
        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);

        if (CheckpointManager.Instance != null && currentPlayer != null)
        {
            CheckpointManager.Instance.RespawnPlayer(currentPlayer);
        }

        if (currentPlayer != null)
        {
            PlayerMovement pm = currentPlayer.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.ResumeMovement();
        }

        if (currentGuardPatrol != null)
        {
            currentGuardPatrol.ResetGuard();
        }

        if (currentGuardCatch != null)
        {
            currentGuardCatch.OnMinigamePlayerLost();
        }

        Debug.Log("MINIGAME KALAH! PLAYER KEMBALI KE CHECKPOINT!");
    }

    int EvaluateWinner(RPSChoice player, RPSChoice guard)
    {
        if (player == guard)
            return 0;

        if ((player == RPSChoice.Kertas && guard == RPSChoice.Batu) ||
            (player == RPSChoice.Gunting && guard == RPSChoice.Kertas) ||
            (player == RPSChoice.Batu && guard == RPSChoice.Gunting))
        {
            return 1;
        }

        return -1;
    }

    Sprite GetCardSprite(RPSChoice choice)
    {
        switch (choice)
        {
            case RPSChoice.Kertas:
                return kertasSprite;
            case RPSChoice.Gunting:
                return guntingSprite;
            case RPSChoice.Batu:
                return batuSprite;
            default:
                return kertasSprite;
        }
    }
}