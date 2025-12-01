using System;
using System.Collections.Generic;
using UnityEngine;

namespace SleepGame.Data
{
    [Serializable]
    public class NeighborDataRow
    {
        public string neighborId; // N_001
        public string displayName; // ±ÙÀ°³ú
        public string layoutId;    // L_01
        [TextArea]
        public string description;
    }

    [CreateAssetMenu(fileName = "NeighborTable", menuName = "GameData/NeighborTable")]
    public class NeighborTableSO : ScriptableObject
    {
        public List<NeighborDataRow> neighbors = new List<NeighborDataRow>();

        public NeighborDataRow GetById(string id)
        {
            return neighbors.Find(n => n.neighborId == id);
        }
    }

    [Serializable]
    public class DistractionDataRow
    {
        public string distractionId; // D_N001_A
        public string ownerId;       // N_001
        public string sourceId;      // N_001 or E_005
        public string tag;           // sound µî
        public int intensity;        // 1~6
        public string sfxId;         // S_001
        public string placeId;       // P_303 µî (¾øÀ¸¸é ºó ¹®ÀÚ¿­ °¡´É)
        [TextArea]
        public string description;
    }

    [CreateAssetMenu(fileName = "DistractionTable", menuName = "GameData/DistractionTable")]
    public class DistractionTableSO : ScriptableObject
    {
        public List<DistractionDataRow> distractions = new List<DistractionDataRow>();

        public DistractionDataRow GetById(string id)
        {
            return distractions.Find(d => d.distractionId == id);
        }
    }
}
