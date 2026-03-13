using System.Collections.Generic;
using UnityEngine;

public class DangerZone : AreaEffector
{
    public float damagePerTick = 10f;

    protected override void OnActivate() { }
    protected override void OnDeactivate() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerZoneHandler>().EnterDangerZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var handler = other.GetComponent<PlayerZoneHandler>();

            // 1핸들러가 존재하는지 확인
            // 오브젝트가 아직 파괴되지 않았는지 확인
            if (handler != null)
            {
                handler.ExitDangerZone(this);
            }
        }
    }
}
