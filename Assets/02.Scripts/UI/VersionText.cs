using UnityEngine;
using TMPro;
using UnityEditor;
public class VersionText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    
    void Awake()
    {
        string version = Application.version;
        tmp.text = "ver" + version + ": " ;    
    }
}
