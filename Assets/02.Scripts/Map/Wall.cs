using UnityEngine;

public class wall : MonoBehaviour
{
    [SerializeField] Collider2D col;
  

    void OnTriggerExit2D(Collider2D collision)
    {
        Bounds b = col.bounds;
        Vector3 pos = collision.transform.position;

        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

        collision.transform.position = pos;
    }

}
