using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SpellSelectionEntry : MonoBehaviour
{
    public Image icon;
    public TMP_Text spellName;
    public Button button;

    public void Setup(Spell spell, Sprite sprite, Color color, Action<Spell> callback)
    {
        spellName.text = spell.spellName;
        icon.sprite = sprite;
        icon.color = color;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            callback?.Invoke(spell);
        });
    }
}