using GravityCollect;
using UnityEngine;

/// <summary> 선택된 스테이지에 맞춰 기획서 규격의 맵과 플레이어를 스폰하는 로더 </summary>
public class StageMapLoader : MonoBehaviour
{
    [Header("Stage Map Prefabs (1 ~ 16)")]
    [SerializeField] private GameObject[] _arrStageMapPrefab;

    [Header("Player Prefab")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _defaultPlayerSpawnPos = new Vector3(0f, 1f, 0f);

    private GameObject _spawnedMapObj;
    private GameObject _spawnedPlayerObj;
    private Transform _endPointTransform;

    public GameObject SpawnedMapObj => _spawnedMapObj;
    public GameObject SpawnedPlayerObj => _spawnedPlayerObj;
    public Transform EndPointTransform => _endPointTransform;

    private void Start()
    {
        LoadStageAndSpawn();
    }

    /// <summary> 선택된 스테이지에 맞는 맵과 플레이어 생성 </summary>
    public void LoadStageAndSpawn()
    {
        int stageIndex = UIManager.Instance != null ? UIManager.Instance.SelectedStageIndex : 0;
        SpawnMap(stageIndex);
        SpawnPlayer();

        // GameSceneUI에 플레이어 바인딩 요청
        GameSceneUI gameUI = Object.FindFirstObjectByType<GameSceneUI>();
        if (gameUI != null)
        {
            gameUI.TryBindPlayer();
        }
    }

    private void SpawnMap(int stageIndex)
    {
        if (_arrStageMapPrefab == null || _arrStageMapPrefab.Length == 0)
        {
            Debug.LogWarning("[StageMapLoader] 등록된 맵 프리팹이 없습니다.");
            return;
        }

        // 범위 보정 (0 ~ 배열 크기-1)
        int clampedIndex = Mathf.Clamp(stageIndex, 0, _arrStageMapPrefab.Length - 1);
        GameObject mapPrefab = _arrStageMapPrefab[clampedIndex];

        if (mapPrefab != null)
        {
            _spawnedMapObj = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            _spawnedMapObj.name = $"Map_Stage_{clampedIndex + 1}_{mapPrefab.name}";
            _endPointTransform = FindPointTransform(_spawnedMapObj.transform, "End_Point", "EndPoint");
        }
    }

    private Transform FindPointTransform(Transform root, string exactName, string altName)
    {
        Transform direct = root.Find(exactName);
        if (direct != null)
        {
            return direct;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            if (child.name.Equals(exactName) || child.name.Contains(altName))
            {
                return child;
            }
        }

        return null;
    }

    private void SpawnPlayer()
    {
        // 씬 내에 이미 플레이어가 존재하는지 확인
        RotateAction existingPlayer = Object.FindFirstObjectByType<RotateAction>();
        if (existingPlayer != null)
        {
            _spawnedPlayerObj = existingPlayer.gameObject;
            FixPlayerGroundCheckDistance(_spawnedPlayerObj);
            return;
        }

        if (_playerPrefab == null)
        {
            Debug.LogWarning("[StageMapLoader] Player 프리팹이 등록되지 않았습니다.");
            return;
        }

        // 스폰 위치 및 회전 계산 (맵 내 Start_Point 오브젝트를 우선 탐색)
        Vector3 spawnPos = _defaultPlayerSpawnPos;
        Quaternion spawnRot = Quaternion.identity;

        if (_spawnedMapObj != null)
        {
            Transform startPointTr = FindPointTransform(_spawnedMapObj.transform, "Start_Point", "StartPoint");

            if (startPointTr != null)
            {
                // Start_Point 발판 바로 위에 슬라임 배치
                spawnPos = startPointTr.position + startPointTr.up * 0.5f;
                spawnRot = startPointTr.rotation;
            }
        }

        _spawnedPlayerObj = Instantiate(_playerPrefab, spawnPos, spawnRot);
        _spawnedPlayerObj.name = "Player";

        // 바닥 감지 거리 보정 (원인 해결)
        FixPlayerGroundCheckDistance(_spawnedPlayerObj);
    }

    /// <summary> 프리팹 기본값(0.2)으로 인해 바닥 감지가 실패하여 즉사하는 현상 방지 보정 </summary>
    private void FixPlayerGroundCheckDistance(GameObject playerObj)
    {
        if (playerObj == null)
        {
            return;
        }

        JumpActionSample jump = playerObj.GetComponent<JumpActionSample>();
        if (jump != null)
        {
            var field = typeof(JumpActionSample).GetField("_groundCheckDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                float curDist = (float)field.GetValue(jump);
                if (curDist < 0.5f)
                {
                    field.SetValue(jump, 0.8f);
                }
            }
        }
    }
}
