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

        if (!ActiveItems.Contains(this))
            ActiveItems.Add(this);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        ActiveItems.Remove(this);

        IsBossSpawnedDirt = false;
        IsCleaned = false;
        IsAbsorbing = false;

        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsAbsorbing) return;

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

    public static List<InGameExpItem> GetAllUncleaned()
    {
        List<InGameExpItem> result = new List<InGameExpItem>();

        for (int i = 0; i < ActiveItems.Count; i++)
        {
            InGameExpItem item = ActiveItems[i];
            if (item != null && !item.IsCleaned && !item.IsAbsorbing)
                result.Add(item);
        }

        return result;
    }

    public static List<InGameExpItem> GetBossSpawnedUncleaned()
    {
        List<InGameExpItem> result = new List<InGameExpItem>();

        for (int i = 0; i < ActiveItems.Count; i++)
        {
            InGameExpItem item = ActiveItems[i];
            if (item != null && !item.IsCleaned && !item.IsAbsorbing && item.IsBossSpawnedDirt)
                result.Add(item);
        }

        return result;
    }
}