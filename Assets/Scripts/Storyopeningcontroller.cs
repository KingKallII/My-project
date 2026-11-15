using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StoryOpeningController : MonoBehaviour
{
    public enum CharacterPosition
    {
        Kiri = 0,
        Tengah = 1,
        Kanan = 2,
        SesuaiPosisiUI = 3,
        KustomXY = 4
    }

    [System.Serializable]
    public class StorySlide
    {
        [Tooltip("Catatan nama slide di Inspector.")]
        public string slideTitle = "Slide Cerita";

        [Header("1. Asset Background (Latar Belakang)")]
        [Tooltip("Masukkan PNG Background untuk slide ini (contoh: kamar malam / jalanan malam).")]
        public Sprite backgroundSprite;

        [Header("2. Asset Karakter (Terpisah dari Background)")]
        [Tooltip("Masukkan PNG Karakter (contoh: Doni Kece / Doni Kaget). Kosongkan jika di slide ini tidak ada karakter tambahan.")]
        public Sprite characterSprite;

        [Tooltip("Pilih posisi karakter di layar (Kiri, Tengah, atau Kanan).")]
        public CharacterPosition characterPosition = CharacterPosition.Kiri;

        [Tooltip("Jika memilih KustomXY, isi koordinat X dan Y karakter di sini.")]
        public Vector2 customCharacterPos = Vector2.zero;

        [Header("3. Nama Pembicara & Teks Dialog (Muncul 1 per 1)")]
        [Tooltip("Ketik nama siapa yang sedang berbicara di slide ini (contoh: Reporter, Doni, Penculik).")]
        public string speakerName = "Doni";

        [Tooltip("Ketik kalimat dialog untuk slide ini. Jika dikosongkan, Text Box otomatis disembunyikan pada slide ini.")]
        [TextArea(2, 4)]
        public string dialogueSentence = "";

        [Tooltip("Opsional: Diisi hanya jika slide ini memakai gambar Text Box khusus yang berbeda.")]
        public Sprite customTextBoxSprite;

        [Header("4. Sound Effect (SFX)")]
        [Tooltip("Opsional: Masukkan Sound Effect yang berbunyi saat slide ini muncul.")]
        public AudioClip slideSFX;

        [Range(0f, 1f)]
        public float sfxVolume = 1f;

        [Tooltip("Jika dicentang, suara SFX dari slide sebelumnya akan dihentikan saat masuk ke slide ini.")]
        public bool stopPreviousSFX = true;
    }

    [Header("Tujuan Scene Setelah Cerita Selesai")]
    [Tooltip("Nama scene gameplay utama yang akan dimuat setelah slide terakhir selesai.")]
    public string gameplaySceneName = "main";

    [Header("Komponen UI di Scene Story Pembuka")]
    [Tooltip("1. UI Image untuk Background (Latar Belakang penuh).")]
    public Image backgroundImage;

    [Tooltip("2. UI Image untuk Karakter (di atas Background).")]
    public Image characterImage;

    [Tooltip("3. UI Image untuk Text Box Ngomong (kotak dialog di bagian bawah).")]
    public Image textBoxImage;

    [Tooltip("4. Komponen TextMeshPro (TMP_Text) di dalam Text Box untuk teks kalimat yang muncul 1 per 1.")]
    public TMP_Text dialogueText;

    [Tooltip("5. Komponen TextMeshPro (TMP_Text) pada kotak kecil di atas Text Box (di bawah karakter) untuk menampilkan Nama Pembicara (misal: Doni / Reporter).")]
    public TMP_Text speakerNameText;

    [Header("Pengaturan Ukuran Huruf Tetap (Anti Berubah Ukuran)")]
    [Tooltip("Ukuran huruf tetap (Font Size) untuk teks kalimat dialog (misal 36).")]
    public float fixedFontSize = 36f;

    [Tooltip("Ukuran huruf tetap (Font Size) untuk teks Nama Pembicara (misal 28). Jika diisi 0, mengikuti ukuran di Inspector TMP_Text.")]
    public float speakerNameFontSize = 28f;

    [Header("Pengaturan Posisi Karakter (Kiri / Tengah / Kanan)")]
    public float leftCharacterPosX = -520f;
    public float centerCharacterPosX = 0f;
    public float rightCharacterPosX = 520f;
    public float characterPosY = -40f;

    [Header("Pengaturan Efek Mengetik (Typewriter) & Transisi")]
    [Tooltip("Kecepatan munculnya huruf satu per satu dalam detik (misal 0.035).")]
    [Range(0.01f, 0.15f)]
    public float typingSpeed = 0.035f;

    [Tooltip("Opsional: Suara ketikan halus setiap huruf muncul (boleh dikosongkan).")]
    public AudioClip typingBeepSFX;

    [Range(0f, 1f)]
    public float typingBeepVolume = 0.25f;

    [Tooltip("Durasi transisi fade saat gambar Background berganti ke lokasi baru (detik).")]
    [Range(0f, 1f)]
    public float backgroundFadeDuration = 0.3f;

    [Tooltip("Jeda minimum antar klik (detik).")]
    public float clickCooldown = 0.2f;

    [Header("Audio Source")]
    [Tooltip("AudioSource untuk memutar SFX cerita (dibuat otomatis jika kosong).")]
    public AudioSource sfxAudioSource;

    [Header("Daftar Urutan Slide Cerita (Seperti PPT)")]
    public List<StorySlide> storySlides = new List<StorySlide>();

    private int currentSlideIndex = 0;
    private bool isTransitioningSlide = false;
    private bool isTypingText = false;
    private string currentFullSentence = "";
    private float lastClickTime = 0f;

    private RectTransform characterRect;
    private Vector2 defaultCharacterAnchoredPos;
    private Sprite defaultTextBoxSprite;

    private Coroutine slideCoroutine;
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
            }
        }

        if (characterImage != null)
        {
            characterRect = characterImage.GetComponent<RectTransform>();
            if (characterRect != null)
                defaultCharacterAnchoredPos = characterRect.anchoredPosition;
        }

        if (textBoxImage != null)
        {
            defaultTextBoxSprite = textBoxImage.sprite;

            // Cari SpeakerNameText otomatis di dalam TextBox jika belum ditarik ke slot Inspector
            if (speakerNameText == null)
            {
                Transform foundName = textBoxImage.transform.Find("SpeakerNameText");
                if (foundName != null)
                    speakerNameText = foundName.GetComponent<TMP_Text>();
            }
        }

        LockDialogueFontSize();
    }

    void LockDialogueFontSize()
    {
        if (dialogueText != null)
        {
            dialogueText.enableAutoSizing = false;
            if (fixedFontSize > 1f)
            {
                dialogueText.fontSize = fixedFontSize;
            }
        }

        if (speakerNameText != null)
        {
            speakerNameText.enableAutoSizing = false;
            if (speakerNameFontSize > 1f)
            {
                speakerNameText.fontSize = speakerNameFontSize;
            }
        }
    }

    void Start()
    {
        LockDialogueFontSize();

        if (storySlides != null && storySlides.Count > 0)
        {
            currentSlideIndex = 0;
            slideCoroutine = StartCoroutine(ShowSlideRoutine(currentSlideIndex, true));
        }
        else
        {
            Debug.LogWarning("Daftar Story Slides masih kosong! Langsung memuat scene gameplay.");
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    void Update()
    {
        if (isTransitioningSlide)
            return;

        if (Time.unscaledTime - lastClickTime < clickCooldown)
            return;

        bool advancePressed = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            advancePressed = true;
        }

        if (Keyboard.current != null &&
            (Keyboard.current.spaceKey.wasPressedThisFrame ||
             Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.rightArrowKey.wasPressedThisFrame))
        {
            advancePressed = true;
        }

        if (advancePressed)
        {
            lastClickTime = Time.unscaledTime;
            OnUserClickOrAdvance();
        }
    }

    public void OnUserClickOrAdvance()
    {
        if (isTypingText)
        {
            CompleteTypingImmediately();
        }
        else
        {
            NextSlide();
        }
    }

    void CompleteTypingImmediately()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueText != null)
        {
            LockDialogueFontSize();
            dialogueText.text = currentFullSentence;
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        isTypingText = false;
    }

    public void NextSlide()
    {
        if (isTransitioningSlide)
            return;

        currentSlideIndex++;

        if (currentSlideIndex < storySlides.Count)
        {
            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(ShowSlideRoutine(currentSlideIndex, false));
        }
        else
        {
            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(FinishStoryAndLoadGameplay());
        }
    }

    public void SkipStory()
    {
        StopAllCoroutines();
        if (sfxAudioSource != null)
            sfxAudioSource.Stop();

        SceneManager.LoadScene(gameplaySceneName);
    }

    IEnumerator ShowSlideRoutine(int index, bool isFirstSlide)
    {
        isTransitioningSlide = true;
        isTypingText = false;

        StorySlide slide = storySlides[index];

        bool backgroundChanged = isFirstSlide ||
            (backgroundImage != null &&
             slide.backgroundSprite != null &&
             backgroundImage.sprite != slide.backgroundSprite);

        // 1. Jika Background berganti ke gambar lokasi baru, lakukan efek fade halus
        if (!isFirstSlide && backgroundChanged && backgroundFadeDuration > 0.01f && backgroundImage != null)
        {
            if (textBoxImage != null)
                textBoxImage.gameObject.SetActive(false);

            if (characterImage != null)
                characterImage.gameObject.SetActive(false);

            float halfFade = backgroundFadeDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < halfFade)
            {
                elapsed += Time.deltaTime;
                float c = Mathf.Lerp(1f, 0f, elapsed / halfFade);
                backgroundImage.color = new Color(c, c, c, 1f);
                yield return null;
            }
        }

        // 2. Pasang gambar Background
        if (backgroundImage != null && slide.backgroundSprite != null)
        {
            backgroundImage.sprite = slide.backgroundSprite;
        }

        // 3. Terangkan kembali Background
        if (backgroundChanged && backgroundFadeDuration > 0.01f && backgroundImage != null)
        {
            float halfFade = isFirstSlide ? backgroundFadeDuration : backgroundFadeDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < halfFade)
            {
                elapsed += Time.deltaTime;
                float c = Mathf.Lerp(0f, 1f, elapsed / halfFade);
                backgroundImage.color = new Color(c, c, c, 1f);
                yield return null;
            }
        }

        if (backgroundImage != null)
            backgroundImage.color = Color.white;

        // 4. Atur gambar Karakter terpisah beserta posisinya (Kiri / Tengah / Kanan)
        if (characterImage != null)
        {
            if (slide.characterSprite != null)
            {
                characterImage.sprite = slide.characterSprite;
                characterImage.color = Color.white;
                characterImage.gameObject.SetActive(true);
                ApplyCharacterPosition(slide);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }

        // 5. Putar Sound Effect (SFX) untuk slide ini
        if (sfxAudioSource != null)
        {
            if (slide.stopPreviousSFX)
            {
                sfxAudioSource.Stop();
            }

            if (slide.slideSFX != null)
            {
                sfxAudioSource.PlayOneShot(slide.slideSFX, slide.sfxVolume);
            }
        }

        isTransitioningSlide = false;

        // 6. Tampilkan Text Box, Nama Pembicara (Doni / Reporter), & mulai efek ketikan huruf 1 per 1
        bool hasDialogue = !string.IsNullOrEmpty(slide.dialogueSentence);

        if (textBoxImage != null)
        {
            if (hasDialogue)
            {
                if (slide.customTextBoxSprite != null)
                    textBoxImage.sprite = slide.customTextBoxSprite;
                else if (defaultTextBoxSprite != null)
                    textBoxImage.sprite = defaultTextBoxSprite;

                textBoxImage.color = Color.white;
                textBoxImage.gameObject.SetActive(true);
            }
            else
            {
                textBoxImage.gameObject.SetActive(false);
            }
        }

        // Perbarui teks Nama Pembicara pada kotak cokelat di atas Text Box (di bawah karakter)
        if (speakerNameText != null)
        {
            LockDialogueFontSize();
            speakerNameText.gameObject.SetActive(hasDialogue);
            speakerNameText.text = hasDialogue ? slide.speakerName : "";
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(hasDialogue);

            if (hasDialogue)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                typingCoroutine = StartCoroutine(TypewriterTextRoutine(slide.dialogueSentence));
            }
            else
            {
                dialogueText.text = "";
            }
        }
    }

    void ApplyCharacterPosition(StorySlide slide)
    {
        if (characterRect == null)
            return;

        Vector2 pos = new Vector2(leftCharacterPosX, characterPosY);

        switch (slide.characterPosition)
        {
            case CharacterPosition.Kiri:
                pos = new Vector2(leftCharacterPosX, characterPosY);
                break;
            case CharacterPosition.Tengah:
                pos = new Vector2(centerCharacterPosX, characterPosY);
                break;
            case CharacterPosition.Kanan:
                pos = new Vector2(rightCharacterPosX, characterPosY);
                break;
            case CharacterPosition.SesuaiPosisiUI:
                pos = defaultCharacterAnchoredPos;
                break;
            case CharacterPosition.KustomXY:
                pos = slide.customCharacterPos;
                break;
        }

        characterRect.anchoredPosition = pos;
    }

    IEnumerator TypewriterTextRoutine(string sentence)
    {
        isTypingText = true;
        currentFullSentence = sentence;

        LockDialogueFontSize();

        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalChars = dialogueText.textInfo.characterCount;

        for (int i = 1; i <= totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            char currentChar = sentence[Mathf.Clamp(i - 1, 0, sentence.Length - 1)];
            if (typingBeepSFX != null && sfxAudioSource != null && !char.IsWhiteSpace(currentChar))
            {
                sfxAudioSource.PlayOneShot(typingBeepSFX, typingBeepVolume);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTypingText = false;
    }

    IEnumerator FinishStoryAndLoadGameplay()
    {
        isTransitioningSlide = true;

        if (sfxAudioSource != null)
            sfxAudioSource.Stop();

        if (textBoxImage != null)
            textBoxImage.gameObject.SetActive(false);

        if (characterImage != null)
            characterImage.gameObject.SetActive(false);

        if (backgroundImage != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float c = Mathf.Lerp(1f, 0f, elapsed / duration);
                backgroundImage.color = new Color(c, c, c, 1f);
                yield return null;
            }
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}