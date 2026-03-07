using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/SpendableItem")]
public class SpendableItemSO : ItemBaseSO
{
    [SerializeField] private SPENDABLE_STAT _type;
    [SerializeField] private int _optValue;
    [SerializeField] private string _optDescription;
    [SerializeField] private string _script;
    [SerializeField] private string _usingAfterIllust;


    public SPENDABLE_STAT Type => _type;
    public int OptValue => _optValue;
    public string OptDescription => _optDescription;
    public string Script => _script;
    public string UsingAfterIllust => _usingAfterIllust;
}

public enum SPENDABLE_STAT
{
    Enter, Exp, EnergyUp
}
