using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Smooth;

public class PlayerManager : NetworkBehaviour {

    public SmoothSyncNetcode Ball;

    public void Start() {
        if(IsOwner) {
            Server.Singleton.TriggerClientEnabled(this);
        }
    }
}
