using UnityEngine;

public class PlayerLevelSystem
{
    private OutGameLevelSystem _outGameLevelSystem;

    private CharIngameTableSO _ingameExpData;

    // 인게임 레벨 데이터
    public int InGameLevel { get; private set; }
    public float InGameCurrentExp { get; private set; }
    public float InGameMaxExp
    {
        get
        {
            if (_ingameExpData == null)
                _ingameExpData = DataManager.Instance.GetSOData<CharIngameTableSO>();

            return _ingameExpData.GetById(InGameLevel).char_ingame_exp;
        }
    }

    // 아웃게임 레벨 데이터
    public int OutGameLevel => _outGameLevelSystem.CurrentLevel;
    public float OutGameCurrentExp => _outGameLevelSystem.CurrentExp;
    public float OutGameMaxExp => _outGameLevelSystem.MaxExp;


    public PlayerLevelSystem()
    {
        _outGameLevelSystem = OutGameLevelSystem.Instance;
        InGameLevel = 1;
    }


    // 외부 게임모드에 따른 경험치 획득 메서드
    public bool GainExp(GAME_MODE gameMode, float gainExp)
    {
        bool isLevelUp = false;

        switch (gameMode)
        {
            case GAME_MODE.InGame:
                InGameCurrentExp += gainExp;
                isLevelUp = InGameCurrentExp >= InGameMaxExp ? true : false;

                if (isLevelUp)
                    LevelUp(gameMode);
                break;


            case GAME_MODE.OutGame:
                _outGameLevelSystem.GainExp(gainExp);
                break;
        }

        return isLevelUp;
    }

    // 외부 게임모드에 따른 레벨업 메서드
    private void LevelUp(GAME_MODE gameMode)
    {
        switch (gameMode)
        {
            case GAME_MODE.InGame:
                InGameCurrentExp -= InGameMaxExp;
                InGameLevel++;
                break;

                //case GAME_MODE.OutGame:
                //    OutGameCurrentExp -= OutGameMaxExp;
                //    OutGameLevel++;
                //    break;
        }
    }



    public enum GAME_MODE { InGame, OutGame }
}
