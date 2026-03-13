using UnityEngine;


public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public PlayerLoadout Loadout { get; private set; }


    private void Awake()
    {
        #region 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        Loadout = new PlayerLoadout();

    }
}
