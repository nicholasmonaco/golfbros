using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Smooth;

public class Server : NetworkBehaviour {
    public const int MaxPlayers = 8;
    public static readonly ulong[] _EmptyUIntArray = new ulong[0];



    public static Server Singleton;
    public static bool Validated = true;

    private void Awake() {
        PlayerDataBank = new List<PlayerData>(MaxPlayers);

        SetSelfSingleton();
    }

    public static void SetSingleton() {
        Singleton = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
    }

    public void SetSelfSingleton() {
        Singleton = this;
    }


    public ulong ClientId => NetworkManager.Singleton.LocalClientId;
    public bool IsNetServer => NetworkManager.Singleton.IsServer;
    private NetworkManager NetManager => NetworkManager.Singleton;
    private UnityTransport NetTransport => NetManager.GetComponent<UnityTransport>();


    private static Dictionary<ulong, ulong[]> ClientIndivAllocs;
    private static Dictionary<ulong, List<ulong>> ClientInverseSenderAllocs;

    public static ClientRpcParams Clients_AllBut(ulong clientId) {
        return new ClientRpcParams{ Send = new ClientRpcSendParams { TargetClientIds = ClientInverseSenderAllocs == null ? _EmptyUIntArray : ClientInverseSenderAllocs[clientId].ToArray() } };
    }

    public static ClientRpcParams Clients_Only(ulong clientId) {
        return new ClientRpcParams{ Send = new ClientRpcSendParams { TargetClientIds = ClientIndivAllocs == null ? _EmptyUIntArray : ClientIndivAllocs[clientId] } };
    }




    public static HostData LocalHostData;
    public static JoinData LocalJoinData;
    public static PlayerCustomizationData CustomizationData;


    public static List<PlayerData> PlayerDataBank;
    private int _playerIdCounter = 0;
    public static int SelfPlayerId { get; private set; } = -1;

    public Action<PlayerData> OnPlayer_Join;
    public Action<int> OnPlayer_Leave;


    

    public void StartHost(string port, Action onEndAttemptCallback, Action onSuccessCallback) {
        // NetManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(versionHash);

        NetTransport.ConnectionData.Port = Convert.ToUInt16(FormatPort(port));
        // NetManager.ConnectionApprovalCallback += ApprovalCheck;

        // NetManager.OnServerStarted += () => {
        //     if(IsServer) {
        //         // todo
        //     }
        // };

        if(NetManager.StartHost()) {
            Debug.Log("Started host.");

            NetManager.OnClientConnectedCallback += OnClientConnect;
            NetManager.OnClientDisconnectCallback += OnClientDisconnect;

            PlayerDataBank.Clear();
            _playerIdCounter = 0;

            PlayerData playerData = BuildLocalPlayerData();
            playerData.IsHost = true;
            AddPlayer_ServerRpc(playerData);

            onSuccessCallback();
        } else {
            Debug.Log("Failed to start host.");
        }

        onEndAttemptCallback();
    }


    public void JoinServer(string ip, string port, TMPro.TMP_Text statusText, Action onEndAttemptCallback, Action onSuccessCallback) {
        StartCoroutine(JoinServer_C(ip, port, statusText, onEndAttemptCallback, onSuccessCallback));
    }

