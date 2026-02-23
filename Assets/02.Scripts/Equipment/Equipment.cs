using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
    [field: SerializeField] public int EquipmentID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public PARTS EquipmentPart { get; private set; }
    [field: SerializeField] public int MaxQuantity { get; private set; }
    [field: SerializeField] public string Icon { get; private set; }
    [field: SerializeField] public string IconClickEffect { get; private set; }
    [field: SerializeField] public string IconClickSound { get; private set; }


    [field: SerializeField] public Sprite SpriteImage { get; private set; }

    [SerializeField] private EquipmentEventChannelSO _onChangeEquipment;
    
    //
    private Button _button;
    //


    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnChangeEquipment(this));
    }



    public T ApplyEquipment<T>() where T : Equipment
    {
        return GetComponent<T>();
    }

    public void OnChangeEquipment(Equipment equipment) => _onChangeEquipment.OnStartEvent(equipment);




    public enum PARTS
    {
        Weapon, Hat, Cloak, Shoes
    }

    public enum StatType
    {
        AttackPower, CriticalChance, MaxHp, DefensePower, MoveSpeed, EvasionChance
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        var image = GetComponent<Image>();
        if(image == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Image컴포넌트가 없음");
            return;
        }
        if (SpriteImage != null)
            image.sprite = SpriteImage;
    }
#endif
}
