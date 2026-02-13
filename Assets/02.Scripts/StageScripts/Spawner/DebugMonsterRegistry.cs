using UnityEngine;
using System.Collections.Generic;

public class DebugMonsterRegistry : MonoBehaviour, IMonsterRegistry
{
    private List<GameObject> _monsters = new List<GameObject>();

    //SpawnSystem에서 몬스터가 생성될 때마다 이 레지스트리에 등록
    //현재는 게임 오브젝트로 작업하나, 실제 구현에서는 몬스터 클래스로 변경할 예정

    public void RegisterMonster(GameObject monster)
    {
        if(monster == null)
        {
            return;
        }

        _monsters.Add(monster);
    }

    public void KillAllMonsters()
    {
        Debug.Log("KillAllMonsters called. Total monsters: " + _monsters.Count);

        for(int i = 0; i < _monsters.Count; i++)
        {
            GameObject obj = _monsters[i];
            if(obj != null)
            {
               Destroy(obj);
            }
        }   

        _monsters.Clear();
    }
}
