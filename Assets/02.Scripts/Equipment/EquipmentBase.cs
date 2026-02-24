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

    [SerializeField] private EquipmentEventChannelSO _onChangeEquipment;


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



    //
    [field: SerializeField] public Sprite SpriteImage { get; private set; }
    private Button _button;
    //


    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnChangeEquipment(this));
    }

    // 장비별 스텟 증가량 부여
    private void SetIncreaseStat(int id, int groupID, EquipmentIncreaseStat.STAT type, int value)
    {
        var stat = new EquipmentIncreaseStat(id, groupID, type, value);
    }

    public void OnChangeEquipment(EquipmentBase equipment) => _onChangeEquipment.OnStartEvent(equipment);

    public int GetIncreaseStat(EquipmentIncreaseStat.STAT stat) => StatValue;


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
