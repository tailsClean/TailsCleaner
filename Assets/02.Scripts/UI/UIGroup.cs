using UnityEngine;
using UnityEngine.UI;


public class UIGroup : MonoBehaviour
{
    [field: SerializeField] public UI_GROUP Group { get; private set; }

    [Header("나가기 버튼")]
    [SerializeField] private InventoryUIOpenSet _exitButton;


    protected virtual void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        if(_exitButton.PushButton != null)
            _exitButton.PushButton.onClick.AddListener(() => ItemManager.Instance.OpenUI(_exitButton.OpenGroup));
    }
}
public enum UI_GROUP
{
    //StageClear, StageOver, 
    EquipmentPanel, RelicPanel, ReinforceResourcePanel, SpendablePanel,
    Inventory, EquipEnhanceUI, RelicEnhanceUI, EquipCraftingUI, SellingUI
}