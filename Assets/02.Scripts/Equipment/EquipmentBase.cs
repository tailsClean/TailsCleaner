using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EquipmentIncreaseStat;
using static EquipmentGrade;

public class EquipmentBase : MonoBehaviour
{

    [Header("장비 기본 정보")]
    [field: SerializeField] public int EquipmentID { get; private set; }
    [field: SerializeField] public PARTS EquipmentPart { get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; }        // 최대 소지 수량
    [field: SerializeField] public int GroupID { get; private set; }         // 해당 파츠 고유ID
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public string IconSprite { get; private set; }
    [field: SerializeField] public string IconClickEffect { get; private set; }
    [field: SerializeField] public string IconClickSound { get; private set; }

    [SerializeField] private EquipmentEventChannelSO _onWearEquipment;

    //
    [field: SerializeField] public Sprite SpriteImage { get; private set; }
    private Button _button;
    //


    #region 장비 스텟
    // 임시 스텟
    [Header("장비 스텟")]
    [field: SerializeField] public int StatID { get; private set; }
    [field: SerializeField] public int StatGroupID { get; private set; }
    [field: SerializeField] public STAT StatType { get; private set; }
    [field: SerializeField] public int StatValue { get; private set; }

    public Dictionary<STAT , EquipmentIncreaseStat> IncreaseStat { get; private set; }

    public void AddIncreaseStat(STAT statType) => IncreaseStat.Add(statType, new EquipmentIncreaseStat(this));
    #endregion

    #region 강화 데이터
    // 임시 강화
    [Header("강화 데이터")]
    [field: SerializeField] public int EnhanceID { get; private set; }
    [field: SerializeField] public int EnhanceGroupID { get; private set; }
    [field: SerializeField] public int EnhanceLevel { get; private set; } = 1;
    [field: SerializeField] public bool IsEnhanceMaxLevel { get; private set; }
    [field: SerializeField] public float EnhanceAddValue { get; private set; }
    [field: SerializeField] public int EnhanceCostGold { get; private set; }
    [field: SerializeField] public int EnhanceCostBluePrint { get; private set; }
    [field: SerializeField] public int EnhanceBluePrintID { get; private set; }

    public EquipemtEnhance EquipEnhance { get; private set; }

    public void OnEnhance() => EnhanceLevel++;
    #endregion

    #region 등급 데이터
    // 임시 등급
    [field: SerializeField] public int GradeID { get; private set; }
    [field: SerializeField] public int GradeGroupID { get; private set; }
    [field: SerializeField] public GRADE Grade { get; private set; }
    [field: SerializeField] public bool IsGradeMaxGrade { get; private set; }
    [field: SerializeField] public int GradeCostID { get; private set; }
    [field: SerializeField] public int GradeCostCount { get; private set; }
    [field: SerializeField] public float GradeStatRate { get; private set; } = 1;   // 스텟 증가량(곱연산)
    [field: SerializeField] public int GradePrice { get; private set; }             // 등급별 판매 가격

    public EquipmentGrade EquipGrade { get; private set; }
    #endregion

    public void OnUpgrade()
    {
        Grade++;
        EquipGrade.OnUpgrade();
    }


    private void Awake()
    {
        IncreaseStat = new();
        AddIncreaseStat(StatType);
        EquipEnhance = new EquipemtEnhance(this);
        EquipGrade = new EquipmentGrade(this);
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnWearEquipment(this));
    }

    public void OnWearEquipment(EquipmentBase equipment) => _onWearEquipment.OnStartEvent(equipment);

    public int GetIncreaseStat(STAT stat)
    {
        float statValue = IncreaseStat[stat].Value;
        float enhanceValue = EquipEnhance.AddValue;
        float gradeValue = EquipGrade.StatRate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return (int)result;
    }

    public void Remove() => Destroy(gameObject);


    public enum PARTS
    {
        Weapon, Hat, Cloak, Shoes
    }

    

    #region 에디터 설정
#if UNITY_EDITOR
    private void OnValidate()
    {
        var image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Image컴포넌트가 없음");
            return;
        }
        if (SpriteImage != null)
            image.sprite = SpriteImage;
    }
#endif
    #endregion
}
