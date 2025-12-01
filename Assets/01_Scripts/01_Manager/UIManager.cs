using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("TestUI 오브젝트")]
    [SerializeField] private TextMeshProUGUI promptText;
    public TextMeshProUGUI PromptText => promptText;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }
}
