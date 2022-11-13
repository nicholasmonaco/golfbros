using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseLoader : MonoBehaviour {
    public CourseType LobbyCourse = CourseType.Sandbox;
    [SerializeField] private CourseBank CourseBank;
    
    [Space(10)]

    [SerializeField] private Transform CourseContainer;




    public void LoadCourse(CourseType courseType) {
        ClearCourse();

        if(CourseBank.TryGetValue(courseType, out CourseData data)) {
            CoursePrefabData course = Instantiate(data.CoursePrefab, Vector3.zero, Quaternion.identity, CourseContainer).GetComponent<CoursePrefabData>();
            Game.Manager.CourseData = course;

            RenderSettings.skybox = data.SkyboxMaterial;

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
    }
}