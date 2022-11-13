using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuScreen_Lobby : MenuScreen {
    [SerializeField] private PlayerListPopulator PlayerListPopulator;

    [SerializeField] private List<Selectable> Interactables;

    [Space(10)]

    [SerializeField] private TMP_Dropdown Option_StageSelect;
    [SerializeField] private Toggle Option_CanJump;
    [SerializeField] private Toggle Option_PlayerCollisionEnabled;
    [SerializeField] private Toggle Option_PowerupsEnabled;


    public override void OnLoad() {
        PlayerListPopulator.ResetPlayerList();

        Game.Manager.MenuManager.ResetReady();

        // Set interactiblity
        SetLobbyOptionInteractability();

        // Add callback to new player additions/removals so that they are added/removed from the lobby board
        Server.Singleton.OnPlayer_Join += PlayerListPopulator.AddPlayerEntry;
        Server.Singleton.OnPlayer_Leave += PlayerListPopulator.RemovePlayerEntry;

        Server.Singleton.OnLobbySettingsUpdate += UpdateLobbySettings;
    }

    public override void OnExit() {
        Server.Singleton.OnPlayer_Join -= PlayerListPopulator.AddPlayerEntry; // if we want to use this same lobby thing in the actual game, we should make this remove when this detects server disconnect instead
        Server.Singleton.OnPlayer_Leave -= PlayerListPopulator.RemovePlayerEntry;

        Server.Singleton.OnLobbySettingsUpdate -= UpdateLobbySettings;
    }



    private void SetLobbyOptionInteractability() {
        bool interactable = Server.Singleton.IsNetHost;
        
        foreach(Selectable s in Interactables) {
            s.interactable = interactable;
        }
    }

    private void UpdateLobbySettings() {
        Option_StageSelect.value = (int)Server.CurrentLobbySettings.Course;

        Option_CanJump.isOn = Server.CurrentLobbySettings.CanJump;
        Option_PlayerCollisionEnabled.isOn = Server.CurrentLobbySettings.PlayerCollisionEnabled;
        Option_PowerupsEnabled.isOn = Server.CurrentLobbySettings.PowerupsEnabled;
    }



    private void SendLobbySettingUpdate() {
        if(Server.Singleton.IsNetHost) {
            Server.Singleton.DistributeLobbyData_ClientRpc(Server.CurrentLobbySettings, Server.Clients_AllBut(Server.ClientId));
        }
    }


    #region Lobby Settings Updaters

    public void UpdateLobbySetting_Stage(int index) {
        Server.CurrentLobbySettings.Course = (CourseType)index;
        SendLobbySettingUpdate();
    }

    public void UpdateLobbySetting_CanJump(bool enabled) {
        Server.CurrentLobbySettings.CanJump = enabled;
        SendLobbySettingUpdate();
    }

    public void UpdateLobbySetting_PlayerCollision(bool enabled) {
        Server.CurrentLobbySettings.PlayerCollisionEnabled = enabled;
        SendLobbySettingUpdate();
    }

    public void UpdateLobbySetting_PowerupsEnabled(bool enabled) {
        Server.CurrentLobbySettings.PowerupsEnabled = enabled;
        SendLobbySettingUpdate();
    }

    

    #endregion

}
