using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListPopulator : MonoBehaviour {
    [SerializeField] private GameObject PlayerLobbyEntryPrefab;

    [Space(5)]

    [SerializeField] private RectTransform PlayerListContainer;
    [SerializeField] private float Offset = 0;
    [SerializeField] private float Spacing = 0;


    [NonSerialized] public List<PlayerLobbyEntry> PlayerLobbyEntries;



    public void ResetPlayerList() {
        if(PlayerLobbyEntries == null) PlayerLobbyEntries = new List<PlayerLobbyEntry>(Server.MaxPlayers);

        for(int i=0;i<PlayerLobbyEntries.Count;i++) {
            // Real logic
            // todo

            // Destroy element
            Destroy(PlayerLobbyEntries[i].gameObject);
        }

        PlayerLobbyEntries.Clear();
    }


    public void RebuildLobbyUI() {
        ResetPlayerList();

        foreach(PlayerData playerData in Server.PlayerDataBank) {
            AddPlayerEntry(playerData);
        }
    }


    public void AddPlayerEntry(PlayerData playerData) {
        PlayerLobbyEntry entry = Instantiate(PlayerLobbyEntryPrefab, PlayerListContainer).GetComponent<PlayerLobbyEntry>();
        entry.Load(playerData.PlayerId, playerData.PlayerName, playerData.PlayerColor);
        // entry.SetUpdateCallback();

        bool isThis = playerData.PlayerId == Server.SelfPlayerId;
        entry.SetInteractable(isThis);

        if(isThis) {
            entry.SetUpdateCallback(UpdateSelfPlayerEntry);
        }

        // Fix positioning
        Vector2 rtPos = entry.RT.anchoredPosition;
        rtPos.y = Offset + Spacing * (PlayerLobbyEntries.Count - 1);
        entry.RT.anchoredPosition = rtPos;
    }

    public void RemovePlayerEntry(int id) {
        for(int i=0;i<PlayerLobbyEntries.Count;i++) {
            if(PlayerLobbyEntries[i].Id == id) {
                // Real logic
                // todo

                // Destroy element
                Destroy(PlayerLobbyEntries[i].gameObject);

                // Remove from list
                PlayerLobbyEntries.RemoveAt(i);
                break;
            }
        }

        RecalculateRTPositions();
    }


    private void UpdateSelfPlayerEntry() {
        PlayerData data = null;
        for(int i=0;i<Server.PlayerDataBank.Count;i++) {
            if(Server.PlayerDataBank[i].PlayerId == Server.SelfPlayerId) {
                data = Server.PlayerDataBank[i];
            }
        }

        if(data != null) {
            Server.Singleton.DistributePlayerDataChange_ServerRpc(data);
        }
    }


    private void RecalculateRTPositions() {
        for(int i=0;i<PlayerLobbyEntries.Count;i++) {
            PlayerLobbyEntry entry = PlayerLobbyEntries[i];
            Vector2 rtPos = entry.RT.anchoredPosition;
            rtPos.y = Offset + Spacing * i;
            entry.RT.anchoredPosition = rtPos;
        }
    }
}
