using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Smooth;

public class BallShooter : MonoBehaviour {
    [SerializeField] private Rigidbody PairedRB;
    [SerializeField] private SphereCollider PairedCollider;
    [SerializeField] private SmoothSyncNetcode SmoothTransform;
    [SerializeField] private MeshRenderer BallRenderer;
    [SerializeField] private NetworkObject PairedNO;
    [SerializeField] private GameObject ShotAssetContainer;
    [SerializeField] private LayerMask GroundMask;
    [SerializeField] private LayerMask BallMask;

    [Space(5)]

    [SerializeField] private MeshRenderer ShotArrowRenderer;
    private Vector3 ShotArrowScale_Base;
    private float ShotArrowScale_MaxZ;


    [Space(5)]

    
    private bool Shootable = false;
    [HideInInspector] public Vector3 GravityDir = Vector3.down;

    [SerializeField] private float JumpForce = 5;

    [Space(5)]

    [SerializeField] private float ViewportRadius = 0.1f;

    [SerializeField] private float Sensitivity_Force = 1;
    [SerializeField] private float Sensitivity_Rotation = 1;

    [Space(5)]

    [SerializeField] private float Power_Force = 1;
    [SerializeField] private float StopThreshold = 0.01f; 
    [SerializeField] private float StopDuration = 1.35f;


    private float ForceAmount = 0;
    private float RotationAngle = 0;
    private bool ShotEngaged = false;
    private float StopTimer = 0;

    private bool _lastReset = false;
    private bool _lastJump = false;



    private IEnumerator Start() {
        if(!PairedNO.IsOwner) {
            Destroy(this);
        }

        ShotArrowScale_Base = ShotArrowRenderer.transform.localScale;
        ShotArrowScale_MaxZ = ShotArrowScale_Base.z;

        _lastReset = false;
        _lastJump = false;

        while(!IsGrounded()) yield return null;

        ToggleLock(false);
    }


    private bool IsGrounded() {
        return Physics.Raycast(PairedRB.position, GravityDir, PairedCollider.radius + 0.01f, GroundMask, QueryTriggerInteraction.Ignore);
    }


    public Vector3 GetNormal() {
        if(Physics.Raycast(PairedRB.position, GravityDir, out RaycastHit hit, 1, GroundMask, QueryTriggerInteraction.Ignore)) {
            return hit.normal;
        }

        return -GravityDir;
    }


    private void Update() {
        // Jump
        bool inJump = InputHandler.Sets(InputState.Game).Jump;
        if(inJump && !_lastJump) {
            Jump();
        }
        _lastJump = inJump;


        // Reset
        bool inReset = InputHandler.Sets(InputState.Game).Reset;
        if(inReset && !_lastReset) {
            ResetBall();
        }
        _lastReset = inReset;
    }


    public void FixedUpdate() {
        if(!Shootable) {
            if(IsGrounded() && PairedRB.velocity.sqrMagnitude <= StopThreshold) {
                StopTimer -= Time.fixedDeltaTime;
                if(StopTimer <= 0) {
                    PairedRB.velocity = Vector3.zero;
                    ToggleLock(true);
                }
            } else {
                StopTimer = StopDuration;
            }
        } else {
            if(ShotEngaged) {
                // Update shoot interface
                UpdateArrowValues(Game._internalInput.Player.MousePos.ReadValue<Vector2>());
                SetArrow();

                if(!InputHandler.Sets(InputState.Game).Activate) {
                    Hit(ForceAmount, RotationAngle, true);
                }

            } else if(InputHandler.Sets(InputState.Game).Activate) {
                ShotEngaged = true;
            }
        }
    }


    private void UpdateArrowValues(Vector2 usedPos) {
        Vector2 mousePos = usedPos;
        mousePos = Game.Manager.CameraController.Camera.ScreenToViewportPoint(mousePos);

        Vector2 ballViewportPos = Game.Manager.CameraController.Camera.WorldToViewportPoint(PairedRB.position);

        Vector2 viewPortDiff = mousePos - ballViewportPos;
        Vector2 viewDir = viewPortDiff.normalized;
        float magLimit = Mathf.Clamp(viewPortDiff.magnitude, 0.002f, ViewportRadius);
        Vector2 finalDiff = viewDir * magLimit;

        // Amount that finaldiff is in direction of camera
        float forceDiff = Mathf.InverseLerp(0, ViewportRadius, magLimit);

        float rotationDiff = Mathf.Atan2(-viewDir.y, viewDir.x) * Mathf.Rad2Deg;

        ForceAmount = Mathf.Clamp01(forceDiff);
        RotationAngle = rotationDiff;
    }

    
    public void Hit(float force, float rotationAngle, bool countTowardsScore) {
        Vector3 normal = GetNormal();

        Vector3 flatForward = Game.Manager.CameraController.Camera.transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        float camAngle = Vector3.SignedAngle(Vector3.right, flatForward, -GravityDir);

        Quaternion shotRot = Quaternion.AngleAxis(camAngle + rotationAngle, normal);
        Vector3 forceDir = shotRot * Vector3.Cross(Vector3.right, normal);

        float finalForce = force * Power_Force;

        Hit_Internal(forceDir * finalForce);
    }

    private void Hit_Internal(Vector3 forceDir) {
        ToggleLock(false);

        ShotEngaged = false;
        StopTimer = StopDuration;
        
        PairedRB.AddForce(forceDir, ForceMode.Impulse);
    }



    private void SetArrow() {
        // Set scale
        Vector3 arrowScale = new Vector3(ShotArrowScale_Base.x, ShotArrowScale_Base.y, ShotArrowScale_MaxZ * ForceAmount);
        ShotArrowRenderer.transform.localScale = arrowScale;

        // Set position
        Vector3 p = transform.position;
        p -= new Vector3(0, transform.localScale.x / 2f + 0.015f, 0);
        ShotArrowRenderer.transform.position = p;

        // Set rotation
        Vector3 flatForward = Game.Manager.CameraController.Camera.transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        float camAngle = Vector3.SignedAngle(Vector3.right, flatForward, -GravityDir);

        Quaternion shotRot = Quaternion.AngleAxis(camAngle + RotationAngle, -GravityDir);
        ShotArrowRenderer.transform.rotation = shotRot;
    }


    public void ToggleLock(bool locked) {
        Shootable = locked;
        PairedRB.constraints = locked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;

        ForceAmount = 0;
        RotationAngle = 0;

        ShotAssetContainer.SetActive(locked);

        if(locked) {
            Vector3 p = PairedRB.position + new Vector3(0, 1, 0);
            UpdateArrowValues(p);
            SetArrow();
        }
    }



    public void ResetBall() {
        HoleData curHole = Game.Manager.CourseData.HoleDataList[Server.CurrentGameData.HoleIndex];

        PairedRB.velocity = Vector3.zero;
        PairedRB.angularVelocity = Vector3.zero;
        SmoothTransform.setPosition(curHole.StartPoint.position, true);
        PairedRB.velocity = Vector3.zero;
        PairedRB.angularVelocity = Vector3.zero;
    }


    public void Jump() {
        if(!IsGrounded()) return;

        Vector3 normal = GetNormal();
        PairedRB.AddForce(normal * JumpForce, ForceMode.Impulse);
    }


    public void SetColor(Color c) {
        BallRenderer.material.SetColor("_Color", c);
    }


    private void OnCollisionEnter(Collision collision) {
        if((collision.gameObject.layer & BallMask) != 0) {
            // Handle player collision
            Hit_Internal(collision.impulse);
        }
    }
}
