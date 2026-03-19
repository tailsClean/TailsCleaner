using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerLoadoutUI : UIGroup
{
    [Header("장비일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutSlots;

    [Header("유물일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutRelicSlots;

    [Header("팝업 출력을 위한 버튼")]
    [SerializeField] private List<Button> _popUpButtons;
    [SerializeField] private ItemPopup _popUp;



    
    private PlayerLoadout _loadout;



    private void OnEnable()
    {
        if (_loadout != null)
        {
            UpdateLoadoutSlot();
            UpdateRelicLoadoutSlot();
            SetPopupRelicButtons();
        }

    }


    protected override void Start()
    {
        base.Start();
        _loadout = ItemManager.Instance.Loadout;

            UpdateLoadoutSlot();

            UpdateRelicLoadoutSlot();

        SetPopupButtons();
    }


    private void UpdateLoadoutSlot()
    {
        if (_loadoutSlots != null && _loadoutSlots.Count > 0)
        {
            int i = 0;
            foreach (var equip in _loadout.MyEquipments.Values)
            {
                _loadoutSlots[i++].SetSlot(equip.Data.Equipmnet.id);
            }
        }
    }

    private void UpdateRelicLoadoutSlot()
    {

        if(_loadoutRelicSlots != null && _loadoutRelicSlots.Count > 0)
        {
            int i = 0;
            foreach (var relic in _loadout.MyRelics)
            {
                _loadoutRelicSlots[i++].SetSlot(relic.Data.Relic.id);
            }
            for (; i < _loadoutRelicSlots.Count; i++)
            {
                _loadoutRelicSlots[i].Init();
            }
        }
    }


    private void SetPopupButtons()
    {
        int i = 0;
        if(_popUpButtons.Count == 4)
        {
            
            foreach (var equip in _loadout.MyEquipments.Values)
            {
                _popUpButtons[i].onClick.AddListener(() => _popUp.gameObject.SetActive(true));
                _popUpButtons[i++].onClick.AddListener(() => _popUp.SetSlot(new ItemInstance(equip.Data.UniqueID, equip.EnhanceLevel, equip.Grade)));
            }
        }
    }

    private void SetPopupRelicButtons()
    {
        int i = 0;
        if (_popUpButtons.Count == 3)
        {
            foreach (var relic in _loadout.MyRelics)
            {
                _popUpButtons[i].onClick.RemoveAllListeners();
                _popUpButtons[i].onClick.AddListener(() => _popUp.gameObject.SetActive(true));
                _popUpButtons[i++].onClick.AddListener(() => _popUp.SetSlot(new ItemInstance(relic.Data.UniqueID, relic.EnhanceLevel, GRADE.None)));
            }
            for (; i < _popUpButtons.Count; i++)
            {
                _popUpButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}
