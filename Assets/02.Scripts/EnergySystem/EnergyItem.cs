using UnityEngine;

public class EnergyItem : MonoBehaviour
{
    [SerializeField] private int _energyRecoveryAmount;
    [SerializeField] private IntEventChannelSO _onIncreaseEnergy;

    [ContextMenu("아이템 사용")]
    public void UseItem() => _onIncreaseEnergy.OnStartEvent(_energyRecoveryAmount);
}
