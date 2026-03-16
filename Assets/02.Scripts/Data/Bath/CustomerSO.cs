
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerSO", menuName = "Data/CustomerSO")]
public class CustomerSO : ScriptableObject
{
    public List<Customer> dataList = new List<Customer>();

    public Customer GetById(int id)
        => dataList.Find(x => x.monster_id == id);
}
