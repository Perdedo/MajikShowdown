using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SpellNodeDescription : MonoBehaviour
{
    [Header("Description Texts")]
    public TextMeshProUGUI descText;
    public TextMeshProUGUI statsText;

    [Header("Type Node")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI collisionsText;
    public Toggle playersToggle;
    public Toggle enemiesToggle;
    public Toggle objectsToggle;
    public Image playersToggleIcon;
    public Image enemiesToggleIcon;
    public Image objectsToggleIcon;

    public Sprite checkSprite;
    public Sprite xSprite;

    SpellType currentType;

    void Start()
    {
        playersToggle.onValueChanged.AddListener(SetPlayersCollision);
        enemiesToggle.onValueChanged.AddListener(SetEnemiesCollision);
        objectsToggle.onValueChanged.AddListener(SetObjectsCollision);
    }

    public void ShowDescription(SpellNode node)
    {
        SpellDescription(node);
        StatsDescription(node);
        MultiplierDescription(node);
        CollisionDescription(node);
    }

    public void HideDescription(SpellNode node)
    {
        descText.text = "";
        statsText.text = "";
        multiplierText.gameObject.SetActive(false);
        collisionsText.gameObject.SetActive(false);
        playersToggle.gameObject.SetActive(false);
        enemiesToggle.gameObject.SetActive(false);
        objectsToggle.gameObject.SetActive(false);
    }

    void SpellDescription(SpellNode node)
    {
        descText.text = node.spellDescription;
    }

    void StatsDescription(SpellNode node)
    {
        statsText.text = "";
        StatTypes stats = node.BaseStats;
        SpellType typeNode = node as SpellType;

        if (stats.Speed != 0)
            CreateStatsText("Speed:" + stats.Speed);

        if (stats.Duration != 0)
            CreateStatsText("Duration:" + stats.Duration);

        if (stats.Size != 0)
            CreateStatsText("Size:" + stats.Size);

        if (stats.Damage != 0)
            CreateStatsText("Damage:" + stats.Damage);

        if (stats.Piercing != 0)
            CreateStatsText("Piercing:" + stats.Piercing);

        if (stats.Bounce != 0)
            CreateStatsText("Bounce:" + stats.Bounce);

        if (stats.Knockback != 0)
            CreateStatsText("Knockback:" + stats.Knockback);
    }

    void CreateStatsText(string statName)
    {
        statsText.text += statName + "\n";
    }

    void MultiplierDescription(SpellNode node)
    {
        multiplierText.text = "";
        SpellType typeNode = node as SpellType;
        if (typeNode == null)
        {
            multiplierText.gameObject.SetActive(false);
            return;
        }
        multiplierText.gameObject.SetActive(true);
        var m = typeNode.StatMultipliers;
        CreateMultiplierText("Speed Multiplier:", m.Speed);
        CreateMultiplierText("Duration Multiplier:", m.Duration);
        CreateMultiplierText("Size Multiplier:", m.Size);
        CreateMultiplierText("Damage Multiplier:", m.Damage);
        CreateMultiplierText("Piercing Multiplier:", m.Piercing);
        CreateMultiplierText("Bounce Multiplier:", m.Bounce);
        CreateMultiplierText("Knockback Multiplier:", m.Knockback);
    }

    void CreateMultiplierText(string stat, float value)
    {
        multiplierText.text += stat + " x" + value + "\n";
    }

    void CollisionDescription(SpellNode node)
    {
        currentType = node as SpellType;
        bool isType = currentType != null;
        collisionsText.gameObject.SetActive(isType);
        playersToggle.gameObject.SetActive(isType);
        enemiesToggle.gameObject.SetActive(isType);
        objectsToggle.gameObject.SetActive(isType);
        if (!isType) return;
        playersToggle.SetIsOnWithoutNotify(currentType.Collisions.Players);
        enemiesToggle.SetIsOnWithoutNotify(currentType.Collisions.Enemies);
        objectsToggle.SetIsOnWithoutNotify(currentType.Collisions.Objects);
        playersToggleIcon.sprite = currentType.Collisions.Players ? checkSprite : xSprite;
        enemiesToggleIcon.sprite = currentType.Collisions.Enemies ? checkSprite : xSprite;
        objectsToggleIcon.sprite = currentType.Collisions.Objects ? checkSprite : xSprite;
    }

    void SetPlayersCollision(bool value)
    {
        if (currentType == null) return;
        Debug.Log("ANTES: Players Collisions = " + currentType.Collisions.Players);
        var col = currentType.Collisions;
        col.Players = value;
        currentType.Collisions = col;
        Debug.Log("DEPOIS: Players Collisions = " + currentType.Collisions.Players);
        playersToggleIcon.sprite = value ? checkSprite : xSprite;
    }

    void SetEnemiesCollision(bool value)
    {
        if (currentType == null) return;
        Debug.Log("ANTES: Enemies Collisions = " + currentType.Collisions.Enemies);
        var col = currentType.Collisions;
        col.Enemies = value;
        currentType.Collisions = col;
        Debug.Log("DEPOIS: Enemies Collisions = " + currentType.Collisions.Enemies);
        enemiesToggleIcon.sprite = value ? checkSprite : xSprite;
    }

    void SetObjectsCollision(bool value)
    {
        if (currentType == null) return;
        Debug.Log("ANTES: Objects Collisions = " + currentType.Collisions.Objects);
        var col = currentType.Collisions;
        col.Objects = value;
        currentType.Collisions = col;
        Debug.Log("DEPOIS: Objects Collisions = " + currentType.Collisions.Objects);
        objectsToggleIcon.sprite = value ? checkSprite : xSprite;
    }
}