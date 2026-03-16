using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private TextMeshProUGUI _lvl;
    [SerializeField] private Slider _expBar;
    [SerializeField] private PlayerBase _player;
    [SerializeField] private FloatEventChannelSO _onHit;
    [SerializeField] private FloatEventChannelSO _onGainExp;
    [SerializeField] private IntEventChannelSO _onLevelUp;

    private void OnEnable() 
    {
        _onHit.AddListener(UpdateHp);
        _onGainExp.AddListener(UpdateExp);
        _onLevelUp.AddListener(UpdateLevel);
        _player = FindAnyObjectByType<PlayerBase>();
    }

    private void OnDisable()
    {
        _onHit.RemoveListener(UpdateHp);
        _onGainExp.RemoveListener(UpdateExp);
        _onLevelUp.RemoveListener(UpdateLevel);
    }

    private void Start()
    {
        _hpBar.maxValue = _player.MaxHp;
        _hpBar.value = _player.MaxHp;
        _expBar.maxValue = _player.InGameMaxExp;
        _lvl.text = "레벨: 1";

        if (!_player)
            Debug.LogError("플레이어 넣어주세요");
    }

    private void UpdateHp(float hp)
    {
        _hpBar.maxValue = _player.MaxHp;
        _hpBar.value = hp;
    }

    private void UpdateExp(float exp) => _expBar.value = exp;
    private void UpdateLevel(int level)
    {
        _lvl.text = "레벨: " + level.ToString();
        _expBar.maxValue = _player.InGameMaxExp;
    }
}
