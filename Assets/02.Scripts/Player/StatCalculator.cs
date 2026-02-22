
using System.Collections.Generic;

public class StatCalculator
{
    // 플레이어 이동속도 계산 메서드
    public int GetMoveSpeed(int moveSpeed, Dictionary<Equipment.PARTS, Equipment> equipments)
    {
        var shoes = equipments[Equipment.PARTS.Shoes].ApplyEquipment<ShoesEquipment>();
        int increase = shoes.MoveSpeedIncrease;
        return moveSpeed + increase;
    }
}
