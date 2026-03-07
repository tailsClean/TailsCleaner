using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnhancementUI : MonoBehaviour
{
    [SerializeField] private GameObject _playerObj;

    private PlayerEnhancementSelector _player;
    private Dictionary<EQUIP_PARTS, EquipmentBase> _equipments;
    public Image _weaponImage;
    public Image _hatImage;
    public Image _cloakImage;
    public Image _shoseImage;
    public Image[] _relicSlot;

    public Sprite baseImg;

    private void Awake()
    {
        _player = _playerObj.GetComponent<PlayerEnhancementSelector>();
        if (_player == null)
            Debug.LogWarning("UI출력을 위한 플레이어가 제대로 세팅되지 않음");
    }

    private void OnEnable()
    {
        _player.OnSetEquipment += UpdateImage;
        _player.OnSetRelic += UpdateImage;
    }

    private void OnDisable()
    {
        _player.OnSetEquipment -= UpdateImage;
    }

    private void Start()
    {
        _equipments = _player.MyEquipment;

        foreach (var equip in _equipments)
            UpdateImage((EquipmentBase.PARTS)equip.Key);
    }


    // UI갱신 메서드
    private void UpdateImage(EquipmentBase.PARTS equipmentType)
    {
        var equipment = _player.MyEquipment[(EQUIP_PARTS) equipmentType];

        switch (equipmentType)
        {
            case EquipmentBase.PARTS.Weapon:
                _weaponImage.sprite = equipment.SpriteImage;
                break;
            case EquipmentBase.PARTS.Hat:
                _hatImage.sprite = equipment.SpriteImage;
                break;
            case EquipmentBase.PARTS.Shoes:
                _shoseImage.sprite = equipment.SpriteImage;
                break;
            case EquipmentBase.PARTS.Cloak:
                _cloakImage.sprite = equipment.SpriteImage;
                break;
        }
    }

    private void UpdateImage()
    {
        var relics = _player.MyRelic;
        for(int i = 0; i < relics.Count; i++)
        {
            if(relics[i] == null)
            {
                _relicSlot[i].sprite = baseImg;
                continue;
            }

            _relicSlot[i].sprite = relics[i].SpriteImage;
        }
    }
}
