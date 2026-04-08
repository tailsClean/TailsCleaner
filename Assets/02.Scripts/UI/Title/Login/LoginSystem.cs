using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Extensions;
using Firebase.Database;

public class LoginSystem : MonoBehaviour
{
    [Header("로그인 패널")]
    [SerializeField] private GameObject _login1;
    [SerializeField] private GameObject _login2;

    [Header("로딩 화면")]
    [SerializeField] private GameObject _loadingScreen;

    [Header("------------------------------------------------------")]

    [Header("체크박스")]
    [SerializeField] private CheckBoxSprites _chekBoxSprites;
    [SerializeField] private List<CheckBox> _checkBoxes;

    [Header("로그인 버튼")]
    [SerializeField] private Button _loginButton;
    [SerializeField] Button _googleLoginBtn;
    [SerializeField] Button _guestLoginBtn;
    [SerializeField] Button _enterBtn;
    [SerializeField] Button _logoutBtn;

    private static bool _isFirebaseReady = false;
    private static bool _isFirebaseInitializing = false;
    private static FirebaseAuth _auth;
    private static event Action OnFirebaseReady;

    private const string WEB_CLIENT_ID = "769814245650-db36h61fdh23dv03gbj5atkgk47ldhgq.apps.googleusercontent.com";
    private bool _isLoggedIn;

   private void Awake()
    {
        _loginButton.interactable = false;

        foreach (var checkBox in _checkBoxes)
        {
            checkBox.checkBoxSprites = _chekBoxSprites;
            checkBox.SetAction(ActiveLogin);
        }

        
        _googleLoginBtn.onClick.AddListener(OnGoogleLogin);
        _guestLoginBtn.onClick.AddListener(OnGuestLogin);
        _enterBtn.onClick.AddListener(OnEnterBtn);
        _logoutBtn.onClick.AddListener(OnLogout);

        if (_isFirebaseReady)
        {
            SetupAuth();
            return;
        }

        OnFirebaseReady += SetupAuth; 

        if (_isFirebaseInitializing) return;

        _isFirebaseInitializing = true;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            _isFirebaseInitializing = false;
            if (task.Result == DependencyStatus.Available)
            {
                
                _auth = FirebaseAuth.DefaultInstance;
                _isFirebaseReady = true;
            }

            OnFirebaseReady?.Invoke(); 
            OnFirebaseReady = null;
        });
    }

    private void SetupAuth()
    {
        if (_auth == null) return; // 혹시 모를 null 방어

        _auth.StateChanged += ChangeLoginState;
        _isLoggedIn = _auth.CurrentUser != null;
        _logoutBtn.gameObject.SetActive(_isLoggedIn);
    }

    private void OnEnable()
    {
        _login2?.SetActive(false);
        _login1.SetActive(false);
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
        _auth.StateChanged -= ChangeLoginState;
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

    private void OnEnterBtn()
    {
        if (_isLoggedIn)
        {
            FirebaseManager.Instance.InitDB();
            GameManager.Instance.InitDB();
            _loadingScreen.SetActive(true);   
        }
        else
           _login1.SetActive(true);
    }

      private void OnGoogleLogin()
    {
        _googleLoginBtn.interactable = false;

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = WEB_CLIENT_ID,
            RequestIdToken = true,
            RequestEmail = true
        };

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
    }

     private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            var ex = task.Exception?.InnerException;
            if (ex != null)
            {
                var statusProp = ex.GetType().GetProperty("Status");
                var status = statusProp?.GetValue(ex);
                Debug.LogWarning($"구글 로그인 실패 Status: {status}");
            }
            
            _googleLoginBtn.interactable = true;
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

        _auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        {
            _googleLoginBtn.interactable = true;

            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                Debug.LogError($"Firebase 인증 실패: {authTask.Exception}");
                return;
            }

            FirebaseUser user = authTask.Result;
            //Debug.Log($"로그인 성공 | 이름: {user.DisplayName} | UID: {user.UserId}");     
        });
       
    }

    private void OnGuestLogin()
    {
        _guestLoginBtn.interactable = false;

        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            _guestLoginBtn.interactable = true;

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"게스트 로그인 실패: {task.Exception}");
                return;
            }

            Debug.Log($"게스트 로그인 성공 | UID: {task.Result.User.UserId}");
           
        });   
    }

    private void ChangeLoginState(object sender, EventArgs a )
    {
        _isLoggedIn = _auth.CurrentUser != null;
        _logoutBtn.gameObject.SetActive(_isLoggedIn);
        //Debug.Log($"StateChanged 호출 | _isLoggedIn: {_isLoggedIn}");
       
    }

    private async void OnLogout()
    {
        FirebaseUser user = _auth.CurrentUser;

        if (user != null && user.IsAnonymous)
        {
            var DB = FirebaseDatabase.DefaultInstance.RootReference;
             
             await DB.Child("users")
            .Child(user.UserId)
            .RemoveValueAsync();

            await _auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"게스트 계정 삭제 실패: {task.Exception}");
                    return;
                }

                Debug.Log("게스트 계정이 삭제되었습니다.");
            });
        }
        _auth.SignOut();
        gameObject.SetActive(true);
    }
}
