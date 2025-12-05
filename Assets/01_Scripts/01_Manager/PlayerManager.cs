using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("PlayerManager").AddComponent<PlayerManager>();
            }
            return _instance;
        }
    }

    public Player Player
    {
        get { return _player; }
        set { _player = value; }
    }
    private Player _player;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }


    // 플레이어 스폰 설정
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(25f, 10f, -19f);    // 303호
    [SerializeField] private Vector3 defaultSpawnRotation = new Vector3(0, 0, 0);


    // 플레이어를 303호 시작 위치로 리셋
    // GameManager의 Action 페이즈 시작 시 호출됨
    public void ResetToHomePosition()
    {
        if (_player == null)
        {
            Debug.LogWarning("[PlayerManager] Player가 할당되지 않았습니다!");
            return;
        }

        Transform playerTransform = _player.transform;
        
        // 스폰 포인트 기본 좌표로
        playerTransform.position = defaultSpawnPosition;
        playerTransform.rotation = Quaternion.Euler(defaultSpawnRotation);

        // PlayerLocationTracker 업데이트
        var locationTracker = FindObjectOfType<PlayerLocationTracker>();
        if (locationTracker != null)
        {
            locationTracker.SetFloor(3);
            locationTracker.EnterHouse("303");
        }
    }
}