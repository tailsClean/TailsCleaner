using UnityEngine;

public class DungeonSlot : MonoBehaviour
{
    private TowerTable _towerTable;
    private int _towerId;
    private int _needStageId;
    private string _towerIconResource;

    public void ChangeTowerTable(TowerTable towerTable)
    {
        _towerTable = towerTable;
        RefreshTower();
    }

    public void RefreshTower()
    {
        _towerId = _towerTable.tower_id;
        _needStageId = _towerTable.need_stage_id;
        _towerIconResource = _towerTable.tower_icon_resource;
    }
}
