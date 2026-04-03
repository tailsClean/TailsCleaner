using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : MonoBehaviour
{
    [SerializeField]private Slider _bossHPBar;
    [SerializeField] private FloatEventChannelSO _onBossHit;
    private BossMonster _currentBoss;

    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        _currentBoss = FindAnyObjectByType<BossMonster>();
        _onBossHit.AddListener(UpdateHp);
        UpdateHp(_currentBoss.MaxHp);
    }
    void OnDisable()
    {
        _onBossHit.RemoveListener(UpdateHp);
    }

    private void UpdateHp(float hp)
    {
        _bossHPBar.maxValue = _currentBoss.MaxHp;
        _bossHPBar.value = hp;
    }
}
