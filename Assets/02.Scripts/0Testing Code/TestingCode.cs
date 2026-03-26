using System;
using System.Collections.Generic;
using UnityEngine;

public class TestingCode : MonoBehaviour
{
    [Header("에너지")]
    public int value;
    [Header("아웃게임 레벨")]
    public float Level;
    public float currentEXP;

    [Header("===========================================================")]
    [Header("참조")]
    public EnergySystem energy;
    public OutGameLevelSystem level;

    private void Awake()
    {
        if (energy == null)
            energy = EnergySystem.Instance;
        if(level == null)
            level = OutGameLevelSystem.Instance;
    }

    private void Update()
    {
        value = energy.CurrentEnergy;
        Level = level.CurrentLevel;
        currentEXP = level.CurrentExp;
    }
}
