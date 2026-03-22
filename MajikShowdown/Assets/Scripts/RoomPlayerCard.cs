using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerCard : MonoBehaviour
{
    public SyncedUIElement playerNameText, playerImage, readyImage;
    public Sprite readySpr, notReadySpr;

    public string playerName;
    public bool avatarReceived;
    public int connectionID;
    public ulong playerSteamID;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    private void Start()
    {
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if(callback.m_steamID.m_SteamID == playerSteamID)
        {
            playerImage.SyncRawImage(GetSteamImageAsTexture(callback.m_iImage));
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
        avatarReceived = true;
        return texture;
    }

    void GetPlayerIcon()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamID);
        if(imageID == -1)
        {
            return;
        }
        playerImage.SyncRawImage(GetSteamImageAsTexture(imageID));
    }

    public void SetPlayerValues()
    {
        playerNameText.SetText(playerName);
        if(!avatarReceived)
        {
            GetPlayerIcon();
        }
    }

    public void SwitchReadyIcon(bool ready)
    {
        if(ready)
        {
            readyImage.SyncImage(readySpr);
        }
        else
        {
            readyImage.SyncImage(notReadySpr);
        }
    }
}
