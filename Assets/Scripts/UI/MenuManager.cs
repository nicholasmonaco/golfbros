using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {
    [HideInInspector] public MenuScreenId CurrentScreenId = MenuScreenId.Main;

    [SerializeField] private ServerConnector ServerConnector;
    [SerializeField] private PlayerListPopulator LobbyListPopulator;

    [SerializeField] private MenuScreenBank MenuScreens;
    private MenuScreen CurrentScreen => MenuScreens[CurrentScreenId];

    [Space(10)]

    [SerializeField] private List<Button> HostOptionButtons;
    [SerializeField] private List<Button> JoinOptionButtons;
    [SerializeField] private Button ReadyButton;
    [SerializeField] private TMP_Dropdown CourseDropdown;

    [Space(3)]

    [SerializeField] private TMP_Text ConnectionStatusText;

    [Space(5)]

    [SerializeField] private TMP_Text Label_HoleIndex;
    [SerializeField] private TMP_Text Label_ShotCounter;



    private void Start() {
        PopulateCourseOptions();

        SwitchMenuScreen(MenuScreenId.Main, true);
    }


    public void SwitchMenuScreen(int newScreen) => SwitchMenuScreen((MenuScreenId)newScreen, false);

    public void SwitchMenuScreen(MenuScreenId newScreen, bool force = false) {
        if(CurrentScreenId == newScreen && !force) return;

        CurrentScreen.OnExit();
        CurrentScreen.gameObject.SetActive(false);
        CurrentScreenId = newScreen;
        CurrentScreen.gameObject.SetActive(true);
        CurrentScreen.OnLoad();
    }



    public void TryStartHost() {
        foreach(Button button in HostOptionButtons) {
            button.interactable = false;
        }

        Action callback = () => {
            foreach(Button button in HostOptionButtons) {
                button.interactable = true;
            }
        };

        Action successCallback = () => {
            SwitchMenuScreen(MenuScreenId.Lobby);
        };

        ServerConnector.StartHost(Server.LocalHostData.Port, callback, successCallback);
    }


    public void TryConnect() {
        foreach(Button button in JoinOptionButtons) {
            button.interactable = false;
        }

        Action callback = () => {
            foreach(Button button in JoinOptionButtons) {
                button.interactable = true;
            }
        };

        Action successCallback = () => {
            SwitchMenuScreen(MenuScreenId.Lobby);
        };

        Action fullyLoadedCallback = () => {
            LobbyListPopulator.RebuildLobbyUI();
        };

        ServerConnector.JoinServer(Server.LocalJoinData.IP, Server.LocalJoinData.Port, ConnectionStatusText, callback, successCallback, fullyLoadedCallback);
    }


    public void LeaveLobby() {
        Server.Singleton.LeaveLobby();

        ConnectionStatusText.text = "";

        SwitchMenuScreen(MenuScreenId.Main);
    }


    public void ReadyUp() {
        ReadyButton.interactable = false;
        Server.Singleton.ReadyUp_ServerRpc();
    }

    public void ResetReady() {
        ReadyButton.interactable = true;
    }


    public void RefreshLobbyVisuals() {
        LobbyListPopulator.RebuildLobbyUI();
    }



    private void PopulateCourseOptions() {
        List<string> courses = new List<string>() {
            "Jacklands",
            "Juicelands",
            "Normlands",
            "Sandox",
            "Nutlands",
            "Glomplands"
        };

        CourseDropdown.ClearOptions();
        CourseDropdown.AddOptions(courses);
    }



    #region Game UI

    public void SetHoleIndex(int index) {
        Label_HoleIndex.text = $"Hole {index}";
    }

    public void SetShotCount(int count) {
        Label_ShotCounter.text = $"Shots: <color=#b0940b>{count}</color>";
    }

    #endregion

}

public enum MenuScreenId {
    None = 0,

    Main = 1,
    Options = 2,

    HostOptions = 10,
    JoinOptions = 11,
    Lobby = 12,
    
    Game_Start = 20,
    Game_Main = 21,
    Game_Transition = 22,
    Game_End = 23,
}
