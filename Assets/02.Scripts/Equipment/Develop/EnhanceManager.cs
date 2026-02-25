using UnityEngine;
using UnityEngine.UI;

public class EnhanceManager : MonoBehaviour
{
    [SerializeField] private EquipmentBase _enhanceEquipment;
    [SerializeField] private Image _image;
    [SerializeField] private Button _enhanceButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Text _moneyText;
    [SerializeField] private Text _costText;
    [SerializeField] private Text _enhanceLevelText;

    private bool _isEnhancable;
    
    //
    private int _money = 100;
    //

    private void Update()
    {
        
        _moneyText.text = "재화: " + _money.ToString();

        if (_enhanceEquipment != null)
        {
            _image.sprite = _enhanceEquipment.SpriteImage;
            _costText.text = "비용: " + _enhanceEquipment.EnhanceCostGold.ToString();
            _enhanceLevelText.text = "강화수치: " + _enhanceEquipment.EnhanceLevel.ToString();
        }
        else
        {
            _costText.text = "비용";
            _enhanceLevelText.text = "강화수치: 0";
        }
    }


    public void OnEnhance()
    {
        CheckCost();

        if (_isEnhancable)
            CheckMaxLevel();

        if(_isEnhancable)
        {
            SpendCost();
            _enhanceEquipment.OnEnhance();
            Debug.Log("강화성공!");
        }    
    }

    // 재화와 코스트 비교
    private void CheckCost()
    {
        int cost = _enhanceEquipment.EnhanceCostGold;
        _isEnhancable = _money >= cost;

        if(!_isEnhancable)
            Debug.Log("강화 비용이 부족합니다.");
    }

    // 최대 강화수치인지 확인
    private void CheckMaxLevel()
    {
        _isEnhancable = !_enhanceEquipment.IsEnhanceMaxLevel;

        if (!_isEnhancable)
            Debug.Log("강화수치가 최대입니다.");
    }

    // 코스트 소모
    private void SpendCost() => _money -= _enhanceEquipment.EnhanceCostGold;
    public void OnBack() => Debug.Log("뒤로 돌아가기 눌림");
}
