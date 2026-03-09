using UnityEngine;


[CreateAssetMenu(fileName = "Consume ItemSO", menuName = "ItemData/ConsumeItem")]
public class ConsumeItemSO : ItemBaseSO
{
    [Header("소모품 정보")]
    [SerializeField] private int _iD;
    [SerializeField] private CONSUME_STAT _type;
    [SerializeField] private int _optValue;
    [SerializeField] private string _optDescription;
    [SerializeField] private string _script;
    [SerializeField] private string _usingAfterIllust;


    public CONSUME_STAT Type => _type;
    public int OptValue => _optValue;
    public string OptDescription => _optDescription;
    public string Script => _script;
    public string UsingAfterIllust => _usingAfterIllust;
}

public enum CONSUME_STAT
{
    Enter, Exp, EnergyUp
}
