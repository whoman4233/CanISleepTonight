using UnityEngine;

public class TestDoor : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        return "Open";
    }

    public void OnInteract()
    {
        Debug.Log("상호작용 실행");
    }
}
