using System.Collections.Generic;

/// <summary>
/// 장비 선택 -> 플레이어 생성 사이의 최종 장착장비 정보 전달 클래스
/// </summary>
public static class PlayerDataTransfer
{

    public static Dictionary<EquipmentBase.PARTS, EquipmentBase> Equipments { get; private set; }

    public static void SetEquipments(Dictionary<EquipmentBase.PARTS, EquipmentBase> dict) =>
        Equipments = dict;
}

