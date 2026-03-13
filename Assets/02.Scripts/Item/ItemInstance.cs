

using System;
using System.Xml;
using UnityEngine;

/// <summary>
/// 인벤토리의 아이템의 정보와 수량을 담은 정보 전달용 구조체
public struct ItemInstance
{
    public readonly int ID;
    public readonly int EnhanceLevel;
    public readonly EQUIP_GRADE Grade;

    public int Amount { get; private set; }
    public const int NoneEnhanceLevel = -1;

    public const int NoneStackAmount = -1;

    /// <summary>
    /// 아이템의 존재여부 확인용 존재할 수 없는 인벤토리 인스턴스
    /// </summary>
    public static ItemInstance None => new ItemInstance(-1, -1, EQUIP_GRADE.None);

    /// <summary>
    /// 스택형 아이템의 아이템객체
    /// </summary>
    /// <param name="id"></param>
    public ItemInstance(int id)
    {
        ID = id;
        EnhanceLevel = NoneEnhanceLevel;
        Grade = EQUIP_GRADE.None;
        Amount = 1;
    }

    /// <summary>
    /// 강화 or 등급값이 필요한 아이템객체
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enhanceLevel"></param>
    /// <param name="grad"></param>
    public ItemInstance(int id, int enhanceLevel, EQUIP_GRADE grad)
    {
        ID = id;
        EnhanceLevel = enhanceLevel;
        Grade = grad;
        Amount = 1;
    }

    public void SetAmount(int amount) => Amount = amount;

    #region 인벤토리 Key 조회용 메서드(자동 적용)
    public override bool Equals(object obj)
    {
        if (obj is ItemInstance other)
        {
            return ID == other.ID &&
                   EnhanceLevel == other.EnhanceLevel &&
                   Grade == other.Grade;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID, EnhanceLevel, Grade);
    }
    #endregion
}
