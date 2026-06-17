using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;
using Mirror;
using System.Collections;
using System;
//using UnityEditor.Experimental.GraphView;

public class SpellNodeDescription : NetworkBehaviour
{
    [Header("Description Sections")]
    public GameObject descriptionSection;
    public GameObject elementSection;
    public GameObject extraSection;
    public GameObject triggerSection;
    public GameObject collisionSection;
    public GameObject statsSection;
    public GameObject multipliersSection;

    [Header("Separators")]
    public GameObject firstSep;
    public GameObject secondSep;
    public GameObject thirdSep;
    public GameObject fourthSep;
    public GameObject fifthSep;
    public GameObject sixthSep;

    [Header("Rune Element Sprites")]
    public Sprite fireIcon;
    public Sprite iceIcon;
    public Sprite earthIcon;
    public Sprite lightningIcon;
    public Sprite radianceIcon;
    public Sprite darknessIcon;
    public Sprite poisonIcon;
    public Image elementIcon;

    [Header("Rune Informations")]
    public Image cooldownIcon;
    public TextMeshProUGUI nodeCooldown;
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI nodeType;
    public TextMeshProUGUI descText;

    [Header("Extra")]
    public ExtraSlot[] extraSlots;
    Vector2[] oneExtra =
    {
        new Vector2(0, -25)
    };

    Vector2[] twoExtras =
    {
    new Vector2(-75, -25),
    new Vector2(75, -25)
    };

    Vector2[] threeExtras =
    {
    new Vector2(-125, -25),
    new Vector2(0, -25),
    new Vector2(125, -25)
    };

    [Header("Trigger")]
    public TMP_Dropdown spellDropdown;
    public TMP_Dropdown triggerDropdown;

    [Header("Collision")]
    public Toggle selfToggle;
    public Toggle alliesToggle;
    public Toggle enemiesToggle;
    public Toggle objectsToggle;
    public Image selfToggleIcon;
    public Image alliesToggleIcon;
    public Image enemiesToggleIcon;
    public Image objectsToggleIcon;
    public Sprite checkSprite;
    public Sprite xSprite;


    [Header("Rune Stats")]
    public GameObject nodeStatsContainer;
    public TextMeshProUGUI nodeSpeedText;
    public Image nodeSpeedImage;
    public TextMeshProUGUI nodeDurationText;
    public Image nodeDurationImage;
    public TextMeshProUGUI nodeSizeText;
    public Image nodeSizeImage;
    public TextMeshProUGUI nodeDamageText;
    public Image nodeDamageImage;
    public TextMeshProUGUI nodePiercingText;
    public Image nodePiercingImage;
    public TextMeshProUGUI nodeBounceText;
    public Image nodeBounceImage;
    public TextMeshProUGUI nodeKnockbackText;
    public Image nodeKnockbackImage;

    [Header("Rune Multipliers")]
    public GameObject nodeMultipliersContainer;
    public TextMeshProUGUI multiSpeedText;
    public Image multiSpeedImage;
    public TextMeshProUGUI multiDurationText;
    public Image multiDurationImage;
    public TextMeshProUGUI multiSizeText;
    public Image multiSizeImage;
    public TextMeshProUGUI multiDamageText;
    public Image multiDamageImage;
    public TextMeshProUGUI multiPiercingText;
    public Image multiPiercingImage;
    public TextMeshProUGUI multiBounceText;
    public Image multiBounceImage;
    public TextMeshProUGUI multiKnockbackText;
    public Image multiKnockbackImage;


    [Header("References and Helpers")]
    public SpellCaster caster;
    public HexGrid grid;
    Color activeColor = Color.white;
    Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    SpellType currentType;
    SpellTrigger currentTrigger;
    SpellNode currentNode;
    List<Spell> availableSpells = new List<Spell>();

    public bool network = true;

