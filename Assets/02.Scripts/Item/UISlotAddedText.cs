using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UISlotAddedText : UISlot
{
    [Header("추가 텍스트")]
    [SerializeField] private List<TextTypeBundle> _textList;


    /// <summary>
    /// 추가 텍스트 출력 메서드
    /// </summary>
    /// <param name="textType"></param>
    /// <param name="value"></param>
    public void SetAddedText(TEXT_TYPE textType, string value = null)
    {
        foreach (var textBundle in _textList)
        {
            if(textBundle.textType == textType)
            {
                textBundle.SetText(value);
                return;
            }
        }

        Debug.Log($"<color=yellow>{textType}의 텍스트를 찾지 못했습니다.</color>", this);
    }



    [Serializable]
    public class TextTypeBundle
    {
        public TextMeshProUGUI text;
        public TEXT_TYPE textType;

        public void SetText(string value) => text.text = value;
    }



    public enum TEXT_TYPE
    {
        Name, Desc
    }
}