using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 조합법 UI 패널.
/// 모든 조합법을 등급별로 표시하고, 재료 보유 여부를 확인할 수 있다.
/// </summary>
public class CombineBookUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button closeButton;

    [Header("탭 버튼")]
    [SerializeField] private Button tabBasicButton;
    [SerializeField] private Button tabHiddenButton;
    [SerializeField] private Button tabLegendaryButton;
    [SerializeField] private Button tabTranscendentButton;

    private enum CombineTab { Basic, Hidden, Legendary, Transcendent }
    private CombineTab currentTab = CombineTab.Basic;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        if (tabBasicButton != null) tabBasicButton.onClick.AddListener(() => ShowTab(CombineTab.Basic));
        if (tabHiddenButton != null) tabHiddenButton.onClick.AddListener(() => ShowTab(CombineTab.Hidden));
        if (tabLegendaryButton != null) tabLegendaryButton.onClick.AddListener(() => ShowTab(CombineTab.Legendary));
        if (tabTranscendentButton != null) tabTranscendentButton.onClick.AddListener(() => ShowTab(CombineTab.Transcendent));
    }

    private void OnEnable() { RefreshUI(); }

    private void ShowTab(CombineTab tab) { currentTab = tab; RefreshUI(); }

    private void RefreshUI()
    {
        if (contentParent != null)
            foreach (Transform child in contentParent) Destroy(child.gameObject);

        if (currentTab == CombineTab.Basic) ShowBasicCombines();
        else ShowSpecialCombines();
    }

    private void ShowBasicCombines()
    {
        string[] basicInfo = {
            "흔함 x3 -> 안흔함",
            "안흔함 x3 -> 특별함",
            "특별함 x3 -> 희귀함",
            "희귀함 + 조합법 -> 전설/히든"
        };
        foreach (string info in basicInfo) CreateTextEntry(info, Color.white);

        if (SummonManager.Instance != null)
        {
            CreateTextEntry("\n--- 조합 가능 ---", Color.yellow);
            var units = SummonManager.Instance.InventoryUnits;
            Dictionary<string, int> unitCounts = new Dictionary<string, int>();
            foreach (var unit in units)
            {
                if (unit == null || unit.UnitData == null) continue;
                string key = $"{unit.UnitData.unitName}({unit.UnitData.grade})";
                if (!unitCounts.ContainsKey(key)) unitCounts[key] = 0;
                unitCounts[key]++;
            }
            foreach (var kvp in unitCounts)
            {
                Color c = kvp.Value >= 3 ? Color.green : Color.gray;
                CreateTextEntry($"  {kvp.Key}: {kvp.Value}/3{(kvp.Value >= 3 ? " 조합가능!" : "")}", c);
            }
        }
    }

    private void ShowSpecialCombines()
    {
        if (CombineManager.Instance == null) return;
        CombineRecipe[] recipes = CombineManager.Instance.GetAllSpecialRecipes();
        if (recipes == null || recipes.Length == 0)
        {
            CreateTextEntry("특수 조합법이 없습니다.", Color.gray);
            return;
        }

        UnitGrade targetGrade = currentTab switch
        {
            CombineTab.Hidden => UnitGrade.Hidden,
            CombineTab.Legendary => UnitGrade.Legendary,
            CombineTab.Transcendent => UnitGrade.Transcendent,
            _ => UnitGrade.Common
        };

        foreach (var recipe in recipes)
        {
            if (recipe == null || recipe.resultUnit == null) continue;
            if (recipe.resultUnit.grade != targetGrade) continue;

            string materials = "";
            foreach (var mat in recipe.materials)
            {
                if (mat.unitData != null) materials += $"{mat.unitData.unitName} x{mat.count} + ";
            }
            if (recipe.woodRequired > 0) materials += $"목재 x{recipe.woodRequired}";
            else materials = materials.TrimEnd('+', ' ');

            string entry = $"{recipe.resultUnit.unitName} ({recipe.resultUnit.grade})\n  재료: {materials}";
            CreateTextEntry(entry, recipe.resultUnit.GetGradeColor());
        }
    }

    private void CreateTextEntry(string text, Color color)
    {
        if (contentParent == null) return;
        GameObject entryObj = new GameObject("RecipeEntry");
        entryObj.transform.SetParent(contentParent, false);
        Text textComp = entryObj.AddComponent<Text>();
        textComp.text = text;
        textComp.color = color;
        textComp.fontSize = 14;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform rt = entryObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 40);
        ContentSizeFitter fitter = entryObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}
