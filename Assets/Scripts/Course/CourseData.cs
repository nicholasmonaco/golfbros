using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CourseData", menuName = "Scriptable Objects/Course Data")]
public class CourseData : ScriptableObject {
    public string CourseName = "Course";
    public GameObject CoursePrefab;
    public Material SkyboxMaterial;

    [Space(8, order = 0)]
    [Header("Music", order = 1)]
 
    public MusicData MusicData;

}
