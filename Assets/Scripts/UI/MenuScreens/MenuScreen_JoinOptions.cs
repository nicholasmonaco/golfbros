using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuScreen_JoinOptions : MenuScreen {
    [SerializeField] private TMP_InputField Field_IP;
    [SerializeField] private TMP_InputField Field_Port;
    
    
    public override void OnLoad() {
        if(Server.CustomizationData == null) {
            Server.CustomizationData = new PlayerCustomizationData();
        }

        if(Server.LocalJoinData == null) {
            Server.LocalJoinData = new JoinData();
            return;
        }

        Field_Port.text = Server.LocalJoinData.Port;
        Field_IP.text = Server.LocalJoinData.IP;
    }


    public override void OnExit() {
        if(Server.LocalJoinData == null) {
            Server.LocalJoinData = new JoinData();

            Server.LocalJoinData.Port = Field_Port.text;
            Server.LocalJoinData.IP = Field_IP.text;
        }
    }



    public void UpdateLocalJoinData() {
        if(Server.LocalJoinData == null) {
            Server.LocalJoinData = new JoinData();
        }

        Server.LocalJoinData.Port = Field_Port.text;
        Server.LocalJoinData.IP = Field_IP.text;
    }
}
