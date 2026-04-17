using System;
using System.Collections.Generic;

public class RewardCreator
{
    public int StageGroupID { get; private set; }
    public RewardDTO CurrentReward { get; private set; }


    public RewardCreator()
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
            // 획득할 수 없다면 획득x(첫 클리어시에만 습득하는 보상 필터링)
            if (rewardData.isNonReward)
                continue;

            // 드랍확률에 들어가지 않으면 획득x
            if(!CheckDropable(rewardData))
                continue;

            SetCurrentReward(rewardData.data);

            // 첫 클리어 보상일 경우 isNonReward = true
            RewardTable data = rewardData.data;
            if(data.reward_category == REWARD_CATEGORY.First)
                rewardData.isNonReward = true;
        }
    }

    // 드랍확률에 들어가지 않으면 획득x
    private bool CheckDropable(RewardDataBundle.RewardData reward)
    {
        int dropPercent = (int)reward.data.drop_rate * 100;
        int random = UnityEngine.Random.Range(1, 101);

        if (random <= dropPercent)
            return true;
        
        else
            return false;
    }

    // 현재 보상에 값(아이템의 정보, 획득 수량)을 세팅
    private void SetCurrentReward(RewardTable rewardData)
    {
        int random = UnityEngine.Random.Range(rewardData.min_count, rewardData.max_count + 1);

        CurrentReward.SetDTO(rewardData, random);
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

    // 보상의 종류에 따라 보상을 분류해서 추가함
    public void SetDTO(RewardTable rewardData, int amount)
    {
        switch (rewardData.reward_type)
        {
            case REWARD_TYPE.Gold:
                amount = UnityEngine.Random.Range(rewardData.min_count, rewardData.max_count + 100);
                amount /= 100;
                GoldAmount = amount * 100;
                break;

            case REWARD_TYPE.Equipment:
                var equip = new ItemInstance(rewardData.item_id, ItemInstance.NoneEnhanceLevel, GRADE.Dirty);
                equip.SetAmount(amount);
                Items.Add(equip);
                break;

            case REWARD_TYPE.BluePrint:
                var item = new ItemInstance(rewardData.item_id);
                item.SetAmount(amount);
                Items.Add(item);
                break;
        }
    }


    public void Init()
    {
        GoldAmount = 0;
        Items.Clear();
    }
}