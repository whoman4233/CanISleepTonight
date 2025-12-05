using UnityEngine;

public class NoiseCheckZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;    // Player 가 Zone 에 들어온 게 아니라면, return

        // 소음 UI 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNoiseUI(true);
        }

        // 현재 소음 업데이트
        UpdateNoiseLevel();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;    // Player 가 Zone 에 있는 게 아니라면, return

        // 소음 UI 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNoiseUI(true);
        }

        UpdateNoiseLevel();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;    // Player 가 Zone 에서 나간 게 아니라면, return

        // 소음 UI 숨김
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNoiseUI(false);
        }
    }

    private void UpdateNoiseLevel()
    {
        // GameManager에서 현재 소음 계산
        if (GameManager.Instance != null)
        {
            float currentNoise = GameManager.Instance.CalculateCurrentNoise();

            // UIManager에 소음 전달
            if (UIManager.Instance != null)
            {
                // TODO : GameManager.Instance.CalculateCurrentNoise() 완성되면 currentNoise 를 인자로 전달하기
                UIManager.Instance.UpdateNoiseLevel(currentNoise);
            }
        }
    }
}
