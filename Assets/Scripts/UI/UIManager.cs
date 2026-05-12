using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 게임 UI를 관리하는 매니저.
/// 골드, 라이프, 웨이브 정보, 보스 HP, 조합법 패널, 모바일 속도 버튼 등을 표시한다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("상단 HUD")]
    public Text goldText;
    public Text livesText;
    public Text waveText;
    public Text gameTimeText;
    public Text enemyCountText;

    [Header("소환 버튼")]
    public Button summonButton;
    public Text summonCostText;

    [Header("속도 버튼 (모바일용)")]
    public Button speed1xButton;
    public Button speed2xButton;
    public Button speed3xButton;
    public Text currentSpeedText;

    [Header("보스 HP 바")]
    public GameObject bossHPPanel;
    public Slider bossHPSlider;
    public Text bossNameText;
    public Text bossHPText;

    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;

    [Header("조합법 패널")]
    public GameObject recipePanel;
    public Button recipeToggleButton;
    public Text recipeToggleButtonText;
    public Transform recipeListParent;
    public GameObject recipeItemPrefab;

    [Header("인벤토리 정보")]
    public Text inventoryCountText;

    private bool isRecipePanelOpen = false;

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

        // 소환 버튼
        if (summonButton != null)
            summonButton.onClick.AddListener(OnSummonButtonClicked);

        // 재시작 버튼
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        // 속도 버튼 (모바일)
        if (speed1xButton != null) speed1xButton.onClick.AddListener(() => SetSpeed(1f));
        if (speed2xButton != null) speed2xButton.onClick.AddListener(() => SetSpeed(2f));
        if (speed3xButton != null) speed3xButton.onClick.AddListener(() => SetSpeed(3f));

        // 조합법 버튼
        if (recipeToggleButton != null)
            recipeToggleButton.onClick.AddListener(ToggleRecipePanel);

        // 초기 UI
        UpdateAllUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (bossHPPanel != null) bossHPPanel.SetActive(false);
        if (recipePanel != null) recipePanel.SetActive(false);

        // 조합법 목록 생성
        BuildRecipeList();
    }

    private void Update()
    {
        UpdateWaveUI();
        UpdateGameTimeUI();
        UpdateBossUI();
        UpdateInventoryUI();
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance == null) return;
        UpdateGoldUI(GameManager.Instance.Gold);
        UpdateLivesUI(GameManager.Instance.Lives);
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null) goldText.text = $"💰 {gold}G";
        if (summonCostText != null && SummonManager.Instance != null)
            summonCostText.text = $"소환 ({SummonManager.Instance.summonCost}G)";

        // 골드 부족 시 소환 버튼 비활성화
        if (summonButton != null && SummonManager.Instance != null)
            summonButton.interactable = gold >= SummonManager.Instance.summonCost;
    }

    private void UpdateLivesUI(int lives)
    {
        if (livesText != null) livesText.text = $"❤ {lives}";
    }

    private void UpdateWaveUI()
    {
        if (WaveManager.Instance == null) return;
        if (waveText != null)
            waveText.text = $"Wave {WaveManager.Instance.CurrentWave}";
        if (enemyCountText != null)
            enemyCountText.text = $"적: {WaveManager.Instance.EnemiesAlive}";
    }

    private void UpdateInventoryUI()
    {
        if (inventoryCountText == null || SummonManager.Instance == null) return;
        int current = SummonManager.Instance.GetInventoryUnits().Count;
        int max = SummonManager.Instance.maxInventorySize;
        inventoryCountText.text = $"인벤토리: {current}/{max}";
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

    private void SetSpeed(float speed)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameSpeed(speed);
        if (currentSpeedText != null)
            currentSpeedText.text = $"x{speed}";
    }

    public void ToggleRecipePanel()
    {
        isRecipePanelOpen = !isRecipePanelOpen;
        if (recipePanel != null)
            recipePanel.SetActive(isRecipePanelOpen);
        if (recipeToggleButtonText != null)
            recipeToggleButtonText.text = isRecipePanelOpen ? "닫기" : "조합법";
    }

    private void BuildRecipeList()
    {
        if (recipeListParent == null || SummonManager.Instance == null) return;

        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        List<CombineRecipe> recipes = SummonManager.Instance.GetAllRecipes();
        foreach (var recipe in recipes)
        {
            if (recipe.material == null) continue;

            GameObject item = new GameObject($"Recipe_{recipe.material.unitName}");
            item.transform.SetParent(recipeListParent, false);
            item.AddComponent<RectTransform>();
            Text t = item.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 22;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = GetGradeColor(recipe.material.grade);

            string resultNames = "";
            if (recipe.possibleResults != null && recipe.possibleResults.Length > 0)
                foreach (var r in recipe.possibleResults)
                    resultNames += r.unitName + " ";
            else
                resultNames = "최고 등급";

            t.text = $"★{(int)recipe.material.grade} {recipe.material.unitName} x{recipe.count} → {resultNames.Trim()}";

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.minHeight = 32f;
        }
    }

    private Color GetGradeColor(UnitGrade grade)
    {
        return grade switch
        {
            UnitGrade.Common    => Color.white,
            UnitGrade.Uncommon  => new Color(0.4f, 1f, 0.4f),
            UnitGrade.Rare      => new Color(0.4f, 0.6f, 1f),
            UnitGrade.Epic      => new Color(0.8f, 0.4f, 1f),
            UnitGrade.Legendary => new Color(1f, 0.85f, 0.2f),
            _                   => Color.white
        };
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
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if (gameOverText != null)
        {
            string waveInfo = WaveManager.Instance != null ? $"{WaveManager.Instance.CurrentWave}" : "?";
            gameOverText.text = $"게임 오버!\nWave {waveInfo} 도달\n시간: {gameTimeText?.text}";
        }
    }
}
