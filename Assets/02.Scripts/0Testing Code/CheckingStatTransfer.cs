
using UnityEngine;

public class CheckingStatTransfer : MonoBehaviour
{
    private PlayerStatTransfer transfer;
    
    public int ID;

    public float Maxhp;

    public float AttackPower;

    public float DefensePower;

    public float EvasionChance;

    public float CriticalChance;

    public float CriticalDamageMultiplier;

    public float CriticalResistance;

    public float HealthRegain;

    public float PickupRange;

    public float MoveSpeed;

    public float AttackSpeed;

    public float ItemDropRate;

    public float GoldGainRate;

    public float ExpGainRate;

    [Header("계정 레벨")]
    public bool Maxlevel;
    public int level;
    private OutGameLevelSystem system;

    private void Start()
    {
        transfer = PlayerStatManager.Instance.StatTransfer;
        system = OutGameLevelSystem.Instance;
    }

    private void Update()
    {
        transfer = PlayerStatManager.Instance.StatTransfer;
        Maxhp = transfer.Maxhp;
        AttackPower = transfer.AttackPower;
        DefensePower = transfer.DefensePower;
        EvasionChance = transfer.EvasionChance;
        CriticalChance = transfer.CriticalChance;
        CriticalDamageMultiplier = transfer.CriticalDamageMultiplier;
        CriticalResistance = transfer.CriticalResistance;
        HealthRegain = transfer.HealthRegain;
        MoveSpeed = transfer.MoveSpeed;
        AttackSpeed = transfer.AttackSpeed;
        ItemDropRate = transfer.ItemDropRate;
        GoldGainRate = transfer.GoldGainRate;
        ExpGainRate = transfer.ExpGainRate;

        level = system.CurrentLevel;
        Maxlevel = system.IsMaxLevel;
    }
}

