using UnityEngine;
using System.Collections;

/// <summary>
/// 스토리 보스 컨트롤러.
/// 특정 라운드에 등장하는 스토리 보스를 관리한다.
/// 제한시간 내 처치하지 못하면 패배.
/// </summary>
public class StoryBossController : MonoBehaviour
{
    [Header("보스 데이터")]
    [SerializeField] private float maxHP = 10000f;
    [SerializeField] private float currentHP;
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float timer;
    [SerializeField] private string bossName = "스토리 보스";
    [SerializeField] private int questNumber = 1;

    [Header("상태")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isDefeated = false;

    public float HPPercent => maxHP > 0 ? currentHP / maxHP : 0f;
    public float TimeRemaining => timer;
    public bool IsActive => isActive;
    public string BossName => bossName;

    public void ActivateBoss(float hp, float time, string name, int quest)
    {
        maxHP = hp;
        currentHP = hp;
        timeLimit = time;
        timer = time;
        bossName = name;
        questNumber = quest;
        isActive = true;
        isDefeated = false;

        Debug.Log($"[StoryBoss] {bossName} 등장! HP: {maxHP}, 제한시간: {timeLimit}초");
    }

    private void Update()
    {
        if (!isActive || isDefeated) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // 시간 초과 → 패배
            Debug.Log($"[StoryBoss] {bossName} 제한시간 초과! 패배!");
            isActive = false;
            // GameManager에서 패배 처리
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isActive || isDefeated) return;

        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Defeat();
        }
    }

    private void Defeat()
    {
        isDefeated = true;
        isActive = false;
        Debug.Log($"[StoryBoss] {bossName} 처치 완료! 퀘스트 #{questNumber} 클리어!");

        // 보상 지급
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(100 * questNumber);
            GameManager.Instance.AddWood(questNumber);
        }

        Destroy(gameObject, 1f);
    }
}
