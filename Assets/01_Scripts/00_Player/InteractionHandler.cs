using TMPro;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    [Header("상호작용 세팅")]
    [SerializeField] private float checkRate = 0.1f;
    [SerializeField] private float interactRange = 1f;
    [SerializeField] private LayerMask interactableMask;

    private TextMeshProUGUI promptText;

    private float lastCheckTime;
    private Camera mainCamera;

    private IInteractable curInteractable;
    private GameObject curInteractObject;

    private InteractableOutliner curOutliner;

    private void Start()
    {
        mainCamera = Camera.main;

        promptText = UIManager.Instance.PromptText;
        promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;
            TryInteraction();
        }
    }

    private void TryInteraction()
    {
        // 카메라(화면) 중앙에서 ray 발사
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, checkRate);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            // 새로운 오브젝트를 감지했을 때
            if (curInteractObject != hit.collider.gameObject)
            {
                ClearInteraction();

                curInteractObject = hit.collider.gameObject;
                curInteractable = curInteractObject.GetComponent<IInteractable>();

                // 아웃라인 컨트롤러 찾기
                curOutliner = curInteractObject.GetComponent<InteractableOutliner>();
                if (curOutliner != null)
                {
                    curOutliner.SetHighlight(true);
                }

                if (curInteractable != null)
                    SetPromptText(curInteractable.GetInteractionPrompt());
                else
                    Debug.Log("상호작용 가능 오브젝트 없음!");
            }
        }
        else
        {
            ClearInteraction();
        }
    }

    private void SetPromptText(string text)
    {
        if (promptText == null)
        {
            Debug.LogWarning("promptText UI 가 없습니다!");
            return;
        }
        
        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    private void ClearInteraction()
    {
        if (curOutliner != null)
        {
            curOutliner.SetHighlight(false);
            curOutliner = null;
        }
        curInteractObject = null;
        curInteractable = null;

        promptText.text = "";
        promptText.gameObject.SetActive(false);
    }

    public void Interact()
    {
        if (curInteractable == null) return;

        curInteractable.OnInteract();
        ClearInteraction();
    }
}
