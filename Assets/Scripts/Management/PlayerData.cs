using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerData : INetworkSerializable {
    public int PlayerId = -1;
    public bool IsHost = false;

    [NonSerialized] public ulong ClientId;
    public bool Ready;

    public NetworkBehaviourReference LinkedPlayerManagerRef;
    public PlayerManager LinkedPlayerManager => (PlayerManager)LinkedPlayerManagerRef;

    public string PlayerName = "Player";
    public Color PlayerColor = Color.white;


    public PlayerData() {
        PlayerId = -1;
        IsHost = false;
        PlayerName = "Player";
        PlayerColor = Color.white;
    }

    public PlayerData(int playerId, string playerName, Color playerColor, bool isHost = false) {
        PlayerId = playerId;
        IsHost = isHost;
        PlayerName = playerName;
        PlayerColor = playerColor;
    }


    public void CopyData(PlayerData other) {
        PlayerName = other.PlayerName;
        PlayerColor = other.PlayerColor;
        Ready = other.Ready;
    }


    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref IsHost);
        serializer.SerializeValue(ref PlayerColor);
        serializer.SerializeValue(ref Ready);
        serializer.SerializeValue(ref LinkedPlayerManagerRef);

        serializer.SerializeValue(ref PlayerName);
    }
}
