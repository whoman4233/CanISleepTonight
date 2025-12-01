using System;
using UnityEngine;

public class PlayerCondition : MonoBehaviour
{
    private bool isLife = true;

    //Condition fatigue { get { return uICondition.health; } }
    //Condition stress { get { return uICondition.hunger; } }

    [SerializeField] private float stress;
    [SerializeField] private float fatigue;
    [SerializeField] private float maxValue;

    private void Start()
    {
        // 시작 시 UI값 초기 반영
        UpdateStressUI();
        UpdateFatigueUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddStress(10);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            AddFatigue(30);
    }

    public void AddStress(float value)
    {
        stress = Mathf.Clamp(stress + value, 0, 100);
        UpdateStressUI();
    }

    public void AddFatigue(float value)
    {
        fatigue = Mathf.Clamp(fatigue + value, 0, 100);
        UpdateFatigueUI();
    }

    private void UpdateStressUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StressBar.fillAmount = stress / maxValue;
        }

        Debug.Log($"스트레스 : {stress} / {maxValue}");
    }

    private void UpdateFatigueUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FatigueBar.fillAmount = fatigue / maxValue;
        }

        Debug.Log($"피로 : {fatigue} / {maxValue}");
    }
}
