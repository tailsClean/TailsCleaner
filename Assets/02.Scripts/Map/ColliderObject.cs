using UnityEngine;

public class ColliderObject : MonoBehaviour
{

    [SerializeField] Collider2D col;
    private void Start()
    {
        col = GetComponent<Collider2D>();
    }
    private Vector3 _pushDir;

     void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        UpdatePushDir(collision.transform.position);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Bounds b = col.bounds;
        Vector3 pos = collision.transform.position;

        if (!b.Contains(pos)) return;

        float distLeft = Mathf.Abs(pos.x - b.min.x);
        float distRight = Mathf.Abs(pos.x - b.max.x);
        float distBottom = Mathf.Abs(pos.y - b.min.y);
        float distTop = Mathf.Abs(pos.y - b.max.y);

        float minX = Mathf.Min(distLeft, distRight);
        float minY = Mathf.Min(distBottom, distTop);

        float threshold = 0.2f; // X, Y 거리 차이가 이 값 이하면 코너로 판단
        if (Mathf.Abs(minX - minY) < threshold)
            UpdatePushDir(pos); // 코너에서만 방향 업데이트

        if (_pushDir == Vector3.left) pos.x = b.min.x - 0.01f;
        else if (_pushDir == Vector3.right) pos.x = b.max.x + 0.01f;
        else if (_pushDir == Vector3.down) pos.y = b.min.y - 0.01f;
        else if (_pushDir == Vector3.up) pos.y = b.max.y + 0.01f;

        collision.transform.position = pos;
    }

    private void UpdatePushDir(Vector3 pos)
    {
        Bounds b = col.bounds;

        float distLeft = Mathf.Abs(pos.x - b.min.x);
        float distRight = Mathf.Abs(pos.x - b.max.x);
        float distBottom = Mathf.Abs(pos.y - b.min.y);
        float distTop = Mathf.Abs(pos.y - b.max.y);

        float minX = Mathf.Min(distLeft, distRight);
        float minY = Mathf.Min(distBottom, distTop);

        if (minX < minY)
            _pushDir = distLeft < distRight ? Vector3.left : Vector3.right;
        else
            _pushDir = distBottom < distTop ? Vector3.down : Vector3.up;
    }


}
