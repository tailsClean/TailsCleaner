using UnityEngine;

public class PlayerLevelSystem
{
    public float InGameCurrentExp { get; private set; }
    public float InGameMaxExp { get; private set; }
    public int InGameLevel { get; private set; }

    public float OutGameCurrentExp { get; private set; }
    public float OutGameMaxExp { get; private set; }
    public int OutGameLevel { get; private set; }


    public PlayerLevelSystem(PlayerBase player)
    {
        InGameMaxExp = player._inGameMaxExp;
        OutGameMaxExp = player._outGameMaxExp;
        InGameLevel = 1;
        OutGameLevel = 1;
    }


    // 외부 게임모드에 따른 경험치 획득 메서드
    public bool GainExp(GameMode gameMode, float gainExp)
    {
        bool isLevelUp = false;

        switch(gameMode)
        {
            case GameMode.InGame:
                InGameCurrentExp += gainExp;
                isLevelUp = InGameCurrentExp > InGameMaxExp ? true : false;

                if(isLevelUp)
                    LevelUp(gameMode);
                break;


            case GameMode.OutGame:
                OutGameCurrentExp += gainExp;
                isLevelUp = OutGameCurrentExp > OutGameMaxExp ? true : false;

                if(isLevelUp)
                    LevelUp(gameMode);
                break;
        }

        return isLevelUp;
    }

    // 외부 게임모드에 따른 레벨업 메서드
    public void LevelUp(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.InGame:
                InGameCurrentExp -= InGameMaxExp;
                InGameLevel++;
                break;

            case GameMode.OutGame:
                OutGameCurrentExp -= OutGameMaxExp;
                OutGameLevel++;
                break;
        }
    }



    public enum GameMode { InGame,  OutGame }
}
