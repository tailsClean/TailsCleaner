
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OutGameLevelSystem : MonoBehaviour
{
    public static OutGameLevelSystem Instance;

    private CharLevelTableSO _levelData;

    public bool IsMaxLevel { get; private set; }
    public int CurrentLevel { get; private set; }
    public float CurrentExp { get; private set; }

    public float MaxExp
    {
        get
        {
            // 데이터 테이블에서 값을 읽어오기
            if (_levelData == null)
                _levelData = DataManager.Instance.GetSOData<CharLevelTableSO>();

            return _levelData.GetById(CurrentLevel).char_exp_limit;
        }
    }


    private async void Awake()
    {
        #region 싱글톤
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        transform.SetParent(null);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        Init();
        await LoadLevel();
    }
    private void Start()
    {
        FirebaseManager.Instance.AddLoadData(LoadLevel);
        FirebaseManager.Instance.AddSaveData(SaveLevel);
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RemoveLoadData(LoadLevel);
            FirebaseManager.Instance.RemoveSaveData(SaveLevel);
        }
    }



    public void GainExp(float gainExp)
    {
        if (IsMaxLevel)
        { WarningText.ShowText("계정레벨이 최대 레벨입니다."); return; }

        CurrentExp += gainExp;

        // 무한루프를 방지한 안전코드
        int repeatCount = 0;
        while(CurrentExp >= MaxExp && repeatCount < 50)
        {
            LevelUp();
            repeatCount++;
        }

        _ =SaveLevel();

    }
    private void LevelUp()
    {
        // 최대 레벨인지 확인
        if (CurrentLevel >= _levelData.dataList.Count)
        {
            IsMaxLevel = true;
            CurrentExp = MaxExp;
            return;
        }

        CurrentExp -= MaxExp;
        CurrentLevel++;
        
    }

    public void Init()
    {
        IsMaxLevel = false;
        CurrentLevel = 1;
        CurrentExp = 0;
    }

    #region Firebase 저장/로드

    public async Task SaveLevel()
    {
        var data = new Dictionary<string, object>
        {
            { "level", CurrentLevel },
            { "exp", CurrentExp }
        };

        await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Level")
            .UpdateChildrenAsync(data);
    }

    public async Task LoadLevel()
    {
        var snapshot = await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Level")
            .GetValueAsync();

        if (!snapshot.Exists)
        {
            Init(); // 신규 유저 기본값
            return;
        }

        CurrentLevel = int.Parse(snapshot.Child("level").Value.ToString());
        CurrentExp = float.Parse(snapshot.Child("exp").Value.ToString());
        IsMaxLevel = CurrentLevel >= _levelData.dataList.Count;
    }

    #endregion
}

