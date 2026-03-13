using UnityEngine;

public class SafeZone : AreaEffector
{
    private PlayerZoneHandler playerHandler;
    protected override void OnActivate()
    {
        // 활성화 시점에 플레이어에게 패턴 시작 알림
        playerHandler = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerZoneHandler>();

        if (playerHandler != null)
        {
            playerHandler.RegisterSafeZonePattern(true);
        }
    }
    protected override void OnDeactivate()
    {
        // 소멸 시점에 패턴 종료 알림
        if (playerHandler != null)
        {
            playerHandler.RegisterSafeZonePattern(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerZoneHandler>().EnterSafeZone();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerZoneHandler>().ExitSafeZone();
    }
}