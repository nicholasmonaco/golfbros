using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Components;
using Smooth;

public class ServerConnector : MonoBehaviour {
    private NetworkManager NetManager => NetworkManager.Singleton;
    private UnityTransport NetTransport => NetManager.GetComponent<UnityTransport>();


    [SerializeField] private GameObject ServerManagerPrefab;



    private void Start() {
        NetManager.ConnectionApprovalCallback += Server.ApprovalCheck;
    }


    public void StartHost(string port, Action onEndAttemptCallback, Action onSuccessCallback) {
        NetManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Game.VersionData.VersionHash);

        // NetTransport.ConnectionData.Port = Convert.ToUInt16(Server.FormatPort(port));
        NetTransport.SetConnectionData(
            "127.0.0.1",
            Convert.ToUInt16(Server.FormatPort(port)),
            "0.0.0.0"
        );

        NetManager.OnServerStarted += () => {
            if(NetManager.IsServer) {
                NetworkObject no = Instantiate(ServerManagerPrefab).GetComponent<NetworkObject>();
                DontDestroyOnLoad(no.gameObject);
                no.Spawn();
            }
        };

        if(NetManager.StartHost()) {
            Debug.Log("Started host.");

            Server.SetSingleton();

            Server.Singleton.Setup_Host();

            onSuccessCallback();
        } else {
            Debug.Log("Failed to start host.");
        }

        onEndAttemptCallback();
    }



    public void JoinServer(string ip, string port, TMPro.TMP_Text statusText, Action onEndAttemptCallback, Action onSuccessCallback, Action onFullyLoadedCallback) {
        StartCoroutine(JoinServer_C(ip, port, statusText, onEndAttemptCallback, onSuccessCallback, onFullyLoadedCallback));
    }


    private IEnumerator JoinServer_C(string ip, string port, TMPro.TMP_Text statusText, Action onEndAttemptCallback, Action onSuccessCallback, Action onFullyLoadedCallback) {
        // Reset
        Server.Connected = false;
        Server.Validated = false;

        // Set callbacks
        Server.Setup_Join_Init();

        // Set IP
        string rawIP = ip.Trim();
        string checkedIp = rawIP == "" ? "127.0.0.1" : rawIP;
        // NetTransport.ConnectionData.Address = checkedIp;

        // Block self connection
        if(!Application.isEditor && checkedIp == "127.0.0.1") {
            statusText.text = "Invalid IP Address";
            onEndAttemptCallback();
            yield break;
        }

        // Set port
        // NetTransport.ConnectionData.Port = Convert.ToUInt16(Server.FormatPort(port));

        NetTransport.SetConnectionData(
            checkedIp,
            Convert.ToUInt16(Server.FormatPort(port)) // ,
            // "0.0.0.0"
        );

        // Ping
        const float maxPingTime = 7.5f; // Max time of the ping (in seconds)
        float pingTimer = 0;
        string connectionAddress = $"{NetTransport.ConnectionData.Address}:{NetTransport.ConnectionData.Port}";
        Ping ping = new Ping(NetTransport.ConnectionData.Address);

        string baseMsg = $"Connecting to {connectionAddress}...\n";
        Debug.Log(baseMsg);
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
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Game.VersionData.VersionHash);

            yield return null;

            // Connection should be good to go, so start the client
            Debug.Log("Starting client...");
            statusText.text = "Starting client...";
            bool startSuccessful = NetManager.StartClient();
            bool secondLevelFail = false;

            if(startSuccessful) {
                Debug.Log("Waiting for connection establish...");
                statusText.text = "Waiting for connection establish...";
                // while(!Connected) yield return null;
                
                Debug.Log("Validating client...");
                statusText.text = "Validating client...";

                float validationTimer = NetManager.NetworkConfig.ClientConnectionBufferTimeout;
                while(validationTimer > 0 && !Server.Validated) {
                    yield return null;
                    validationTimer -= Time.deltaTime;
                }

                if(Server.Validated) {
                    Debug.Log("Client started successfully.");
                    statusText.text = "Client started successfully.";

                    Server.SetSingleton();

                    yield return StartCoroutine(Server.Singleton.Setup_Join(onFullyLoadedCallback));

                    onSuccessCallback();

                    statusText.text = "";

                } else {
                    secondLevelFail = true;
                    Debug.Log("Failed to validate client.");
                    statusText.text = "Failed to validate client.";
                }

            } else {
                secondLevelFail = true;
                Debug.Log("Failed to start client.");
                statusText.text = "Failed to start client.";
            }

            if(secondLevelFail) {
                NetManager.Shutdown();
            }
        }

        onEndAttemptCallback();
    }

}
