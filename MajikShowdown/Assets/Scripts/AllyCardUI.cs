
using Mirror;
using Steamworks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyCardUI : NetworkBehaviour
{
    public Player myPlayer;
    [Header("Ally Image")]
    public RawImage allyImage;

    [Header("Player Health")]
    public Slider healthSlider;

    [Header("Ally Name")]
    public TMP_Text allyNameText;

    [Header("Ready")]
    public Image readyImage;
    public Sprite readySprite;
    public Sprite notReadySprite;

    protected Callback<AvatarImageLoaded_t> imageLoaded;
    public NetworkIdentity id;
    public override void OnStartServer()
    {
        id = GetComponentInParent<NetworkIdentity>(true);
    }

    void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == myPlayer.playerSteamID)
        {
            allyImage.texture = GetSteamImageAsTexture(callback.m_iImage);
            gameObject.SetActive(true);
        }
        else
        {
            return;
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture;
    }
    public void Initialize(Player p)
    {
        myPlayer = p;
        myPlayer.myCards.Add(this);
        SetupAllyHealth(myPlayer.DamageHandler.MaxHealth, myPlayer.DamageHandler.MaxHealth);
        if(myPlayer.steamSynced && myPlayer.nameSynced)
        {
            SetAllyName(myPlayer.playerName);
            imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
        }
        else
        {
            StartCoroutine(WaitInitialize());
        }
    }
    
    IEnumerator WaitInitialize()
    {
        yield return new WaitUntil(() => myPlayer.steamSynced);
        yield return new WaitUntil(() => myPlayer.nameSynced);
        SetAllyName(myPlayer.playerName);
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    /*public void SetAllyImage()
    {
        
    }*/

    public void SetAllyName(string name)
    {
        allyNameText.text = name;
    }

    public void SetupAllyHealth(float health, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;

        // Provavelmente vai precisar de mais coisas relacionadas ao mp, mas por enquanto é só isso mesmo
    }

    [TargetRpc]
    public void UpdateAllyHealth(NetworkConnectionToClient target, float health)
    {
        healthSlider.value = health;
    }


    /*public void AttAllyReadyImage(bool isReady)
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
    }*/

    [TargetRpc]
    public void UpdateAllyReady(NetworkConnectionToClient target, bool isReady)
    {
        if (isReady)
        {
            readyImage.sprite = readySprite;
        }
        else
        {
            readyImage.sprite = notReadySprite;
        }
    }

    [TargetRpc]
    public void Disable(NetworkConnectionToClient target)
    {
        gameObject.SetActive(false);
    }
}
