using UnityEngine;

#if UNITY_EDITOR
public class UISetter : MonoBehaviour
{
    [SerializeField] private float Horizental;
    [SerializeField] private float Virtical;
    [SerializeField] private float X;
    [SerializeField] private float Y;

    private RectTransform _mine;

    private void OnValidate()
    {
        if(_mine == null)
        {
            _mine = GetComponent<RectTransform>();
            return;
        }

        if(Horizental == 0 ||  Virtical == 0)
            return;

        _mine.sizeDelta = new Vector2(Horizental, Virtical);

        float x = Horizental / 2 + X;
        float y = Virtical / 2 + Y;

        _mine.position = new Vector2(x, 1080 - y);
    }
}
#endif