    void Start()
    {
        selfToggle.onValueChanged.AddListener(SetSelfCollision);
        alliesToggle.onValueChanged.AddListener(SetAlliesCollision);
        enemiesToggle.onValueChanged.AddListener(SetEnemiesCollision);
        objectsToggle.onValueChanged.AddListener(SetObjectsCollision);
        spellDropdown.onValueChanged.AddListener(SetTriggerSpell);
        triggerDropdown.onValueChanged.AddListener(SetTriggerType);
    }

    public void ShowDescription(SpellNode node)
    {
        currentNode = node;
        UpdateElementIcon();
        NodeInfosDescription(node);
        ExtraDescription(node);
        TriggerDescription(node);
        CollisionDescription(node);
        StatsDescription(node);
        MultiplierDescription(node);
        CheckNode(node);
        UpdateExtraSection(node);
    }

    void CheckNode(SpellNode node)
    {
        if (node is SpellType) CoreDesc();
        else if (node is SpellTrigger) TriggerDesc();
        else if (node is SpellEffect) EffectDesc();
        else if (node is SpellTrajectory) TrajectoryDesc();
        else if (node is SpellStat) StatDesc();
        else if (node is SpellCastPoint) CastPointDesc();
        else HideAll();
    }

    void UpdateExtraSection(SpellNode node)
    {
        bool hasExtra = NodeHasExtras(node);

        extraSection.SetActive(hasExtra);

        if (hasExtra)
        {
            thirdSep.SetActive(true);
        }
        else
        {
            thirdSep.SetActive(false);
        }
    }

    private struct SectionConfig
    {
        public bool Element, Description, Extra, Trigger, Collision, Stats, Multipliers;
        public bool Sep1, Sep2, Sep3, Sep4, Sep5, Sep6;
    }

    void ApplyConfig(SectionConfig c)
    {
        elementSection.gameObject.SetActive(c.Element);
        descriptionSection.gameObject.SetActive(c.Description);
        extraSection.gameObject.SetActive(c.Extra);
        triggerSection.gameObject.SetActive(c.Trigger);
        collisionSection.gameObject.SetActive(c.Collision);
        statsSection.gameObject.SetActive(c.Stats);
        multipliersSection.gameObject.SetActive(c.Multipliers);

        firstSep.gameObject.SetActive(c.Sep1);
        secondSep.gameObject.SetActive(c.Sep2);
        thirdSep.gameObject.SetActive(c.Sep3);
        fourthSep.gameObject.SetActive(c.Sep4);
        fifthSep.gameObject.SetActive(c.Sep5);
        sixthSep.gameObject.SetActive(c.Sep6);
    }

    void CoreDesc() => ApplyConfig(new SectionConfig
    {
        Element = true,
        Description = true,
        Collision = true,
        Stats = true,
        Multipliers = true,
        Sep1 = true,
        Sep2 = true,
        Sep5 = true,
        Sep6 = true
    });

    void EffectDesc()
    {
        ApplyConfig(new SectionConfig
        {
            Description = true,
            Stats = true,
            Sep2 = true,
        });
    }


    void StatDesc() => ApplyConfig(new SectionConfig
    {
        Description = true,
        Stats = true,
        Sep2 = true
    });

    void TriggerDesc() => ApplyConfig(new SectionConfig
    {
        Description = true,
        Trigger = true,
        Sep2 = true
    });

    void TrajectoryDesc() => ApplyConfig(new SectionConfig
    {
        Description = true,
        Stats = true,
        Sep2 = true
    });

    void CastPointDesc() => ApplyConfig(new SectionConfig
    {
        Description = true,
        Sep2 = true
    });

public void HideAll() => ApplyConfig(new SectionConfig());

    bool NodeHasExtras(SpellNode node)
    {
        var fields = node.GetType().GetFields();

        foreach (var field in fields)
        {
            if (Attribute.GetCustomAttribute(field, typeof(AddExtraVar)) != null)
                return true;
        }

        return false;
    }

    public void NodeCoolDownDescription(SpellNode node)
    {
        nodeCooldown.text = $"{node.Cooldown:F1}s";
    }

