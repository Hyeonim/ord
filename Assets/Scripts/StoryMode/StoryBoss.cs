using UnityEngine;
using System;

/// <summary>
/// 스토리 모드의 고정형 보스 오브젝트.
/// 맵 중앙에 배치되며, 유닛이 근처에 배치되면 공격 대상이 된다.
/// 체력을 모두 깎으면 대량 보상이 지급된다.
/// </summary>
public class StoryBoss : MonoBehaviour
{
    [Header("보스 설정")]
    public string bossName = "스토리 보스";
    public float maxHP = 10000f;
    public int goldReward = 1000;
    public bool giveRareUnit = true;

    [Header("상태")]
    [SerializeField] private float currentHP;
    public bool IsAlive => currentHP > 0f;
    public float HPRatio => currentHP / maxHP;

    [Header("시각 효과")]
    private Renderer bossRenderer;
    private Vector3 originalScale;

    // 이벤트
    public event Action<float> OnDamaged;       // 데미지 받을 때 (남은 HP 비율)
    public event Action OnDefeated;             // 처치 시

    private void Awake()
    {
        currentHP = maxHP;
        bossRenderer = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;

        if (bossRenderer != null)
        {
            bossRenderer.material.color = new Color(0.8f, 0f, 0f); // 진한 빨강
        }
    }

    /// <summary>
    /// 데미지를 받는다.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0f, currentHP);

        OnDamaged?.Invoke(HPRatio);

        // 피격 시각 효과
        StartCoroutine(DamageEffect());

        if (currentHP <= 0f)
        {
            OnBossDefeated();
        }
    }

    /// <summary>
    /// 보스 처치 시 보상 지급 및 이벤트 발생.
    /// </summary>
    private void OnBossDefeated()
    {
        Debug.Log($"[StoryBoss] {bossName} 처치! 보상 지급!");

        // 골드 보상
        GameManager.Instance.AddGold(goldReward);

        // 희귀 유닛 보상
        if (giveRareUnit)
        {
            StoryModeManager.Instance.GiveRareUnitReward();
        }

        // 퀘스트 완료 처리
        StoryModeManager.Instance.OnBossDefeated(this);

        OnDefeated?.Invoke();

        // 보스 제거 (이펙트 후)
        StartCoroutine(DeathEffect());
    }

    /// <summary>
    /// 보스를 리셋한다 (다음 스토리 퀘스트용).
    /// </summary>
    public void ResetBoss(float newMaxHP, int newGoldReward, string newName)
    {
        maxHP = newMaxHP;
        goldReward = newGoldReward;
        bossName = newName;
        currentHP = maxHP;
        gameObject.SetActive(true);
        transform.localScale = originalScale;

        if (bossRenderer != null)
        {
            bossRenderer.material.color = new Color(0.8f, 0f, 0f);
        }
    }

    private System.Collections.IEnumerator DamageEffect()
    {
        if (bossRenderer != null)
        {
            Color original = bossRenderer.material.color;
            bossRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            bossRenderer.material.color = original;
        }

        // HP에 따라 크기 약간 줄이기 (시각적 피드백)
        float scale = 0.8f + 0.2f * HPRatio;
        transform.localScale = originalScale * scale;
    }

    private System.Collections.IEnumerator DeathEffect()
    {
        // 사망 이펙트: 점점 작아지며 사라짐
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 에디터에서 보스 범위를 시각적으로 표시.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
