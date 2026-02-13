public class SkillStat         // 추가 스킬 스탯
{
    public float Damage;            // 공격력 배율
    public float Cooldown;          // 쿨타임 감소
    public float Duration;          // 지속 시간
    public float ProjectileSpeed;   // 투사체 속도
    public int ProjectileCount;     // 투사체 수 추가
    public int PierceCount;         // 관통 횟수
    public float TickRate;          // 장판 피해 주기
    public float Knockback;         // 넉백 수치
    public float Size;              // 크기 비율

    // 합
    public void Add(SkillStat other)
    {
        Damage += other.Damage;
        Cooldown += other.Cooldown;
        Duration += other.Duration;
        ProjectileSpeed += other.ProjectileSpeed;
        ProjectileCount += other.ProjectileCount;
        PierceCount += other.PierceCount;
        TickRate += other.TickRate;
        Knockback += other.Knockback;
        Size += other.Size;
    }

    // 곱 (공용)
    public void Multiply(SkillStat multiplier)
    {
        if (multiplier.Damage != 0) Damage *= multiplier.Damage;
        if (multiplier.Cooldown != 0) Cooldown *= multiplier.Cooldown;
        if (multiplier.Duration != 0) Duration *= multiplier.Duration;
        if (multiplier.ProjectileSpeed != 0) ProjectileSpeed *= multiplier.ProjectileSpeed;
        if (multiplier.ProjectileCount != 0) ProjectileCount *= multiplier.ProjectileCount;
        if (multiplier.PierceCount != 0) PierceCount *= multiplier.PierceCount;
        if (multiplier.TickRate != 0) TickRate *= multiplier.TickRate;
        if (multiplier.Knockback != 0) Knockback *= multiplier.Knockback;
        if (multiplier.Size != 0) Size *= multiplier.Size;
    }

    // 공용 스탯용 생성
    public static SkillStat CreateMultiplier()
    {
        return new SkillStat
        {
            Damage = 1f,
            Cooldown = 1f,
            Duration = 1f,
            ProjectileSpeed = 1f,
            ProjectileCount = 1,
            PierceCount = 1,
            TickRate = 1f,
            Knockback = 1f,
            Size = 1f
        };
    }
}



public class PlayerStatBonus        // 추가 플레이어 스탯
{
    public int Defence;             // 방어력
    public float MoveSpeed;         // 이동 속도 
    public float CriticalRate;      // 치명타율
    public float CriticalDamage;    // 치명타 피해
    public float EvasionRate;       // 회피율
    public float ExpGain;           // 획득 경험치 증가량

    // 스탯 더하기
    public void Add(PlayerStatBonus other)
    {
        Defence += other.Defence;
        MoveSpeed += other.MoveSpeed;
        CriticalRate += other.CriticalRate;
        CriticalDamage += other.CriticalDamage;
        EvasionRate += other.EvasionRate;
        ExpGain += other.ExpGain;
    }
}
