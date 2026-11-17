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

    [Header("Pengaturan Animasi Kalah & Layar Fade 'KAMU TERTANGKAP!'")]
    [Tooltip("Masukkan sprite PNG 'FadePanelTertangkap' (latar hitam bertuliskan KAMU TERTANGKAP!). Jika kosong, otomatis mengambil dari CCTVAlarmEffect.")]
    public Sprite caughtScreenSprite;

    [Tooltip("Durasi transisi fade-in menuju layar hitam 'KAMU TERTANGKAP!' saat kalah minigame (detik).")]
    public float loseFadeInDuration = 0.4f;

    [Tooltip("Durasi tampilnya layar hitam 'KAMU TERTANGKAP!' sebelum layar kembali terang di Checkpoint (detik).")]
    public float loseBlackHoldDuration = 1.5f;

    [Tooltip("Durasi transisi fade-out dari layar hitam kembali ke permainan setelah respawn di Checkpoint (detik).")]
    public float loseFadeOutDuration = 0.45f;

    [Tooltip("Seberapa jauh gambar yang kalah turun ke bawah (piksel UI) sesuai desain Figma.")]
    public float loseDropOffset = 35f;

    [Tooltip("Kekuatan getaran (shake) saat karakter kaget/kalah.")]
    public float shakeIntensity = 12f;

    [Tooltip("Durasi animasi kaget sebelum masuk ke layar fade hitam (detik).")]
    public float resultDisplayDuration = 1.5f;

    private GameObject currentPlayer;
    private GuardCatch currentGuardCatch;
    private GuardPatrol currentGuardPatrol;

    private Image minigamePanelImage;
    private Sprite defaultMinigameBgSprite;
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

        // Jika slot caughtScreenSprite belum diisi manual, ambil otomatis dari CCTVAlarmEffect (jika ada)
        if (caughtScreenSprite == null && CCTVAlarmEffect.Instance != null && CCTVAlarmEffect.Instance.caughtScreenSprite != null)
        {
            caughtScreenSprite = CCTVAlarmEffect.Instance.caughtScreenSprite;
        }

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
        {
            minigamePanelImage = minigamePanel.GetComponent<Image>();
            if (minigamePanelImage != null)
                defaultMinigameBgSprite = minigamePanelImage.sprite;
        }

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

    public void StartMinigame(GameObject playerObj, GuardCatch guardCatch, GuardPatrol guardPatrol)
    {
        SaveDefaultTransforms();

        // Sinkronkan caughtScreenSprite dari CCTVAlarmEffect jika masih kosong
        if (caughtScreenSprite == null && CCTVAlarmEffect.Instance != null && CCTVAlarmEffect.Instance.caughtScreenSprite != null)
        {
            caughtScreenSprite = CCTVAlarmEffect.Instance.caughtScreenSprite;
        }

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

    IEnumerator PlayIntroAndSelectionSequence()
    {
        isWaitingForChoice = false;

        // Pastikan gambar background kembali menggunakan sprite background VS asli
        if (minigamePanelImage != null && defaultMinigameBgSprite != null)
            minigamePanelImage.sprite = defaultMinigameBgSprite;

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

        if (playerPortraitImage != null && playerNormalSprite != null)
            playerPortraitImage.sprite = playerNormalSprite;

        if (guardPortraitImage != null && guardNormalSprite != null)
            guardPortraitImage.sprite = guardNormalSprite;

        // 1. Layar hitam sekitar 1 detik
        if (minigamePanelImage != null)
            minigamePanelImage.color = Color.black;

        yield return new WaitForSeconds(blackScreenDuration);

        // 2. Munculkan background VS terlebih dahulu
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

        yield return new WaitForSeconds(0.15f);

        // 3. Munculkan Player (dari kiri) & Guard (dari kanan)
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

        // 4. Munculkan pilihan kartu satu per satu (Kertas -> Gunting -> Batu)
        yield return StartCoroutine(RevealChoiceCardsOneByOne());

        isWaitingForChoice = true;
    }

    IEnumerator RevealChoiceCardsOneByOne()
    {
        if (choiceButtonsContainer != null)
            choiceButtonsContainer.SetActive(true);

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

        if (btnKertas != null)
        {
            btnKertas.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnKertas.transform, kertasDefaultScale));
            yield return new WaitForSeconds(cardStaggerDelay);
        }

        if (btnGunting != null)
        {
            btnGunting.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnGunting.transform, guntingDefaultScale));
            yield return new WaitForSeconds(cardStaggerDelay);
        }

        if (btnBatu != null)
        {
            btnBatu.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateSingleButtonPopIn(btnBatu.transform, batuDefaultScale));
        }
    }

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
                float subT = t / 0.7f;
                targetTransform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, subT);
            }
            else
            {
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

        RPSChoice guardChoice = (RPSChoice)Random.Range(0, 3);

        if (activeSequenceCoroutine != null)
            StopCoroutine(activeSequenceCoroutine);

        activeSequenceCoroutine = StartCoroutine(PlayRoundSequence(playerChoice, guardChoice));
    }

    IEnumerator PlayRoundSequence(RPSChoice playerChoice, RPSChoice guardChoice)
    {
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

        yield return new WaitForSeconds(0.4f);

        int result = EvaluateWinner(playerChoice, guardChoice);

        if (result == 0)
        {
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
            // PLAYER MENANG
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
            // PLAYER KALAH -> Animasi kaget lalu masuk ke layar Fade Hitam "KAMU TERTANGKAP!" seperti CCTV!
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

            yield return StartCoroutine(FinishMinigamePlayerLoseWithFadeRoutine());
        }
    }

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

    /// <summary>
    /// Saat Player KALAH Kertas-Gunting-Batu:
    /// 1. Layar minigame melakukan fade ke hitam.
    /// 2. Menampilkan gambar hitam bertuliskan "KAMU TERTANGKAP!" (sama seperti saat tertangkap CCTV).
    /// 3. Memindahkan Player ke Checkpoint & me-reset Guard di balik layar hitam.
    /// 4. Melakukan fade-out halus kembali ke permainan di Checkpoint.
    /// </summary>
    IEnumerator FinishMinigamePlayerLoseWithFadeRoutine()
    {
        // 1. Fade layar minigame menjadi gelap (hitam)
        if (minigamePanelImage != null)
        {
            float elapsed = 0f;
            while (elapsed < loseFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / loseFadeInDuration);
                minigamePanelImage.color = Color.Lerp(Color.white, Color.black, t);
                yield return null;
            }
            minigamePanelImage.color = Color.black;
        }

        // Sembunyikan karakter, kartu adu, dan teks "Pilih Strategi"
        if (playerPortraitImage != null)
            playerPortraitImage.gameObject.SetActive(false);

        if (guardPortraitImage != null)
            guardPortraitImage.gameObject.SetActive(false);

        if (playerSelectedCardImage != null)
            playerSelectedCardImage.gameObject.SetActive(false);

        if (guardSelectedCardImage != null)
            guardSelectedCardImage.gameObject.SetActive(false);

        if (statusTitleText != null)
            statusTitleText.gameObject.SetActive(false);

        // 2. Tampilkan gambar "KAMU TERTANGKAP!" (caughtScreenSprite) sama seperti saat tertangkap CCTV
        if (minigamePanelImage != null && caughtScreenSprite != null)
        {
            minigamePanelImage.sprite = caughtScreenSprite;
            minigamePanelImage.color = Color.white;
        }

        // 3. Pindahkan Player ke Checkpoint & reset Guard ke Titik A selama layar hitam "KAMU TERTANGKAP!" tampil
        if (CheckpointManager.Instance != null && currentPlayer != null)
        {
            CheckpointManager.Instance.RespawnPlayer(currentPlayer);
        }

        if (currentGuardPatrol != null)
        {
            currentGuardPatrol.ResetGuard();
        }

        // Tahan tampilan layar hitam "KAMU TERTANGKAP!" selama loseBlackHoldDuration (default 1.5 detik)
        yield return new WaitForSeconds(loseBlackHoldDuration);

        // 4. Fade-out halus dari layar hitam kembali ke permainan di Checkpoint
        if (minigamePanelImage != null)
        {
            Color startColor = minigamePanelImage.color;
            Color transparentColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            float elapsedOut = 0f;
            while (elapsedOut < loseFadeOutDuration)
            {
                elapsedOut += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedOut / loseFadeOutDuration);
                minigamePanelImage.color = Color.Lerp(startColor, transparentColor, t);
                yield return null;
            }
        }

        // Kembalikan sprite background VS asli untuk pemakaian minigame berikutnya
        if (minigamePanelImage != null)
        {
            if (defaultMinigameBgSprite != null)
                minigamePanelImage.sprite = defaultMinigameBgSprite;

            minigamePanelImage.color = Color.white;
        }

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

        if (currentGuardCatch != null)
        {
            currentGuardCatch.OnMinigamePlayerLost();
        }

        Debug.Log("MINIGAME KALAH! LAYAR FADE TERTANGKAP SELESAI & PLAYER KEMBALI KE CHECKPOINT!");
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