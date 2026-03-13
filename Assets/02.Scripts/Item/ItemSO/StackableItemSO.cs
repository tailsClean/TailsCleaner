using UnityEngine;


[CreateAssetMenu(fileName = "Stackable ItemSO", menuName = "ItemData/StackableItem")]
public class StackableItemSO : ItemBaseSO
{
    [Header("재화/강화재료 정보")]
    [SerializeField] private int _iD;
    [SerializeField] private string _name;

    public override string Name => _name;
}