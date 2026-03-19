using Mirror;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Game Title")]
    public GameObject gameTitle;

    [Header("Menu Buttons")]
    public GameObject menuHostGameButton;
    public GameObject menuJoinGameButton;
    public GameObject menuOptionsButton;
    public GameObject menuCreditsButton;
    public GameObject menuQuitGameButton;

    [Header("Menu Panels")]
    public GameObject menuJoinGamePanel;
    public GameObject menuOptionsPanel;
    public GameObject menuCreditsPanel;
    public GameObject menuQuitGamePanel;

    [Header("Room Texts")]
    public TextMeshProUGUI roomID;
    public TextMeshProUGUI players;
    public TextMeshProUGUI playersReady;

    [Header("Room Buttons")]
    public GameObject gameRulesButton;
    public GameObject optionsButton;
    public GameObject readyButton;
    public TextMeshProUGUI readyButtonText;
    public GameObject leaveRoomButton;
    public GameObject startGameButton;

    [Header("Room Panels")]
    public GameObject gameRulesPanel;
    public GameObject optionsPanel;
    public GameObject leaveRoomPanel;

    [Header("Join Game Panel Objects")]
    public GameObject enterRoomCodeInputField;
    public GameObject errorMessage;
    public GameObject leaveJoinGamePanel;

    [Header("Options Panel Objects")]
    public Slider _masterVolumeSlider;
    public Slider _musicSlider;
    public Slider _sfxSlider; 
    public Toggle vsyncToggle;
    public Image vsyncToggleImage; 
    public Sprite toggleConfirm;
    public Sprite toggleDeny; 
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    public TMP_Dropdown resDropdown;
    public TMP_Dropdown screenModeDropdown;

    [Header("Credits Panel Objects")]
    public GameObject creditsScrollView;
    public GameObject leaveCreditsPanel;

    [Header("Quit Game Panel Objects")]
    public GameObject confirmQuitGame;
    public GameObject leaveQuitGamePanel;

    [Header("GameRules Panel Objects")]
    public GameObject leaveGameRulesPanel;

    [Header("Leave Room Panel Objects")]
    public GameObject confirmLeaveRoom;
    public GameObject returnLeaveRoomPanel;

    [Header("Test Panels")]
    public GameObject gridPanel;

    [HideInInspector]
    public ConfigData data;
    public HexGrid activeGrid;

    void Awake()
    {
        UiMenuSetup();
    }

    void Start()
    {
        GameManager.Instance.uiController = this;
        ResolutionDropdown();
        ScreenModeDropdown();
        File.Delete(Application.persistentDataPath + "/configSave.json");
        if (File.Exists(Application.persistentDataPath + "/configSave.json"))
        {
            SaveManager.LoadConfig();
            Debug.Log("Save Carregado");
        }
        else
        {
            data = new ConfigData(0, 0, 0f, -15f, -15f, false);
            Debug.Log("Sem Save Carregado");
        }
        ConfigUpdate();
        if(vsyncToggle != null)
        {
            vsyncToggle.onValueChanged.RemoveAllListeners();
            vsyncToggle.onValueChanged.AddListener(ChangeVsyncToggle);
        }
        AudioController.instance.StartMusic();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(gridPanel.activeSelf)
            {
                ClosePanel(gridPanel);
            }
            else
            {
                OpenPanel(gridPanel);
            }
        }
    }
    public void UiMenuSetup()
    {
        if (gameTitle != null)
        {
            gameTitle.SetActive(true);
        }
        if (menuHostGameButton != null)
        {
            menuHostGameButton.SetActive(true);
        }
        if (menuJoinGameButton != null)
        {
            menuJoinGameButton.SetActive(true);
        }
        if (menuOptionsButton != null)
        {
            menuOptionsButton.SetActive(true);
        }
        if (menuCreditsButton != null)
        {
            menuCreditsButton.SetActive(true);
        }
        if (menuQuitGameButton != null)
        {
            menuQuitGameButton.SetActive(true);
        }
        if (menuJoinGamePanel != null)
        {
            menuJoinGamePanel.SetActive(false);
        }
        if (menuOptionsPanel != null)
        {
            menuOptionsPanel.SetActive(false);
        }
        if (menuCreditsPanel != null)
        {
            menuCreditsPanel.SetActive(false);
        }
        if (menuQuitGamePanel != null)
        {
            menuQuitGamePanel.SetActive(false);
        }
        if (roomID != null)
        {
            roomID.gameObject.SetActive(true);
        }
        if (players != null)
        {
            players.gameObject.SetActive(true);
        }
        if (playersReady != null)
        {
            playersReady.gameObject.SetActive(true);
        }
        if (gameRulesButton != null)
        {
            gameRulesButton.SetActive(true);
        }
        if (optionsButton != null)
        {
            optionsButton.SetActive(true);
        }
        if (readyButton != null)
        {
            readyButton.SetActive(true);
        }
        if (leaveRoomButton != null)
        {
            leaveRoomButton.SetActive(true);
        }
        if (startGameButton != null)
        {
            startGameButton.SetActive(true);
            startGameButton.GetComponent<Button>().interactable = false;
            /*if(NetworkServer.active)
            {
                startGameButton.GetComponent<SyncedUIElement>().ShowOnlyForHost(false);
            }*/
        }
        if (gameRulesPanel != null)
        {
            gameRulesPanel.SetActive(false);
        }
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        if (gridPanel != null)
        {
            gridPanel.SetActive(false);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Loading Scene: " + sceneName);
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            IsMenuObjectsVisible(false);
            if (panel == menuOptionsPanel)
            {
                AttVolumeSliders();
            }
        }
        else if (SceneManager.GetActiveScene().name == "Room")
        {
            IsRoomObjectsVisible(false);
            if(panel == optionsPanel)
            {
                AttVolumeSliders();
            }
        }
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            IsMenuObjectsVisible(true);
            if(panel == menuOptionsPanel)
            {
                SaveManager.SaveConfig();
            }
        }
        else if (SceneManager.GetActiveScene().name == "Room")
        {
            IsRoomObjectsVisible(true);
            if(panel == optionsPanel)
            {
                SaveManager.SaveConfig();
            }
        }
        /*if(panel == gridPanel)
        {
            HexGrid.instance.selectedNode = null;
        }*/
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Leaving Game");
    }

    public void IsMenuObjectsVisible(bool state)
    {
        gameTitle.SetActive(state);
        menuHostGameButton.SetActive(state);
        menuJoinGameButton.SetActive(state);
        menuOptionsButton.SetActive(state);
        menuCreditsButton.SetActive(state);
        menuQuitGameButton.SetActive(state);
    }

    public void IsRoomObjectsVisible(bool state)
    {
        roomID.gameObject.SetActive(state);
        players.gameObject.SetActive(state);
        playersReady.gameObject.SetActive(state);
        gameRulesButton.SetActive(state);
        optionsButton.SetActive(state);
        readyButton.SetActive(state);
        leaveRoomButton.SetActive(state);
        startGameButton.SetActive(state);
    }

    public void ChangeMasterVolume()
    {
        float value = _masterVolumeSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f);
        data.master = dB;
        AudioController.instance.ChangeMasterVol(dB);
    }

    public void ChangeMusicVolume()
    {
        float value = _musicSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f); 
        data.music = dB;
        AudioController.instance.ChangeMusicVol(dB);
    }

    public void ChangeSFXVolume()
    {
        float value = _sfxSlider.value;
        float dB = (value == 0f) ? -80f : Mathf.Lerp(-30f, 0f, value / 30f);
        data.sfx = dB;
        AudioController.instance.ChangeSFXVol(dB);
    }

    public void AttVolumeSliders()
    {
        if (AudioController.instance.mixer != null)
        {
            AudioController.instance.mixer.GetFloat("MasterVol", out float aux1);
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = Mathf.RoundToInt(Mathf.InverseLerp(-30f, 0f, aux1) * 30f);
            }
            AudioController.instance.mixer.GetFloat("MusicVol", out float aux2);
            if (_musicSlider != null)
            {
                _musicSlider.value = Mathf.RoundToInt(Mathf.InverseLerp(-30f, 0f, aux2) * 30f);
            }
            AudioController.instance.mixer.GetFloat("SFXVol", out float aux3);
            if (_sfxSlider != null)
            {
                _sfxSlider.value = Mathf.RoundToInt(Mathf.InverseLerp(-30f, 0f, aux3) * 30f); ;
            }
        }
    }

    public void ResolutionDropdown()
    {
        allRes = Screen.resolutions;
        Array.Sort(allRes, (a, b) =>
        {
            int widthComparison = b.width.CompareTo(a.width);
            return widthComparison == 0 ? b.height.CompareTo(a.height) : widthComparison;
        });
        string newRes;
        List<string> resStringList = new List<string>();
        foreach (Resolution res in allRes)
        {
            float aspectRatio = (float)res.width / res.height;
            if (Math.Abs(aspectRatio - 16f / 9f) < 0.01f)
            {
                if (res.width >= 800)
                {
                    newRes = res.width.ToString() + "x" + res.height.ToString();
                    if (!resStringList.Contains(newRes))
                    {
                        resStringList.Add(newRes);
                        selectedResList.Add(res);
                    }
                }
            }
        }
        if (resDropdown != null)
        {
            resDropdown.ClearOptions();
            resDropdown.AddOptions(resStringList);
        }
    }

    public void ChangeRes()
    {
        data.selectedRes = resDropdown.value;
        Screen.SetResolution(selectedResList[data.selectedRes].width, selectedResList[data.selectedRes].height, Screen.fullScreenMode);
    }

    public void ScreenModeDropdown()
    {
        List<string> screenModes = new List<string> { "Fullscreen Mode", "Borderless Mode", "Window Mode" };
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(screenModes);
            screenModeDropdown.onValueChanged.AddListener((int index) =>
            {
                if (index == 0)
                {
                    data.screenMode = 0;
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                }
                else if (index == 1)
                {
                    data.screenMode = 1;
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                else if (index == 2)
                {
                    data.screenMode = 2;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }
            });
        }
    }

    public void UpdateVsyncToggleImages(bool isOn)
    {
        if (isOn)
        {
            vsyncToggleImage.sprite = toggleConfirm;
        }
        else
        {
            vsyncToggleImage.sprite = toggleDeny;
        }
    }
    public void ChangeVsyncToggle(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        data.vsyncEnabled = isOn;
        UpdateVsyncToggleImages(isOn);
    }

    public void ConfigUpdate()
    {
        if (resDropdown != null)
        {
            resDropdown.value = data.selectedRes;
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.value = data.screenMode;
        }
        AudioController.instance.ChangeMasterVol(data.master);
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = Mathf.InverseLerp(-30f, 0f, data.master) * 30f;
        }
        AudioController.instance.ChangeMusicVol(data.music);
        if (_musicSlider != null)
        {
            _musicSlider.value = Mathf.InverseLerp(-30f, 0f, data.music) * 30f;
        }
        AudioController.instance.ChangeSFXVol(data.sfx);
        if (_sfxSlider != null)
        {
            _sfxSlider.value = Mathf.InverseLerp(-30f, 0f, data.sfx) * 30f;
        }
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = data.vsyncEnabled;
            QualitySettings.vSyncCount = data.vsyncEnabled ? 1 : 0;
            UpdateVsyncToggleImages(data.vsyncEnabled);
        }
    }

    public void StartGame()
    {
        NetworkManager.singleton.GetComponent<RoomManager>().GoToGameScene();
    }

    public void CreateRoom()
    {
        NetworkManager.singleton.StartHost();
    }

    public void EnterRoomCode(string roomCode)
    {
        NetworkManager.singleton.StartClient();
    }

    public void ReadyOrNotButton()
    {
        foreach(NetworkRoomPlayer rp in NetworkManager.singleton.GetComponent<NetworkRoomManager>().roomSlots)
        {
            if(rp.isOwned && rp.isLocalPlayer)
            {
                if(!rp.readyToBegin)
                {
                    readyButtonText.text = "Cancel";
                }
                else
                {
                    readyButtonText.text = "Ready";
                }
                rp.CmdChangeReadyState(!rp.readyToBegin);
            }
        }
    }
}
