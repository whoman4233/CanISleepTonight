[System.Serializable]
public class DistractionDataRow
{
    public string distractionId;   // D_E003_A
    public string ownerId;         // E_003 or N_xxx
    public string noiseId;         // E_003 등 (필요하면)
    public string tag;             // sound 등
    public int level;              // CSV Level -> intensity 로 사용 가능
    public string sfxId;           // CSV SFXID (S_001)
    public string placeId;
    public string info;            // 설명 텍스트
}
