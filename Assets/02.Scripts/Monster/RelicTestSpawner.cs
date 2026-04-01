using UnityEngine;
using UnityEngine.InputSystem;
public class RelicTestSpawner : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private GameObject _relicPrefab; // Project 창에 있는 Relic 프리팹 연결
    [SerializeField] private int _testRelicId = 1;    // 소환해볼 특정 유물 ID

    [Header("소환 위치")]
    [SerializeField] private Transform _spawnPoint;   // 소환될 위치 (미지정 시 스패너 위치)

    void Update()
    {
        // Keyboard.current가 null인지 체크하는 것이 안전합니다.
        if (Keyboard.current == null) return;

        // 1. 숫자 9 키 체크 (Input.GetKeyDown 대신 사용)
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            SpawnTestRelic(_testRelicId);
        }

        // 2. 숫자 0 키 체크
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            // 실제 데이터 범위에 맞춰 Random 범위를 조절하세요.
            int randomId = Random.Range(1, 6);
            SpawnTestRelic(randomId);
        }
    }

    public void SpawnTestRelic(int id)
    {
        if (_relicPrefab == null)
        {
            Debug.LogError("[Test] 유물 프리팹이 연결되지 않았습니다! Inspector를 확인하세요.");
            return;
        }

        // 1. 단순 Instantiate 사용
        GameObject relicObj = Instantiate(_relicPrefab);

        // 2. 위치 설정
        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : transform.position;
        relicObj.transform.position = pos;

        // 3. Setup 호출하여 데이터 및 이미지 적용
        var relicItem = relicObj.GetComponent<FieldRelicItem>();
        if (relicItem != null)
        {
            relicItem.Setup(id);
            Debug.Log($"[Test] 유물 ID:{id} 소환 완료 (Instantiate)");
        }
        else
        {
            Debug.LogError("[Test] 프리팹에 FieldRelicItem 컴포넌트가 없습니다!");
        }
    }
}