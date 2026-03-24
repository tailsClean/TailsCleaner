using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InGameExpItem : PoolObject, IPickable
{
    public static readonly List<InGameExpItem> ActiveItems = new List<InGameExpItem>();

    [SerializeField] private int _expPoint = 10;
    [SerializeField] private FloatEventChannelSO _onPickupExp;

    public Transform MyTransform => transform;

    public bool IsBossSpawnedDirt { get; private set; }
    public bool IsCleaned { get; private set; }
    public bool IsAbsorbing { get; private set; }

    public void SetExp(int exp)
    {
        _expPoint = Mathf.Max(0, exp);
    }

    public void SetBossSpawnedDirt(bool value)
    {
        IsBossSpawnedDirt = value;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        IsBossSpawnedDirt = false;
        IsCleaned = false;
        IsAbsorbing = false;

        RegisterSelf();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        UnregisterSelf();

        IsBossSpawnedDirt = false;
        IsCleaned = false;
        IsAbsorbing = false;

        StopAllCoroutines();
    }

    private void OnEnable()
    {
        // 풀링/일반 활성화 모두 안전하게 잡기 위해 보조 등록
        RegisterSelf();
    }

    private void OnDisable()
    {
        // 풀링/비활성화 모두 안전하게 정리
        UnregisterSelf();
    }

    private void OnDestroy()
    {
        UnregisterSelf();
    }

    private void RegisterSelf()
    {
        if (!ActiveItems.Contains(this))
            ActiveItems.Add(this);
    }

    private void UnregisterSelf()
    {
        ActiveItems.Remove(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsAbsorbing) return;
        if (IsCleaned) return;

        if (collision.TryGetComponent<PlayerBase>(out var player))
        {
            IsCleaned = true;
            _onPickupExp?.OnStartEvent(_expPoint);

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(this);
            else
                Destroy(gameObject);
        }
    }

    public IEnumerator MoveToBossAndAbsorb(Transform bossTransform, float duration, Action onAbsorbed = null)
    {
        if (bossTransform == null) yield break;
        if (IsCleaned) yield break;
        if (IsAbsorbing) yield break;

        IsAbsorbing = true;

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (bossTransform == null) yield break;
            if (IsCleaned) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, bossTransform.position, t);
            yield return null;
        }

        if (!IsCleaned)
        {
            IsCleaned = true;
            onAbsorbed?.Invoke();

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(this);
            else
                Destroy(gameObject);
        }
    }

    private static void CleanupInvalidItems()
    {
        for (int i = ActiveItems.Count - 1; i >= 0; i--)
        {
            InGameExpItem item = ActiveItems[i];

            if (item == null || !item.gameObject.activeInHierarchy)
            {
                ActiveItems.RemoveAt(i);
            }
        }
    }

    // 일반 몬스터 드랍 / 필드 미청소 전용.
    // trigger_exp_absorb 용도
    public static List<InGameExpItem> GetAllUncleaned()
    {
        CleanupInvalidItems();

        List<InGameExpItem> result = new List<InGameExpItem>();

        for (int i = 0; i < ActiveItems.Count; i++)
        {
            InGameExpItem item = ActiveItems[i];

            if (item == null) continue;
            if (item.IsCleaned) continue;
            if (item.IsAbsorbing) continue;
            if (item.IsBossSpawnedDirt) continue; // 핵심: 보스가 뿌린 더러움 제외

            result.Add(item);
        }

        return result;
    }

    // 보스가 직접 뿌린 더러움 전용.
    // trigger_dirty_spawn 용도
    public static List<InGameExpItem> GetBossSpawnedUncleaned()
    {
        CleanupInvalidItems();

        List<InGameExpItem> result = new List<InGameExpItem>();

        for (int i = 0; i < ActiveItems.Count; i++)
        {
            InGameExpItem item = ActiveItems[i];

            if (item == null) continue;
            if (item.IsCleaned) continue;
            if (item.IsAbsorbing) continue;
            if (!item.IsBossSpawnedDirt) continue;

            result.Add(item);
        }

        return result;
    }

    public static int GetFieldDropCount()
    {
        return GetAllUncleaned().Count;
    }

    public static int GetBossDirtCount()
    {
        return GetBossSpawnedUncleaned().Count;
    }
}