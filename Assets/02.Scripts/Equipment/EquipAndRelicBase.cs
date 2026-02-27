using UnityEngine;


public abstract class EquipAndRelicBase : MonoBehaviour
{
    [Header("공통 기본 정보")]
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int GroupID { get; private set; }                // 해당 파츠 고유ID
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public string IconSprite { get; private set; }
    [field: SerializeField] public string IconClickEffect { get; private set; }
    [field: SerializeField] public string IconClickSound { get; private set; }
}