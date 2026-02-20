using System.Collections.Generic;

public static class PlayerDataTransfer
{

    public static Dictionary<Equipment.PARTS, Equipment> Equipments { get; private set; }

    public static void SetEquipments(Dictionary<Equipment.PARTS, Equipment> dict) =>
        Equipments = dict;
}

