using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    [SerializeField] Button _googleLoginBtn;
    [SerializeField] Button _guestLoginBtn;

    private const string WebClientId = "769814245650-db36h61fdh23dv03gbj5atkgk47ldhgq.apps.googleusercontent.com";
    private FirebaseAuth _auth;

    private void Awake()
    {
        _googleLoginBtn.onClick.AddListener(OnGoogleLogin);
        _guestLoginBtn.onClick.AddListener(OnGuestLogin);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {task.Result}");
            }
        });
    }

    private void OnGoogleLogin()
    {
        _googleLoginBtn.interactable = false;

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = WebClientId,
            RequestIdToken = true,
            RequestEmail = true
        };

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
    }

    private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogWarning("구글 로그인 실패 또는 취소");
            _googleLoginBtn.interactable = true;
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

        _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
        {
            _googleLoginBtn.interactable = true;

            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                Debug.LogError($"Firebase 인증 실패: {authTask.Exception}");
                return;
            }

            FirebaseUser user = authTask.Result;
            Debug.Log($"로그인 성공 | 이름: {user.DisplayName} | UID: {user.UserId}");
            
        });
    }

    private void OnGuestLogin()
    {
        _guestLoginBtn.interactable = false;

        _auth.SignInAnonymouslyAsync().ContinueWith(task =>
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

   
}
