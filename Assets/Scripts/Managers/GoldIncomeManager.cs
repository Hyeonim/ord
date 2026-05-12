using UnityEngine;

/// <summary>
/// 시간 경과에 따른 자동 골드 수입을 관리한다.
/// 일정 간격으로 골드가 자동 지급된다.
/// </summary>
public class GoldIncomeManager : MonoBehaviour
{
    [Header("설정")]
    public float incomeInterval = 10f;  // 골드 지급 간격 (초)
    public int baseIncome = 20;         // 기본 수입량
    public int incomePerWave = 5;       // 웨이브당 추가 수입

    private float timer = 0f;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;
        if (timer >= incomeInterval)
        {
            timer = 0f;
            GiveIncome();
        }
    }

    private void GiveIncome()
    {
        int waveBonus = 0;
        if (WaveManager.Instance != null)
        {
            waveBonus = WaveManager.Instance.CurrentWave * incomePerWave;
        }

        int totalIncome = baseIncome + waveBonus;
        GameManager.Instance.AddGold(totalIncome);
        Debug.Log($"[GoldIncome] 자동 수입: +{totalIncome} (기본 {baseIncome} + 웨이브 보너스 {waveBonus})");
    }
}
