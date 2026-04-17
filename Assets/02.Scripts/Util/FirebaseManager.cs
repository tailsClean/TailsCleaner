using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using System;


public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance { get => instance; private set => instance = value;}

    private DatabaseReference db;
    public DatabaseReference DB => db;
    public string UID => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
    [SerializeField] private AsyncEventChannelSO _onGameStart;
    [SerializeField] private AsyncEventChannelSO _onGameEnd;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        
        
    }
    public void InitDB()
    {
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        db = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("InitDB 완료");
    }

    public async Task Load()
    {
        if(UID == null)
        {
            Debug.LogError("사용자 정보가 존재하지 않습니다.");
            return;
        }
        await _onGameStart.OnStartEvent();

    }
    public void AddLoadData(Func<Task> task) =>_onGameStart.AddListener(task);
    public void RemoveLoadData(Func<Task> task) => _onGameStart.RemoveListener(task);

    public void AddSaveData(Func<Task> task) => _onGameEnd.AddListener(task);
    public void RemoveSaveData(Func<Task> task) => _onGameEnd.RemoveListener(task);

    private async void OnApplicationQuit() 
    {
        await _onGameEnd.OnStartEvent();
    }

    private async void OnApplicationPause(bool pause) 
    {
        if(pause) await _onGameEnd.OnStartEvent();
    }
   

}
