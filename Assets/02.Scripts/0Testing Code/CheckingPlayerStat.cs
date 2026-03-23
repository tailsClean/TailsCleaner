using UnityEngine;

public class CheckingPlayerStat : MonoBehaviour
{
    public PlayerBase Player;

    [Header("<color=green>========실시간 스탯 확인용========</color>")]
    [Header("")]

    [Header("공격 스탯")]
    public float AttackPower;
    public float CriticalChance;
    public float CriticalDamageMultiplier;
    public float AttackSpeed;

    [Header("방어 스탯")]
    public float MaxHp;
    public float CurrentHp;
    public float DefensePower;
    public float EvasionChance;

    [Header("쉴드")]
    public int MaxShield;
    public int CurrentShield;

    [Header("이동속도 + 획득량")]
    public float MoveSpeed;

    public float ItemDropRate;
    public float GoldGainRate;
    public float ExpGainRate;


    [Header("경험치")]
    public int InGameLevel;
    public float InGameMaxExp;
    public float InGameCurrentExp;
    public int OutGameLevel;
    public float OutGameMaxExp;
    public float OutGameCurrentExp;

    [Header("경험치 획득범위")]
    public float PickupRange;

    private void Awake()
    {
        if(Player == null)
            Player = GetComponent<PlayerBase>();
    }

    private void Update()
    {
        TestSetStat(Player);
    }

    public void TestSetStat(PlayerBase player)
    {
        AttackPower = player.AttackPower;
        CriticalChance = player.CriticalChance;
        CriticalDamageMultiplier = player.CriticalDamageMultiplier;
        AttackSpeed = player.AttackSpeed;

        MaxHp = player.MaxHp;
        CurrentHp = player.CurrentHp;
        DefensePower = player.DefensePower;
        EvasionChance = player.EvasionChance;

        MaxShield = player.MaxShield;
        CurrentShield = player.CurrentShield;
        MoveSpeed = player.MoveSpeed;
        ItemDropRate = player.ItemDropRate;
        GoldGainRate = player.GoldGainRate;
        ExpGainRate = player.ExpGainRate;

        InGameLevel = player.InGameLevel;
        InGameMaxExp = player.InGameMaxExp;
        InGameCurrentExp = player.InGameCurrentExp;
        OutGameLevel = player.OutGameLevel;
        OutGameMaxExp = player.OutGameMaxExp;
        OutGameCurrentExp = player.OutGameCurrentExp;
        PickupRange = player.PickupRange;
    }





}
