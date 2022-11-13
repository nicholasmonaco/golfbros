using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class LobbySettings : INetworkSerializable {
    public CourseType Course = CourseType.Jacklands;

    [Space(5)]

    public bool CanJump = false;
    public bool PlayerCollisionEnabled = true;
    public bool PowerupsEnabled = true;



    public LobbySettings() {
        SetDefault();
    }


    public void SetDefault() {
        Course = CourseType.Jacklands;

        CanJump = false;
        PlayerCollisionEnabled = true;
        PowerupsEnabled = true;
    }


    public void CopyData(LobbySettings other) {
        Course = other.Course;

        CanJump = other.CanJump;
        PlayerCollisionEnabled = other.PlayerCollisionEnabled;
        PowerupsEnabled = other.PowerupsEnabled;
    }


    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Course);
        serializer.SerializeValue(ref CanJump);
        serializer.SerializeValue(ref PlayerCollisionEnabled);
        serializer.SerializeValue(ref PowerupsEnabled);
    }
}
