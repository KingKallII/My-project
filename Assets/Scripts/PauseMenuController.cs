using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Pengaturan Nama Scene")]
    [Tooltip("Nama scene Main Menu yang dimuat saat memilih tombol MAIN MENU.")]
    public string mainMenuSceneName = "Main menu";

    [Header("Panel Utama Pause (Muncul Saat Tekan ESC)")]
    [Tooltip("Panel latar belakang gelap keseluruhan saat game di-pause (PauseOverlayPanel).")]
    public GameObject pauseRootPanel;

    [Tooltip("Kotak menu PAUSED yang berisi 5 tombol (Resume, Restart, Settings, Main Menu, Quit).")]
    public GameObject pauseMenuBox;

    [Tooltip("Opsional: Masukkan panel Hotbar/Inventory agar disembunyikan sementara saat game di-pause.")]
    public GameObject hotbarPanel;

    [Header("5 Tombol di Dalam Kotak PAUSED")]
    public Button resumeButton;
    public Button closePauseXButton;
    public Button restartButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Tooltip("Opsional: Tombol ikon Pause (||) di pojok kiri atas layar gameplay.")]
    public Button topCornerPauseButton;

    [Header("Panel Pop-Up Quit Game? (Peringatan Saat Klik QUIT di Pause)")]
    [Tooltip("Kotak pop-up 'Quit Game? Are you sure you want to quit? Any unsaved progress will be lost'.")]
    public GameObject quitConfirmPanel;

    [Tooltip("Tombol 'Yes' di dalam kotak Quit Game?.")]
    public Button confirmQuitYesButton;

    [Tooltip("Tombol 'No' di dalam kotak Quit Game?.")]
    public Button cancelQuitNoButton;

    [Tooltip("Tombol silang (X) di pojok kanan atas kotak Quit Game?.")]
    public Button closeQuitXButton;

    [Header("Panel Pop-Up Settings di Dalam Pause (Opsional)")]
    [Tooltip("Kotak pop-up SETTINGS (Music Volume & SFX Volume) di dalam menu Pause.")]
    public GameObject settingsPanel;
    public Slider musicVolumeSlider;
    public TMP_Text musicPercentageText;
    public Slider sfxVolumeSlider;
    public TMP_Text sfxPercentageText;
    public Button closeSettingsXButton;

    [Header("Audio & SFX Klik Tombol")]
    [Tooltip("Masukkan file suara klik tombol (Button Click SFX).")]
    public AudioClip buttonClickSFX;

    [Tooltip("Opsional: AudioSource musik latar di dalam scene gameplay.")]
    public AudioSource gameplayMusicAudioSource;

    [Tooltip("Opsional: AudioSource untuk memutar SFX klik tombol (dibuat otomatis jika kosong).")]
    public AudioSource sfxAudioSource;

    private bool isPaused = false;
    private float currentMusicVolume = 1f;
    private float currentSFXVolume = 0.5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Pastikan waktu berjalan normal saat scene gameplay dimulai
        Time.timeScale = 1f;

        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
                sfxAudioSource.loop = false;
            }
        }

        // Ambil pengaturan volume yang sudah disimpan dari Main Menu
        currentMusicVolume = PlayerPrefs.GetFloat(MainMenuController.PREF_MUSIC_VOL, 1f);
        currentSFXVolume = PlayerPrefs.GetFloat(MainMenuController.PREF_SFX_VOL, 0.5f);

        ApplyVolumes();

        // Sembunyikan seluruh panel Pause, Settings, dan Quit saat permainan baru dimulai
        if (pauseRootPanel != null)
            pauseRootPanel.SetActive(false);

        if (pauseMenuBox != null && pauseMenuBox != pauseRootPanel)
            pauseMenuBox.SetActive(false);

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Start()
    {
        SetupButtonListeners();
        SetupVolumeSliders();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Tekan tombol ESC untuk membuka / menutup Pause Menu
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Jangan buka Pause jika komputer labirin CCTV sedang terbuka (karena ESC dipakai menutup komputer)
            if (CCTVMazeMinigameManager.Instance != null &&
                CCTVMazeMinigameManager.Instance.mazeMinigamePanel != null &&
                CCTVMazeMinigameManager.Instance.mazeMinigamePanel.activeSelf)
            {
                return;
            }

            OnEscapePressed();
        }
    }

    void OnEscapePressed()
    {
        if (!isPaused)
        {
            PauseGame();
        }
        else
        {
            // Jika sedang di dalam sub-panel Quit Confirm atau Settings, tekan ESC kembali ke kotak PAUSED dulu
            if (quitConfirmPanel != null && quitConfirmPanel.activeSelf)
            {
                CloseQuitConfirm();
            }
            else if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    void SetupButtonListeners()
    {
        BindButton(resumeButton, ResumeGame);
        BindButton(closePauseXButton, ResumeGame);
        BindButton(restartButton, RestartGame);
        BindButton(settingsButton, OpenSettings);
        BindButton(mainMenuButton, ReturnToMainMenu);
        BindButton(quitButton, OpenQuitConfirm);
        BindButton(topCornerPauseButton, TogglePause);

        BindButton(confirmQuitYesButton, ConfirmQuitGame);
        BindButton(cancelQuitNoButton, CloseQuitConfirm);
        BindButton(closeQuitXButton, CloseQuitConfirm);

        BindButton(closeSettingsXButton, CloseSettings);
    }

    void BindButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null)
            return;

        btn.onClick.RemoveListener(action);
        btn.onClick.RemoveListener(PlayClickSFX);
        btn.onClick.AddListener(PlayClickSFX);
        btn.onClick.AddListener(action);
    }

    void SetupVolumeSliders()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            musicVolumeSlider.value = musicVolumeSlider.maxValue > 1.5f
                ? currentMusicVolume * 100f
                : currentMusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
            sfxVolumeSlider.value = sfxVolumeSlider.maxValue > 1.5f
                ? currentSFXVolume * 100f
                : currentSFXVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        UpdateVolumePercentageTexts();
    }

    public void PlayClickSFX()
    {
        if (sfxAudioSource != null && buttonClickSFX != null)
        {
            sfxAudioSource.PlayOneShot(buttonClickSFX, currentSFXVolume);
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    /// <summary>
    /// Menjeda permainan (Time.timeScale = 0) dan memunculkan kotak menu PAUSED.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (hotbarPanel != null)
            hotbarPanel.SetActive(false);

        if (pauseRootPanel != null)
        {
            pauseRootPanel.SetActive(true);
            pauseRootPanel.transform.SetAsLastSibling();
        }

        if (pauseMenuBox != null)
            pauseMenuBox.SetActive(true);

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("GAME PAUSED (ESC)");
    }

    /// <summary>
    /// Tombol RESUME / X: Melanjutkan permainan kembali.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuBox != null)
            pauseMenuBox.SetActive(false);

        if (pauseRootPanel != null)
            pauseRootPanel.SetActive(false);

        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);

        Debug.Log("GAME RESUMED");
    }

    /// <summary>
    /// Tombol RESTART: Mengulang kembali scene gameplay dari awal.
    /// </summary>
    public void RestartGame()
    {
        StartCoroutine(LoadSceneRealtime(SceneManager.GetActiveScene().name));
    }

    /// <summary>
    /// Tombol SETTINGS: Membuka kotak pengaturan volume di dalam Pause Menu.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel belum dihubungkan di PauseMenuController!");
            return;
        }

        if (pauseMenuBox != null && pauseMenuBox != pauseRootPanel && !settingsPanel.transform.IsChildOf(pauseMenuBox.transform))
            pauseMenuBox.SetActive(false);

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuBox != null)
            pauseMenuBox.SetActive(true);
    }

    /// <summary>
    /// Tombol MAIN MENU: Kembali ke scene Main Menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadSceneRealtime(mainMenuSceneName));
    }

    /// <summary>
    /// Tombol QUIT di dalam kotak PAUSED:
    /// Membuka pop-up peringatan "Quit Game? Are you sure you want to quit? Any unsaved progress will be lost".
    /// </summary>
    public void OpenQuitConfirm()
    {
        if (quitConfirmPanel != null)
        {
            if (pauseMenuBox != null && pauseMenuBox != pauseRootPanel && !quitConfirmPanel.transform.IsChildOf(pauseMenuBox.transform))
                pauseMenuBox.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            quitConfirmPanel.SetActive(true);
            quitConfirmPanel.transform.SetAsLastSibling();
        }
        else
        {
            ConfirmQuitGame();
        }
    }

    /// <summary>
    /// Tombol "No" atau "X" pada pop-up Quit Game?: Batal keluar dan kembali ke kotak PAUSED.
    /// </summary>
    public void CloseQuitConfirm()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (pauseMenuBox != null)
            pauseMenuBox.SetActive(true);
    }

    /// <summary>
    /// Tombol "Yes" pada pop-up Quit Game?: Benar-benar keluar dari game.
    /// </summary>
    public void ConfirmQuitGame()
    {
        StartCoroutine(QuitGameRealtime());
    }

    public void OnMusicSliderChanged(float rawValue)
    {
        currentMusicVolume = (musicVolumeSlider != null && musicVolumeSlider.maxValue > 1.5f)
            ? Mathf.Clamp01(rawValue / 100f)
            : Mathf.Clamp01(rawValue);

        PlayerPrefs.SetFloat(MainMenuController.PREF_MUSIC_VOL, currentMusicVolume);
        PlayerPrefs.Save();

        ApplyVolumes();
        UpdateVolumePercentageTexts();
    }

    public void OnSFXSliderChanged(float rawValue)
    {
        currentSFXVolume = (sfxVolumeSlider != null && sfxVolumeSlider.maxValue > 1.5f)
            ? Mathf.Clamp01(rawValue / 100f)
            : Mathf.Clamp01(rawValue);

        PlayerPrefs.SetFloat(MainMenuController.PREF_SFX_VOL, currentSFXVolume);
        PlayerPrefs.Save();

        ApplyVolumes();
        UpdateVolumePercentageTexts();
    }

    void ApplyVolumes()
    {
        if (gameplayMusicAudioSource != null)
            gameplayMusicAudioSource.volume = currentMusicVolume;

        if (sfxAudioSource != null)
            sfxAudioSource.volume = currentSFXVolume;
    }

    void UpdateVolumePercentageTexts()
    {
        if (musicPercentageText != null)
            musicPercentageText.text = Mathf.RoundToInt(currentMusicVolume * 100f) + "%";

        if (sfxPercentageText != null)
            sfxPercentageText.text = Mathf.RoundToInt(currentSFXVolume * 100f) + "%";
    }

    IEnumerator LoadSceneRealtime(string targetScene)
    {
        Time.timeScale = 1f;
        isPaused = false;

        yield return new WaitForSecondsRealtime(0.15f);
        SceneManager.LoadScene(targetScene);
    }

    IEnumerator QuitGameRealtime()
    {
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.15f);

        Debug.Log("Keluar dari Game melalui Pause Menu!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}