using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [SerializeField] private List<UIGroup> _uiGroupList;

    private Dictionary<UI_GROUP, UIGroup> _uiGroups;


    private void Awake()
    {
        #region 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

    }


    public void OpenUI(UI_GROUP group)
    {
        foreach(var uiGroup in _uiGroupList)
        {
            if(uiGroup.Group != group)
                uiGroup.gameObject.SetActive(false);

            else
                uiGroup.gameObject.SetActive(true);
        }
    }
}


//[Serializable]
//public class InventoryUIOpenSet
//{
//    [field: SerializeField] public Button OpenButton { get; private set; }
//    [field: SerializeField] public UI_GROUP Group { get; private set; }
//}