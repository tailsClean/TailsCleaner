using System;
using UnityEngine;


// 강화 데이터 클래스
[Serializable]
public class EnhanceData
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int GroupID { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public bool IsMaxLevel { get; private set; }
    [field: SerializeField] public float AddValue { get; private set; }
    [field: SerializeField] public int CostGold { get; private set; }
    [field: SerializeField] public int CostBluePrint { get; private set; }
    [field: SerializeField] public int BluePrintID { get; private set; }
}
