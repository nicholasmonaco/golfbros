using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HoleData {
    public CameraTrackMode CameraMode = CameraTrackMode.HolePoint;

    public Transform CameraTrackPoint;

    [Space(5)]

    public Transform StartPoint;
    public List<Transform> GoalPoint;
}

public enum CameraTrackMode {
    HolePoint = 0,
    BallTrack = 1,
    BallTrackYLock = 2
}