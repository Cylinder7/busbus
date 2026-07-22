using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Documents (Drag from Hierarchy)")]
    [Tooltip("Drag your Main Menu UI Document GameObject here")]
    public UIDocument mainMenuDoc;
    [Tooltip("Drag your Garage UI Document GameObject here")]
    public UIDocument garageMenuDoc;

    [Header("Level to Load")]
    public string raceSceneName = "Track1";

    private VisualElement mainRoot;
    private VisualElement garageRoot;

    // --- GAME DATA ---
    private int upgradePoints = 5;
    private int topSpeed = 65;       
    private int handling = 2;        
    private float boostTime = 1.5f;  
    private int turning = 2;         

    // --- GARAGE UI ELEMENTS ---
    private Label pointsLabel;
    private Label speedValueLabel;
    private Label handlingValueLabel;
    private Label boostValueLabel;
    private Label turningValueLabel; 

    private void Start()
    {
        // 1. Safety Check! Ensure both documents are assigned in the Inspector
        if (mainMenuDoc == null || garageMenuDoc == null)
        {
            Debug.LogError("❌ UI ERROR: Missing UI Documents! Click your MainMenuController object in Unity and drag both UI Documents into the empty slots.");
            return;
        }

        // 2. Grab the specific root containers from each document
        mainRoot = mainMenuDoc.rootVisualElement.Q<VisualElement>("main-container");
        garageRoot = garageMenuDoc.rootVisualElement.Q<VisualElement>("garage-container");

        if (mainRoot == null || garageRoot == null)
        {
            Debug.LogError("❌ UI ERROR: Couldn't find 'main-container' or 'garage-container'. Make sure your .uxml files have those names assigned!");
            return;
        }

        // --- MAIN MENU SETUP ---
        Button startBtn = mainRoot.Q<Button>("StartButton");
        Button garageBtn = mainRoot.Q<Button>("GarageButton");
        Button quitBtn = mainRoot.Q<Button>("QuitButton");

        if (startBtn != null) startBtn.clicked += StartRace;
        if (garageBtn != null) garageBtn.clicked += OpenGarage;
        if (quitBtn != null) quitBtn.clicked += QuitGame;

        // --- GARAGE MENU SETUP ---
        Button backBtn = garageRoot.Q<Button>("BackButton");
        if (backBtn != null) backBtn.clicked += CloseGarage;

        pointsLabel = garageRoot.Q<Label>("PointsLabel");
        speedValueLabel = garageRoot.Q<Label>("SpeedValue");
        handlingValueLabel = garageRoot.Q<Label>("HandlingValue");
        boostValueLabel = garageRoot.Q<Label>("BoostValue");
        turningValueLabel = garageRoot.Q<Label>("TurningValue"); 

        Button upgradeSpeedBtn = garageRoot.Q<Button>("UpgradeSpeed");
        Button upgradeHandlingBtn = garageRoot.Q<Button>("UpgradeHandling");
        Button upgradeBoostBtn = garageRoot.Q<Button>("UpgradeBoost");
        Button upgradeTurningBtn = garageRoot.Q<Button>("UpgradeTurning"); 
        
        if (upgradeSpeedBtn != null) upgradeSpeedBtn.clicked += UpgradeSpeed;
        if (upgradeHandlingBtn != null) upgradeHandlingBtn.clicked += UpgradeHandling;
        if (upgradeBoostBtn != null) upgradeBoostBtn.clicked += UpgradeBoost;
        if (upgradeTurningBtn != null) upgradeTurningBtn.clicked += UpgradeTurning; 
        
        // Initialize starting views: Show Main, Hide Garage
        mainRoot.style.display = DisplayStyle.Flex;
        garageRoot.style.display = DisplayStyle.None;

        UpdateGarageUI();
        Debug.Log("✅ UI successfully linked and ready!");
    }

    // --- START RACE LOGIC ---
    private void StartRace()
    {
        Debug.Log("🏎️ Start Button Clicked! Hiding menu...");
        mainRoot.style.display = DisplayStyle.None;

        if (!string.IsNullOrEmpty(raceSceneName))
        {
            Debug.Log($"Attempting to load 3D Track: {raceSceneName}");
            SceneManager.LoadScene(raceSceneName);
        }
        else
        {
            Debug.Log("No scene name provided. Assuming the track is in this current scene!");
        }
    }

    // --- GARAGE LOGIC ---
    private void UpgradeSpeed()
    {
        if (upgradePoints > 0)
        {
            upgradePoints--;
            topSpeed += 5; 
            UpdateGarageUI();
        }
    }

    private void UpgradeHandling()
    {
        if (upgradePoints > 0 && handling < 10) 
        {
            upgradePoints--;
            handling += 1;
            UpdateGarageUI();
        }
    }

    private void UpgradeBoost()
    {
        if (upgradePoints > 0)
        {
            upgradePoints--;
            boostTime += 0.5f; 
            UpdateGarageUI();
        }
    }

    private void UpgradeTurning()
    {
        if (upgradePoints > 0 && turning < 10) 
        {
            upgradePoints--;
            turning += 1;
            UpdateGarageUI();
        }
    }

    private void UpdateGarageUI()
    {
        if (pointsLabel == null) return; 

        pointsLabel.text = $"UPGRADE POINTS: {upgradePoints}";
        pointsLabel.style.color = upgradePoints > 0 ? new StyleColor(new Color(0.18f, 0.8f, 0.44f)) : new StyleColor(Color.red);

        speedValueLabel.text = $"{topSpeed} MPH";
        handlingValueLabel.text = $"{handling} / 10";
        boostValueLabel.text = $"{boostTime:F1}s"; 
        if (turningValueLabel != null) turningValueLabel.text = $"{turning} / 10";
    }

    // --- NAVIGATION LOGIC ---
    private void OpenGarage()
    {
        mainRoot.style.display = DisplayStyle.None;
        garageRoot.style.display = DisplayStyle.Flex;
        UpdateGarageUI();
    }

    private void CloseGarage()
    {
        garageRoot.style.display = DisplayStyle.None;
        mainRoot.style.display = DisplayStyle.Flex;
    }

    private void QuitGame()
    {
        Debug.Log("Quit Button Clicked!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}