using UnityEngine;


public class PlayerLevelSystem
{
    private PlayerBase _player;
    private int _maxExp;

    public StatDelta LevelUpDelta { get; private set; }
    public bool IsLevelUp { get; private set; }

    public PlayerLevelSystem(PlayerBase player, int maxExp)
    {
        _player = player;
        _maxExp = maxExp;
    }

    public int GainExperience(int currentExp, int gainExp)
    {
        currentExp += gainExp;
        if (currentExp > _maxExp)
        {
            IsLevelUp = true;
            LevelUp();
            return currentExp - _maxExp;
        }

        IsLevelUp = false;
        return currentExp;
    }

    //레벨업 시, 스텟변화량을 받아서 증가
    private void LevelUp()
    {
        LevelUpDelta = new StatDelta(1);
    }


    // 주위 아이템(경험치) 끌어모으는 메서드
    public void ItemPickup(Transform playerTr, IPickable item)
    {
        Vector2 itemPos = item.MyTransform.position;
        Vector2 myPos = playerTr.position;

        // 마지막 인자갑은 끌어당기는 속도
        item.MyTransform.position = Vector2.MoveTowards(itemPos, myPos, 1f * Time.deltaTime);
    }

    public struct StatDelta
    {
        public readonly int MaxHp;
        public readonly int AttackPower;
        public readonly int DefensePower;
        public readonly int HealthRegen;
        public readonly int CombatLevel;
        public readonly int CombatMaxExp;

        public StatDelta(int lelTest)
        {
            MaxHp = 0;
            AttackPower = 0;
            DefensePower = 0;
            HealthRegen = 0;
            CombatLevel = lelTest;
            CombatMaxExp = 0;
        }

        //public StatDelta(int maxHp, int att, int def, int healthRegen, int maxExp, int currentExp)
        //{
        //    MaxHp = maxHp;
        //    AttackPower = att;
        //    DefensePower = def;
        //    HealthRegen = healthRegen;
        //    CombatLevel = 1;
        //    CombatMaxExp = maxExp;
        //    CurrentExp = currentExp;
        //}
    }
}
