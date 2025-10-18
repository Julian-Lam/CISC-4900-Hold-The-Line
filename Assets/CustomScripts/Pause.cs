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
    public GameObject inventoryMenu;
    public GameObject buyStationMenu;

    public static bool isAnInterfaceActive;

    public static bool isInventoryOpen;

    public static bool allowFriendlyFire;

    public InputActionAsset userInput;

    private InputAction pause;
    private InputAction inventory;

    public TextMeshProUGUI friendlyFireText;

    public Character player;
    void Start()
    {
        ResumeGame();
        userInput.FindActionMap("GameSystem").Enable();
        pause = userInput.FindAction("Pause");
        inventory = userInput.FindAction("Inventory");
        gameOverMenu.SetActive(false);
        inventoryMenu.SetActive(false);
    }

    void Update()
    {
        PauseSystem();
        InventorySystem();
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
        isAnInterfaceActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        inventoryMenu.SetActive(false);
        buyStationMenu.SetActive(false);
        Time.timeScale = 1;
        isAnInterfaceActive = false;
        isInventoryOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMenu()
    {
        Character.Clearlists();
        SceneManager.LoadSceneAsync(0);
        isAnInterfaceActive = false;
    }

    public void PauseSystem()
    {
        if (pause.WasPressedThisFrame())
        {
            if (!isAnInterfaceActive)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void InventorySystem()
    {
        if (inventory.WasPressedThisFrame())
        {
            if (!isAnInterfaceActive && !isInventoryOpen)
            {
                Time.timeScale = 0;
                inventoryMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isAnInterfaceActive = true;
                isInventoryOpen = true;
            }
            else if (isInventoryOpen)
            {
                CloseInventory();
            }
        }
    }

    public void CloseInventory()
    {
        inventoryMenu.SetActive(false);
        ResumeGame();
    }
    public void EnableGameOver()
    {
        gameOverMenu.SetActive(true);
        isAnInterfaceActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Character.Clearlists();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameOverMenu.SetActive(false);
        Time.timeScale = 1;
        isAnInterfaceActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
