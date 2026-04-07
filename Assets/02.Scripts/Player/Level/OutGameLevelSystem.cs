
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OutGameLevelSystem : MonoBehaviour, IConsumItemTarget
{
    public static OutGameLevelSystem Instance;

    private CharLevelTableSO _levelData;

    public bool IsMaxValue => IsMaxLevel;

    [field: SerializeField] public int CurrentLevel { get; private set; }
    public float CurrentExp { get; private set; }
    [field: SerializeField] public bool IsMaxLevel
    {
        get
        {

            if (_levelData == null)
                _levelData = DataManager.Instance.GetSOData<CharLevelTableSO>();

            return CurrentLevel >= _levelData.dataList.Count;
        }
    }

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


    // 인터페이스 구현 메서드
    public void IncreaseValue(float value) => GainExp(value);

    public void GainExp(float gainExp)
    {
        if (IsMaxLevel) return;

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
        if (IsMaxLevel)
        {
            CurrentExp = MaxExp;
            return;
        }

        CurrentExp -= MaxExp;
        CurrentLevel++;
        
    }


    public void Init()
    {
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
        //IsMaxLevel = CurrentLevel >= _levelData.dataList.Count;
    }


    #endregion
}

