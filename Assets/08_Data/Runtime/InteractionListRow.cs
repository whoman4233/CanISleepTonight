using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InteractionRow
{
    public string interactionId;          // I_N001_01
    [TextArea]
    public string interactionText;        // 버튼에 찍힐 텍스트

    public InteractionCategory category;  // 대화/설득/수리
    public InteractionReqType reqType;    // None / Item / State / Stat
    public string reqValue;               // calm, Item_Hammer, stress 등

    public InteractionResultType resultType;  // ChangeType, ShowText, ...
    public string target;                     // N_001, D_N001_A, E_003, System, stress, Time ...
    public string resultValueRaw;             // Idle / Dead / T_N001_01 / -10 / End_B01 ...
}

[Serializable]
public class InteractionListRow
{
    public string listId;                 // IL_N001_A
    public string targetId;               // N_001 / E_003 ...
    public int priority;                  // 숫자 클수록 먼저 검사
    public string conditionKey;           // None or Var_XXX
    public string conditionValue;         // None or TRUE/FALSE 등

    public List<string> linkedInteractionIds = new(); // I_N001_01, I_N001_02 ...
}
