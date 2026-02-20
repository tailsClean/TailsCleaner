using UnityEngine;
using MonsterEnum;

// 테스트용 더미 몬스터
// - MonsterBase를 상속해 RuleBasedMonsterSpawner가 요구하는 target/is3DMode 설정을 그대로 받는다.
// - 프리팹별로 인스펙터에서 스탯/표시만 다르게 세팅하여 (일반/미들/보스) 3종을 만든다.
public sealed class DummyMonster : MonsterBase
{
    [Header("--- Dummy Visual ---")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("--- Dummy Preset ---")]
    [Tooltip("일반/미들/보스 구분용 (테스트 전용)")]
    [SerializeField] private MonsterType _dummyType = MonsterType.Normal;

    [SerializeField] private float _dummyHp = 3.0f;
    [SerializeField] private float _dummyMoveSpeed = 2.0f;

    protected override void Awake()
    {
        base.Awake();

        // MonsterBase 기본값에 더미 프리셋 반영
        monsterType = _dummyType;
        hp = _dummyHp;
        moveSpeed = _dummyMoveSpeed;
    }

    protected override void Start()
    {
        base.Start();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}