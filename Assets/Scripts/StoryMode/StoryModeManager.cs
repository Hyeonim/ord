using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스토리 모드(퀘스트 타겟 시스템)를 관리하는 매니저.
/// 스토리 보스의 생성, 진행, 보상을 관리한다.
/// </summary>
public class StoryModeManager : MonoBehaviour
{
    public static StoryModeManager Instance { get; private set; }

    [Header("참조")]
    public StoryBoss storyBoss;
    public UnitDatabase unitDatabase;
    public Transform bossSpawnPoint;  // 보스 배치 위치 (맵 중앙)

    [Header("퀘스트 설정")]
    public StoryQuest[] quests;
    [SerializeField] private int currentQuestIndex = 0;

    [Header("보상 설정")]
    public int rareUnitRewardGrade = 3; // 보상으로 주는 유닛의 최소 등급

    [Header("상태")]
    [SerializeField] private bool isQuestActive = false;
    [SerializeField] private int totalBossesDefeated = 0;

    public bool IsQuestActive => isQuestActive;
    public int CurrentQuestNumber => currentQuestIndex + 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 첫 번째 퀘스트 시작
        StartNextQuest();
    }

    /// <summary>
    /// 다음 스토리 퀘스트를 시작한다.
    /// </summary>
    public void StartNextQuest()
    {
        if (quests != null && currentQuestIndex < quests.Length)
        {
            StoryQuest quest = quests[currentQuestIndex];
            ActivateBoss(quest);
        }
        else
        {
            // 동적 퀘스트 생성 (무한 모드)
            GenerateDynamicQuest();
        }

        isQuestActive = true;
        Debug.Log($"[StoryMode] 퀘스트 #{CurrentQuestNumber} 시작!");
    }

    /// <summary>
    /// 퀘스트 데이터에 따라 보스를 활성화한다.
    /// </summary>
    private void ActivateBoss(StoryQuest quest)
    {
        if (storyBoss == null) return;

        storyBoss.ResetBoss(quest.bossHP, quest.goldReward, quest.bossName);
        storyBoss.transform.position = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
        storyBoss.gameObject.SetActive(true);
    }

    /// <summary>
    /// 동적으로 퀘스트를 생성한다 (정의된 퀘스트 소진 후).
    /// </summary>
    private void GenerateDynamicQuest()
    {
        if (storyBoss == null) return;

        float bossHP = 10000f * Mathf.Pow(1.5f, totalBossesDefeated);
        int goldReward = 1000 + totalBossesDefeated * 500;
        string bossName = $"스토리 보스 Lv.{totalBossesDefeated + 1}";

        storyBoss.ResetBoss(bossHP, goldReward, bossName);
        storyBoss.transform.position = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
        storyBoss.gameObject.SetActive(true);
    }

    /// <summary>
    /// 보스가 처치되었을 때 호출된다.
    /// </summary>
    public void OnBossDefeated(StoryBoss boss)
    {
        totalBossesDefeated++;
        isQuestActive = false;
        currentQuestIndex++;

        Debug.Log($"[StoryMode] 퀘스트 완료! 총 보스 처치: {totalBossesDefeated}");

        // 일정 시간 후 다음 퀘스트 시작
        Invoke(nameof(StartNextQuest), 5f);
    }

    /// <summary>
    /// 희귀 유닛 보상을 지급한다.
    /// </summary>
    public void GiveRareUnitReward()
    {
        if (unitDatabase == null) return;

        // 최소 Rare 등급 이상의 유닛을 보상으로 지급
        UnitGrade rewardGrade = (UnitGrade)Mathf.Min(rareUnitRewardGrade + totalBossesDefeated / 3, 5);
        UnitData[] candidates = unitDatabase.GetUnitsByGrade(rewardGrade);

        if (candidates.Length > 0)
        {
            UnitData rewardUnit = candidates[Random.Range(0, candidates.Length)];
            
            // 인벤토리에 직접 추가
            if (SummonManager.Instance != null)
            {
                // 보상 유닛 생성
                GameObject unitObj = Instantiate(
                    rewardUnit.prefab,
                    Vector3.zero,
                    Quaternion.identity
                );
                UnitController unit = unitObj.GetComponent<UnitController>();
                unit.Initialize(rewardUnit, UnitPlacement.Inventory);
                SummonManager.Instance.ReturnToInventory(unit);

                Debug.Log($"[StoryMode] 보상 유닛 획득: {rewardUnit.unitName} (★{(int)rewardUnit.grade})");
            }
        }
    }
}

/// <summary>
/// 스토리 퀘스트 데이터 구조체.
/// </summary>
[System.Serializable]
public class StoryQuest
{
    public string questName;
    public string bossName;
    public float bossHP = 10000f;
    public int goldReward = 1000;
    public string description;
}
