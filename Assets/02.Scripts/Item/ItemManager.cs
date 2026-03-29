using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class ItemManager : MonoBehaviour
{
    [Header("인벤토리/재화가방")]
    [field: SerializeField] public ItemInventory Inventory { get; private set; }
    [field: SerializeField] public ItemCurrency Currency { get; private set; }      // 재화 가방

    [Header("UI그룹 출력 관리")]
    [SerializeField] private List<UIGroup> _uiGroupList;

    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeLoadout;


    public static ItemManager Instance { get; private set; }


    private void Awake()
    {
        #region 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        transform.SetParent(null);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion


    }

    public void OpenUI(UI_GROUP group)
    {
        foreach (var uiGroup in _uiGroupList)
        {
            if (uiGroup.Group == group)
                uiGroup.gameObject.SetActive(true);

            else
                uiGroup.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class InventoryUIOpenSet
{
    [field: SerializeField] public Button PushButton { get; private set; }
    [field: SerializeField] public UI_GROUP OpenGroup { get; private set; }
}