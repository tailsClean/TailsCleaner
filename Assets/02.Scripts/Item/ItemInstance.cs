using System;

/// <summary>
/// 인벤토리의 아이템의 정보와 수량을 담은 정보 전달용 구조체
/// <summary>
[Serializable]
public struct ItemInstance
{
    public int ID;
    public string Name;
    public int EnhanceLevel;
    public GRADE Grade;
    public ITEM_TYPE ItemType;

    public int Amount { get; private set; }
    public const int NoneEnhanceLevel = -1;

    public const int NoneStackAmount = -1;

    /// <summary>
    /// 아이템의 존재여부 확인용 존재할 수 없는 인벤토리 인스턴스
    /// </summary>
    public static ItemInstance None => new ItemInstance("Zero");

    /// <summary>
    /// 스택형 아이템의 아이템객체
    /// </summary>
    /// <param name="id"></param>
    public ItemInstance(int id)
    {
        var item = ItemDB.GetData(id);
        ID = id;
        Name = item.Name;
        EnhanceLevel = NoneEnhanceLevel;
        Grade = GRADE.None;
        ItemType = item.Type;

        Amount = 1;
    }

    /// <summary>
    /// 강화 or 등급값이 필요한 아이템객체
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enhanceLevel"></param>
    /// <param name="grad"></param>
    public ItemInstance(int id, int enhanceLevel, GRADE grad)
    {
        var item = ItemDB.GetData(id);
        ID = id;
        Name = item != null ? item.Name : $"<color=red>{id}의 아이템은 없습니다.</color>";
        EnhanceLevel = enhanceLevel;
        Grade = grad;
        ItemType = item != null ? item.Type : ITEM_TYPE.None;

        Amount = 1;
    }

    private ItemInstance(string zero)
    {
        ID = -1; Name = zero; EnhanceLevel = -1; Grade = GRADE.None; ItemType = ITEM_TYPE.System; Amount = 1;
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
