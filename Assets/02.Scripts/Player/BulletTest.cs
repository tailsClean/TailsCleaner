using UnityEngine;

public class BulletTest : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;

    void Update()
    {
        //transform.Translate(  Time.deltaTime * _moveSpeed);
        Destroy(gameObject, 5f);
    }
}
