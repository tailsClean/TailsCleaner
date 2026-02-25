using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private EquipmentBase _upgradeEquipment;
    [SerializeField] private List<EquipmentBase> _materialEquipments;

    [Header("이미지 세팅값(변경안 해도 됨)")]
    [SerializeField] private Image _upgradeImage;
    [SerializeField] private List<Image> _materialImage;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Text _upgradeGradeText;
    [SerializeField] private Text _upgradePartsText;
    [SerializeField] private Text _materialCostCountText;

    private bool _isUpgradable;
    private Sprite _baseSprite;

    private void Awake()
    {
        _baseSprite = _materialImage[0].sprite;
    }



    private void Update()
    {
        if (_upgradeEquipment == null)
        {
            _upgradeImage.sprite = _baseSprite;
            return;
        }
        _upgradeImage.sprite = _upgradeEquipment.SpriteImage;
        _upgradeGradeText.text = "등급: " + _upgradeEquipment.Grade.ToString();
        _upgradePartsText.text = "부위: " + _upgradeEquipment.EquipmentPart.ToString();
        _materialCostCountText.text = "필요갯수: " + _upgradeEquipment.GradeCostCount.ToString();

        for (int i = 0; i < _materialImage.Count; i++)
        {
            if (_materialEquipments != null && i < _materialEquipments.Count && _materialEquipments[i] != null)
                _materialImage[i].sprite = _materialEquipments[i].SpriteImage;
            else
            {
                _materialImage[i].sprite = _baseSprite;
            }
        }
    }

    public void OnUpgrade()
    {
        CheckMaterialCount();

        CheckMaxGrade();

        CheckMaterialGrade();

        if (_isUpgradable)
        {
            _upgradeEquipment.OnUpgrade();
            Debug.Log("등급 업그레이드 성공!");
            ResetMaterials();
        }
    }

    private void CheckMaxGrade()
    {
        _isUpgradable = !_upgradeEquipment.IsGradeMaxGrade;
        if (!_isUpgradable)
            Debug.Log("최대 등급의 장비입니다.");
    }

    private void CheckMaterialGrade()
    {
        if (!_isUpgradable)
            return;

        foreach (var equip in  _materialEquipments)
        {
            if(equip == null)
                break;

            _isUpgradable = _upgradeEquipment.Grade == equip.Grade;
            if (!_isUpgradable)
                break;

            if (!CheckMaterialParts(equip))
                return;
        }

        if (!_isUpgradable)
            Debug.Log("재료 장비의 등급이 적합하지 않습니다.");
    }

    private bool CheckMaterialParts(EquipmentBase equip)
    {
        _isUpgradable = _upgradeEquipment.EquipmentPart == equip.EquipmentPart;

        if (!_isUpgradable)
            Debug.Log("재료 장비의 부위가 적합하지 않습니다.");
        return _isUpgradable;
    }

    private void CheckMaterialCount() => 
        _isUpgradable = _upgradeEquipment.GradeCostCount >= _materialEquipments.Count;

    private void ResetMaterials()
    {
        foreach(var equip in _materialEquipments)
        {
            if(equip != null)
                equip.Remove();
        }
    }
}
