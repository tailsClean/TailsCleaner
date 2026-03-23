using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerLoadout
{
    private Dictionary<PART, EquipmentBase> _myEquipments;
    private List<RelicBase> _myRelics;
    private List<RelicBase> _outputRelics;                                      // 외부 출력용 리스트

    public Dictionary<PART, EquipmentBase> MyEquipments => _myEquipments;
    public List<RelicBase> MyRelics => _outputRelics;

    private readonly RelicBase _relicZero;
    private readonly int _relicSlotLength = 3;

    private VoidEventChannelSO _onChangeLoadout;

    public PlayerLoadout(VoidEventChannelSO onChangeLoadout)
    {
        _onChangeLoadout = onChangeLoadout;

        _relicZero = new RelicBase();
        _myRelics = new List<RelicBase>();
        _outputRelics = new List<RelicBase>();
        for(int i = 0; i < _relicSlotLength; i++)
        {
            _myRelics.Add(_relicZero);
        }


        _myEquipments = new Dictionary<PART, EquipmentBase>
        {
            {PART.Weapon, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultWeapon)},
            {PART.Helmet, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultHat)},
            {PART.Cloak, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultCloak)},
            {PART.Shoes, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultShose)}
        };
    }

    // 장비의 스텟 증가량을 반환
    public float GetIncreaseStat(EQUIP_STAT_TYPE stat)
    {
        if (_myEquipments == null)
            return 0;

        EquipmentBase equipment = null;
        switch (stat)
        {
            case EQUIP_STAT_TYPE.Attack:
                equipment = _myEquipments[PART.Weapon];
                break;

            case EQUIP_STAT_TYPE.CriticalRate:
                equipment = _myEquipments[PART.Helmet];
                break;

            case EQUIP_STAT_TYPE.MaxHP or EQUIP_STAT_TYPE.Defense:
                equipment = _myEquipments[PART.Cloak];
                break;

            case EQUIP_STAT_TYPE.MoveSpeed or EQUIP_STAT_TYPE.Dodge:
                equipment = _myEquipments[PART.Shoes];
                break;

            default:
                return 0;
        }

        return equipment.GetIncreaseStat(stat);
    }

    // 유물리스트의 스텟 증가량을 반환
    public float GetIncreaseStat(STAT_TYPE stat)
    {
        float result = 0;
        foreach (var relic in _myRelics)
        {
            if (relic == null || relic == _relicZero)
                continue;


            if (relic.Data.Relic.stat_type == stat)
                result += relic.Data.Relic.stat_value;
        }
        return result;
    }

    public void SetRelic(ItemInstance item)
    {
        RelicBase relic = ItemDB.CreateItem<RelicBase>(item.ID);
        relic.SetEnhanceLevel(item.EnhanceLevel);
        for(int i = 0; i < _myRelics.Count; i++)
        {
            if (_myRelics[i] == _relicZero)
            {
                _myRelics[i] = relic;
                _outputRelics.Add(relic);
                _onChangeLoadout.OnStartEvent();
                return;
            }
        }

        Debug.Log($"<color=yellow>유물칸이 꽉 차서 {item.Name} 장착 실패</color>");
    }

    public void RemoveRelic(int id, int enhanceLevel)
    {
        for (int i = 0; i < _myRelics.Count; i++)
        {
            var relic = _myRelics[i];
            if (relic != null && relic.Data.Relic.id == id && relic.EnhanceLevel == enhanceLevel)
            {
                _myRelics.RemoveAt(i);
                _outputRelics.Remove(relic);
                _myRelics.Add(_relicZero);
                _onChangeLoadout.OnStartEvent();
                return;
            }
        }
        
    }

    public void OnChangeLoadout() => _onChangeLoadout.OnStartEvent();
}