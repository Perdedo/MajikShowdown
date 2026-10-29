
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyCardUI : MonoBehaviour
{
    [Header("Ally Image")]
    public Image allyImage;

    [Header("Player Health")]
    public Slider healthSlider;

    [Header("Ally Name")]
    public TMP_Text allyNameText;

    [Header("Ready")]
    public Image readyImage;
    public Sprite readySprite;
    public Sprite notReadySprite;

    public void SetAllyImage()
    {
        // Pegar a imagem da steam(igual o que rola no room) e colocar no card
    }

    public void SetAllyName()
    {
        // Pegar o nome do player da steam(igual o que rola no room) e colocar no card
    }

    public void SetAllyHealth(float health, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;

        // Provavelmente vai precisar de mais coisas relacionadas ao mp, mas por enquanto é só isso mesmo
    }

    public void AttAllyReadyImage(bool isReady)
    {
        if (isReady)
        {
            readyImage.sprite = readySprite;
        }
        else
        {
            readyImage.sprite = notReadySprite;
        }

        // Provavelmente vai precisar de mais coisas relacionadas ao mp, mas por enquanto é só isso mesmo
    }
}
