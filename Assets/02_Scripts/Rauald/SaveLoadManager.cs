using System.IO;
using UnityEngine;

public class SaveLoadManager
{
    private string filePath => Path.Combine(Application.persistentDataPath, "gameData.json");
    private SaveLoadData curData;
    public SaveLoadData CurData => curData;

    /// <summary> 스테이지 저장 </summary>
    /// <param name="index"> 현재 스테이지 번호 </param>
    /// <param name="isClear"> 클리어 여부 </param>
    /// <param name="score"> 점수 </param>
    /// <param name="starCnt"> 별 개수 </param>
    public void StageSaveData(int index, bool isClear, int score, int starCnt)
    {
        // 현재 스테이지를 클리어 한 기록이 없고, 클리어 했다면
        if (!curData.ListStageSaveData[index].IsClear && isClear)
        {
            // 클리어 저장
            curData.ListStageSaveData[index].IsClear = isClear;
        }

        // 최고 기록 점수가 현재 점수보다 낮다면
        if (curData.ListStageSaveData[index].Score < score)
        { 
            // 기록 갱신
            curData.ListStageSaveData[index].Score = score;
        }

        // 최고 별 점수가 현재 별보다 낮다면
        if (curData.ListStageSaveData[index].StarCnt < starCnt)
        {
            // 별 갱신
            curData.ListStageSaveData[index].StarCnt = starCnt;
        }

        SaveData(curData);
    }

    /// <summary> 스킨 저장 </summary>
    /// <param name="index"> 스킨 번호 </param>
    /// <param name="isSkin"> 스킨 여부 </param>
    public void SkinSaveData(int index, bool isSkin)
    {
        // 현재 스킨이 없고, 스킨을 획득했다면
        if (!curData.ListSkinSaveData[index].IsSkin && isSkin)
        {
            // 스킨 저장
            curData.ListSkinSaveData[index].IsSkin = isSkin;
        }

        SaveData(curData);
    }

    /// <summary> 미션 저장 </summary>
    /// <param name="index"> 미션 번호 </param>
    /// <param name="isClear"></param>
    /// <param name="progress"></param>
    public void MissionSaveData(int index, int dataIndex, bool isClear, int progress)
    {
        // 현재 미션 csv 번호 저장
        curData.ListMissionSaveData[index].MissionDataIndex = dataIndex;

        // 해당 미션을 클리어 하지 않았고, 클리어 한 상태라면
        if (!curData.ListMissionSaveData[index].IsClear && isClear)
        {
            // 클리어 저장
            curData.ListMissionSaveData[index].IsClear = isClear;
        }

        // 현재 진행상황 저장
        curData.ListMissionSaveData[index].CurProgress = progress;

        SaveData(curData);
    }

    public void ForceQuitSaveData(int mapIndex, Transform playerTr, int jellies, int score, Transform[] itemTr, int[] itemType)
    {
        curData.IsForceSave = true;

        curData.MapIndex = mapIndex;

        curData.PosX = playerTr.position.x;
        curData.PosY = playerTr.position.y;
        curData.PosZ = playerTr.position.z;

        Vector3 roation = playerTr.eulerAngles;

        curData.RotX = roation.x;
        curData.RotY = roation.y;
        curData.RotZ = roation.z;

        curData.jellieCnt = jellies;

        curData.score = score;

        for (int i = 0; i < itemTr.Length; i++)
        {
            curData.ListItemSaveData.Add(new ItemSaveData
            {
                PosX = itemTr[i].position.x,
                PosY = itemTr[i].position.y,
                PosZ = itemTr[i].position.z,

                type = itemType[i]
            });
        }

        SaveData(curData);
    }        

    // 이어하기 했으면 막기
    public void ContinueLoadData()
    {
        // 이어하기 데이터 접근 제한
        curData.IsForceSave = false;

        curData.ListItemSaveData.Clear();

        SaveData(curData);
    }

    private void SaveData(SaveLoadData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    // 불러오기
    public SaveLoadData LoadData()
    {
        // 저장된 파일이 있다면
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            curData = JsonUtility.FromJson<SaveLoadData>(json);
        }
        else
        {
            // 신규 유저 파일 초기화.
            curData = new SaveLoadData
            {
                Gold = 0,
                // 스테이지 데이터 받기. 스테이지 개수만큼 만들기
                // 스킨 데이터 받기. 스킨 개수만큼 만들기
                // 미션 데이터 받기. 미션 개수는 3개. 랜덤 지정이지만 여기서 만들지 않는다.

                IsForceSave = false,

                MapIndex = 0,

                PosX = 0,
                PosY = 0,
                PosZ = 0,

                RotX = 0,
                RotY = 0,
                RotZ = 0,

                jellieCnt = 0,

                score = 0,
            };
        }

        return curData;
    }
}