using UnityEngine;
using TMPro;

public class WinOnTouchWinWall : MonoBehaviour
{
    public Transform wall;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textComponent;

    void Awake()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }
            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInParent<CanvasGroup>();
            }
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus"))
        {
            ShowUI();
        }
    }

    public void SetWinText(string newText)
    {
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
        else
        {
            Debug.LogWarning("WinOnTouchWinWall: No TextMeshProUGUI found. Make sure the text is on this object or one of its children.", this);
        }
    }

    public void ShowUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (textComponent != null)
        {
            var textColor = textComponent.color;
            textColor.a = 1f;
            textComponent.color = textColor;
        }
    }

    public void HideUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (textComponent != null)
        {
            var textColor = textComponent.color;
            textColor.a = 0f;
            textComponent.color = textColor;
        }
    }
}