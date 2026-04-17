using UnityEngine;
using TMPro;

public class VersionText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    
    void Awake()
    {
        string version = Application.version;
        tmp.text = "ver " + version;    
    }
}
