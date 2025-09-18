using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isGamePaused;

    public InputActionAsset userInput;

    private InputAction pause;

    void Start()
    {
        ResumeGame();
        userInput.FindActionMap("GameSystem").Enable();
        pause = userInput.FindAction("Pause");
    }

    void Update()
    {
        PauseSystem();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMenu()
    {
        SceneManager.LoadSceneAsync(0);
        isGamePaused = false;
    }

    public void PauseSystem()
    {
        if (pause.WasPressedThisFrame())
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}
