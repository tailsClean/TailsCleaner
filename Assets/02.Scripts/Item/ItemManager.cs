using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [SerializeField] private List<UIGroup> _uiGroupList;

    private Dictionary<UI_GROUP, UIGroup> _uiGroups;

    public PlayerLoadout Loadout { get; private set; }


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
    private void Start()
    {
        Loadout = new PlayerLoadout();
    }

    public void OpenUI(UI_GROUP group)
    {
        foreach (var uiGroup in _uiGroupList)
        {
            if (uiGroup.Group != group)
                uiGroup.gameObject.SetActive(false);

            else
                uiGroup.gameObject.SetActive(true);
        }
    }

    [ContextMenu("UI 그룹 매핑")]
    public void asd()
    {
        foreach (var equip in Loadout.MyEquipments.Values)
        {
            Debug.Log("========");
            Debug.Log(equip.Data.Equipmnet.id);
            Debug.Log(equip.Data.Equipmnet.part);
            Debug.Log("========");
        }
        
    }
}

[Serializable]
public class InventoryUIOpenSet
{
    [field: SerializeField] public Button PushButton { get; private set; }
    [field: SerializeField] public UI_GROUP OpenGroup { get; private set; }
}