using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Level to Load")]
    public string raceSceneName = "Track1";

    private VisualElement mainRoot;
    private VisualElement garageRoot;

    // --- GAME DATA ---
    private int upgradePoints = 5;
    private int topSpeed = 65;       
    private float handling = 2f;        
    private float boostTime = 1.5f;  

    // --- NEW: COSMETIC DATA ---
    private string[] paintColors = { "City Yellow", "Crimson Red", "Neon Blue", "Toxic Green", "Midnight Black" };
    private int currentPaintIndex = 0;

    // --- GARAGE UI ELEMENTS ---
    private Label pointsLabel;
    private Label speedValueLabel;
    private Label handlingValueLabel;
    private Label boostValueLabel;
    private Label paintValueLabel;

    private void Start()
    {
        // 1. Find all UIDocuments in the scene dynamically
        UIDocument[] allUIDocuments = FindObjectsByType<UIDocument>();

        if (allUIDocuments.Length == 0)
        {
            Debug.LogError("❌ UI ERROR: No UIDocument found in the scene! Did you add one to the Hierarchy?");
            return;
        }

        // 2. Loop through them and identify which is which based on the root element
        foreach (var doc in allUIDocuments)
        {
            var root = doc.rootVisualElement;

            if (root.Q<VisualElement>("main-container") != null)
            {
                mainRoot = root.Q<VisualElement>("main-container");
            }
            if (root.Q<VisualElement>("garage-container") != null)
            {
                garageRoot = root.Q<VisualElement>("garage-container");
            }
        }

        if (mainRoot == null || garageRoot == null)
        {
            Debug.LogError("❌ UI ERROR: Found UIDocuments, but couldn't find your containers! Check if your MainMenu or GarageMenu UI Documents are active.");
            return;
        }

        // --- MAIN MENU SETUP ---
        Button startBtn = mainRoot.Q<Button>("StartButton");
        Button garageBtn = mainRoot.Q<Button>("GarageButton");
        Button quitBtn = mainRoot.Q<Button>("QuitButton");

        if (startBtn != null) startBtn.clicked += () => SceneManager.LoadScene(raceSceneName);
        if (garageBtn != null) garageBtn.clicked += OpenGarage;
        if (quitBtn != null) quitBtn.clicked += QuitGame;

        // --- GARAGE MENU SETUP ---
        Button backBtn = garageRoot.Q<Button>("BackButton");
        if (backBtn != null) backBtn.clicked += CloseGarage;

        // Find Stat Labels
        pointsLabel = garageRoot.Q<Label>("PointsLabel");
        speedValueLabel = garageRoot.Q<Label>("SpeedValue");
        handlingValueLabel = garageRoot.Q<Label>("HandlingValue");
        boostValueLabel = garageRoot.Q<Label>("BoostValue");
        paintValueLabel = garageRoot.Q<Label>("PaintValue"); // NEW

        // Find Upgrade Buttons & Attach Logic
        Button upgradeSpeedBtn = garageRoot.Q<Button>("UpgradeSpeed");
        Button upgradeHandlingBtn = garageRoot.Q<Button>("UpgradeHandling");
        Button upgradeBoostBtn = garageRoot.Q<Button>("UpgradeBoost");
        
        // Find New Paint Buttons & Attach Logic
        Button prevPaintBtn = garageRoot.Q<Button>("PrevPaint");
        Button nextPaintBtn = garageRoot.Q<Button>("NextPaint");

        if (upgradeSpeedBtn != null) upgradeSpeedBtn.clicked += UpgradeSpeed;
        if (upgradeHandlingBtn != null) upgradeHandlingBtn.clicked += UpgradeHandling;
        if (upgradeBoostBtn != null) upgradeBoostBtn.clicked += UpgradeBoost;
        
        if (prevPaintBtn != null) prevPaintBtn.clicked += PreviousPaint;
        if (nextPaintBtn != null) nextPaintBtn.clicked += NextPaint;

        // Initialize starting views
        mainRoot.style.display = DisplayStyle.Flex;
        garageRoot.style.display = DisplayStyle.None;

        UpdateGarageUI();

        Debug.Log("✅ Main Menu & Garage perfectly linked and ready!");
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
            handling += 0.5f;
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

    // --- NEW: PAINT LOGIC ---
    
    private void NextPaint()
    {
        currentPaintIndex++;
        
        // If we go past the end of the array, loop back to 0
        if (currentPaintIndex >= paintColors.Length)
        {
            currentPaintIndex = 0;
        }
        UpdateGarageUI();
    }

    private void PreviousPaint()
    {
        currentPaintIndex--;
        
        // If we go below 0, loop to the very last color in the array
        if (currentPaintIndex < 0)
        {
            currentPaintIndex = paintColors.Length - 1;
        }
        UpdateGarageUI();
    }

    private void UpdateGarageUI()
    {
        if (pointsLabel == null) return; 

        pointsLabel.text = $"UPGRADE POINTS: {upgradePoints}";
        pointsLabel.style.color = upgradePoints > 0 ? new StyleColor(new Color(0.18f, 0.8f, 0.44f)) : new StyleColor(Color.red);

        speedValueLabel.text = $"{topSpeed} MPH";
        handlingValueLabel.text = $"{handling:F1} / 10";
        boostValueLabel.text = $"{boostTime:F1}s"; 
        
        // Update the paint text based on the current index
        if (paintValueLabel != null)
        {
            paintValueLabel.text = paintColors[currentPaintIndex];
        }
    }

    // --- NAVIGATION LOGIC ---

    private void OpenGarage()
    {
        if (mainRoot != null) mainRoot.style.display = DisplayStyle.None;
        if (garageRoot != null) garageRoot.style.display = DisplayStyle.Flex;
        UpdateGarageUI();
    }

    private void CloseGarage()
    {
        if (garageRoot != null) garageRoot.style.display = DisplayStyle.None;
        if (mainRoot != null) mainRoot.style.display = DisplayStyle.Flex;
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}