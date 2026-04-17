

public interface IPlayerData
{
    int ID { get; }
    float Maxhp { get; }
    float AttackPower { get; }
    float DefensePower { get; }
    float EvasionChance { get; }
    float CriticalChance { get; }
    float CriticalDamageMultiplier { get; }
    float CriticalResistance { get; }
    float HealthRegain { get; }
    float PickupRange { get; }
    float MoveSpeed { get; }
    float AttackSpeed { get; }
    float ItemDropRate { get; }
    float GoldGainRate { get; }
    float ExpGainRate { get; }

}

public interface IDamageable
{
    void TakeDamage(float damage);
}

public interface IInvincible : IDamageable
{
    void SetInvincibleTime(float time);
}

public interface IRevive
{
    void OnRevive();
}