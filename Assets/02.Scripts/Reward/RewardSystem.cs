using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardSystem
{
    [field: SerializeField] public int StageGroupID { get; private set; }
    public RewardDTO CurrentReward { get; private set; }


    public RewardSystem()
    {
        CurrentReward = new RewardDTO();
    }


    // 그룹ID로 보상 얻는 메서드
    public void OnGainReward(int groupID)
    {
        StageGroupID = groupID;

        SetRewardAmount();
    }


    
    // 해당 그룹ID의 보상을 모두 계산
    private void SetRewardAmount()
    {
        CurrentReward.Init();
        var dataBundle = RewardDB.GetRewardTable(StageGroupID);
        foreach(var rewardData in dataBundle.datas)
        {
            if (rewardData.isNonReward)
                continue;

            SetCurrentReward(rewardData.data);

            RewardTable data = rewardData.data;
            if(data.reward_category == REWARD_CATEGORY.First)
                rewardData.isNonReward = true;
        }
    }

    // 현재 보상에 값을 세팅
    private void SetCurrentReward(RewardTable rewardData)
    {
        int random = UnityEngine.Random.Range(rewardData.min_count, rewardData.max_count + 1);
        switch (rewardData.reward_type1)
        {
            case REWARD_TYPE.Gold:
                random = UnityEngine.Random.Range(rewardData.min_count, rewardData.max_count + 100);
                random /= 100;
                CurrentReward.GoldAmount = random * 100;
                break;

            case REWARD_TYPE.Equipment:
                var equip = new ItemInstance(rewardData.item_id, ItemInstance.NoneEnhanceLevel, GRADE.Dirty);
                equip.SetAmount(random);
                CurrentReward.Items.Add(equip);
                break;

            case REWARD_TYPE.BluePrint:
                var item = new ItemInstance(rewardData.item_id);
                item.SetAmount(random);
                CurrentReward.Items.Add(item);
                break;

        }
        Debug.Log($"{rewardData.item_id}의 갯수: {random}");
    }
}

[Serializable]
public class RewardDTO
{
    public int GoldAmount;
    public List<ItemInstance> Items;

    public RewardDTO()
    {
        Items = new List<ItemInstance>();
    }


    public void Init()
    {
        GoldAmount = 0;
        Items.Clear();
    }
}