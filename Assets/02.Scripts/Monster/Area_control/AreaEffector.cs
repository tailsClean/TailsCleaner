using UnityEngine;
using System.Collections;

public abstract class AreaEffector : MonoBehaviour
{
    public float previewTime = 2.0f;
    public float activeTime = 5.0f;
    public float radius = 3.0f;

    public GameObject previewEffect;
    public GameObject activeEffect;
    public GameObject destroyEffect;

    protected bool isActive = false;
    private Coroutine areaRoutine;

    protected virtual void OnEnable()
    {
        transform.localScale = new Vector3(radius, radius, 1f);
        isActive = false;

        // 예고/활성 이펙트 초기 상태 설정
        if (previewEffect != null) previewEffect.SetActive(false);
        if (activeEffect != null) activeEffect.SetActive(false);

        if (areaRoutine != null) StopCoroutine(areaRoutine);
        areaRoutine = StartCoroutine(AreaRoutine());
    }

    // 비활성화될 때 코루틴 정리
    protected virtual void OnDisable()
    {
        if (areaRoutine != null)
        {
            StopCoroutine(areaRoutine);
            areaRoutine = null;
        }
    }

    IEnumerator AreaRoutine()
    {
        if (previewEffect != null) previewEffect.SetActive(true);
        yield return new WaitForSeconds(previewTime);

        if (previewEffect != null) previewEffect.SetActive(false);
        if (activeEffect != null) activeEffect.SetActive(true);

        isActive = true;
        OnActivate();

        yield return new WaitForSeconds(activeTime);

        OnDeactivate();

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (ObjectPoolManager.Instance != null)
        {
            // 붙어있는 PoolObject를 가져옴
            PoolObject po = GetComponent<PoolObject>();

            if (po != null)
            {
                // PoolObject 타입으로 반납
                ObjectPoolManager.Instance.ReturnObject(po);
            }
            else
            {
                // PoolObject가 없으면 매니저 규칙 위반이므로 그냥 파괴
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }

    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
}