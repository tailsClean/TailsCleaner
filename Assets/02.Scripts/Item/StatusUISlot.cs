using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class StatusUISlot : UISlot
{
    [Header("추가 텍스트")]
    [SerializeField] private List<TextMeshProUGUI> _textList;


    // 슬롯에 특정 아이템을 넣으면 자동으로 아이템의 정보를 UI로 출력
    public void SetSlots(TEXT_TYPE textType, string value = null) =>
         _textList[(int)textType].text = value;


    // int를 통해서도 값을 출력할 수 있도록 오버로딩
    public void SetSlots(TEXT_TYPE textType, int value) => 
        _textList[(int)textType].text = value.ToString();



    public enum TEXT_TYPE
    {
        Name, Desc
    }
}