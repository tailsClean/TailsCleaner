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
                continue;

            slot.SetStlot(statKey, statValue);
        }
    }

    #region 내부 메서드

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
            PLAYER_STAT.GoldGainRate =>             "골드 획득량",
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

        public void SetStlot(string statName, float value)
        {
            if (uiSlot == null)
                return;

            uiSlot.SetAddedText(TEXT_TYPE.Name, statName);
            uiSlot.SetAddedText(TEXT_TYPE.Value, value.ToString());
        }
    }
}
