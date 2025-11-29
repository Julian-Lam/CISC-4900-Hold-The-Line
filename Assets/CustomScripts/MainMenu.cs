using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject controlMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        controlMenu.SetActive(false);
    }

    public static void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void OpenControlMenu()
    {
        mainMenu.SetActive(false);
        controlMenu.SetActive(true);
    }

    public void ReturnToMenu()
    {
        mainMenu.SetActive(true);
        controlMenu.SetActive(false);
    }
}
