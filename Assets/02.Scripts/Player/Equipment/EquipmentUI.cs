using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private GameObject _playerObj;

    private IEquipmentable _player;
    private Dictionary<Equipment.PARTS, Equipment> _equipments;
    public Image _weaponImage;
    public Image _hatImage;
    public Image _cloakImage;
    public Image _shoseImage;

    private void Awake()
    {
        _player = _playerObj.GetComponent<IEquipmentable>();
        if (_player == null)
            Debug.LogWarning("UI출력을 위한 플레이어가 제대로 세팅되지 않음");
    }

    private void OnEnable()
    {
        _player.OnSetEquipment += UpdateImage;
    }

    private void OnDisable()
    {
        _player.OnSetEquipment -= UpdateImage;
    }

    private void Start()
    {
        _equipments = _player.MyEquipment;

        foreach (var equip in _equipments)
            UpdateImage(equip.Key);
    }

    // UI갱신 메서드
    private void UpdateImage(Equipment.PARTS equipmentType)
    {
        var equipment = _player.MyEquipment[equipmentType];

        switch (equipmentType)
        {
            case Equipment.PARTS.Weapon:
                _weaponImage.sprite = equipment.SpriteImage;
                break;
            case Equipment.PARTS.Hat:
                _hatImage.sprite = equipment.SpriteImage;
                break;
            case Equipment.PARTS.Shoes:
                _shoseImage.sprite = equipment.SpriteImage;
                break;
            case Equipment.PARTS.Cloak:
                _cloakImage.sprite = equipment.SpriteImage;
                break;
        }

    }
}
