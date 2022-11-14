using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseLoader : MonoBehaviour {
    public CourseType LobbyCourse = CourseType.Sandbox;
    
    [Space(5)]

    [SerializeField] private GameObject FlagPrefab;
    [SerializeField] private LayerMask FlagRaycastMask;

    [SerializeField] private CourseBank CourseBank;
    
    [Space(10)]

    [SerializeField] private Transform CourseContainer;

    private List<GameObject> HoleFlags;


    private void Awake() {
        HoleFlags = new List<GameObject>(3);
    }




    public void LoadCourse(CourseType courseType) {
        ClearCourse();

        if(CourseBank.TryGetValue(courseType, out CourseData data)) {
            CoursePrefabData course = Instantiate(data.CoursePrefab, Vector3.zero, Quaternion.identity, CourseContainer).GetComponent<CoursePrefabData>();
            Game.Manager.CourseData = course;

            RenderSettings.skybox = data.SkyboxMaterial;

            // Music
            if(data.MusicData.HasMusic) {
                StartCoroutine(Game.Manager.MusicPlayer.LoadMusic(data.MusicData, false));
            }

        } else {
            // Disconnect from server
            // todo
        }
    }

    private void ClearCourse() {
        int children = CourseContainer.childCount;
        for(int i=0;i<children;i++) {
            Destroy(CourseContainer.GetChild(0).gameObject);
        }
    }



    public void LoadHole(int index) {
        // Clear old hole data
        for(int i = 0;i<HoleFlags.Count;i++) {
            Destroy(HoleFlags[i]);
        }

        // Set new hole data
        HoleData hole = Game.Manager.CourseData.HoleDataList[index];

        Transform ball = Server.GetLocalPlayer().Ball.transform;

        Transform trackPoint;
        switch(hole.CameraMode) {
            default:
            case CameraTrackMode.BallTrack:
                trackPoint = ball;
                break;
            
            case CameraTrackMode.HolePoint:
                trackPoint = hole.CameraTrackPoint;
                break;

            case CameraTrackMode.BallTrackYLock:
                trackPoint = ball;
                // also add lock
                break;
        }

        Game.Manager.CameraBallFollow.TrackedTransform = trackPoint;

        // Spawn flags
        foreach(GoalData goal in hole.GoalPoint) {
            Vector3 flatforwards = hole.StartPoint.position - goal.GoalPoint.position;
            flatforwards.y = 0;
            Quaternion rotateTowardsStart = Quaternion.LookRotation(flatforwards.normalized, Vector3.up);

            Vector3 pos;
            if(goal.RaycastDown && Physics.Raycast(goal.GoalPoint.position, Vector3.down, out RaycastHit hit, 20, FlagRaycastMask, QueryTriggerInteraction.Ignore)) {
                pos = hit.point;
            } else {
                pos = goal.GoalPoint.position;
            }

            GameObject flag = Instantiate(FlagPrefab, pos, rotateTowardsStart, CourseContainer);
            HoleFlags.Add(flag);
        }

        // Set UI
        Game.Manager.MenuManager.SetHoleIndex(index + 1);
        Game.Manager.MenuManager.SetShotCount(0);
    }
}