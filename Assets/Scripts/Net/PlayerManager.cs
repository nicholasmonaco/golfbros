using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour {

    public void Start() {
        if(IsOwner) {
            Server.Singleton.TriggerClientEnabled();
        }
    }
}
