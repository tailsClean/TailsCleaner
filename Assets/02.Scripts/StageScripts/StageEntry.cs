using UnityEngine;

public class StageEntry : MonoBehaviour
{
    [SerializeField] private int _stageId = 1;

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

        _stageController.StartStage(_plan, _spawner, _register);
    }

}
