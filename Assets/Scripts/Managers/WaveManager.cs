using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 웨이브(라운드) 시스템 관리.
/// 75라운드 동안 적을 생성하고, 보스 라운드를 처리한다.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("참조")]
    public WaypointPath enemyPath;
    public Transform enemyParent;
    public GameObject defaultEnemyPrefab;

    [Header("웨이브 데이터")]
    public WaveData[] waves;

    [Header("설정")]
    public float timeBetweenWaves = 15f;
    public float firstWaveDelay = 5f;
    public float spawnInterval = 0.8f;

    [Header("스케일링")]
    public float baseHP = 100f;
    public float hpScalePerRound = 1.15f;
    public float baseSpeed = 2f;
    public float speedScalePerRound = 1.02f;
    public float baseArmor = 0f;
    public float armorScalePerRound = 0.5f;

    [Header("보스 라운드")]
    public int[] bossRounds = { 10, 20, 30, 40, 50, 60, 70, 75 };

    [Header("상태")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private bool isSpawning = false;

    public int CurrentWave => currentWaveIndex + 1;
    public int EnemiesAlive => enemiesAlive;
    public bool IsWaveActive => isSpawning || enemiesAlive > 0;

    private Coroutine waveCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                yield break;

            // 새 라운드 시작
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewRound();

            yield return StartCoroutine(SpawnWave());

            // 모든 적 처치 대기
            yield return new WaitUntil(() => enemiesAlive <= 0);

            // 라운드 완료
            if (GameManager.Instance != null)
                GameManager.Instance.OnRoundComplete();

            currentWaveIndex++;

            if (currentWaveIndex >= GameManager.Instance.MaxRounds)
                yield break;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        int round = currentWaveIndex + 1;
        bool isBossRound = IsBossRound(round);

        if (waves != null && currentWaveIndex < waves.Length && waves[currentWaveIndex] != null)
        {
            // WaveData 기반 생성
            WaveData waveData = waves[currentWaveIndex];
            foreach (var entry in waveData.entries)
            {
                if (entry.enemyData == null) continue;
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);
                    yield return new WaitForSeconds(waveData.timeBetweenSpawns);
                }
            }
        }
        else
        {
            // 동적 생성
            int enemyCount = GetEnemyCount(round);
            float hp = baseHP * Mathf.Pow(hpScalePerRound, currentWaveIndex);
            float speed = baseSpeed * Mathf.Pow(speedScalePerRound, currentWaveIndex);
            float armor = baseArmor + armorScalePerRound * currentWaveIndex;

            if (isBossRound)
            {
                // 보스 생성
                SpawnDynamicEnemy(hp * 10f, speed * 0.5f, armor * 2f, true);
                yield return new WaitForSeconds(1f);
            }

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnDynamicEnemy(hp, speed, armor, false);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawning = false;
    }

    private void SpawnEnemy(EnemyData data)
    {
        if (data.prefab == null || enemyPath == null) return;

        Vector3 spawnPos = enemyPath.GetWaypointPosition(0);
        GameObject enemyObj = Instantiate(data.prefab, spawnPos, Quaternion.identity, enemyParent);
        enemyObj.SetActive(true);

        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        if (enemy == null) enemy = enemyObj.AddComponent<EnemyController>();
        enemy.Initialize(data, enemyPath);
        enemiesAlive++;
    }

    private void SpawnDynamicEnemy(float hp, float speed, float armor, bool isBoss)
    {
        if (enemyPath == null) return;

        Vector3 spawnPos = enemyPath.GetWaypointPosition(0);
        GameObject enemyObj;

        if (defaultEnemyPrefab != null)
        {
            enemyObj = Instantiate(defaultEnemyPrefab, spawnPos, Quaternion.identity, enemyParent);
        }
        else
        {
            // 프리팹 없으면 동적 생성
            enemyObj = GameObject.CreatePrimitive(isBoss ? PrimitiveType.Capsule : PrimitiveType.Sphere);
            enemyObj.transform.position = spawnPos;
            if (enemyParent != null) enemyObj.transform.SetParent(enemyParent);
        }

        enemyObj.SetActive(true);
        if (isBoss) enemyObj.transform.localScale = Vector3.one * 1.5f;
        else enemyObj.transform.localScale = Vector3.one * 0.6f;

        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        if (enemy == null) enemy = enemyObj.AddComponent<EnemyController>();

        EnemyData dynamicData = ScriptableObject.CreateInstance<EnemyData>();
        dynamicData.enemyName = isBoss ? $"Boss_R{CurrentWave}" : $"Enemy_R{CurrentWave}";
        dynamicData.maxHP = hp;
        dynamicData.moveSpeed = speed;
        dynamicData.armor = armor;
        dynamicData.goldReward = isBoss ? 50 + currentWaveIndex * 5 : 5 + currentWaveIndex;
        dynamicData.isBoss = isBoss;

        enemy.Initialize(dynamicData, enemyPath);
        enemiesAlive++;

        // 보스면 색상 빨강
        Renderer rend = enemyObj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = isBoss ? Color.red : new Color(1f, 0.5f, 0f);
        }
    }

    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    public void OnEnemyReachedEnd()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(1);
        }
    }

    private bool IsBossRound(int round)
    {
        foreach (int br in bossRounds)
        {
            if (round == br) return true;
        }
        return false;
    }

    private int GetEnemyCount(int round)
    {
        return 5 + round / 5 * 2;
    }

    public void ResetWaves()
    {
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        currentWaveIndex = 0;
        enemiesAlive = 0;
        isSpawning = false;

        // 모든 적 제거
        if (enemyParent != null)
        {
            foreach (Transform child in enemyParent)
                Destroy(child.gameObject);
        }

        waveCoroutine = StartCoroutine(WaveLoop());
    }

    public void SkipWave()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var e in enemies) Destroy(e.gameObject);
        enemiesAlive = 0;
    }
}
