using UnityEngine;


public class UIGroup : MonoBehaviour
{
    [field: SerializeField] public UI_GROUP Group;


}
public enum UI_GROUP
{
    //StageClear, StageOver, 
    Equipment, Relic, ReinforceResource, Spendable
}