using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerLoadoutUI : UIGroup
{
    [Header("장비일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutSlots;

    [Header("유물일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutRelicSlots;
    [SerializeField] private TextMeshProUGUI _relicDivisionSlot;
    [SerializeField] private TextMeshProUGUI _relicDivisionDesc;

    [Header("팝업 출력을 위한 버튼")]
    [SerializeField] private List<Button> _popUpButtons;
    [SerializeField] private ItemPopupBase _popUp;

    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeLoadout;


    private PlayerLoadout _loadout;



    private void OnEnable()
    {
        if (_loadout != null)
        {
            UpdateLoadoutSlot();
            UpdateRelicLoadoutSlot();
            SetPopupRelicButtons();
        }
        _onChangeLoadout?.AddListener(UpdateRelicLoadoutSlot);
        _onChangeLoadout?.AddListener(SetPopupRelicButtons);
    }

    private void OnDisable()
    {
        _onChangeLoadout?.RemoveListener(UpdateRelicLoadoutSlot);
        _onChangeLoadout?.RemoveListener(SetPopupRelicButtons);
    }


    protected override void Start()
    {
        base.Start();
        _loadout = PlayerStatManager.Instance.Loadout;

        UpdateLoadoutSlot();

        UpdateRelicLoadoutSlot();
        SetPopupRelicButtons();


        SetPopupButtons();
    }


    // 장비 로드아웃UI슬롯 갱신
    private void UpdateLoadoutSlot()
    {
        if (_loadoutSlots != null && _loadoutSlots.Count > 0)
        {
            int i = 0;
            foreach (var equip in _loadout.MyEquipments.Values)
            {
                if (equip.EnhanceLevel == 0)
                    _loadoutSlots[i++].SetSlot(equip.Data.Equipmnet.id);
                else
                    _loadoutSlots[i++].SetSlot(equip.Data.Equipmnet.id, $"+{equip.EnhanceLevel}");
            }
        }
    }

    // 유물 로드아웃UI슬롯 갱신
    private void UpdateRelicLoadoutSlot()
    {
        if (_loadoutRelicSlots != null && _loadoutRelicSlots.Count > 0)
        {
            int i = 0;
            foreach (var relic in _loadout.MyRelics)
            {
                if (relic.CurrentEnhanceLevel == 0)
                    _loadoutRelicSlots[i++].SetSlot(relic.Data.Relic.id);
                else
                    _loadoutRelicSlots[i++].SetSlot(relic.Data.Relic.id, $"+{relic.CurrentEnhanceLevel}");
            }
            for (; i < _loadoutRelicSlots.Count; i++)
            {
                _loadoutRelicSlots[i].Init();
            }
        }


        UpdateRelicDivision();
    }

    // 유물 공명 효과 출력
    private void UpdateRelicDivision()
    {

        if (_relicDivisionSlot == null)
            return;

        if (_loadout.TryGetRelicDivision(out var divisionType))
        {
            var divisionData = DataManager.Instance.GetSOData<RelicDivisionSO>();

            float value = 0;
            switch(divisionType)
            {
                case RELIC_TYPE.Twinkle:
                    value = divisionData.GetById(RelicDivision.TWINKLE).value;
                    _relicDivisionSlot.text = "반짝반짝 공명";
                    _relicDivisionDesc.text = $"골드 획득량 증가 + {value}";
                    break;

                case RELIC_TYPE.Smooth:
                    value = divisionData.GetById(RelicDivision.SMOOTH).value;
                    _relicDivisionSlot.text = "매끈매끈 공명";
                    _relicDivisionDesc.text = $"장비획득량 증가 + {value}";
                    break;
                case RELIC_TYPE.Swish:

                    value = divisionData.GetById(RelicDivision.SWISH).value;
                    _relicDivisionSlot.text = "쓱싹쓱싹 공명";
                    _relicDivisionDesc.text = $"경험치 획득량 증가 + {value}";
                    break;

                case RELIC_TYPE.Floating:
                    value = divisionData.GetById(RelicDivision.FLOATING).value;
                    _relicDivisionSlot.text = "둥실둥실 공명";
                    _relicDivisionDesc.text = $"아이템 획득 범위 증가 + {value}";
                    break;
            }


        }
        else
            _relicDivisionSlot.text = string.Empty;
    }


    // 팝업 여는 기능 버튼에 부여
    private void SetPopupButtons()
    {
        int i = 0;
        if (_popUpButtons.Count == 4)
        {

            foreach (var equip in _loadout.MyEquipments.Values)
            {
                _popUpButtons[i].onClick.AddListener(() => _popUp.gameObject.SetActive(true));
                _popUpButtons[i++].onClick.AddListener(() => _popUp.SetSlot(new ItemInstance(equip.Data.UniqueID, equip.EnhanceLevel, equip.CurrentGrade)));
            }
        }
    }


    // 플레이어 유물 로드아웃UI
    private void SetPopupRelicButtons()
    {
        int i = 0;
        if (_popUpButtons.Count == 3)
        {
            foreach (var relic in _loadout.MyRelics)
            {
                _popUpButtons[i].onClick.RemoveAllListeners();
                _popUpButtons[i].onClick.AddListener(() => _popUp.gameObject.SetActive(true));
                _popUpButtons[i++].onClick.AddListener(() => _popUp.SetSlot(new ItemInstance(relic.Data.UniqueID, relic.CurrentEnhanceLevel, GRADE.None)));
            }
            for (; i < _popUpButtons.Count; i++)
            {
                _popUpButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}
