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

    [Header("로그인 버튼")]
    [SerializeField] private Button _loginButton;

    private void Awake()
    {
        _loginButton.interactable = false;
        foreach (var checkBox in _checkBoxes)
        {
            checkBox.checkBoxSprites = _chekBoxSprites;
            checkBox.SetAction(ActiveLogin);
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

    private void ActiveLogin()
    {
        if (_checkBoxes == null)
            return;
        foreach(var checkbox in _checkBoxes)
        {
            if (!checkbox.isChecked)
            {
                _loginButton.interactable = false;
                return;
            }
        }

        if(_loginButton == null)
        { Debug.LogError("로그인 버튼 넣어라"); return; }
        _loginButton.interactable = true;
    }


    [Serializable]
    private class CheckBoxSprites
    {
        public Sprite CheckImg;
        public Sprite UnCheckImg;
    }

    [Serializable]
    private class CheckBox
    {
        public Button button;
        public Image image;
        public bool isChecked = false;

        public CheckBoxSprites checkBoxSprites { get; set;  }

        private event Action _onChecking;


        public void SetImg() 
        {
            if (checkBoxSprites == null)
                return;

            if (!isChecked)
                image.sprite = checkBoxSprites.CheckImg;
            else
                image.sprite = checkBoxSprites.UnCheckImg;

            isChecked = image.sprite == checkBoxSprites.CheckImg;

            _onChecking?.Invoke();
        }

        public void SetAction(Action action)
        {
            _onChecking = null;
            _onChecking += action;
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
