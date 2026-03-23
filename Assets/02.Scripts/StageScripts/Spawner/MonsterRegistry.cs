using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRegistry : MonoBehaviour, IMonsterRegistry
{

    private const int MAX_FIELD_MONSTER_COUNT = 50; //필드에 존재할 수 있는 최대 몬스터 수

    private readonly HashSet<GameObject> _aliveMonsters = new HashSet<GameObject>();

    public event Action<GameObject> OnRegistered;
    public event Action<GameObject> OnUnregistered;

    private GameObject _bossObject;

    public int GetAliveCount()
    {
        return _aliveMonsters.Count;
    }

    public bool CanSpawnMore()
    {
        return _aliveMonsters.Count < MAX_FIELD_MONSTER_COUNT;
    }

    public void MarkBoss(GameObject boss)
    {
        _bossObject = boss;
    }

    public bool IsBoss(GameObject obj)
    {
        return obj != null && obj == _bossObject;
    }

    public void ClearBossMark()
    {
        _bossObject = null;
    }

    public void Register(GameObject monster)
    {
        if (monster == null)
            return;

        if (_aliveMonsters.Add(monster))
        {
            MonsterRegistryHook hook = monster.GetComponent<MonsterRegistryHook>();
            if (hook == null)
            {
                hook = monster.AddComponent<MonsterRegistryHook>();
            }

            hook.Bind(this, monster);
            OnRegistered?.Invoke(monster);
        }
    }

    public void Unregister(GameObject monster)
    {
        if (monster == null)
            return;

        if (_aliveMonsters.Remove(monster))
        {
            OnUnregistered?.Invoke(monster);
        }

        if (_bossObject == monster)
        {
            _bossObject = null;
        }
    }

    public void KillAllMonsters()
    {
        KillAllMonsters(includeBoss: false);
    }

    public void KillAllMonsters(bool includeBoss)
    {
        List<GameObject> toRemove = new List<GameObject>(_aliveMonsters);

        for (int i = 0; i < toRemove.Count; i++)
        {
            GameObject obj = toRemove[i];
            if (obj == null)
                continue;

            if (!includeBoss && obj == _bossObject)
                continue;

            if (obj.TryGetComponent<PoolObject>(out var poolObj))
            {
                poolObj.ReturnToPoolAfter(0f);
            }
            else
            {
                Destroy(obj);
            }
        }

        _aliveMonsters.RemoveWhere(obj =>
            obj == null || (includeBoss || obj != _bossObject));
    }

    public void SetAllMonstersPaused(bool paused, bool includeBoss = true)
    {
        List<GameObject> snapshot = new List<GameObject>(_aliveMonsters);

        for (int i = 0; i < snapshot.Count; i++)
        {
            GameObject obj = snapshot[i];
            if (obj == null)
                continue;

            if (!includeBoss && obj == _bossObject)
                continue;

            if (obj.TryGetComponent<MonsterBase>(out var monsterBase))
            {
                monsterBase.SetPaused(paused);
            }
        }
    }

    public void SetBossPaused(bool paused)
    {
        if (_bossObject == null)
            return;

        if (_bossObject.TryGetComponent<MonsterBase>(out var bossMonster))
        {
            bossMonster.SetPaused(paused);
        }
    }
}

public class MonsterRegistryHook : MonoBehaviour
{
    private MonsterRegistry _registry;
    private GameObject _self;

    public void Bind(MonsterRegistry registry, GameObject self)
    {
        _registry = registry;
        _self = self;
    }

    private void OnDisable()
    {
        if (_registry != null && _self != null)
        {
            _registry.Unregister(_self);
        }
    }

    private void OnDestroy()
    {
        if (_registry != null && _self != null)
        {
            _registry.Unregister(_self);
        }
    }
}
