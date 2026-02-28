﻿using UnityEngine;

public interface ISkillable
{
    public int AttackPower { get; }
    public int DefensePower { get; }
    public int MoveSpeed { get; }
    public int CriticalChance { get; }
    public int CriticalDamageMultiplier { get; }    // 치명타 피해 계수
    public int EvasionChance { get; }               // 회피율
    public float PickupRange { get; }               // 경험치 획득 범위
    public float ExpGainRate { get; }        // 경헝치 획득량

    public Vector2 MoveDir { get; }
    public Vector2 AttackDir { get; }
    public Transform AttackTarget {  get; }
}

