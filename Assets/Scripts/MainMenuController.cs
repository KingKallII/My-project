
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [SerializeField] private string gameSceneName = "main";

    public void NewGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        Debug.Log("Fitur Continue belum dibuat.");
    }

    public void OpenSettings()
    {
        Debug.Log("Fitur Settings belum dibuat.");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game ditutup.");
    }
}
