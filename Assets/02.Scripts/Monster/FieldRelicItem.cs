using UnityEngine;

public class FieldRelicItem : MonoBehaviour
{
    private int _relicId;

    public void Setup(int id)
    {
        _relicId = id;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ItemManager나 Inventory에 유물을 추가하는 함수
            if (ItemManager.Instance != null)
            {
                // 이 시점에 인벤토리에 등록되어야 다음 드랍 시 중복 체크가 작동
                ItemManager.Instance.Inventory.AddRelic(_relicId);
                Debug.Log($"[Relic] ID:{_relicId} 유물 획득 완료!");
            }

            // 풀로 반납
            var poolObj = GetComponent<PoolObject>();
            if (poolObj != null)
                ObjectPoolManager.Instance.ReturnObject(poolObj);
            else
                Destroy(gameObject);
        }
    }
}