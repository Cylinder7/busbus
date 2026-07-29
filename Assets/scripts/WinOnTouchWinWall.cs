using UnityEngine;
using TMPro;

public class WinOnTouchWinWall : MonoBehaviour
{
    public Transform wall;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textComponent;

    void Awake()
    {
        // Note: Make sure textComponent is assigned in the Inspector 
        // or attached to THIS same GameObject if using GetComponent.
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        HideUI();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Bus"))
        {
            ShowUI();
        }
    }

    // NEW METHOD: Call this from other scripts to set the text
    public void SetWinText(string newText)
    {
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }

    public void ShowUI()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void HideUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}