    private IEnumerator JoinServer_C(string ip, string port, TMPro.TMP_Text statusText, Action onEndAttemptCallback, Action onSuccessCallback) {
        // Set IP
        string rawIP = ip.Trim();
        NetTransport.ConnectionData.Address = rawIP == "" ? "127.0.0.1" : rawIP;

        if(NetTransport.ConnectionData.Address == "127.0.0.1") {
            statusText.text = "Invalid IP Address";
            onEndAttemptCallback();
            yield break;
        }

        // Set port
        NetTransport.ConnectionData.Port = Convert.ToUInt16(FormatPort(port));

        // Ping
        const float maxPingTime = 7.5f; // Max time of the ping (in seconds)
        float pingTimer = 0;
        string connectionAddress = $"{NetTransport.ConnectionData.Address}:{NetTransport.ConnectionData.Port}";
        Ping ping = new Ping(NetTransport.ConnectionData.Address);

        string baseMsg = $"Connecting to {connectionAddress}...\n";
        statusText.text = baseMsg;
        int lastCount = -1;

        while(!ping.isDone && pingTimer < maxPingTime) {
            yield return null;
            pingTimer += Time.deltaTime;

            int count = Mathf.FloorToInt(pingTimer / 0.75f);
            if(count != lastCount) {
                lastCount = count;
                System.Text.StringBuilder sb = new System.Text.StringBuilder(count);
                for(int i = 0; i <= count % 4 - 1; i++) {
                    string space = i < 4 ? " " : "";
                    sb.Append($"•{space}");
                }
                statusText.text = baseMsg + sb.ToString();
            }
        }

        // Check ping results
        if(!ping.isDone) {
            // Connection not found, failure
            Debug.Log($"Server not found at {connectionAddress}");
            statusText.text = $"Server not found at {connectionAddress}";

        } else {
            // Connection found, success
            Debug.Log("Connection found, connecting...");
            statusText.text = "Connection found, connecting...";

            // Set validation data
            // Net.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Game.VersionData.VersionHash);

            yield return null;

            // Connection should be good to go, so start the client
            statusText.text = "Starting client...";
            bool startSuccessful = NetManager.StartClient();

            if(startSuccessful) {
                statusText.text = "Validating client...";

                float validationTimer = NetManager.NetworkConfig.ClientConnectionBufferTimeout;
                while(validationTimer > 0 && !Server.Validated) {
                    yield return null;
                    validationTimer -= Time.deltaTime;
                }

                if(Server.Validated) {
                    Debug.Log("Client started successfully.");
                    statusText.text = "Client started successfully.";

                    PlayerDataBank.Clear();

                    PlayerData playerData = BuildLocalPlayerData();
                    AddPlayer_ServerRpc(playerData);

                    onSuccessCallback();

                    statusText.text = "";

                } else {
                    Debug.Log("Failed to validate client.");
                    statusText.text = "Failed to validate client.";
                }

            } else {
                Debug.Log("Failed to start client.");
                statusText.text = "Failed to start client.";
            }
        }

        onEndAttemptCallback();
    }



    public void LeaveLobby() {
        // if(NetworkManager.)  // probably want a check here to see if we're connected to a server already (?)

        if(NetworkManager.Singleton.IsServer) EndServer();

        NetworkManager.Singleton.Shutdown();

        // ClientDisconnectLogic();
    }


    private void EndServer() {
        if(IsNetServer) {
            // NetworkManager.Singleton.ConnectionApprovalCallback -= Server.ApprovalCheck;

            // Clear variables
            // todo

            // Remove event listeners
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;

            Debug.Log("Server closed.");
        }
    }




    private void OnClientConnect(ulong connectedClientId) {
        Debug.Log($"Client connected with clientId [{connectedClientId}]");

        // SetValidation_ClientRpc(new ClientRpcParams{ Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { connectedClientId } } });

        ClientRpcParams newConnectionOnly = Clients_Only(connectedClientId);

        foreach(PlayerData pd in PlayerDataBank) {
            // SendPlayerData_ClientRpc(pd, newConnectionOnly);
        }
    }

    private void OnClientDisconnect(ulong disconnectedClientId) {
        Debug.Log($"Client disconnected with clientId [{disconnectedClientId}]");

        // RemovePlayerData(disconnectedClientId);
    }



    public static PlayerData BuildLocalPlayerData() {
        return new PlayerData(-1, CustomizationData.PlayerName, CustomizationData.PlayerColor, false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayer_ServerRpc(PlayerData playerData, ServerRpcParams serverRpcParams = default) {
        playerData.PlayerId = _playerIdCounter++;

        PlayerDataBank.Add(playerData);
        OnPlayer_Join?.Invoke(playerData);

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        if(senderClientId == ClientId) {
            SelfPlayerId = playerData.PlayerId;
        }

        AddPlayer_ClientRpc(playerData, true, Clients_Only(senderClientId));
        AddPlayer_ClientRpc(playerData, false, Clients_AllBut(senderClientId));
    }

    [ClientRpc]
    private void AddPlayer_ClientRpc(PlayerData playerData, bool self, ClientRpcParams clientRpcParams = default) {
        if(self) {
            SelfPlayerId = playerData.PlayerId;
        }
        
        if(!IsHost) {
            PlayerDataBank.Add(playerData);
            OnPlayer_Join?.Invoke(playerData);
        }
    }


    [ClientRpc]
    private void SendPlayerData_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default) {
        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == playerData.PlayerId) return;
        }

        PlayerDataBank.Add(playerData);
    }


    #region Helper Methods

    private static int FormatPort(string rawInput) {
        string rawPort = rawInput.Trim();
        rawPort = rawPort == "" ? "7777" : rawPort;
        int truePort;
        try { truePort = System.Convert.ToInt32(rawPort); }
        catch { truePort = 7777; }
        return truePort;
    }

    #endregion
}