    void NodeInfosDescription(SpellNode node)
    {
        NodeCoolDownDescription(node);
        nodeName.text = node.runeName;
        nodeType.text = node.runeType;
        ChangeTextColor(nodeType, node);
        descText.text = node.runeDescription;
    }

    void ChangeTextColor(TextMeshProUGUI text, SpellNode node)
    {
        Color color = new Color(1f, 1f, 1f, 1f);
        if(node is SpellType)
        {
            color = Color.red;
        }
        else if (node is SpellTrigger)
        {
            color = Color.orange;
        }
        else if (node is SpellEffect)
        {
            color = Color.purple;
        }
        else if (node is SpellTrajectory)
        {
            color = Color.blue;
        }
        else if (node is SpellCastPoint)
        {
            color = Color.green;
        }
        text.color = color;
    }

    void ExtraDescription(SpellNode node)
    {
        foreach (var slot in extraSlots)
        {
            slot.gameObject.SetActive(false);
        }
        var extras = new List<(AddExtraVar attr, SpellNode.ExtraVar extra)>();
        foreach (var field in node.GetType().GetFields())
        {
            var extraAttr = (AddExtraVar)Attribute.GetCustomAttribute(field, typeof(AddExtraVar));
            if (extraAttr == null) continue;
            SpellNode.ExtraVar extra = (SpellNode.ExtraVar)field.GetValue(node);

            extras.Add((extraAttr, extra));
        }
        Vector2[] positions = GetExtraPositions(extras.Count);
        for (int i = 0; i < extras.Count && i < extraSlots.Length; i++)
        {
            ExtraSlot slot = extraSlots[i];
            slot.gameObject.SetActive(true);
            RectTransform rect = slot.GetComponent<RectTransform>();
            rect.anchoredPosition = positions[i];
            slot.nameText.text = extras[i].attr.DisplayName;
            slot.valueText.text = FormatStat(extras[i].extra.Value);
            slot.icon.transform.GetChild(0).GetComponent<Image>().sprite = extras[i].extra.Icon;
            PopupUI popup = slot.icon.GetComponent<PopupUI>();
            if (popup != null)
            {
                popup.SetElementText(extras[i].attr.DisplayName);
            }
        }
    }

    Vector2[] GetExtraPositions(int count)
    {
        return count switch
        {
            1 => oneExtra,
            2 => twoExtras,
            _ => threeExtras
        };
    }

    void StatsDescription(SpellNode node)
    {
        StatTypes stats = node.BaseStats;

        nodeStatsContainer.SetActive(true);

        UpdateStatVisual(nodeSpeedText, nodeSpeedImage, stats.Speed);
        UpdateStatVisual(nodeDurationText, nodeDurationImage, stats.Duration);
        UpdateStatVisual(nodeSizeText, nodeSizeImage, stats.Size);
        UpdateStatVisual(nodeDamageText, nodeDamageImage, stats.Damage);
        UpdateStatVisual(nodePiercingText, nodePiercingImage, stats.Piercing);
        UpdateStatVisual(nodeBounceText, nodeBounceImage, stats.Bounce);
        UpdateStatVisual(nodeKnockbackText, nodeKnockbackImage, stats.Knockback);
    }

    void UpdateStatVisual(TextMeshProUGUI text, Image image, float value)
    {
        bool isActive = value != 0;
        text.text = FormatStat(value);
        text.color = isActive ? activeColor : inactiveColor;
        image.color = isActive ? activeColor : inactiveColor;
        image.transform.GetChild(0).GetComponent<Image>().color = isActive ? activeColor : inactiveColor;
    }

    void UpdateMultiplierVisual(TextMeshProUGUI text, Image image, float value)
    {
        text.text = $"x{FormatStat(value)}";
        image.color = Color.white;
    }

    string FormatStat(float value)
    {
        if (float.IsInfinity(value)) return value > 0 ? "+\u221E" : "-\u221E"; //infinity symbol, positive and negative
        if (float.IsNaN(value)) return "NaN";
        return value.ToString("F1");
    }

