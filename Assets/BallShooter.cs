using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour {
    [SerializeField] private Rigidbody PairedRB;
    [SerializeField] private NetworkObject PairedNO;
    [SerializeField] private GameObject ShotAssetContainer;
    [SerializeField] private LayerMask GroundMask;

    [Space(5)]

    [SerializeField] private MeshRenderer ShotArrowRenderer;
    private Vector3 ShotArrowScale_Base;
    private float ShotArrowScale_MaxZ;


    [Space(5)]

    
    private bool Shootable = false;
    [HideInInspector] public Vector3 GravityDir = Vector3.down;


    [SerializeField] private float Sensitivity_Force = 1;
    [SerializeField] private float Sensitivity_Rotation = 1;

    [Space(5)]

    [SerializeField] private float Power_Force = 1;
    [SerializeField] private float StopThreshold = 0.01f; 


    private float ForceAmount = 0;
    private float RotationAngle = 0;
    private bool ShotEngaged = false;



    private void Start() {
        if(!PairedNO.IsOwner) {
            Destroy(this);
        }

        ShotArrowScale_Base = ShotArrowRenderer.transform.localScale;
        ShotArrowScale_MaxZ = ShotArrowScale_Base.z;

        ToggleLock(false);
    }


    public Vector3 GetNormal() {
        if(Physics.Raycast(PairedRB.position, GravityDir, out RaycastHit hit, 1, GroundMask, QueryTriggerInteraction.Ignore)) {
            return hit.normal;
        }

        return -GravityDir;
    }


    public void FixedUpdate() {
        if(!Shootable) {
            if(PairedRB.velocity.sqrMagnitude <= StopThreshold) {
                PairedRB.velocity = Vector3.zero;
                ToggleLock(true);
            }
        } else {
            if(ShotEngaged) {
                // Update shoot interface
                ForceAmount += -InputHandler.Sets(InputState.Game).Look.y * Sensitivity_Force;

                // float atan = Mathf.Atan2(InputHandler.Sets(InputState.Game).Look.y * Sensitivity_Rotation, InputHandler.Sets(InputState.Game).Look.x * Sensitivity_Rotation);
                Vector2 mousePos = Game._internalInput.Player.MousePos.ReadValue<Vector2>();
                mousePos = Game.Manager.CameraController.Camera.ScreenToViewportPoint(mousePos);
                const float cycles = 5f;
                RotationAngle = 360 * (mousePos.x - 0.5f) * cycles; // Mathf.Atan2((mousePos.y + 0.5f) / 1f, (mousePos.x + 0.5f) / 1f) * Mathf.Rad2Deg;

                ForceAmount = Mathf.Clamp01(ForceAmount);
                RotationAngle = Mathf.Clamp(RotationAngle, 0f, 360);

                SetArrow();

                if(!InputHandler.Sets(InputState.Game).Activate) {
                    Hit(ForceAmount, RotationAngle);
                }

            } else if(InputHandler.Sets(InputState.Game).Activate) {
                ShotEngaged = true;
            }
        }
    }

    
    public void Hit(float force, float rotationAngle) {
        Vector3 normal = GetNormal();

        Vector3 flatForward = Game.Manager.CameraController.Camera.transform.forward;
        flatForward.y = 0;

        float camAngle = Vector3.SignedAngle(flatForward, Vector3.right, -GravityDir);

        Quaternion shotRot = Quaternion.AngleAxis(camAngle + rotationAngle, normal);
        Vector3 forceDir = shotRot * Vector3.Cross(Vector3.right, normal);

        ToggleLock(false);

        ShotEngaged = false;
        
        float finalForce = force * Power_Force;
        PairedRB.AddForce(forceDir * finalForce, ForceMode.Impulse);
    }



    private void SetArrow() {
        // Set scale
        Vector3 arrowScale = new Vector3(ShotArrowScale_Base.x, ShotArrowScale_Base.y, ShotArrowScale_MaxZ * ForceAmount);
        ShotArrowRenderer.transform.localScale = arrowScale;

        // Set rotation
        Vector3 flatForward = Game.Manager.CameraController.Camera.transform.forward;
        flatForward.y = 0;

        float camAngle = Vector3.SignedAngle(flatForward, Vector3.right, -GravityDir);

        Quaternion shotRot = Quaternion.AngleAxis(camAngle + RotationAngle, -GravityDir);
        ShotArrowRenderer.transform.rotation = shotRot;
    }


    public void ToggleLock(bool locked) {
        Shootable = locked;
        PairedRB.constraints = locked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;

        ForceAmount = 0;
        RotationAngle = 0;

        ShotAssetContainer.SetActive(locked);
    }
}
