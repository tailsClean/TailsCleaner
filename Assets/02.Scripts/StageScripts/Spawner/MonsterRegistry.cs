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

    public void MarkBoss(GameObject _boss)
    {
        _bossObject = _boss;
    }

    public bool IsBoss(GameObject _boss)
    {
        return _boss != null && _boss == _bossObject;
    }

    public void Register(GameObject _monster)
    {
        if(_monster == null)
        { return; }

        _aliveMonsters.Add(_monster);

        MonsterRegistryHook _hook = _monster.GetComponent<MonsterRegistryHook>();

        if(_hook == null)
        {
            _hook = _monster.AddComponent<MonsterRegistryHook>();
        }

        _hook.Bind(this, _monster);

        OnRegistered?.Invoke(_monster);
    }

    public void Unregister(GameObject _monster)
    {
        if(_monster == null)
        { return; }

        _aliveMonsters.Remove(_monster);

        OnUnregistered?.Invoke(_monster);
    }

    public void KillAllMonsters()
    {
        List<GameObject> _toDestroy = new List<GameObject>(_aliveMonsters);
        for(int i = 0; i < _toDestroy.Count; i++)
        {
            GameObject _obj = _toDestroy[i];
            if(_obj != null)
            {
                Destroy(_obj);
            }
        }

        _aliveMonsters.Clear();
    }
}

public class MonsterRegistryHook : MonoBehaviour
{
    private MonsterRegistry _registry;
    private GameObject _self;

    public void Bind(MonsterRegistry _registry, GameObject _self)
    {
        this._registry = _registry;
        this._self = _self;
    }

    private void OnDestroy()
    {
        if(_registry != null && _self != null)
        {
            _registry.Unregister(_self);
        }
    }
}
