using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 웨이브 시스템을 관리하는 매니저.
/// 일정 시간마다 적 웨이브를 생성하고, 웨이브 진행 상태를 추적한다.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("참조")]
    public WaypointPath enemyPath;
    public Transform enemyParent;

    [Header("웨이브 데이터")]
    public WaveData[] waves;
    
    [Header("설정")]
    public float timeBetweenWaves = 15f;  // 웨이브 간 대기 시간
    public float firstWaveDelay = 5f;     // 첫 웨이브 시작 전 대기

    [Header("동적 생성 (WaveData 없을 때)")]
    public GameObject defaultEnemyPrefab;
    public int baseEnemyCount = 5;
    public float enemyHPScaling = 1.2f;   // 웨이브당 HP 증가 배율
    public float enemySpeedScaling = 1.05f;

    [Header("상태 (읽기 전용)")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private int totalEnemiesKilled = 0;
    [SerializeField] private bool isSpawning = false;

    public int CurrentWave => currentWaveIndex + 1;
    public int EnemiesAlive => enemiesAlive;
    public bool IsWaveActive => isSpawning || enemiesAlive > 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    /// <summary>
    /// 웨이브 루프: 웨이브를 순차적으로 생성한다.
    /// </summary>
    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                yield break;

            Debug.Log($"[WaveManager] === 웨이브 {CurrentWave} 시작! ===");
            yield return StartCoroutine(SpawnWave());

            // 현재 웨이브의 모든 적이 죽을 때까지 대기
            yield return new WaitUntil(() => enemiesAlive <= 0);

            Debug.Log($"[WaveManager] 웨이브 {CurrentWave} 클리어!");
            currentWaveIndex++;

            // 다음 웨이브 대기
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// 단일 웨이브를 생성한다.
    /// </summary>
    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        if (waves != null && currentWaveIndex < waves.Length)
        {
            // WaveData 기반 생성
            WaveData waveData = waves[currentWaveIndex];
            foreach (var entry in waveData.entries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);
                    yield return new WaitForSeconds(waveData.timeBetweenSpawns);
                }
            }
        }
        else
        {
            // 동적 생성 (무한 웨이브)
            int enemyCount = baseEnemyCount + currentWaveIndex * 2;
            float hpMult = Mathf.Pow(enemyHPScaling, currentWaveIndex);
            float speedMult = Mathf.Pow(enemySpeedScaling, currentWaveIndex);

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnDynamicEnemy(hpMult, speedMult);
                yield return new WaitForSeconds(1f);
            }
        }

        isSpawning = false;
    }

    /// <summary>
    /// EnemyData 기반으로 적을 생성한다.
    /// </summary>
    private void SpawnEnemy(EnemyData data)
    {
        if (data.prefab == null || enemyPath == null) return;

        GameObject enemyObj = Instantiate(data.prefab, enemyPath.GetWaypointPosition(0), Quaternion.identity, enemyParent);
        enemyObj.SetActive(true); // 프리팩이 비활성일 수 있으므로 강제 활성화
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        if (enemy == null) enemy = enemyObj.AddComponent<EnemyController>();
        enemy.Initialize(data, enemyPath);
        enemiesAlive++;
    }

    /// <summary>
    /// 동적으로 적을 생성한다 (WaveData 없을 때).
    /// </summary>
    private void SpawnDynamicEnemy(float hpMultiplier, float speedMultiplier)
    {
        if (defaultEnemyPrefab == null || enemyPath == null) return;

        GameObject enemyObj = Instantiate(defaultEnemyPrefab, enemyPath.GetWaypointPosition(0), Quaternion.identity, enemyParent);
        enemyObj.SetActive(true); // 프리팩이 비활성 상태일 수 있으므로 강제 활성화
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();

        // 동적 EnemyData 생성
        EnemyData dynamicData = ScriptableObject.CreateInstance<EnemyData>();
        dynamicData.enemyName = $"Wave{CurrentWave}_Enemy";
        dynamicData.maxHP = 100f * hpMultiplier;
        dynamicData.moveSpeed = 2f * speedMultiplier;
        dynamicData.goldReward = 10 + currentWaveIndex * 2;

        enemy.Initialize(dynamicData, enemyPath);
        enemiesAlive++;
    }

    /// <summary>
    /// 적이 죽었을 때 호출된다.
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        totalEnemiesKilled++;
    }

    /// <summary>
    /// 현재 웨이브를 강제로 스킵한다 (디버그용).
    /// </summary>
    public void SkipWave()
    {
        // 모든 적 제거
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var e in enemies)
        {
            Destroy(e.gameObject);
        }
        enemiesAlive = 0;
    }
}
