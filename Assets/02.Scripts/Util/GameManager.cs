using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; private set => instance = value;}

    public EnergySystem _energySystem;

    public static int EnergyCount;
    public const int SPEND_ENERGY = 1;
    
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        EnergyCount = _energySystem.MaxEnergy;
    }

    public void EnterStage()
    {
        if(_energySystem.IsStartInGame)
        {
            _energySystem.SpendEnergy(SPEND_ENERGY);
            EnergyCount = _energySystem.CurrentEnergy;
            UIManager.Instance.GoToStage();
        }
        else
        {
            Debug.Log($"에너지가 부족합니다. 현재 에너지: {_energySystem.CurrentEnergy}");
        }
    }

}
