using System.Collections.Generic;
using UnityEngine;


public class TargetingSystem
{
    private Transform _startPointTr;
    private float _searchDistance;
    private float _searchAngle;
    private Queue<Transform> _targets;
    private LayerMask _monsterLayer;


    public TargetingSystem(Transform startPoinTr, LayerMask monsterLayer)
    {
        _startPointTr = startPoinTr;
        _targets = new Queue<Transform>();
        _monsterLayer = monsterLayer;
    }

    // 외부에서 타겟을 반환 요청하는 메서드
    public Transform GetTarget(Vector2 attackDir, float distance, float angle)
    {
        _searchDistance = distance;
        _searchAngle = angle * Mathf.Deg2Rad;

        var targetsAll = SearchTarget();
        CalculateAngle(targetsAll, attackDir);

        return CalculateDistance();
    }

    // 원형 범위 타겟 탐색
    private Collider2D[] SearchTarget() => Physics2D.OverlapCircleAll(_startPointTr.position, _searchDistance, _monsterLayer);

    // 특정 시야각내부의 타겟 선별
    private void CalculateAngle(Collider2D[] targetsAll, Vector2 attackDir)
    {
        foreach (var target in targetsAll)
        {
            Vector2 targetDir = target.transform.position - _startPointTr.position;
            float dot = Vector2.Dot(targetDir.normalized, attackDir.normalized);
            float angle = Mathf.Cos(_searchAngle);

            if (dot > angle)
                _targets.Enqueue(target.transform);
        }
    }

    // 가장 가까운 타겟으로 선별
    private Transform CalculateDistance()
    {
        float min = _searchDistance * _searchDistance;
        Transform selected = null;
        while (_targets.Count > 0)
        {
            var target = _targets.Dequeue();
            float distance = Vector2.SqrMagnitude(target.position - _startPointTr.position);

            if (distance < min)
            {
                min = distance;
                selected = target;
            }
        }

        return selected;
    }
}