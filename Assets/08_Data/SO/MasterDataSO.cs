using UnityEngine;

namespace SleepGame.Data
{
    [CreateAssetMenu(fileName = "MasterGameData", menuName = "GameData/MasterGameData")]
    public class MasterGameDataSO : ScriptableObject
    {
        [Header("Core Tables")]
        public NeighborTableSO neighborTable;
        public DistractionTableSO distractionTable;

        // È®Àå¿ë
        [Header("Optional / Later")]
        public ScriptableObject entityTable;
        public ScriptableObject placeTable;
        public ScriptableObject dayConfigTable;
    }
}
