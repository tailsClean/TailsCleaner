using UnityEngine;
using System.Collections;

public abstract class AreaEffector : MonoBehaviour
{
    [Header("Timing")]
    public float previewTime = 2f;
    public float activeTime = 5f;

    [Header("Shape")]
    public float radius = 3f;

    [Header("Effects")]
    public GameObject previewEffect;
    public GameObject activeEffect;
    public GameObject destroyEffect;

    protected bool isActive = false;
    private Coroutine lifeRoutine;

    protected virtual void OnEnable()
    {
        ResetVisualState();
        ApplyShape();
        StartLifeRoutine();
    }

    protected virtual void OnDisable()
    {
        StopLifeRoutine();
        isActive = false;
        ResetVisualState();
    }

    public virtual void Initialize(float newRadius, float newPreviewTime, float newActiveTime)
    {
        radius = newRadius;
        previewTime = newPreviewTime;
        activeTime = newActiveTime;

        ApplyShape();
    }

    private void StartLifeRoutine()
    {
        StopLifeRoutine();
        lifeRoutine = StartCoroutine(LifeRoutine());
    }

    private void StopLifeRoutine()
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
    }

    private void ResetVisualState()
    {
        if (previewEffect != null) previewEffect.SetActive(false);
        if (activeEffect != null) activeEffect.SetActive(false);
    }

    protected virtual void ApplyShape()
    {
        // radius를 반지름으로 간주 → 스케일은 지름
        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
    }

    private IEnumerator LifeRoutine()
    {
        if (previewEffect != null)
            previewEffect.SetActive(true);

        yield return new WaitForSeconds(previewTime);

        if (previewEffect != null)
            previewEffect.SetActive(false);

        if (activeEffect != null)
            activeEffect.SetActive(true);

        isActive = true;
        OnActivate();

        yield return new WaitForSeconds(activeTime);

        isActive = false;
        OnDeactivate();

        if (activeEffect != null)
            activeEffect.SetActive(false);

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        ReturnToPoolOrDestroy();
    }

    private void ReturnToPoolOrDestroy()
    {
        if (ObjectPoolManager.Instance != null)
        {
            PoolObject po = GetComponent<PoolObject>();
            if (po != null)
            {
                ObjectPoolManager.Instance.ReturnObject(po);
                return;
            }
        }

        Destroy(gameObject);
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
}