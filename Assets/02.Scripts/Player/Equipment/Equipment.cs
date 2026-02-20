using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
    [SerializeField] private EquipmentEventChannelSO _onChangeEquipment;

    [field: SerializeField] public PARTS EquipmentPart { get; private set; }
    [field: SerializeField] public Sprite SpriteImage { get; private set; }

    
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
        Weapon, Hat, Cloak, Shoes, Relic
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
