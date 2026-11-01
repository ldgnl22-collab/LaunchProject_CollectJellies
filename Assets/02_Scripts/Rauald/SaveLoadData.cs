using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

/// <summary> 스테이지 저장 </summary>
[System.Serializable]
public class StageSaveData
{
    // 스테이지 번호
    public int StageIndex;
    // 스테이지 클리어 여부
    public bool IsClear;
    // 최고 점수
    public int Score;
    // 별 개수
    public int StarCnt;
}

/// <summary> 스킨 저장 </summary>
[System.Serializable]
public class SkinSaveData
{
    // 스킨 번호
    public int SkinIndex;
    // 스킨 여부
    public bool IsSkin;
}

/// <summary> 미션 저장 </summary>
[System.Serializable]
public class MissionSaveData
{
    // 미션 번호
    public int MissionIndex;
    // 미션 csv 번호
    public int MissionDataIndex;
    // 미션 클리어 여부
    public bool IsClear;
    // 미션 현재 진행도
    public int CurProgress;
}

/// <summary> 아이템 저장 </summary>
[System.Serializable]
public class ItemSaveData
{
    // 좌표 X
    public float PosX;
    // 좌표 Y
    public float PosY;
    // 좌표 Z
    public float PosZ;
    // 아이템 종류 (int 값으로 저장)
    public int type;
}

/// <summary> 세이브 로드 데이터 </summary>
[System.Serializable]
public class SaveLoadData
{
    // 골드
    public int Gold;
    // 스테이지
    public List<StageSaveData> ListStageSaveData = new();
    // 스킨
    public List<SkinSaveData> ListSkinSaveData = new();
    // 미션
    public List<MissionSaveData> ListMissionSaveData = new();

    // 강제 종료시 저장 파일
    public bool IsForceSave;
    // 맵 번호
    public int MapIndex;
    // 플레이어 x 좌표
    public float PosX;
    // 플레이어 y 좌표
    public float PosY;
    // 플레이어 z 좌표
    public float PosZ;

    // 플레이어 x 각도
    public float RotX;
    // 플레이어 y 각도
    public float RotY;
    // 플레이어 z 각도
    public float RotZ;
    // 보조 젤리 개수
    public int jellieCnt;
    // 현재 점수
    public int score;
    // 아이템
    public List<ItemSaveData> ListItemSaveData = new();

}