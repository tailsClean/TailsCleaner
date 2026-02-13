using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private GameObject _playerObj;

    private IEquipmentable _player;
    private Dictionary<PlayerBase.EQUIPMENT, PlayerEquipment> _equipments;
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _player = _playerObj.GetComponent<IEquipmentable>();
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
    private void UpdateImage(PlayerBase.EQUIPMENT equipmentType)
    {
        var equipments = _player.MyEquipment;

        _image.sprite = equipments[equipmentType].SpriteImage;
    }
}
