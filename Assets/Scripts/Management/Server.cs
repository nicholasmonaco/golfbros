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
    public static bool Connected = false;
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


    public static ulong ClientId => NetworkManager.Singleton.LocalClientId;
    public bool IsNetServer => NetworkManager.Singleton.IsServer;
    public bool IsNetHost => NetworkManager.Singleton.IsHost;
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

    public static LobbySettings CurrentLobbySettings = new LobbySettings();
    public static GameLobbyData CurrentGameData = new GameLobbyData();


    public static List<PlayerData> PlayerDataBank;
    private int _playerIdCounter = 0;
    public static int SelfPlayerId { get; private set; } = -1;

    public static PlayerManager GetLocalPlayer() {
        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == SelfPlayerId) return pd.LinkedPlayerManager;
        }

        return null;
    }

    public Action<PlayerData> OnPlayer_Join;
    public Action<int> OnPlayer_Leave;

    public Action OnLobbySettingsUpdate = null;

    

    public void Setup_Host() {
        Connected = true;

        CurrentLobbySettings.SetDefault();

        ClientIndivAllocs = new Dictionary<ulong, ulong[]>(MaxPlayers);
        ClientInverseSenderAllocs = new Dictionary<ulong, List<ulong>>(MaxPlayers);

        NetManager.OnClientConnectedCallback += OnClientConnect_Server;
        NetManager.OnClientDisconnectCallback += OnClientDisconnect_Server;

        NetManager.OnClientConnectedCallback += OnClientConnect_Client;
        NetManager.OnClientDisconnectCallback += OnClientDisconnect_Client;

        PlayerDataBank.Clear();
        _playerIdCounter = 0;

        OnClientConnect_Client(ClientId);

        // PlayerData playerData = BuildLocalPlayerData();
        // playerData.IsHost = true;
        // AddPlayer_ServerRpc(playerData);
    }


    public static void Setup_Join_Init() {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect_Client;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect_Client;

        CurrentLobbySettings.SetDefault();
    }

    public IEnumerator Setup_Join(Action onFullyLoadedCallback) {
        _onFullyLoadedCallback = onFullyLoadedCallback;
        
        yield return null;
    }


    public static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        string receivedVersionHash = System.Text.Encoding.ASCII.GetString(request.Payload);

        bool approve = string.Equals(receivedVersionHash, Game.VersionData.VersionHash);

        string approveMsg = approve ? "Approved" : "Failure";
        Debug.Log($"Checking connection with version hash '{receivedVersionHash}', status: {approveMsg}");

        // DEBUG
        // approve = true;

        bool createPlayerObject = true;
        uint? prefabHash = null;

        response.CreatePlayerObject = createPlayerObject;
        response.PlayerPrefabHash = prefabHash;
        response.Approved = approve;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
    }



    public void LeaveLobby() {
        // if(NetworkManager.)  // probably want a check here to see if we're connected to a server already (?)

        NetManager.OnClientConnectedCallback -= OnClientConnect_Client;
        NetManager.OnClientDisconnectCallback -= OnClientDisconnect_Client;

        ClientIndivAllocs = null;
        ClientInverseSenderAllocs = null;

        if(NetworkManager.Singleton.IsServer) EndServer();

        NetworkManager.Singleton.Shutdown();

        // ClientDisconnectLogic();
    }


    private void EndServer() {
        if(IsNetServer) {
            // NetworkManager.Singleton.ConnectionApprovalCallback -= Server.ApprovalCheck;

            // Clear variables
            // todo

            // Clear client lists
            ClientIndivAllocs = null;
            ClientInverseSenderAllocs = null;

            // Remove event listeners
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect_Server;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect_Server;

            Debug.Log("Server closed.");
        }
    }





    private void OnClientConnect_Server(ulong connectedClientId) {
        Debug.Log($"Client connected with clientId [{connectedClientId}]");

        // ClientRpcParams newConnectionOnly = Clients_Only(connectedClientId);
    }

    private void OnClientDisconnect_Server(ulong disconnectedClientId) {
        Debug.Log($"Client disconnected with clientId [{disconnectedClientId}]");

        // RemovePlayerData(disconnectedClientId);

        RemovePlayer_ServerRpc(disconnectedClientId);
    }



    private Action _onFullyLoadedCallback = null;

    public void TriggerClientEnabled(PlayerManager linkedPlayer) {
        Validated = true;
        Connected = true;

        PlayerDataBank.Clear();

        PlayerData playerData = BuildLocalPlayerData();
        playerData.LinkedPlayerManagerRef = linkedPlayer;
        if(IsServer || IsHost) playerData.IsHost = true;

        TriggerClientEnabled_ServerRpc(playerData);
    }

    [ClientRpc]
    private void TriggerLoadEnd_ClientRpc(ClientRpcParams clientRpcParams = default) {
        _onFullyLoadedCallback?.Invoke();

        _onFullyLoadedCallback = null;

        RequestBallSpawn_ServerRpc();
    } 

    [ServerRpc(RequireOwnership = false)]
    private void TriggerClientEnabled_ServerRpc(PlayerData locallyBuiltPlayerData, ServerRpcParams serverRpcParams = default) {
        // Fuck i have to make these manually cause the allocs dont exist yet
        ClientRpcParams newConnectionOnly = new ClientRpcParams{ Send = new ClientRpcSendParams { TargetClientIds = new ulong[1]{ serverRpcParams.Receive.SenderClientId } } };
        // ClientRpcParams notNewConnectionOnly = Clients_AllBut(serverRpcParams.Receive.SenderClientId);

        // Send out previous full player list to new player
        for(int i=0;i<PlayerDataBank.Count;i++) {
            SendPlayerData_ClientRpc(PlayerDataBank[i], newConnectionOnly);
        }

        // Add new player for everyone
        AddPlayer_ServerRpc(locallyBuiltPlayerData, serverRpcParams);

        // Send game data to connection
        SetCurrentGameData_ClientRpc(CurrentGameData);
        

        // Notify new player that they're good to go
        TriggerLoadEnd_ClientRpc(newConnectionOnly);

        // Load current stage on server if this is the server
        if(locallyBuiltPlayerData.IsHost) {
            CurrentGameData.LoadedCourse = Game.Manager.CourseLoader.LobbyCourse;
            StartGame_ServerRpc();

        } else {
            // Load the current stage for the non-host
            StartGame_ClientRpc(false, newConnectionOnly);
        }


        LoadCurrentHole_ClientRpc(newConnectionOnly);
    }



    private static void OnClientConnect_Client(ulong connectedClientId) {
        if(connectedClientId == ClientId) {
            Debug.Log("Set self to connected.");
            Connected = true;
        }
    }


    private static void OnClientDisconnect_Client(ulong connectedClientId) {
        if(connectedClientId == ClientId) {
            Debug.Log("Set self to disconnected.");
            Connected = false;
        }
    }



    public static PlayerData BuildLocalPlayerData() {
        return new PlayerData(-1, CustomizationData.PlayerName, CustomizationData.PlayerColor, false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayer_ServerRpc(PlayerData playerData, ServerRpcParams serverRpcParams = default) {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        // Add new client ID to list of not itself client ID lists
        foreach(List<ulong> otherInverseList in ClientInverseSenderAllocs.Values) {
            otherInverseList.Add(senderClientId);
        }

        // Create this client's inverse Id list
        ClientInverseSenderAllocs.Add(senderClientId, new List<ulong>(MaxPlayers - 1));
        foreach(ulong otherClientId in ClientIndivAllocs.Keys) {
            ClientInverseSenderAllocs[senderClientId].Add(otherClientId);
        }

        // Add this client to the list of available clientIds
        ClientIndivAllocs.Add(senderClientId, new ulong[1]{ senderClientId });


        // Define gameplay player id
        playerData.PlayerId = _playerIdCounter++;
        playerData.ClientId = senderClientId;

        PlayerDataBank.Add(playerData);
        // OnPlayer_Join?.Invoke(playerData); // Don't need to call this here, its a client thing

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
        }

        OnPlayer_Join?.Invoke(playerData);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayer_ServerRpc(ulong clientId) {
        // Get client id
        // ulong clientId = serverRpcParams.Receive.SenderClientId;

        // Remove client id alloc
        if(ClientIndivAllocs.ContainsKey(clientId)) ClientIndivAllocs.Remove(clientId);

        // Remove this client's inverse Id list
        if(ClientInverseSenderAllocs.ContainsKey(clientId)) ClientInverseSenderAllocs.Remove(clientId);

        // Remove client ID to list of not itself client ID lists
        foreach(List<ulong> otherInverseList in ClientInverseSenderAllocs.Values) {
            if(otherInverseList.Contains(clientId)) otherInverseList.Remove(clientId);
        }



        PlayerData removedData = null;
        int removeIndex = -1;
        for(int i=0;i<PlayerDataBank.Count;i++) {
            PlayerData pd = PlayerDataBank[i];
            if(pd.ClientId == clientId) {
                removedData = pd;
                removeIndex = i;
                break;
            }
        }


        if(removedData != null) {
            PlayerDataBank.RemoveAt(removeIndex);
        }


        RemovePlayer_ClientRpc(removedData.PlayerId);
    }

    [ClientRpc]
    private void RemovePlayer_ClientRpc(int playerId) {
        PlayerData removedData = null;
        int removeIndex = -1;
        for(int i=0;i<PlayerDataBank.Count;i++) {
            PlayerData pd = PlayerDataBank[i];
            if(pd.PlayerId == playerId) {
                removedData = pd;
                removeIndex = i;
                break;
            }
        }

        if(removedData != null) {
            if(!IsHost) {
                PlayerDataBank.RemoveAt(removeIndex);
            }
        }

        OnPlayer_Leave?.Invoke(playerId);
    }


    [ClientRpc]
    private void SendPlayerData_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default) {
        PlayerDataBank.Add(playerData);
    }



    [ServerRpc(RequireOwnership = false)]
    public void DistributePlayerDataChange_ServerRpc(PlayerData playerData, ServerRpcParams serverRpcParams = default) {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == playerData.PlayerId) {
                pd.CopyData(playerData);
                break;
            }
        }

        DistributePlayerDataChange_ClientRpc(playerData);
    }

    [ClientRpc]
    private void DistributePlayerDataChange_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default) {
        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == playerData.PlayerId) {
                pd.CopyData(playerData);
                break;
            }
        }

        Game.Manager.MenuManager.RefreshLobbyVisuals();
    }




    [ServerRpc(RequireOwnership = false)]
    public void ReadyUp_ServerRpc(ServerRpcParams serverRpcParams = default) {
        PlayerData data = null;
        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.ClientId == serverRpcParams.Receive.SenderClientId) {
                pd.Ready = true;
                data = pd;
            }
        }

        if(data != null) {
            DistributePlayerDataChange_ClientRpc(data);
        }

        bool allPlayersReady = true;
        foreach(PlayerData pd in PlayerDataBank) {
            if(!pd.Ready) {
                allPlayersReady = false;
                break;
            }
        }

        if(allPlayersReady) {
            // Start game
            CurrentGameData.LoadedCourse = CurrentLobbySettings.Course;
            CurrentGameData.HoleIndex = 0;
            SetCurrentGameData_ClientRpc(CurrentGameData);

            

            StartGame_ServerRpc(true);
        }
    }



    [ClientRpc]
    public void DistributeLobbyData_ClientRpc(LobbySettings lobbySettings, ClientRpcParams clientRpcParams = default) {
        CurrentLobbySettings.CopyData(lobbySettings);
        
        // Callback for update
        OnLobbySettingsUpdate?.Invoke();
    }



    #region Gameplay

    [ServerRpc]
    public void StartGame_ServerRpc(bool closeMenus = false) {
        Game.Manager.CourseLoader.LoadCourse(CurrentGameData.LoadedCourse);

        StartGame_ClientRpc(closeMenus);

        LoadCurrentHole_ClientRpc();
    }


    [ClientRpc]
    private void StartGame_ClientRpc(bool closeMenus = false, ClientRpcParams clientRpcParams = default) {
        if(!IsHost) {
            Game.Manager.CourseLoader.LoadCourse(CurrentGameData.LoadedCourse);
        }

        if(closeMenus) {
            Game.Manager.MenuManager.SwitchMenuScreen(MenuScreenId.Game_Main);

            Game.Manager.CameraController.InMenu = false;
        }   
    }





    [ServerRpc(RequireOwnership = false)]
    private void RequestBallSpawn_ServerRpc(ServerRpcParams serverRpcParams = default) {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        NetworkObject ballNO = Instantiate(Game.Manager.BallPrefab).GetComponent<NetworkObject>();
        ballNO.SpawnWithOwnership(clientId);

        SmoothSyncNetcode smooth = ballNO.GetComponent<SmoothSyncNetcode>();
        SetPlayerBall_ClientRpc(smooth, Clients_Only(clientId));
    }

    [ClientRpc]
    private void SetPlayerBall_ClientRpc(NetworkBehaviourReference ballSmoothRef, ClientRpcParams clientRpcParams = default) {
        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == SelfPlayerId) {
                pd.LinkedPlayerManager.Ball = (SmoothSyncNetcode)ballSmoothRef;
            }
        }
    }



    [ServerRpc]
    public void LoadHole_ServerRpc(int holeIndex) {
        CurrentGameData.HoleIndex = holeIndex;

        SetCurrentGameData_ClientRpc(CurrentGameData);
    }


    [ClientRpc]
    public void SetCurrentGameData_ClientRpc(GameLobbyData currentGameData) {
        CurrentGameData.CopyData(currentGameData);
    }


    [ClientRpc]
    private void LoadCurrentHole_ClientRpc(ClientRpcParams clientRpcParams = default) {
        StartCoroutine(LoadCurrentHole_C());
    }

    private IEnumerator LoadCurrentHole_C() {
        // Teleport player to start point
        HoleData hole = Game.Manager.CourseData.HoleDataList[CurrentGameData.HoleIndex];

        foreach(PlayerData pd in PlayerDataBank) {
            if(pd.PlayerId == SelfPlayerId) {
                while(pd.LinkedPlayerManager.Ball == null) {
                    yield return null;
                }

                Game.Manager.CourseLoader.LoadHole(CurrentGameData.HoleIndex);

                pd.LinkedPlayerManager.Ball.setPosition(hole.StartPoint.position, true);

                break;
            }
        }
    }


    #endregion



    #region Helper Methods

    public static int FormatPort(string rawInput) {
        string rawPort = rawInput.Trim();
        rawPort = rawPort == "" ? "7777" : rawPort;
        int truePort;
        try { truePort = System.Convert.ToInt32(rawPort); }
        catch { truePort = 7777; }
        return truePort;
    }

    #endregion
}
