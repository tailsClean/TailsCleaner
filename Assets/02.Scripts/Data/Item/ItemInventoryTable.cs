
using System;

[Serializable]
public class ItemInventoryTable
{
    public int inven_id;
    public ITEM_TYPE item_type;
    public int item_id;
    public int quantity;
    public int remain_time;
    public int slot_index;
    public string user_id;
}
