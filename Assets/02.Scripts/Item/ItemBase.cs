using UnityEngine;
using UnityEngine.UI;


public abstract class ItemBase : MonoBehaviour
{
    [Header("아이템 기본 정보")]
    public bool IsItem;
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public ITEM_TYPE ItemType { get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; } = 1;
    [field: SerializeField] public int ItemNameKey { get; private set; }
    [field: SerializeField] public string ImageSprite { get; private set; }

    //
    public abstract Sprite GetSprite();
    //


    #region 에디터 설정
#if UNITY_EDITOR
    private void OnValidate()
    {
        var image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Image컴포넌트가 없음");
            return;
        }
        if (GetSprite() != null)
            image.sprite = GetSprite();
    }
#endif
    #endregion
}

public enum ITEM_TYPE
{
    System, Equipment, Relic, Reinforcement, Consume
}