using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    [Header("로그인 패널")]
    [SerializeField] private GameObject _login1;
    [SerializeField] private GameObject _login2;

    [Header("------------------------------------------------------")]

    [Header("체크박스")]
    [SerializeField] private CheckBoxSprites _chekBoxSprites;
    [SerializeField] private List<CheckBox> _checkBoxes;

    private void Awake()
    {
        foreach(var checkBox in _checkBoxes)
        {
            checkBox.checkBoxSprites = _chekBoxSprites;
        }
    }

    private void OnEnable()
    {
        _login2?.SetActive(false);
    }

    private void Start()
    {
        SetButton();
    }

    private void OnDestroy()
    {
        foreach (var checkBox in _checkBoxes)
        {
            checkBox.button.onClick.RemoveAllListeners();
        }
    }


    private void SetButton()
    {
        foreach(var checkBox in _checkBoxes)
        {
            checkBox.button.onClick.AddListener(checkBox.SetImg);
        }
    }


    [Serializable]
    private class CheckBoxSprites
    {
        public Sprite CheckImg;
        public Sprite UnDoCheckImg;
    }

    [Serializable]
    private class CheckBox
    {
        public Button button;
        public Image image;
        public bool isChecked = false;

        public CheckBoxSprites checkBoxSprites { get; set; }


        public void SetImg() 
        {
            if (checkBoxSprites == null)
                return;

            if (!isChecked)
                image.sprite = checkBoxSprites.CheckImg;
            else
                image.sprite = checkBoxSprites.UnDoCheckImg;
        }
    }


    private void OnValidate()
    {
        if (_checkBoxes == null)
            return;

        foreach(var checkBox in _checkBoxes)
        {
            if(checkBox.button != null)
                checkBox.image = checkBox.button.GetComponent<Image>();
        }
    }
}
