using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HoleData {
    public CameraTrackMode CameraMode = CameraTrackMode.HolePoint;

    public Transform CameraTrackPoint;

    [Space(5)]

    public Transform StartPoint;
    public List<GoalData> GoalPoint;
}

public enum CameraTrackMode {
    HolePoint = 0,
    BallTrack = 1,
    BallTrackYLock = 2
}

[Serializable]
public class GoalData {
    public Transform GoalPoint;
    public Vector3 Scale = Vector3.one;

    public Collider UsedCollider = null;
}