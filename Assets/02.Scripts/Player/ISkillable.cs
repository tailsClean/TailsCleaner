

public interface ISkillable
{
    public int AttackDamage { get; }
    public int DefensePower { get; }
    public int MoveSpeed { get; }
    public int CriticalChance { get; }
    public int CriticalDamageMultiplier { get; }    // 치명타 피해 계수
    public int EvasionChance { get; }               // 회피율
    public float PickupRange { get; }               // 경험치 획득 범위
    public float ExperienceGainRate { get; }        // 경헝치 획득량
}

