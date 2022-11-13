using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class GameLobbyData : INetworkSerializable {
    public CourseType LoadedCourse;
    public int HoleIndex;


    public GameLobbyData() {
        LoadedCourse = CourseType.Sandbox;
        HoleIndex = 0;
    }


    public void CopyData(GameLobbyData other) {
        LoadedCourse = other.LoadedCourse;
        HoleIndex = other.HoleIndex;
    }


    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref LoadedCourse);
        serializer.SerializeValue(ref HoleIndex);
    }
}