    void MultiplierDescription(SpellNode node)
    {
        SpellType typeNode = node as SpellType;
        if (typeNode == null)
        {
            nodeMultipliersContainer.SetActive(false);
            return;
        }
        nodeMultipliersContainer.SetActive(true);
        var m = typeNode.StatMultipliers;
        UpdateMultiplierVisual(multiSpeedText, multiSpeedImage, m.Speed);
        UpdateMultiplierVisual(multiDurationText, multiDurationImage, m.Duration);
        UpdateMultiplierVisual(multiSizeText, multiSizeImage, m.Size);
        UpdateMultiplierVisual(multiDamageText, multiDamageImage, m.Damage);
        UpdateMultiplierVisual(multiPiercingText, multiPiercingImage, m.Piercing);
        UpdateMultiplierVisual(multiBounceText, multiBounceImage, m.Bounce);
        UpdateMultiplierVisual(multiKnockbackText, multiKnockbackImage, m.Knockback);
    }

    void CollisionDescription(SpellNode node)
    {
        currentType = node as SpellType;
        bool isType = currentType != null;
        selfToggle.gameObject.SetActive(isType);
        alliesToggle.gameObject.SetActive(isType);
        enemiesToggle.gameObject.SetActive(isType);
        objectsToggle.gameObject.SetActive(isType);
        if (!isType) return;
        selfToggle.SetIsOnWithoutNotify(currentType.Collisions.Self);
        alliesToggle.SetIsOnWithoutNotify(currentType.Collisions.Allies);
        enemiesToggle.SetIsOnWithoutNotify(currentType.Collisions.Enemies);
        objectsToggle.SetIsOnWithoutNotify(currentType.Collisions.Objects);
        selfToggleIcon.sprite = currentType.Collisions.Self ? checkSprite : xSprite;
        alliesToggleIcon.sprite = currentType.Collisions.Allies ? checkSprite : xSprite;
        enemiesToggleIcon.sprite = currentType.Collisions.Enemies ? checkSprite : xSprite;
        objectsToggleIcon.sprite = currentType.Collisions.Objects ? checkSprite : xSprite;
    }

    void SetSelfCollision(bool value)
    {
        if (currentType == null) return;
        var col = currentType.Collisions;
        col.Self = value;
        currentType.Collisions = col;
        selfToggleIcon.sprite = value ? checkSprite : xSprite;
        currentType.OwnerSpell?.UpdateSpell();
    }

    void SetAlliesCollision(bool value)
    {
        if (currentType == null) return;
        var col = currentType.Collisions;
        col.Allies = value;
        currentType.Collisions = col;
        alliesToggleIcon.sprite = value ? checkSprite : xSprite;
        currentType.OwnerSpell?.UpdateSpell();
    }

    void SetEnemiesCollision(bool value)
    {
        if (currentType == null) return;
        var col = currentType.Collisions;
        col.Enemies = value;
        currentType.Collisions = col;
        enemiesToggleIcon.sprite = value ? checkSprite : xSprite;
        currentType.OwnerSpell?.UpdateSpell();
    }

    void SetObjectsCollision(bool value)
    {
        if (currentType == null) return;
        var col = currentType.Collisions;
        col.Objects = value;
        currentType.Collisions = col;
        objectsToggleIcon.sprite = value ? checkSprite : xSprite;
        currentType.OwnerSpell?.UpdateSpell();
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
        if(!isServer && network)
        {
            CMDTriggerDescription(node.Interface.acquisitionOrder);
        }
    }

    [Command]
    void CMDTriggerDescription(int index)
    {
        SpellNode node = caster.commander.interfaces.Find(i => i.acquisitionOrder == index).Node;
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
        if(!isServer && network)
        {
            CMDSetupSpellDropdown();
        }
    }

    [Command]
    void CMDSetupSpellDropdown()
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

