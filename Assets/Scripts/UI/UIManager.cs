using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 UI를 관리하는 매니저.
/// 골드, 라이프, 웨이브 정보, 보스 HP 등을 표시한다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("상단 정보 UI")]
    public Text goldText;
    public Text livesText;
    public Text waveText;
    public Text gameTimeText;

    [Header("소환 버튼")]
    public Button summonButton;
    public Text summonCostText;

    [Header("보스 HP 바")]
    public GameObject bossHPPanel;
    public Slider bossHPSlider;
    public Text bossNameText;
    public Text bossHPText;

    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;

    [Header("디버그 정보")]
    public Text debugText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldUI;
            GameManager.Instance.OnLivesChanged += UpdateLivesUI;
            GameManager.Instance.OnGameOverEvent += ShowGameOver;
        }

        // 소환 버튼 연결
        if (summonButton != null)
        {
            summonButton.onClick.AddListener(OnSummonButtonClicked);
        }

        // 재시작 버튼 연결
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        // 초기 UI 업데이트
        UpdateAllUI();

        // 게임 오버 패널 숨기기
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (bossHPPanel != null) bossHPPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateWaveUI();
        UpdateGameTimeUI();
        UpdateBossUI();
        UpdateDebugUI();
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance == null) return;
        UpdateGoldUI(GameManager.Instance.Gold);
        UpdateLivesUI(GameManager.Instance.Lives);
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null) goldText.text = $"골드: {gold}";
        if (summonCostText != null && SummonManager.Instance != null)
            summonCostText.text = $"소환 ({SummonManager.Instance.summonCost}G)";
    }

    private void UpdateLivesUI(int lives)
    {
        if (livesText != null) livesText.text = $"라이프: {lives}";
    }

    private void UpdateWaveUI()
    {
        if (waveText != null && WaveManager.Instance != null)
        {
            waveText.text = $"웨이브: {WaveManager.Instance.CurrentWave} | 적: {WaveManager.Instance.EnemiesAlive}";
        }
    }

    private void UpdateGameTimeUI()
    {
        if (gameTimeText != null && GameManager.Instance != null)
        {
            float time = GameManager.Instance.GameTime;
            int minutes = (int)(time / 60f);
            int seconds = (int)(time % 60f);
            gameTimeText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateBossUI()
    {
        if (bossHPPanel == null) return;

        StoryBoss boss = FindObjectOfType<StoryBoss>();
        if (boss != null && boss.IsAlive && boss.gameObject.activeInHierarchy)
        {
            bossHPPanel.SetActive(true);
            if (bossHPSlider != null) bossHPSlider.value = boss.HPRatio;
            if (bossNameText != null) bossNameText.text = boss.bossName;
            if (bossHPText != null) bossHPText.text = $"{(int)(boss.HPRatio * 100)}%";
        }
        else
        {
            bossHPPanel.SetActive(false);
        }
    }

    private void UpdateDebugUI()
    {
        if (debugText != null)
        {
            string info = "[디버그 키]\n";
            info += "G: 골드+100 | S: 소환\n";
            info += "N: 웨이브 스킵 | 1/2/3: 속도\n";
            info += "R: 재시작 (게임오버 시)";
            debugText.text = info;
        }
    }

    private void OnSummonButtonClicked()
    {
        if (SummonManager.Instance != null)
        {
            SummonManager.Instance.SummonRandomUnit();
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = $"게임 오버!\n웨이브: {WaveManager.Instance.CurrentWave}\n시간: {gameTimeText?.text}";
            }
        }
    }
}
