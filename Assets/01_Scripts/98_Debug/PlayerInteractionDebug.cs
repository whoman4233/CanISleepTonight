using UnityEngine;

public class PlayerInteractionDebug : MonoBehaviour
{
    public float rayDistance = 2f;
    public LayerMask interactionMask;
    public NeighborManager neighborManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))  // 디버그용 Dead
        {
            TryKillTarget();
        }
    }

    void TryKillTarget()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, interactionMask))
        {
            var anchor = hit.collider.GetComponent<DistractionAnchor>();
            if (anchor != null && anchor.Runtime.isAlive)
            {
                neighborManager.KillDistraction(anchor.Runtime.Id);
                Debug.Log($"Distraction Dead: {anchor.Runtime.Id}");
            }
        }
    }
}
