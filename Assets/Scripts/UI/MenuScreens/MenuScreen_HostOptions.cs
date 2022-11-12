using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuScreen_HostOptions : MenuScreen {
    [SerializeField] private TMP_InputField Field_Port;
    [SerializeField] private TMP_InputField Field_Password;
    
    
    public override void OnLoad() {
        if(Server.CustomizationData == null) {
            Server.CustomizationData = new PlayerCustomizationData();
        }

        if(Server.LocalHostData == null) {
            Server.LocalHostData = new HostData();
            return;
        }

        Field_Port.text = Server.LocalHostData.Port;
        Field_Password.text = Server.LocalHostData.Password;
    }


    public override void OnExit() {
        if(Server.LocalHostData == null) {
            Server.LocalHostData = new HostData();

            Server.LocalHostData.Port = Field_Port.text;
            Server.LocalHostData.UsePassword = false;
            Server.LocalHostData.Password = "";
        }
    }



    public void UpdateLocalHostData() {
        if(Server.LocalHostData == null) {
            Server.LocalHostData = new HostData();
        }

        Server.LocalHostData.Port = Field_Port.text;
        Server.LocalHostData.UsePassword = Field_Password.text.Trim().Length > 0;
        Server.LocalHostData.Password = Field_Password.text;
    }
}
