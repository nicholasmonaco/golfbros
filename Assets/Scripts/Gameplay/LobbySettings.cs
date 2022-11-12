using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LobbySettings {
    public CourseType Course = CourseType.Jacklands;

    [Space(5)]

    public bool CanJump = false;
    public bool PlayerCollisionEnabled = true;
    public bool PowerupsEnabled = true;
}
