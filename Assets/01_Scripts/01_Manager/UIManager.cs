using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("TestUI 오브젝트")]
    [SerializeField] private TextMeshProUGUI promptText;
    public TextMeshProUGUI PromptText => promptText;

    [SerializeField] private Image stressBar;

    public Image StressBar => stressBar;

    [SerializeField] private Image fatigueBar;

    public Image FatigueBar => fatigueBar;


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
