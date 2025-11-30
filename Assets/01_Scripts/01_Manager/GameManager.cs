using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    bool isOpened = false;
    public bool IsSettingOpen => isOpened;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

}