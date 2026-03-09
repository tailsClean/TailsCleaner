using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 스텟 계산기
/// </summary>
public class PlayerStatCalculator
{
    private PlayerEnhancementSlots _enhanceInventory;

    public PlayerStatCalculator(PlayerEnhancementSlots enhanceInventory)
    {
        _enhanceInventory = enhanceInventory;
    }

    // 장비로 인한 증가값 계산
    public float GetFinalSat(float initialStat, EQUIP_STAT increaseStat)
    {
        return initialStat + _enhanceInventory.GetIncreaseStat(increaseStat);
    }

    // 유물로 인한 증가값 계산
    public float GetFinalSat(float initialStat, RELIC_STAT increaseStat)
    {
        float increaseRate = 1 + _enhanceInventory.GetIncreaseStat(increaseStat);
        return initialStat * increaseRate;
    }
}