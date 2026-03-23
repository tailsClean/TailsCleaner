using System.Collections.Generic;

public static class RewardDB
{
    private static Dictionary<int, RewardDataBundle> _rewardDicts;


    public static RewardDataBundle GetRewardTable(int groupID)
    {
        if (_rewardDicts == null)
            Init();

        return _rewardDicts[groupID];
    }

    private static void Init()
    {
        _rewardDicts = new Dictionary<int, RewardDataBundle>();

        var rewardTable = DataManager.Instance.GetSOData<RewardTableSO>();
        foreach(var rewardData in rewardTable.dataList)
        {
            int groupID = rewardData.reward_group_id;
            if (_rewardDicts.TryGetValue(groupID, out RewardDataBundle reward))
                _rewardDicts[groupID].datas.Add(new RewardDataBundle.RewardData(rewardData));

            else
                _rewardDicts.Add(groupID, new RewardDataBundle(rewardData));
        }
    }
}

public class RewardDataBundle
{
    public List<RewardData> datas = new List<RewardData>();

    public RewardDataBundle(RewardTable data)
    {
        datas.Add(new RewardData(data));
    }
    
    public class RewardData
    {
        public RewardTable data;
        public bool isNonReward;

        public RewardData(RewardTable data)
        {
            this.data = data;
            isNonReward = false;
        }
    }
}