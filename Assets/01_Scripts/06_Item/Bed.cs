using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        float currentNoise = GameManager.Instance.CalculateCurrentNoise();

        if (currentNoise >= 60f)
            return "시끄러워서 잘 수 없다.";
        
        return "잠자기 [E]";
    }

    public void OnInteract()
    {
        GameManager.Instance.OnPlayerStartSleep();
    }
}
