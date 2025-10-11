using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class Pause : MonoBehaviour
{
    public static Pause Instance
    {
        get; private set;
    }
    
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public static bool isGamePaused;

    public static bool allowFriendlyFire;

    public InputActionAsset userInput;

    private InputAction pause;

    public TextMeshProUGUI friendlyFireText;

    public Character player;
    void Start()
    {
        ResumeGame();
        userInput.FindActionMap("GameSystem").Enable();
        pause = userInput.FindAction("Pause");
        gameOverMenu.SetActive(false);
    }

    void Update()
    {
        PauseSystem();
        friendlyFireText.text = allowFriendlyFire.ToString();
        EnemyCharacter.DeleteCorpses();
        if (player.health <= 0)
        {
            EnableGameOver();
        }
    }

    public void ToggleFriendlyFire()
    {
        allowFriendlyFire = !allowFriendlyFire;
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

    public void EnableGameOver()
    {
        gameOverMenu.SetActive(true);
        isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Character.Clearlists();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameOverMenu.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
