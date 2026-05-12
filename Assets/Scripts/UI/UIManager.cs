using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모바일 UI 관리.
/// 자원 표시, 소환 버튼, 속도 조절, 조합법 패널 등을 관리한다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("상단 HUD")]
    [SerializeField] private Text goldText;
    [SerializeField] private Text wispText;
    [SerializeField] private Text woodText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text unitCountText;

    [Header("하단 버튼")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button combineBookButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button autoCombineButton;

    [Header("속도 표시")]
    [SerializeField] private Text speedText;

    [Header("패널")]
    [SerializeField] private GameObject combineBookPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button restartButton;

    [Header("알림")]
    [SerializeField] private Text notificationText;
    private float notifTimer = 0f;

    private int currentSpeedIndex = 0;
    private float[] speedOptions = { 1f, 1.5f, 2f, 3f };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGold;
            GameManager.Instance.OnWispsChanged += UpdateWisps;
            GameManager.Instance.OnWoodChanged += UpdateWood;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnRoundChanged += UpdateRound;
            GameManager.Instance.OnUnitCountChanged += UpdateUnitCount;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        if (summonButton != null) summonButton.onClick.AddListener(OnSummonClicked);
        if (combineBookButton != null) combineBookButton.onClick.AddListener(ToggleCombineBook);
        if (speedButton != null) speedButton.onClick.AddListener(CycleSpeed);
        if (autoCombineButton != null) autoCombineButton.onClick.AddListener(ToggleAutoCombine);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);

        UpdateAllUI();
        if (combineBookPanel != null) combineBookPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (notifTimer > 0f)
        {
            notifTimer -= Time.unscaledDeltaTime;
            if (notifTimer <= 0f && notificationText != null) notificationText.text = "";
        }
        HandleDebugKeys();
    }

    private void HandleDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.S)) OnSummonClicked();
        if (Input.GetKeyDown(KeyCode.G) && GameManager.Instance != null) GameManager.Instance.AddGold(100);
        if (Input.GetKeyDown(KeyCode.W) && GameManager.Instance != null) GameManager.Instance.AddWood(3);
        if (Input.GetKeyDown(KeyCode.N) && WaveManager.Instance != null) WaveManager.Instance.SkipWave();
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(2);
        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    private void UpdateGold(int value) { if (goldText != null) goldText.text = $"골드: {value}"; }
    private void UpdateWisps(int value) { if (wispText != null) wispText.text = $"위스프: {value}"; }
    private void UpdateWood(int value) { if (woodText != null) woodText.text = $"목재: {value}"; }
    private void UpdateLives(int value) { if (livesText != null) livesText.text = $"라이프: {value}"; }
    private void UpdateRound(int value)
    {
        if (roundText != null)
            roundText.text = $"라운드: {value}/{(GameManager.Instance != null ? GameManager.Instance.MaxRounds : 75)}";
    }
    private void UpdateUnitCount(int value)
    {
        if (unitCountText != null)
            unitCountText.text = $"유카: {value}/{(GameManager.Instance != null ? GameManager.Instance.MaxUnitCount : 30)}";
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance == null) return;
        UpdateGold(GameManager.Instance.Gold);
        UpdateWisps(GameManager.Instance.Wisps);
        UpdateWood(GameManager.Instance.Wood);
        UpdateLives(GameManager.Instance.Lives);
        UpdateRound(GameManager.Instance.CurrentRound);
        UpdateUnitCount(GameManager.Instance.UnitCount);
    }

    private void OnSummonClicked()
    {
        if (SummonManager.Instance != null)
        {
            bool success = SummonManager.Instance.SummonWithWisp();
            if (!success) ShowNotification("위스프 부족 또는 인벤토리 가득!");
        }
    }

    private void ToggleCombineBook()
    {
        if (combineBookPanel != null) combineBookPanel.SetActive(!combineBookPanel.activeSelf);
    }

    private void CycleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedOptions.Length;
        SetSpeed(currentSpeedIndex);
    }

    private void SetSpeed(int index)
    {
        currentSpeedIndex = index;
        float speed = speedOptions[index];
        if (GameManager.Instance != null) GameManager.Instance.SetGameSpeed(speed);
        if (speedText != null) speedText.text = $"x{speed}";
    }

    private void ToggleAutoCombine()
    {
        if (CombineManager.Instance != null)
        {
            CombineManager.Instance.ToggleAutoCombine();
            ShowNotification($"자동 조합: {(CombineManager.Instance.IsAutoCombineEnabled ? "ON" : "OFF")}");
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Victory || state == GameState.Defeat)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null)
                gameOverText.text = state == GameState.Victory ?
                    $"승리!\n유카: {GameManager.Instance.UnitCount}" : "패배!";
        }
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationText != null) { notificationText.text = message; notifTimer = duration; }
        Debug.Log($"[UI] {message}");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGold;
            GameManager.Instance.OnWispsChanged -= UpdateWisps;
            GameManager.Instance.OnWoodChanged -= UpdateWood;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnRoundChanged -= UpdateRound;
            GameManager.Instance.OnUnitCountChanged -= UpdateUnitCount;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}
