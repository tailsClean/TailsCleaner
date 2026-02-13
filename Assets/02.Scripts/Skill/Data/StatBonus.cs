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

    // 스탯 더하기
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
