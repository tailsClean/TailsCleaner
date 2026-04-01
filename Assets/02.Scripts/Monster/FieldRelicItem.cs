using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FieldRelicItem : MonoBehaviour
{
    [Header("유물 설정")]
    [SerializeField] private int _relicId;

    [Header("렌더링")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        // 인스펙터 연결을 깜빡했을 경우를 대비한 자동 할당
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(int id)
    {
        _relicId = id;

        var relicTable = DataManager.Instance.GetSOData<RelicSO>();
        var relicData = relicTable?.GetById(id);

        if (relicData == null)
        {
            // Debug.LogError($"[Relic] ID {id} 데이터가 테이블에 없습니다!");
            return;
        }

        // 이전 이미지 초기화 (풀링에서 재사용될 때 잔상 방지)
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = null;
        }

        // 주소값 정리
        string address = relicData.sprite.Trim();
        // Debug.Log($"[Relic] 데이터 검증 - ID: {id} | 이름: [{address}]");

        // 3. Addressables 비동기 로드
        if (_spriteRenderer != null)
        {
            Addressables.LoadAssetAsync<Sprite>(address).Completed += (handle) =>
            {
                // 생성된 오브젝트가 로드 완료 시점에 이미 풀로 돌아갔을 경우를 대비
                if (this == null || !gameObject.activeInHierarchy)
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                        Addressables.Release(handle); // 메모리 해제
                    return;
                }

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _spriteRenderer.sprite = handle.Result;
                    // Debug.Log($"<color=green> [Addressables] '{address}' 로드 성공!</color>");

                    // 레이어/Z축 문제 확인을 위한 강제 활성화 체크
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.color = Color.white;
                }
                else
                {
                    // Debug.LogError($"<color=red> [Addressables] '{address}' 로드 실패!</color> 에러: {handle.OperationException}");
                }
            };
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.Inventory.AddRelic(_relicId);
                Debug.Log($"[Relic] ID:{_relicId} 유물 획득 완료!");
            }

            // 반납 시 스프라이트 비워주기
            if (_spriteRenderer != null) _spriteRenderer.sprite = null;

            var poolObj = GetComponent<PoolObject>();
            if (poolObj != null)
                ObjectPoolManager.Instance.ReturnObject(poolObj);
            else
                Destroy(gameObject);
        }
    }
}