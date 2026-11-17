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

    public const string PREF_MUSIC_VOL = "MidnightCaptive_MusicVol";
    public const string PREF_SFX_VOL = "MidnightCaptive_SFXVol";

    private float currentMusicVolume = 1f;
    private float currentSFXVolume = 0.5f;
    private bool isLoadingScene = false;

    void Awake()
    {
        // Pastikan waktu game berjalan normal saat berada di Main Menu
        Time.timeScale = 1f;

        SetupAudioSources();

        currentMusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f);
        currentSFXVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.5f);

        ApplyVolumesToAudioSources();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Start()
    {
        SetupSlidersAndButtons();
        RegisterClickSoundToAllButtons();

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
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

            if (musicVolumeSlider.maxValue > 1.5f)
                musicVolumeSlider.value = currentMusicVolume * 100f;
            else
                musicVolumeSlider.value = currentMusicVolume;

            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

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

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }
    }

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
    // FUNGSI TOMBOL MAIN MENU (NEW GAME, CONTINUE, SETTINGS, EXIT)
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

    public void OpenSettings()
    {
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

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Di Main Menu, tombol Exit langsung menutup aplikasi (karena pop-up peringatan Quit
    /// kini dipindahkan ke menu Pause saat permainan sudah dimulai).
    /// </summary>
    public void QuitGame()
    {
        StartCoroutine(QuitDirectlyAfterClickSound());
    }

    IEnumerator LoadSceneAfterClickSound(string targetScene)
    {
        isLoadingScene = true;
        PlayButtonClickSFX();

        yield return new WaitForSecondsRealtime(0.15f);

        SceneManager.LoadScene(targetScene);
    }

    IEnumerator QuitDirectlyAfterClickSound()
    {
        PlayButtonClickSFX();
        yield return new WaitForSecondsRealtime(0.15f);

        Debug.Log("Game ditutup dari Main Menu.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}