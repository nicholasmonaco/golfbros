using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public SmoothFollow CameraBallFollow;
    public CameraController CameraController;
    public MenuManager MenuManager;
    public CourseLoader CourseLoader;
    [HideInInspector] public MusicPlayer MusicPlayer;

    [Space(5)]

    public LayerMask BallMask;
    public LayerMask BoundsMask;
    public LayerMask GoalMask;

    [Space(10)]

    public GameObject BallPrefab;



    private void Awake() {
        Game.Manager = this;
    }
    


    #region Util

    public void ExitGame() => Game.CloseGame();

    #endregion




    #region Gameplay Management

    [HideInInspector] public CoursePrefabData CourseData;


    #endregion

}
