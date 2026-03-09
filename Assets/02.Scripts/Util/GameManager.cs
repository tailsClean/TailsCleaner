using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; private set => instance = value;}


    public EnergySystem _energySystem;

    public static int EnergyCount;
    public const int SPEND_ENERGY = 1;
    public int _maxEnergy;

    
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
        _maxEnergy = _energySystem.MaxEnergy;
        EnergyCount = _maxEnergy;
        
    }

    public void EnterStage()
    {
        if(_energySystem.IsStartInGame)
        {
            UIManager.Instance.GoToStage();
        }
    }

    public void UpdateEnergyCount(int energy)
    {
        EnergyCount = energy;
        UIManager.Instance.EnergyPanel?.UpdateEnergyText();
    }

}
