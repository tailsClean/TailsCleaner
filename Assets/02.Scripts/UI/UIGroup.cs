using UnityEngine;


public class UIGroup : MonoBehaviour
{
    [field: SerializeField] public UISTATE UIState;


    public enum UISTATE
    {
        StageClear, StageOver
    }
}