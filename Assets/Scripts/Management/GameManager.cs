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
    public GameObject CurrentAudioListener;
    public AudioSource GlobalSFXPlayer;

    [Space(5)]

    [SerializeField, Range(0f, 1)] private float Volume_Music = 1;
    [SerializeField, Range(0f, 1)] private float Volume_SFX = 1;

    [Space(5)]

    [SerializeField] private SFXBank _globalSFXBank;

    [Space(5)]

    public LayerMask BallMask;
    public LayerMask BoundsMask;
    public LayerMask GoalMask;

    [Space(10)]

    public GameObject BallPrefab;
    public GameObject AudioListenerPrefab;



    private void Awake() {
        Game.Manager = this;

        Options.Volume_Music = Volume_Music;
        Options.Volume_SFX = Volume_SFX;
    }
    


    #region Util

    public void ExitGame() => Game.CloseGame();


    public void PlayGlobalSFX(string id) {
        if(_globalSFXBank.TryGetValue(id, out AudioClipData data)) {
            GlobalSFXPlayer.PlayOneShot(data.Clip, data.Volume * Options.Volume_Master * Options.Volume_SFX);
        }
    }

    #endregion




    #region Gameplay Management

    [HideInInspector] public CoursePrefabData CourseData;


    #endregion

}
