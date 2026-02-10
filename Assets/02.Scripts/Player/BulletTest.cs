using UnityEngine;

public class BulletTest : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5;

    private Vector2 _dir;


    void Update()
    {
        transform.Translate(_dir*  Time.deltaTime * _moveSpeed);
        Destroy(gameObject, 5f);
    }

    public void Spawn(Vector2 dir)
    {
        _dir = dir.normalized;
    }
}
