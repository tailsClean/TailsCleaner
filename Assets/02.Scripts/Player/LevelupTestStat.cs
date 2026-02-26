using UnityEngine;

public class LevelupTestStat : MonoBehaviour
{
    [Header("테스트용 레벨업시, 스텟 증가량")]
    [Header("(런타임 중 수정시 바로 반영)")]
    public int MaxHp = 10;
    public int AttackPower = 3;
    public int DefensePower = 2;
    public int HealthRegen = 1;
    public int CombatMaxExp = 10;

    public struct StatDelta
    {
        public int MaxHp;
        public int AttackPower;
        public int DefensePower;
        public int HealthRegen;
        public int Level;
        public int MaxExp;
        public int CurrentExp;

        public StatDelta(int currentExp)
        {
            MaxHp = 0;
            AttackPower = 0;
            DefensePower = 0;
            HealthRegen = 0;
            Level = 0;
            MaxExp = 0;
            CurrentExp = currentExp;
        }

        public void LevelUpStatDelta(LevelupTestStat test, int currentExp)
        {
            MaxHp = test.MaxHp;
            AttackPower = test.AttackPower;
            DefensePower = test.DefensePower;
            HealthRegen = test.HealthRegen;
            Level = 1;
            MaxExp = test.CombatMaxExp;
            CurrentExp = currentExp;
        }
    }
}
