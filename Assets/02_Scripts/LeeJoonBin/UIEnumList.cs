/// <summary> 조작법 종류 </summary>
public enum EControlType
{
    TouchThreeSplit = 0,
    BottomButton = 1,
    Swipe = 2
}

/// <summary> 게임 모드 종류 (일반 모드 / 무한 모드) </summary>
public enum EGameModeType
{
    Normal = 0,
    Infinite = 1
}

/// <summary> 스테이지 난이도 종류 (참조용) </summary>
public enum EDifficultyType
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

/// <summary> 윈도우(전체 화면) 종류 </summary>
public enum EWindowType
{
    None = -1,
    Title = 0,
    ControlSelect = 1,
    Lobby = 2,
    Skin = 3,
    Stage = 4,
    InGame = 5
}

/// <summary> 팝업창 종류 </summary>
public enum EPopupType
{
    None = -1,
    Setting = 0,
    Mission = 1,
    StageConfirm = 2,
    Notice = 3,
    ClearResult = 4,
    FailResult = 5
}
