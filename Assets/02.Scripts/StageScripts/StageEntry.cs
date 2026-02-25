using UnityEngine;

public class StageEntry : MonoBehaviour
{
    [SerializeField] private int _stageId = 50201;

    [SerializeField] private bool _useTimeOverride;
    [SerializeField] private int _overrideMainTimeSeconds = 60; 
    [SerializeField] private int _overrideBossTimeSeconds = 30; 

    [SerializeField] private StageController _stageController;
    [SerializeField] private RuleBasedMonsterSpawner _spawner;
    [SerializeField] private MonsterRegistry _register;

    private IStagePlanProvider _planProvider;

    private void Awake()
    {
        _planProvider = new DataParserStagePlanProvider();
    }

    void Start()
    {
        StagePlan _plan = _planProvider.GetStagePlan(_stageId);
        if(_plan == null)
        {
            return;
        }

        if(_useTimeOverride)
        {
            _plan.mainLimitSeconds = _overrideMainTimeSeconds;
            _plan.bossLimitSeconds = _overrideBossTimeSeconds;
        }

        _stageController.StartStage(_plan, _spawner, _register);
    }

}
