using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Text _lvl;
    [SerializeField] private Slider _expBar;
    [SerializeField] private PlayerBase _player;

    private void OnEnable()
    {
        _player.OnUpdateUI += UpdateHp;
        _player.OnGainExp += UpdateExp;
    }

    private void OnDisable()
    {
        _player.OnUpdateUI -= UpdateHp;
        _player.OnGainExp += UpdateExp;
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

    private void UpdateHp(float hp)
    {
        _hpBar.value = hp;
    }

    private void UpdateExp(float level, float exp)
    {
        _lvl.text = "레벨: " + level.ToString();
        _expBar.value = exp;
    }
}
