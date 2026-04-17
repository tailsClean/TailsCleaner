using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerLoadout
{

    // 고정 수치값
    private const int _relicSlotLength = 3;
    private readonly RelicBase _relicZero = new RelicBase();                    // 빈 유물 공간을 의미


    // 장착 장비 필드
    private Dictionary<PART, EquipmentBase> _myEquipments;

    // 유물 관련 필드
    private List<RelicBase> _myRelics;
    private List<RelicBase> _outputRelics;                                      // 외부 출력용 리스트
    private RelicDivisionSystem _relicDivisionSystem;

    private VoidEventChannelSO _onChangeLoadout;


    public Dictionary<PART, EquipmentBase> MyEquipments => _myEquipments;
    public List<RelicBase> MyRelics => _outputRelics;



    public PlayerLoadout(VoidEventChannelSO onChangeLoadout)
    {
        _onChangeLoadout = onChangeLoadout;

        _myRelics = new List<RelicBase>();
        _outputRelics = new List<RelicBase>();
        for (int i = 0; i < _relicSlotLength; i++)
        {
            _myRelics.Add(_relicZero);
        }
        _relicDivisionSystem = new RelicDivisionSystem(this);


        _myEquipments = new Dictionary<PART, EquipmentBase>
        {
            {PART.Weapon, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultWeapon)},
            {PART.Helmet, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultHat)},
            {PART.Cloak, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultCloak)},
            {PART.Shoes, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultShose)}
        };

        FirebaseManager.Instance.AddLoadData(LoadEquipment);
        FirebaseManager.Instance.AddSaveData(SaveEquipment);
    }


    #region 장비&유물 스탯 반환

    // 장비의 스텟 증가량을 반환
    public float GetIncreaseStat(EQUIP_STAT_TYPE stat)
    {
        if (_myEquipments == null)
            return 0;

        EquipmentBase equipment = null;
        switch (stat)
        {
            case EQUIP_STAT_TYPE.Attack:
                equipment = _myEquipments[PART.Weapon];
                break;

            case EQUIP_STAT_TYPE.CriticalRate:
                equipment = _myEquipments[PART.Helmet];
                break;

            case EQUIP_STAT_TYPE.MaxHP or EQUIP_STAT_TYPE.Defense:
                equipment = _myEquipments[PART.Cloak];
                break;

            case EQUIP_STAT_TYPE.MoveSpeed or EQUIP_STAT_TYPE.Dodge:
                equipment = _myEquipments[PART.Shoes];
                break;

            default:
                return 0;
        }

        return equipment.GetIncreaseStat(stat);
    }

    // 유물리스트의 스텟 증가량을 반환
    public float GetIncreaseStat(STAT_TYPE stat)
    {
        float result = 0;
        foreach (var relic in _myRelics)
        {
            if (relic == null || relic == _relicZero)
                continue;


            if (relic.Data.Relic.stat_type == stat)
                result += relic.Data.Relic.stat_value;
        }
        return result;
    }

    // 유물의 공명효과로 증가하는 스탯 증가값
    public float GetRelicDivisionValue(PLAYER_STAT stat)
    {
        float divisionValue = _relicDivisionSystem.GetIncreaseStat(out var statType);

        if (stat != statType)
            return 0;

        return divisionValue;
    }

    #endregion


    #region 유물창

    // 유물 장착 메서드
    public void SetRelic(ItemInstance item)
    {
        // 중복 착용 확인
        foreach(var checkRelic in _outputRelics)
        {
            if(checkRelic.Data.UniqueID == item.ID)
            {
                WarningText.ShowText("착용 중인 유물입니다.");
                Debug.Log("<color=yellow>착용 중인 유물입니다.</color>"); return;
            }
        }

        RelicBase relic = ItemDB.CreateItem<RelicBase>(item.ID);
        relic.SetEnhanceLevel(item.EnhanceLevel);
        for (int i = 0; i < _myRelics.Count; i++)
        {
            if (_myRelics[i] == _relicZero)
            {
                _myRelics[i] = relic;
                _outputRelics.Add(relic);

                WarningText.ShowText($"<color=yellow>{relic.Data.Name} 장착</color>");
                Debug.Log(relic.Data.Name + " 장착!");
                _onChangeLoadout.OnStartEvent();
                return;
            }
        }

        WarningText.ShowText("유물착용칸이 가득 찼습니다.");
        Debug.Log($"<color=yellow>유물칸이 꽉 차서 {item.Name} 장착 실패</color>");
    }

    // 유물 장착 해제 메서드
    public void RemoveRelic(int id, int enhanceLevel)
    {
        for (int i = 0; i < _myRelics.Count; i++)
        {
            var relic = _myRelics[i];
            if (relic != null && relic.Data.Relic.id == id && relic.CurrentEnhanceLevel == enhanceLevel)
            {
                _myRelics.RemoveAt(i);
                _outputRelics.Remove(relic);
                _myRelics.Add(_relicZero);
                _onChangeLoadout.OnStartEvent();
                WarningText.ShowText($"<color=yellow>{relic.Data.Name}이 착용해제 돠었습니다.</color>");
               
                return;
            }
        }
    }

    // 유물의 공명 효과가 활성화됐는지 여부
    public bool TryGetRelicDivision(out RELIC_TYPE divisionType)
    {
        bool isDivision = _relicDivisionSystem.IsDivisionActive;

        if (isDivision)
            divisionType = _relicDivisionSystem.CurrentDivision.division_type;
        else
            divisionType = (RELIC_TYPE)(-1);

        return isDivision;
    }

    #endregion

    #region Firebase 저장/로드
    public async Task SaveEquipment()
    {
        var equipData = new Dictionary<string, object>();
        var relicData = new Dictionary<string, object>();

        foreach (var kvp in _myEquipments)
        {
            equipData[kvp.Key.ToString()] = new Dictionary<string, object>
            {
                { "id", kvp.Value.Data.UniqueID },
                { "enhanceLevel", kvp.Value.EnhanceLevel },
                { "grade", (int)kvp.Value.CurrentGrade }
            };
        }

        for (int i = 0; i < _myRelics.Count; i++)
        {
            if (_myRelics[i] == _relicZero) continue;

            relicData[i.ToString()] = new Dictionary<string, object>
            {
                { "id", _myRelics[i].Data.UniqueID },
                { "enhanceLevel", _myRelics[i].CurrentEnhanceLevel }
            };
        }

        var inventoryData = new Dictionary<string, object>
        {
            { "Equipments", equipData },
            { "Relics", relicData }
        };

        await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Equipment")
            .SetValueAsync(inventoryData);
    }
    
    

    public async Task LoadEquipment()
    {
        var snapshot = await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Equipment")
            .GetValueAsync();

        if (!snapshot.Exists) return;

        // 장비 로드
        var equipSnapshot = snapshot.Child("Equipments");
        foreach (PART part in System.Enum.GetValues(typeof(PART)))
        {
            var child = equipSnapshot.Child(part.ToString());
            if (!child.Exists) continue;

            int id = int.Parse(child.Child("id").Value.ToString());
            int enhanceLevel = int.Parse(child.Child("enhanceLevel").Value.ToString());
            GRADE grade = (GRADE)int.Parse(child.Child("grade").Value.ToString());

            // SO에서 데이터 가져와서 아이템 생성
            var equipment = ItemDB.CreateItem<EquipmentBase>(id);
            equipment.SetEnhanceLevel(enhanceLevel);
            equipment.SetGrade(grade);               

            _myEquipments[part] = equipment;
        }

        // 유물 로드
        var relicSnapshot = snapshot.Child("Relics");
        for (int i = 0; i < _relicSlotLength; i++)
        {
            var child = relicSnapshot.Child(i.ToString());
            if (!child.Exists) continue;

            int id = int.Parse(child.Child("id").Value.ToString());
            int enhanceLevel = int.Parse(child.Child("enhanceLevel").Value.ToString());

            var relic = ItemDB.CreateItem<RelicBase>(id);
            relic.SetEnhanceLevel(enhanceLevel); 
            _myRelics[i] = relic;
            _outputRelics.Add(relic);
        }

        _onChangeLoadout.OnStartEvent();
    }


    #endregion
}