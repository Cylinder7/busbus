using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit
using UnityEngine.SceneManagement; // Required for loading different game levels

public class MainMenuController : MonoBehaviour
{
    [Header("Level to Load")]
    [Tooltip("Type the exact name of your racing scene here (e.g., 'Track1')")]
    public string raceSceneName = "Track1";

    // Variables to hold references to our UI elements
    private UIDocument uiDocument;
    private Button startButton;
    private Button garageButton;
    private Button quitButton;

    private void Start()
    {
        // First, try to find it on the same GameObject
        uiDocument = GetComponent<UIDocument>();

        // If it's not on this object, search the entire scene for it! (Super helpful!)
        if (uiDocument == null)
        {
            uiDocument = FindAnyObjectByType<UIDocument>();
        }

        // If it is STILL missing, the object literally doesn't exist in the hierarchy
        if (uiDocument == null)
        {
            Debug.LogError("❌ UI ERROR: There is no UIDocument in your scene at all! Right-click the Hierarchy > UI Toolkit > UI Document.");
            return;
        }

        // Check if the visual tree actually loaded before searching it
        var root = uiDocument.rootVisualElement;
        
        // Let's add a super-defensive check to see if the UXML is actually loaded
        if (root == null || root.childCount == 0)
        {
            Debug.LogError("❌ UI ERROR: The UI is empty! Make sure your UXML file is saved in UI Builder, AND assigned to the 'Source Asset' slot in the Inspector!");
            return;
        }

        // Search the UI layout to find the buttons by the names you gave them in UI Builder
        startButton = root.Q<Button>("StartButton");
        garageButton = root.Q<Button>("GarageButton");
        quitButton = root.Q<Button>("QuitButton");

        // Warn us if a button was typed incorrectly in UI Builder
        if (startButton == null) Debug.LogWarning("⚠️ StartButton not found! Check the name in UI Builder.");
        if (garageButton == null) Debug.LogWarning("⚠️ GarageButton not found! Check the name in UI Builder.");
        if (quitButton == null) Debug.LogWarning("⚠️ QuitButton not found! Check the name in UI Builder.");

        // Attach functions to the "clicked" events of each button
        if (startButton != null) startButton.clicked += StartGame;
        if (garageButton != null) garageButton.clicked += OpenGarage;
        if (quitButton != null) quitButton.clicked += QuitGame;

        // Print a success message so we know the script reached the very end!
        Debug.Log("✅ UI successfully linked and ready! If you can't see it, check your PanelSettings!");
    }

    private void OnDisable()
    {
        // It is good practice to unregister events when the menu is turned off
        if (startButton != null) startButton.clicked -= StartGame;
        if (garageButton != null) garageButton.clicked -= OpenGarage;
        if (quitButton != null) quitButton.clicked -= QuitGame;
    }

    private void StartGame()
    {
        Debug.Log("Start Button Clicked! Loading Race Scene...");
        
        // This will now actually try to load the scene name typed in the Inspector!
        SceneManager.LoadScene(raceSceneName);
    }

    private void OpenGarage()
    {
        // For now, just print to the console so we know it works
        Debug.Log("Garage Button Clicked! Opening Garage Menu...");
    }

    private void QuitGame()
    {
        Debug.Log("Quit Button Clicked! Exiting Game...");
        
        // This quits a built application. 
        Application.Quit();

        // This magically stops Play Mode while you are testing inside the Unity Editor!
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}