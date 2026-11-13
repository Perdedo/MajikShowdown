using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public TextMeshProUGUI pageNumber;
    public Button previous;
    public Button next;
    public Button ok;
    public Image image;
    public VideoPlayer videoPlayer;
    [HideInInspector] public int currentPageIndex;
    private int totalPages;
    private bool hasVideo;


    private void OnEnable()
    {
        previous.onClick.AddListener(OnPreviousClicked);
        next.onClick.AddListener(OnNextClicked);
        ok.onClick.AddListener(OnOkClicked);
    }

    private void OnDisable()
    {
        previous.onClick.RemoveListener(OnPreviousClicked);
        next.onClick.RemoveListener(OnNextClicked);
        ok.onClick.RemoveListener(OnOkClicked);
    }

    public void ChangePopupDetails(string titleText, string descText, Sprite sprite, VideoClip videoClip, int currentPage, int totalPage)
    {
        SetTitle(titleText);
        if(descText != null)
        {
            SetDesc(descText);
        }
        if(image != null)
        {
            SetImage(sprite);
        }
        if (videoClip != null)
        {
            SetVideoClip(videoClip);
        }
        SetPageNumber(currentPage, totalPage);
        SetPreviousButtonActive(currentPage > 1);
        SetNextButtonActive(currentPage < totalPage);
        SetOkButtonActive(currentPage == totalPage);
        popupPanel.SetActive(true);
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetDesc(string text)
    {
        desc.text = text;
    }

    public void SetPageNumber(int current, int total)
    {
        pageNumber.text = $"{current}/{total}";
    }

    public void SetPreviousButtonActive(bool active)
    {
        previous.gameObject.SetActive(active);
    }

    public void SetNextButtonActive(bool active)
    {
        next.gameObject.SetActive(active);
    }

    public void SetOkButtonActive(bool active)
    {
        ok.gameObject.SetActive(active);
    }

    public void OnPreviousClicked()
    {
        // Handle previous button click
    }

    public void OnNextClicked()
    {
        // Handle next button click
    }

    public void OnOkClicked()
    {
        Debug.Log(popupPanel);
        popupPanel.SetActive(false);
        GameManager.Instance.uiController.playerUI.EnableGameplayCursor();
    }

    public void SetImage(Sprite sprite)
    {
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            image.gameObject.SetActive(false);
        }
    }

    public void SetVideoClip(VideoClip clip)
    {
        if (clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.gameObject.SetActive(true);
            hasVideo = true;
        }
        else
        {
            videoPlayer.gameObject.SetActive(false);
            hasVideo = false;
        }
    }
}
