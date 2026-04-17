using System;
using System.Collections.Generic;
using UnityEngine;
using static UISlotAddedText;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField] private List<SlotBundle> _uiSlots;


    private PlayerStatTransfer _statTransfer;
    private Dictionary<PLAYER_STAT, float> _statDict;



    private void OnEnable()
    {
        SetSlots();
    }



    // 슬롯리스트에 해당 값을 넣는 메서드
    private void SetSlots()
    {
        _statTransfer = PlayerStatManager.Instance.StatTransfer;

        _statDict = _statTransfer.StatDict;

        foreach(var slot in _uiSlots)
        {
            string statKey = GetStatKey(slot.playerStat);
            if (!_statDict.TryGetValue(slot.playerStat, out float statValue))
                slot.SetStlot("-", "-");
            else
                slot.SetStlot(statKey, ChangeStatToPercent(slot.playerStat, statValue));
        }
    }

    #region 내부 메서드

    private string ChangeStatToPercent(PLAYER_STAT stat, float value)
    {
        var basicStat = DataManager.Instance.GetSOData<CharManageTableSO>().GetById(1);


        return stat switch
        {
            PLAYER_STAT.CriticalChance => value.ToString() + "%",
            PLAYER_STAT.CriticalDamageMultiplier => (value * 100).ToString() + "%",
            PLAYER_STAT.CriticalResistance => value.ToString() + "%",
            PLAYER_STAT.EvasionChance => value.ToString() + "%",
            PLAYER_STAT.MoveSpeed => (value / basicStat.char_move_speed * 100) + "%",
            PLAYER_STAT.AttackSpeed => (value / basicStat.char_attack_speed * 100) + "%",
            PLAYER_STAT.HealthRegen => (value / basicStat.char_hp_regain * 100) + "%",
            PLAYER_STAT.PickupRange => (value / basicStat.char_item_range * 100) + "%",
            PLAYER_STAT.GoldGainRate => (value / basicStat.char_dropbonus_gold * 100) + "%",
            PLAYER_STAT.ItemDropRate => (value / basicStat.char_dropbonus_equip * 100) + "%",
            PLAYER_STAT.ExpGainRate => (value / basicStat.char_dropbonus_exp * 100) + "%",
            _ => ((int)value).ToString()
        };
    }

    // 출력할 스탯 이름 지정
    private string GetStatKey(PLAYER_STAT stat)
    {
        return stat switch
        {
            PLAYER_STAT.MaxHp =>                    "최대 체력",
            PLAYER_STAT.AttackPower =>              "공격력",
            PLAYER_STAT.DefensePower =>             "방어력",
            PLAYER_STAT.CriticalChance =>           "치명타 확률",
            PLAYER_STAT.CriticalDamageMultiplier => "치명타 피해",
            PLAYER_STAT.CriticalResistance =>       "치명타 저항",
            PLAYER_STAT.EvasionChance =>            "회피율",
            PLAYER_STAT.MoveSpeed =>                "이동 속도",
            PLAYER_STAT.AttackSpeed =>              "공격 속도",
            PLAYER_STAT.HealthRegen =>              "체력 회복량",
            PLAYER_STAT.PickupRange =>              "아이템 획득 범위",
            PLAYER_STAT.GoldGainRate =>             "금화 획득량",
            PLAYER_STAT.ItemDropRate =>             "아이템 획득량",
            PLAYER_STAT.EquipmentDropRate =>        "장비 획득량",
            PLAYER_STAT.ExpGainRate =>              "경험치 획득량",
            _ => string.Empty
        };
    }

    #endregion


    // 슬롯에 해당 스탯타입을 부여하는 클래스
    [Serializable]
    public class SlotBundle
    {
        public UISlotAddedText uiSlot;      // 메인 아이콘 없이 텍스트 사용
        public PLAYER_STAT playerStat;

        public void SetStlot(string statName, string value)
        {
            if (uiSlot == null)
                return;

            uiSlot.SetAddedText(TEXT_TYPE.Name, statName);
            uiSlot.SetAddedText(TEXT_TYPE.Value, value.ToString());
        }
    }
}

