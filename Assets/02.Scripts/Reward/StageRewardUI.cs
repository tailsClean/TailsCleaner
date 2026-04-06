using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StageRewardUI : MonoBehaviour
{
    [SerializeField] private int _rewardGroupID;
    [SerializeField] private List<UISlot> _rewardSlots;

    private HashSet<int> _checkDuplications;

    private void Awake()
    {
        _checkDuplications = new();
    }

    [ContextMenu("ㅁㅇ")]
    public void asd()
    {
        Debug.Log(GetComponent<ScrollRect>().enabled);
    }

    // 해당 보상그룹ID를 넣어서 슬롯을 UI출력
    public void SetSlots(int rewardGroupID)
    {
        _rewardGroupID = rewardGroupID;

        _checkDuplications.Clear();
        var dataBundle = RewardDB.GetRewardTable(_rewardGroupID);

        if (dataBundle == null)
            return;

        // 골드가 가장 먼저 출력되도록 검수
        CheckGold(dataBundle, out int i);

        foreach (var dataDTO in dataBundle.datas)
        {
            var rewardData = dataDTO.data;
            // 중복된 아이템의 경우에는 출력하지 않도록 판별
            if (_checkDuplications.TryGetValue(rewardData.item_id, out int none))
                continue;

            if (i >= _rewardSlots.Count )
            { Debug.Log("<color=red>미리보기 보상수보다 슬롯이 부족합니다.</color>"); return; }

            _checkDuplications.Add(rewardData.item_id);
            _rewardSlots[i++].SetSlot(rewardData.item_id);
        }
        for(; i < _rewardSlots.Count; i++)
        {
            _rewardSlots[i].Init();
        }
    }

    private void CheckGold(RewardDataBundle dataBundle, out int index)
    {
        foreach(var dataDTO in dataBundle.datas)
        {
            if(dataDTO.data.item_id == ItemID.Gold)
            {
                _checkDuplications.Add(dataDTO.data.item_id);
                _rewardSlots[0].SetSlot(dataDTO.data.item_id);
                index = 1;
                return;
            }
        }
        index = 0;
    }
}