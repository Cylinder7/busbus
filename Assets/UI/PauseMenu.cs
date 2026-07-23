using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument pauseMenuDoc;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // Change this to your actual main menu scene name

    private VisualElement pauseRoot;
    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuDoc == null)
        {
            pauseMenuDoc = GetComponent<UIDocument>();
        }

        if (pauseMenuDoc == null)
        {
            Debug.LogError("❌ UI ERROR: No UIDocument found for the Pause Menu!");
            return;
        }

        pauseRoot = pauseMenuDoc.rootVisualElement.Q<VisualElement>("pause-container");

        if (pauseRoot == null)
        {
            Debug.LogError("❌ UI ERROR: Could not find 'pause-container'.");
            return;
        }

        // Setup Buttons
        Button resumeBtn = pauseRoot.Q<Button>("ResumeButton");
        Button mainMenuBtn = pauseRoot.Q<Button>("MainMenuButton");

        if (resumeBtn != null) resumeBtn.clicked += ResumeGame;
        if (mainMenuBtn != null) mainMenuBtn.clicked += QuitToMainMenu;

        // Hide the pause menu at the start of the game
        pauseRoot.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        // Listen for the Escape key on the keyboard
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseRoot.style.display = DisplayStyle.Flex; // Show UI
        Time.timeScale = 0f; // FREEZE TIME!
        Debug.Log("Game Paused!");
    }

    private void ResumeGame()
    {
        isPaused = false;
        pauseRoot.style.display = DisplayStyle.None; // Hide UI
        Time.timeScale = 1f; // UNFREEZE TIME!
        Debug.Log("Game Resumed!");
    }

    private void QuitToMainMenu()
    {
        // CRITICAL: You MUST unfreeze time before loading a new scene, 
        // otherwise the Main Menu will be permanently frozen!
        Time.timeScale = 1f; 
        
        Debug.Log("Quitting to Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}