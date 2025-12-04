using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NoiseSfxEntry
{
    public string sfxId;       // CSV의 SFXID (예: S_001)
    public AudioClip clip;     // 실제 사운드
    [Range(0f, 1f)]
    public float baseVolume = 1f;
}