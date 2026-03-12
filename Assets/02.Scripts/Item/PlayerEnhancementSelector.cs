using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnhancementSelector : MonoBehaviour
{
    [SerializeField] private EnhancementEventChannelSO _onEquipEnhancement;

    [field: SerializeField]
    public Dictionary<EQUIP_PARTS, EquipmentBase> MyEquipment { get; private set; } = new();
    
    [field: SerializeField]
    public List<RelicBase> MyRelic { get; private set; } = new(3);

    public event Action<EQUIP_PARTS> OnSetEquipment;
    public event Action OnSetRelic;

    private void OnEnable()
    {
        _onEquipEnhancement.AddListener(SetEnhancement);
    }

    private void OnDisable()
    {
        _onEquipEnhancement.RemoveListener(SetEnhancement);
    }

    //
    private void Update()
    {
        //PlayerDataTransfer.SetEquipments(MyEquipment);
        //PlayerDataTransfer.SetRelics(MyRelic);
        CompactRelicSlot();
        OnSetRelic?.Invoke();
    }
    //

    // 장비를 교체하는 메서드
    public void SetEnhancement(IEnhancement enhancement)
    {
        switch(enhancement)
        {
            case EquipmentBase equipment:
                if (!MyEquipment.TryAdd(equipment.EquipmentPart, equipment))
                        MyEquipment[equipment.EquipmentPart] = equipment;
                OnSetEquipment?.Invoke(equipment.EquipmentPart);
                break;

            case RelicBase relic:
                CompactRelicSlot();
                SetRelic(relic);
                OnSetRelic?.Invoke();
                break;
        }
    }

    // 유물 슬롯의 빈공간을 당겨주는 메서드
    private void CompactRelicSlot()
    {
        Queue<RelicBase> que = new Queue<RelicBase>();
        for(int i = 0; i < MyRelic.Count; i++)
        {
            if(MyRelic[i] != null)
                que.Enqueue(MyRelic[i]);

            MyRelic[i] = null;
        }

        for(int i = 0; i < MyRelic.Count; i++)
        {
            if (que.Count == 0)
                break;

            MyRelic[i] = que.Dequeue();
        }
    }

    // 유물을 슬롯에 추가하는 메서드
    private void SetRelic(RelicBase relic)
    {
        int i;
        for (i = 0; i < MyRelic.Count; i++)
        {
            if (MyRelic[i] == null)
            {
                MyRelic[i] = relic;
                break;
            }
        }
        if (i == MyRelic.Count)
            Debug.LogWarning("유물 장착 슬롯 초과");
    }
}
