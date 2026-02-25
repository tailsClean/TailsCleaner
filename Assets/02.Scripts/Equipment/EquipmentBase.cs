using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentBase : MonoBehaviour
{


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


    // 임시 스텟
    [field: SerializeField] public int StatID { get; private set; }
    [field: SerializeField] public int StatGroupID { get; private set; }
    [field: SerializeField] public EquipmentIncreaseStat.STAT StatType { get; private set; }
    [field: SerializeField] public int StatValue { get; private set; }

    //public List<EquipmentIncreaseStat> IncreaseStat { get; private set; }


    // 임시 강화
    [field: SerializeField] public int EnhanceID { get; private set; }
    [field: SerializeField] public int EnhanceGroupID { get; private set; }
    [field: SerializeField] public int EnhanceLevel { get; private set; }
    [field: SerializeField] public bool IsEnhanceMaxLevel { get; private set; }
    [field: SerializeField] public float EnhanceAddValue { get; private set; }
    [field: SerializeField] public int EnhanceCostGold { get; private set; }
    [field: SerializeField] public int EnhanceCostBluePrint { get; private set; }
    [field: SerializeField] public int EnhanceBluePrintID { get; private set; }


    // 임시 등급
    [field: SerializeField] public int GradeID { get; private set; }
    [field: SerializeField] public int GradeGroupID { get; private set; }
    [field: SerializeField] public GRADE Grade { get; private set; }
    [field: SerializeField] public bool GradeMaxGrade { get; private set; }
    [field: SerializeField] public int GradeCostID { get; private set; }
    [field: SerializeField] public int GradeCostCount { get; private set; }
    [field: SerializeField] public float GradeStatRate { get; private set; } = 1;   // 스텟 증가량(곱연산)
    [field: SerializeField] public int GradePrice { get; private set; }             // 등급별 판매 가격



    //
    [field: SerializeField] public Sprite SpriteImage { get; private set; }
    private Button _button;
    //


    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnWearEquipment(this));
    }

    // 장비별 스텟 증가량 부여
    private void SetIncreaseStat(int id, int groupID, EquipmentIncreaseStat.STAT type, int value)
    {
        var stat = new EquipmentIncreaseStat(id, groupID, type, value);
    }

    public void OnWearEquipment(EquipmentBase equipment) => _onWearEquipment.OnStartEvent(equipment);

    public int GetIncreaseStat(EquipmentIncreaseStat.STAT stat) => (int)((StatValue + EnhanceAddValue) * GradeStatRate);


    public enum PARTS
    {
        Weapon, Hat, Cloak, Shoes
    }

    public enum GRADE
    {
        Grimy,     // 꼬질
        Fresh,     // 향긋
        Shiny,     // 반짝
        Pristine   // 깔끔
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
