using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour {
    public Camera Camera;
    public Transform CameraContainerTransform;
    public Transform RevolvePoint;

    [Space(5)]

    public float DistanceFromPoint = 5;
    public float YOffset = 0;

    [Space(5)]

    [SerializeField] private float _menuAngleSpeed = 0;
    [SerializeField] private float _panSensitivity = 1;
    [SerializeField] private float _panLerpSpeed = 20;
    [SerializeField] private float _angle = 0;

    [Space(5)]

    public bool InMenu = true;



    private void Update() {
        if(Application.IsPlaying(gameObject)) {
            if(InMenu) {
                Update_Menu();
            }

            Update_Game();
        }

        // Set position
        Vector3 revolvePoint = RevolvePoint.position + new Vector3(0, YOffset, 0);
        Vector3 offset = (Quaternion.AngleAxis(_angle, Vector3.up) * Vector3.forward) * DistanceFromPoint;

        Vector3 p = revolvePoint + offset;

        float flatDist = new Vector3(offset.x, 0, offset.z).magnitude; // Vector3.Distance(new Vector3(revolvePoint.x, 0, revolvePoint.z), new Vector3(p.x, 0, p.z));
        float yDist = Mathf.Sqrt(Mathf.Abs(DistanceFromPoint * DistanceFromPoint - flatDist * flatDist));

        offset.y = yDist + YOffset;
        Vector3 final = revolvePoint + offset;

        CameraContainerTransform.position = final;

        // Set rotation
        Vector3 lookDir = revolvePoint - final;
        lookDir.y = 0;
        lookDir.Normalize();

        CameraContainerTransform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }



    private void Update_Menu() {
        _angle += _menuAngleSpeed * Time.deltaTime;

        if(_angle >= 360) _angle -= 360;
    }

    private void Update_Game() {
        if(InputHandler.Sets(InputState.Game).Pan) {
            float angle = _angle + InputHandler.Sets(InputState.Game).Look.x * _panSensitivity;
            _angle = Mathf.Lerp(_angle, angle, Time.deltaTime * _panLerpSpeed);

            if(_angle >= 360) _angle -= 360;
        }
    }
}
