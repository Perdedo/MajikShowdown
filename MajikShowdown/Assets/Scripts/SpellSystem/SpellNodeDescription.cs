using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;

public class SpellNodeDescription : MonoBehaviour
{
    [Header("Description Texts")]
    public TextMeshProUGUI nodeCooldown;
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
    public TextMeshProUGUI elementText;

    public Sprite checkSprite;
    public Sprite xSprite;

    SpellType currentType;

    [Header("Trigger")]
    public TMP_Dropdown spellDropdown;
    public TMP_Dropdown triggerDropdown;

    SpellTrigger currentTrigger;
    public SpellCaster caster;
    List<Spell> availableSpells = new List<Spell>();
    public HexGrid grid;
    SpellNode currentNode;

    void Start()
    {
        playersToggle.onValueChanged.AddListener(SetPlayersCollision);
        enemiesToggle.onValueChanged.AddListener(SetEnemiesCollision);
        objectsToggle.onValueChanged.AddListener(SetObjectsCollision);
        spellDropdown.onValueChanged.AddListener(SetTriggerSpell);
        triggerDropdown.onValueChanged.AddListener(SetTriggerType);
    }

    public void ShowDescription(SpellNode node)
    {
        currentNode = node;
        NodeCoolDownDescription(node);
        SpellDescription(node);
        StatsDescription(node);
        MultiplierDescription(node);
        CollisionDescription(node);
        TriggerDescription(node);
        UpdateElementColor();
    }

    public void HideDescription()
    {
        nodeCooldown.text = "";
        descText.text = "";
        statsText.text = "";
        elementText.text = "";
        multiplierText.gameObject.SetActive(false);
        collisionsText.gameObject.SetActive(false);
        playersToggle.gameObject.SetActive(false);
        enemiesToggle.gameObject.SetActive(false);
        objectsToggle.gameObject.SetActive(false);
        spellDropdown.gameObject.SetActive(false);
        triggerDropdown.gameObject.SetActive(false);
    }

    void NodeCoolDownDescription(SpellNode node)
    {
        nodeCooldown.text = "Cooldown : +" + node.Cooldown + "s";
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
        multiplierText.text = "Multipliers\n";
        var m = typeNode.StatMultipliers;
        CreateMultiplierText("Speed: ", m.Speed);
        CreateMultiplierText("Duration: ", m.Duration);
        CreateMultiplierText("Size: ", m.Size);
        CreateMultiplierText("Damage: ", m.Damage);
        CreateMultiplierText("Piercing: ", m.Piercing);
        CreateMultiplierText("Bounce: ", m.Bounce);
        CreateMultiplierText("Knockback: ", m.Knockback);
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

    void TriggerDescription(SpellNode node)
    {
        currentTrigger = node as SpellTrigger;
        bool isTrigger = currentTrigger != null;
        spellDropdown.gameObject.SetActive(isTrigger);
        triggerDropdown.gameObject.SetActive(isTrigger);
        if (!isTrigger) return;
        spellDropdown.onValueChanged.RemoveListener(SetTriggerSpell);
        triggerDropdown.onValueChanged.RemoveListener(SetTriggerType);
        SetupSpellDropdown();
        SetupTriggerDropdown();
        RefreshTriggerUI();
        spellDropdown.onValueChanged.AddListener(SetTriggerSpell);
        triggerDropdown.onValueChanged.AddListener(SetTriggerType);
    }

    void SetupSpellDropdown()
    {
        var spells = caster.spells;
        spellDropdown.ClearOptions();
        availableSpells.Clear();
        List<string> names = new List<string>();
        names.Add("None");
        availableSpells.Add(null);
        foreach (var s in spells)
        {
            names.Add(s.spellName);
            availableSpells.Add(s);
        }
        spellDropdown.AddOptions(names);
    }

    void SetupTriggerDropdown()
    {
        triggerDropdown.ClearOptions();
        var enumNames = System.Enum.GetNames(typeof(SpellTrigger.Triggers));
        triggerDropdown.AddOptions(new List<string>(enumNames));
    }

    void SetTriggerSpell(int index)
    {
        if (currentTrigger == null) return;
        if (index < 0 || index >= availableSpells.Count) return;
        currentTrigger.TriggeredSpell = availableSpells[index];
    }

    void SetTriggerType(int index)
    {
        if (currentTrigger == null) return;

        if (System.Enum.IsDefined(typeof(SpellTrigger.Triggers), index))
        {
            currentTrigger.trigger = (SpellTrigger.Triggers)index;
        }
    }

    public void RefreshTriggerUI()
    {
        if (currentTrigger == null) return;
        int spellIndex = availableSpells.IndexOf(currentTrigger.TriggeredSpell);
        if (spellIndex < 0)
        {
            spellIndex = 0;
        }
        spellDropdown.SetValueWithoutNotify(spellIndex);
        int triggerIndex = (int)currentTrigger.trigger;
        if (triggerIndex < 0 || triggerIndex >= triggerDropdown.options.Count)
        {
            triggerIndex = 0;
        }
        triggerDropdown.SetValueWithoutNotify(triggerIndex);
    }

    public static Color GetElementColor(Elements element)
    {
        switch (element)
        {
            case Elements.Fire:
                return Color.red;

            case Elements.Ice:
                return Color.lightBlue;

            case Elements.Earth:
                return new Color(0.735f, 0.535f, 0.380f);

            case Elements.Lightning:
                return Color.yellow;

            case Elements.Radiance:
                return new Color(1f, 0.985f, 0.61f);

            case Elements.Darkness:
                return new Color(0.825f, 0.45f, 1f); ;

            case Elements.Poison:
                return new Color(0.4f, 0.8f, 0.2f);

            case Elements.None:
            default:
                return Color.white;
        }
    }
    void UpdateElementColor()
    {
        SpellType typeNode = currentNode as SpellType;
        if (typeNode == null)
        {
            elementText.gameObject.SetActive(false);
            return;
        }
        elementText.gameObject.SetActive(true);
        Color color = GetElementColor(typeNode.Element);
        elementText.color = color;
        //elementText.text = typeNode.Element.ToString();
        elementText.text = "Element: " + typeNode.Element.ToString();
    }
}