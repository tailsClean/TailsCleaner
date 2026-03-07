using UnityEngine;


public abstract class PlayerEnhancementSO : ItemBaseSO
{
    [Header("장비/유물 기본 데이터")]
    [SerializeField] private int _groupID;                // 해당 파츠 고유ID
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private string _iconSprite;
    [SerializeField] private string _iconClickEffect;
    [SerializeField] private string _iconClickSound;
    [SerializeField] protected EnhancementEventChannelSO _onEquipEnhancement;

    
    [Header("강화 데이터")]
    [SerializeField] private int _enhanceID;
    [SerializeField] private int _enhanceGroupID;
    [SerializeField] private int _enhanceLevel;
    [SerializeField] private bool _isEnhanceMaxLevel;
    [SerializeField] private float _enhanceAddValue;
    [SerializeField] private int _enhanceCostGold;
    [SerializeField] private int _enhanceCostBluePrint;
    [SerializeField] private int _enhanceBluePrintID;

    // 장비/유물 데이터
    public int GroupID => _groupID;
    public string Name => _name;
    public string Description => _description;
    public string IconClickEffect => _iconClickEffect;
    public string IconClickSound => _iconClickSound;

    // 강화 데이터
    public int EnhanceID => _enhanceID;
    public int EnhanceGroupID => _enhanceGroupID;
    public int EnhanceLevel => _enhanceLevel;
    public bool IsEnhanceMaxLevel => _isEnhanceMaxLevel;
    public float EnhanceAddValue => _enhanceAddValue;
    public int EnhanceCostGold => _enhanceCostGold;
    public int EnhanceCostBluePrint => _enhanceCostBluePrint;
    public int EnhanceBluePrintID => _enhanceBluePrintID;

}