    string GetTriggerLabel(SpellTrigger.Triggers trigger)
    {
        switch (trigger)
        {
            case SpellTrigger.Triggers.OnCast: return "Is Cast";
            case SpellTrigger.Triggers.OnHit: return "Hits";
            case SpellTrigger.Triggers.OnDeath: return "Finish";
            default: return trigger.ToString();
        }
    }

    List<string> GetTriggerOptions()
    {
        List<string> options = new List<string>();
        foreach (SpellTrigger.Triggers t in System.Enum.GetValues(typeof(SpellTrigger.Triggers)))
        {
            options.Add(GetTriggerLabel(t));
        }
        return options;
    }

    void SetupTriggerDropdown()
    {
        triggerDropdown.ClearOptions();
        triggerDropdown.AddOptions(GetTriggerOptions());
        if (!isServer && network)
        {
            CMDSetupTriggerDropdown();
        }
    }

    [Command]
    void CMDSetupTriggerDropdown()
    {
        triggerDropdown.ClearOptions();
        triggerDropdown.AddOptions(GetTriggerOptions());
    }

    /*void SetupTriggerDropdown()
    {
        triggerDropdown.ClearOptions();
        var enumNames = System.Enum.GetNames(typeof(SpellTrigger.Triggers));
        triggerDropdown.AddOptions(new List<string>(enumNames));
        if(!isServer && network)
        {
            CMDSetupTriggerDropdown();
        }
    }

    [Command]
    void CMDSetupTriggerDropdown()
    {
        triggerDropdown.ClearOptions();
        var enumNames = System.Enum.GetNames(typeof(SpellTrigger.Triggers));
        triggerDropdown.AddOptions(new List<string>(enumNames));
    }*/
    

    void SetTriggerSpell(int index)
    {
        if (currentTrigger == null) return;
        if (index < 0 || index >= availableSpells.Count) return;
        currentTrigger.TriggeredSpell = availableSpells[index];
        currentTrigger.UpdateTrigger();
        NodeCoolDownDescription(currentTrigger);
        if(!isServer && network)
        {
            CMDSetTriggerSpell(index);
        }
    }

    void SetTriggerType(int index)
    {
        if (currentTrigger == null) return;

        if (System.Enum.IsDefined(typeof(SpellTrigger.Triggers), index))
        {
            currentTrigger.trigger = (SpellTrigger.Triggers)index;
        }

        if(!isServer && network)
        {
            CMDSetTriggerType(index);
        }
    }

    [Command]
    void CMDSetTriggerSpell(int index)
    {
        if (currentTrigger == null) return;
        if (index < 0 || index >= availableSpells.Count) return;
        currentTrigger.TriggeredSpell = availableSpells[index];
        currentTrigger.UpdateTrigger();
        NodeCoolDownDescription(currentTrigger);
    }

    [Command]
    void CMDSetTriggerType(int index)
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
        if(!isServer && network)
        {
            CMDRefreshTriggerUI();
        }
    }

    [Command]
    public void CMDRefreshTriggerUI()
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

    void UpdateElementIcon()
    {
        SpellType typeNode = currentNode as SpellType;
        if (typeNode == null)
        {
            elementIcon.gameObject.SetActive(false);
            return;
        }
        elementIcon.gameObject.SetActive(true);
        elementIcon.sprite = GetElementSprite(typeNode.Element);
        elementIcon.color = Color.white;
        PopupUI popup = elementIcon.GetComponent<PopupUI>();

        if (popup != null)
        {
            popup.SetElementText(typeNode.Element.ToString());
        }
    }

    Sprite GetElementSprite(Elements element)
    {
        switch (element)
        {
            case Elements.Fire: return fireIcon;
            case Elements.Ice: return iceIcon;
            case Elements.Earth: return earthIcon;
            case Elements.Lightning: return lightningIcon;
            case Elements.Radiance: return radianceIcon;
            case Elements.Darkness: return darknessIcon;
            case Elements.Poison: return poisonIcon;
            case Elements.None:
            default: return null;
        }
    }
}