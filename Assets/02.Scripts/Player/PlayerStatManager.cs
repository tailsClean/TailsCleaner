using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance;

    // 플레이어 베이스에 주입할 클래스
    private OutGameLevelSystem _playerOutLevel;
    private PlayerStatTransfer _statTransfer;
    private PlayerLoadout _playerLoadout;

    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeLoadout;

    public PlayerLoadout Loadout => _playerLoadout;

    // 필요할 때마다 읽어서 갱신이 되어야 함
    public PlayerStatTransfer StatTransfer => GetStatTransfer();



    private void Awake()
    {
        #region 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        transform.SetParent(null);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        _statTransfer = new PlayerStatTransfer();
    }

    private void Start()
    {
        _playerLoadout = new PlayerLoadout(_onChangeLoadout);
    }



    public PlayerStatTransfer GetStatTransfer()
    {
        if(_playerOutLevel == null)
            _playerOutLevel = OutGameLevelSystem.Instance;


        _statTransfer.SetBaseStat(_playerOutLevel.CurrentLevel);
        SetLoadout();

        return _statTransfer;
    }


    #region 내부 메서드

    private void SetLoadout()
    {
        _statTransfer.SetLoadoutStat(_playerLoadout);
    }

    #endregion
}


public enum PLAYER_STAT
{
    MaxHp,
    AttackPower,
    DefensePower,
    CriticalChance,
    CriticalDamageMultiplier,
    CriticalResistance,
    EvasionChance,
    MoveSpeed,
    AttackSpeed,
    HealthRegen,
    PickupRange,
    GoldGainRate,
    ItemDropRate,
    EquipmentDropRate,
    ExpGainRate,
    None
}