using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Pengaturan Urutan Scene")]
    [Tooltip("Nama scene alur cerita pembuka yang dibuka saat klik New Game.")]
    [SerializeField] private string storySceneName = "story pembuka";

    [Tooltip("Nama scene gameplay utama.")]
    [SerializeField] private string gameSceneName = "main";

    [Header("Musik Latar (BGM) & Sound Effect (SFX) Main Menu")]
    [Tooltip("Masukkan file lagu/musik latar Main Menu di sini (otomatis looping).")]
    public AudioClip mainMenuMusicClip;

    [Tooltip("Masukkan file suara klik tombol (Button Click SFX) di sini.")]
    public AudioClip buttonClickSFX;

    [Tooltip("Opsional: AudioSource untuk musik latar (jika kosong, akan dibuat otomatis).")]
    public AudioSource musicAudioSource;

    [Tooltip("Opsional: AudioSource untuk suara klik tombol / SFX (jika kosong, akan dibuat otomatis).")]
    public AudioSource sfxAudioSource;

    [Header("Panel Pop-Up Settings (Pengaturan Volume)")]
    [Tooltip("Masukkan objek Panel Settings (kotak pop-up SETTINGS).")]
    public GameObject settingsPanel;

    [Tooltip("Slider untuk mengatur Music Volume.")]
    public Slider musicVolumeSlider;

    [Tooltip("Teks persentase di sebelah kanan Slider Music (contoh: 100%).")]
    public TMP_Text musicPercentageText;

    [Tooltip("Slider untuk mengatur SFX Volume.")]
    public Slider sfxVolumeSlider;

    [Tooltip("Teks persentase di sebelah kanan Slider SFX (contoh: 50%).")]
    public TMP_Text sfxPercentageText;

    [Tooltip("Opsional: Tombol silang (X) untuk menutup Panel Settings.")]
    public Button closeSettingsButton;

    [Header("Panel Pop-Up Quit Game? (Konfirmasi Keluar)")]
    [Tooltip("Masukkan objek Panel Quit (kotak pop-up 'Quit Game? Are you sure...').")]
    public GameObject quitConfirmPanel;

    [Tooltip("Opsional: Tombol 'Yes' di dalam Panel Quit untuk keluar dari game.")]
    public Button confirmQuitYesButton;

    [Tooltip("Opsional: Tombol 'No' di dalam Panel Quit untuk batal keluar.")]
    public Button cancelQuitNoButton;

    [Tooltip("Opsional: Tombol silang (X) di pojok kanan atas Panel Quit untuk batal keluar.")]
    public Button closeQuitXButton;

    public const string PREF_MUSIC_VOL = "MidnightCaptive_MusicVol";
    public const string PREF_SFX_VOL = "MidnightCaptive_SFXVol";

    private float currentMusicVolume = 1f;
    private float currentSFXVolume = 0.5f;
    private bool isLoadingScene = false;

    void Awake()
    {
        SetupAudioSources();

        // Ambil nilai volume yang tersimpan (Default: Music = 100% [1.0], SFX = 50% [0.5] sesuai desain Figma)
        currentMusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f);
        currentSFXVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.5f);

        ApplyVolumesToAudioSources();

        // Sembunyikan kedua panel pop-up saat baru masuk ke Main Menu
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }

    void Start()
    {
        SetupSlidersAndButtons();
        RegisterClickSoundToAllButtons();

        // Putar musik latar Main Menu secara otomatis
        if (musicAudioSource != null && mainMenuMusicClip != null)
        {
            musicAudioSource.clip = mainMenuMusicClip;
            musicAudioSource.loop = true;
            musicAudioSource.volume = currentMusicVolume;
            if (!musicAudioSource.isPlaying)
                musicAudioSource.Play();
        }
    }

    void SetupAudioSources()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();

        if (musicAudioSource == null)
        {
            if (existingSources.Length > 0)
                musicAudioSource = existingSources[0];
            else
                musicAudioSource = gameObject.AddComponent<AudioSource>();

            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;
        }

        if (sfxAudioSource == null)
        {
            if (existingSources.Length > 1 && existingSources[1] != musicAudioSource)
                sfxAudioSource = existingSources[1];
            else
                sfxAudioSource = gameObject.AddComponent<AudioSource>();

            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }
    }

    void SetupSlidersAndButtons()
    {
        // 1. Atur Slider Music Volume
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

            // Mendukung slider dengan rentang 0..1 maupun 0..100
            if (musicVolumeSlider.maxValue > 1.5f)
                musicVolumeSlider.value = currentMusicVolume * 100f;
            else
                musicVolumeSlider.value = currentMusicVolume;

            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        // 2. Atur Slider SFX Volume
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);

            if (sfxVolumeSlider.maxValue > 1.5f)
                sfxVolumeSlider.value = currentSFXVolume * 100f;
            else
                sfxVolumeSlider.value = currentSFXVolume;

            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        UpdateVolumePercentageTexts();

        // 3. Hubungkan tombol-tombol di dalam Panel Settings dan Panel Quit secara otomatis
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (confirmQuitYesButton != null)
        {
            confirmQuitYesButton.onClick.RemoveListener(ConfirmQuitGame);
            confirmQuitYesButton.onClick.AddListener(ConfirmQuitGame);
        }

        if (cancelQuitNoButton != null)
        {
            cancelQuitNoButton.onClick.RemoveListener(CloseQuitPanel);
            cancelQuitNoButton.onClick.AddListener(CloseQuitPanel);
        }

        if (closeQuitXButton != null)
        {
            closeQuitXButton.onClick.RemoveListener(CloseQuitPanel);
            closeQuitXButton.onClick.AddListener(CloseQuitPanel);
        }
    }

    /// <summary>
    /// Otomatis memasang suara klik (buttonClickSFX) ke seluruh tombol (Button) yang ada di Main Menu Canvas.
    /// </summary>
    void RegisterClickSoundToAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button btn = allButtons[i];
            if (btn != null)
            {
                btn.onClick.RemoveListener(PlayButtonClickSFX);
                btn.onClick.AddListener(PlayButtonClickSFX);
            }
        }
    }

    public void PlayButtonClickSFX()
    {
        if (sfxAudioSource != null && buttonClickSFX != null)
        {
            sfxAudioSource.PlayOneShot(buttonClickSFX, currentSFXVolume);
        }
    }

    public void OnMusicSliderChanged(float rawValue)
    {
        if (musicVolumeSlider != null && musicVolumeSlider.maxValue > 1.5f)
            currentMusicVolume = Mathf.Clamp01(rawValue / 100f);
        else
            currentMusicVolume = Mathf.Clamp01(rawValue);

        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, currentMusicVolume);
        PlayerPrefs.Save();

        ApplyVolumesToAudioSources();
        UpdateVolumePercentageTexts();
    }

    public void OnSFXSliderChanged(float rawValue)
    {
        if (sfxVolumeSlider != null && sfxVolumeSlider.maxValue > 1.5f)
            currentSFXVolume = Mathf.Clamp01(rawValue / 100f);
        else
            currentSFXVolume = Mathf.Clamp01(rawValue);

        PlayerPrefs.SetFloat(PREF_SFX_VOL, currentSFXVolume);
        PlayerPrefs.Save();

        ApplyVolumesToAudioSources();
        UpdateVolumePercentageTexts();
    }

    void ApplyVolumesToAudioSources()
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = currentMusicVolume;

        if (sfxAudioSource != null)
            sfxAudioSource.volume = currentSFXVolume;
    }

    void UpdateVolumePercentageTexts()
    {
        if (musicPercentageText != null)
        {
            int pct = Mathf.RoundToInt(currentMusicVolume * 100f);
            musicPercentageText.text = pct + "%";
        }

        if (sfxPercentageText != null)
        {
            int pct = Mathf.RoundToInt(currentSFXVolume * 100f);
            sfxPercentageText.text = pct + "%";
        }
    }

    // =========================================================================
    // FUNGSI TOMBOL MAIN MENU (NEW GAME, CONTINUE, SETTINGS, QUIT/EXIT)
    // =========================================================================

    public void NewGame()
    {
        if (isLoadingScene)
            return;

        StartCoroutine(LoadSceneAfterClickSound(
            !string.IsNullOrEmpty(storySceneName) ? storySceneName : gameSceneName
        ));
    }

    public void ContinueGame()
    {
        Debug.Log("Tombol Continue ditekan.");
    }

    /// <summary>
    /// Dipanggil saat tombol "Settings" di Main Menu diklik: memunculkan Panel Pop-Up Settings.
    /// </summary>
    public void OpenSettings()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Settings Panel belum dihubungkan di Inspector MainMenuController!");
        }
    }

    /// <summary>
    /// Dipanggil saat tombol silang (X) di Panel Settings diklik.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Dipanggil saat tombol "Exit" / "Quit" di Main Menu diklik:
    /// Membuka pop-up konfirmasi "Quit Game? Are you sure you want to quit?".
    /// </summary>
    public void QuitGame()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
            quitConfirmPanel.transform.SetAsLastSibling();
        }
        else
        {
            // Fallback jika panel konfirmasi belum dipasang: langsung keluar
            ConfirmQuitGame();
        }
    }

    /// <summary>
    /// Dipanggil saat tombol "No" atau tombol "X" pada pop-up Quit Game diklik.
    /// </summary>
    public void CloseQuitPanel()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }

    /// <summary>
    /// Dipanggil saat tombol "Yes" pada pop-up Quit Game diklik: benar-benar menutup aplikasi game.
    /// </summary>
    public void ConfirmQuitGame()
    {
        StartCoroutine(QuitAfterClickSound());
    }

    IEnumerator LoadSceneAfterClickSound(string targetScene)
    {
        isLoadingScene = true;
        PlayButtonClickSFX();

        // Beri jeda singkat 0.15 detik agar suara klik tombol terdengar sebelum pindah scene
        yield return new WaitForSecondsRealtime(0.15f);

        SceneManager.LoadScene(targetScene);
    }

    IEnumerator QuitAfterClickSound()
    {
        PlayButtonClickSFX();
        yield return new WaitForSecondsRealtime(0.15f);

        Debug.Log("Keluar dari Game (Quit Confirmed)!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}