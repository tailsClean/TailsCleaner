using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Text _lvl;
    [SerializeField] private Slider _expBar;
    [SerializeField] private PlayerBase _player;
    [SerializeField] private IntEventChannelSO _onHit;
    [SerializeField] private IntEventChannelSO _onGainExp;
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
        _hpBar.maxValue = _player.Hp;
        _hpBar.value = _player.Hp;
        _expBar.maxValue = 50;
        _lvl.text = "레벨: 1";

        if (!_player)
            Debug.LogError("플레이어 넣어주세요");
    }

    private void UpdateHp(int hp)
    {
        _hpBar.value = hp;
    }

    private void UpdateExp(int exp) => _expBar.value = exp;
    private void UpdateLevel(int level) => _lvl.text = "레벨: " + level.ToString();
}
