using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen_Lobby : MenuScreen {
    [SerializeField] private PlayerListPopulator PlayerListPopulator;


    public override void OnLoad() {
        PlayerListPopulator.ResetPlayerList();

        Game.Manager.MenuManager.ResetReady();

        // Add all currently connected players in the lobby
        // foreach(PlayerData playerData in Server.PlayerDataBank) {
        //     PlayerListPopulator.AddPlayerEntry(playerData);
        // }

        // Add callback to new player additions/removals so that they are added/removed from the lobby board
        Server.Singleton.OnPlayer_Join += PlayerListPopulator.AddPlayerEntry;
        Server.Singleton.OnPlayer_Leave += PlayerListPopulator.RemovePlayerEntry;
    }

    public override void OnExit() {
        Server.Singleton.OnPlayer_Join -= PlayerListPopulator.AddPlayerEntry; // if we want to use this same lobby thing in the actual game, we should make this remove when this detects server disconnect instead
        Server.Singleton.OnPlayer_Leave -= PlayerListPopulator.RemovePlayerEntry;
    }

}
