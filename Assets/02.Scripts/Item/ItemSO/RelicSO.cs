using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RelicSO", menuName = "ItemData/Relic")]
public class RelicSO : ItemBaseSO
{
    [Header("유물 정보")]
    [SerializeField] private int _iD;
    [SerializeField] private RELIC_STAT _statType;
    [SerializeField] private int _statValue;
    [SerializeField] private int _groupID;                // 해당 파츠 고유ID
    [SerializeField] private RELIC_DIVISION _divisionType;
    [SerializeField] private string _name;
    [SerializeField] private string _script;
    [SerializeField] private string _iconSprite;
    [SerializeField] private string _iconClickEffect;
    [SerializeField] private string _iconClickSound;

    [Header("유물 강화")]
    [SerializeField] private EnhancementEventChannelSO _onEquipEnhancement;
    [SerializeField] private List<ItemEnhanceData> _enhanceDataList;

    [Header("유물 계열")]
    [SerializeField] private List<Division> _divisionDataList;


    

    public RELIC_STAT StatType => _statType;
    public int GroupID => _groupID;
    public RELIC_DIVISION DivisionType => _divisionType;
    public override string Name => _name;
    public string Description => _script;
    public string IconClickEffect => _iconClickEffect;
    public string IconClickSound => _iconClickSound;
    public EnhancementEventChannelSO OnEquipEnhancement => _onEquipEnhancement;
    public List<Division> DivisionDatas => _divisionDataList;

    private Dictionary<int, ItemEnhanceData> _enhances;


    // 특정 스탯 증가량을 찾아서 반환
    public int GetIncreaseStat() => _statValue;


    // 해당 강화레벨 데이터를 반환
    public ItemEnhanceData GetEnhanceData(int enhanceLevel)
    {
        if (_enhances == null)
            Init();

        if (!_enhances.TryGetValue(enhanceLevel, out var enhanceData))
            Debug.LogError($"{_name}는 강화 <color=yellow>{enhanceLevel}레벨</color>이 없습니다.");

        return enhanceData;
    }

    private void Init()
    {
        _enhances = new Dictionary<int, ItemEnhanceData> { { 0, null } };
        foreach (var enhanceData in _enhanceDataList)
        {
            _enhances.Add(enhanceData.Level, enhanceData);
        }
    }

    // 유물 계역 데이터 클래스
    [Serializable]
    public class Division
    {
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public RELIC_DIVISION Type { get; private set; }
        [field: SerializeField] public Relic_CONDITION _condition { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Script { get; private set; }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_uniqueID != _iD)
            Debug.LogWarning($"{name}의 고유ID와 유물ID가 다릅니다.");
    }

#endif
}


public enum RELIC_STAT
{
    GoldGainRate,   // 골드 획득량 증가
    ItemDropRate,   // 아이템 획득 확률 증가
    ExpGainRate     // 경험치 획득량 증가
}

public enum RELIC_DIVISION
{
    Sparkle,        // 반짝반짝
    Smooth,         // 매끈매끈
    Swipe           // 쓱쓱싹싹
}

public enum Relic_CONDITION
{
    // 값 미지정
}
