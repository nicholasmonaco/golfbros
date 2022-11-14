using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Smooth;

public class BallShooter : MonoBehaviour {
    public BallUtil BallUtil;

    [Space(5)]

    [SerializeField] private Rigidbody PairedRB;
    [SerializeField] private SphereCollider PairedCollider;
    [SerializeField] private SmoothSyncNetcode SmoothTransform;
    [SerializeField] private NetworkObject PairedNO;
    [SerializeField] private GameObject ShotAssetContainer;
    [SerializeField] private LayerMask GroundMask;
    private AudioSource BallSFXPlayer => BallUtil.BallSFXPlayer;

    [Space(5)]

    [SerializeField] private float FastSFXThreshold = 1;
    [SerializeField] private SFXBank SFXBank;

    [Space(5)]

    [SerializeField] private MeshRenderer ShotArrowRenderer;
    [SerializeField] private float ShotArrowMaxSizeScalar = 1;
    private Vector3 ShotArrowScale_Base;
    private float ShotArrowScale_MaxZ => ShotArrowScale_Base.z;


    [Space(5)]

    
    private bool Shootable = false;
    private bool WonHole = false;
    [HideInInspector] public Vector3 GravityDir = Vector3.down;

    [SerializeField] private float JumpForce = 5;
    [SerializeField] private float JumpWaitDuration = 0.25f;

    [Space(5)]

    [SerializeField] private float ViewportRadius = 0.1f;

    // [SerializeField] private float Sensitivity_Force = 1;

    [Space(5)]

    [SerializeField] private float Power_Force = 1;
    [SerializeField] private AnimationCurve Power_Curve = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private float StopThreshold = 0.01f; 
    [SerializeField] private float StopDuration = 1.35f;

    [Space(5)]

    [SerializeField] private float ResetCooldown = 1.5f;


    private Vector3? _lastStablePosition = null;


    private float ForceAmount = 0;
    private float RotationAngle = 0;
    private bool ShotEngaged = false;
    private float StopTimer = 0;

    private bool _lastReset = false;
    private float _resetHoldTimer = 0;
    private float _resetCooldownTimer = 0;
    private bool _lastJump = false;
    private float _jumpWaitTimer = 0;



    private IEnumerator Start() {
        if(!PairedNO.IsOwner) {
            Destroy(ShotArrowRenderer.gameObject);
            Destroy(this);
        }

        if(Game.Manager.CurrentAudioListener != null) Destroy(Game.Manager.CurrentAudioListener);
        Game.Manager.CurrentAudioListener = Instantiate(Game.Manager.AudioListenerPrefab, transform);
        Game.Manager.CurrentAudioListener.transform.localPosition = Vector3.zero;

        ShotArrowScale_Base = ShotArrowRenderer.transform.localScale;

        _lastReset = false;
        _lastJump = false;
        _jumpWaitTimer = 0;
        _resetHoldTimer = 0;
        _resetCooldownTimer = 0;

        while(!IsGrounded()) yield return null;

        ToggleLock(false);
    }


    private bool IsGrounded(float extraDist = 0) {
        return Physics.Raycast(PairedRB.position, GravityDir, PairedCollider.radius + 0.01f + extraDist, GroundMask, QueryTriggerInteraction.Ignore);
    }


    public Vector3 GetNormal() {
        if(Physics.Raycast(PairedRB.position, GravityDir, out RaycastHit hit, 1, GroundMask, QueryTriggerInteraction.Ignore)) {
            return hit.normal;
        }

        return -GravityDir;
    }


    private void Update() {
        Update_GroundCheck();
        Update_JumpCheck();
        Update_ResetCheck();
    }


    private void Update_GroundCheck() {
        if(Shootable && !IsGrounded(0.001f)) {
            ToggleLock(false);
        }
    }

    private void Update_JumpCheck() {
        // Jump
        if(_jumpWaitTimer >= 0) _jumpWaitTimer -= Time.deltaTime;

        bool inJump = InputHandler.Sets(InputState.Game).Jump || InputHandler.Sets(InputState.Game).Activate;
        if(inJump && !_lastJump) {
            Jump();
        }
        _lastJump = inJump;
    }


    private void Update_ResetCheck() {
        // Reset
        bool inReset = InputHandler.Sets(InputState.Game).Reset;

        if(_resetCooldownTimer < ResetCooldown) {
            _resetCooldownTimer += Time.deltaTime;
            _lastReset = inReset;
            return;
        }
        
        if(!inReset && _lastReset) {
            ResetBall(false);
            _lastReset = inReset;
            return;
        }

        if (inReset) {
            if(_resetHoldTimer > 2) {
                ResetBall(true);
            } else {
                _resetHoldTimer += Time.deltaTime;
            }
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

        float finalForce = Power_Curve.Evaluate(force) * Power_Force;

        Hit_Internal(forceDir * finalForce);

        PlaySFX($"HitBall{Random.Range(0, 2)}");

        if(countTowardsScore) {
            Server.Singleton.IncrementPlayerShotCount_ServerRpc();
        }
    }

    private void Hit_Internal(Vector3 forceDir) {
        ToggleLock(false);

        ShotEngaged = false;
        StopTimer = StopDuration;
        
        PairedRB.AddForce(forceDir, ForceMode.Impulse);
    }



    private void SetArrow() {
        // Set scale
        Vector3 arrowScale = new Vector3(ShotArrowScale_Base.x, ShotArrowScale_Base.y, ShotArrowScale_MaxZ * ForceAmount * ShotArrowMaxSizeScalar);
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
        if(WonHole) return;

        Shootable = locked;
        PairedRB.constraints = locked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;

        ForceAmount = 0;
        RotationAngle = 0;

        ShotAssetContainer.SetActive(locked);

        if(locked) {
            _lastStablePosition = PairedRB.position;
            Vector3 p = PairedRB.position + new Vector3(0, 1, 0);
            UpdateArrowValues(p);
            SetArrow();
            PlaySFX("ShotReady");
        }
    }



    public void ResetBall(bool snapToHoleStart) {
        HoleData curHole = Game.Manager.CourseData.HoleDataList[Server.CurrentGameData.HoleIndex];

        PairedRB.velocity = Vector3.zero;
        PairedRB.angularVelocity = Vector3.zero;

        Vector3 pos = _lastStablePosition == null || snapToHoleStart ? curHole.StartPoint.position : _lastStablePosition.Value;
        SmoothTransform.setPosition(pos, true);
        PairedRB.velocity = Vector3.zero;
        PairedRB.angularVelocity = Vector3.zero;

        _resetHoldTimer = 0;
        _resetCooldownTimer = 0;

        if(snapToHoleStart) {
            _lastStablePosition = null;
        }

        PlaySFX("Reset");
    }


    public void Jump() {
        bool grounded = IsGrounded();
        if(Shootable || _jumpWaitTimer > 0 || !grounded || _lastStablePosition == null) return;

        _jumpWaitTimer = JumpWaitDuration;

        Vector3 normal = GetNormal();
        PairedRB.AddForce(normal * JumpForce, ForceMode.Impulse);
    }


    public void SetColor(Color c) => BallUtil.SetColor(c);


    public void ClearLastStablePos() {
        _lastStablePosition = null;
    }

    public void MarkWonHole(bool won) {
        if(won) {
            ToggleLock(true);
            Shootable = false;

            WonHole = won;
        } else {
            WonHole = won;
            ToggleLock(false);
        }
    }


    private void OnCollisionEnter(Collision collision) {
        if(Game.InMask(collision.gameObject.layer, Game.Manager.BallMask)) {
            // Handle player collision
            Hit_Internal(collision.impulse);
            PlaySFX("Impact_Ball");
        } else {
            string fast = collision.impulse.magnitude >= FastSFXThreshold ? "Fast" : "Slow";
            PlaySFX($"Impact_Wall_{fast}");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(Game.InMask(other.gameObject.layer, Game.Manager.GoalMask)) {
            // Mark hole as finished for this player
            PlaySFX("Goal");
            PlaySFX("GoalCelebrate");
            MarkWonHole(true); // Local handle
            Server.Singleton.MarkPlayerHoleWon_ServerRpc(); // Server handle
        }
        else if(Game.InMask(other.gameObject.layer, Game.Manager.BoundsMask)) {
            // Reset pos
            ResetBall(false);
        }
    }



    private void PlaySFX(string id) {
        if(SFXBank.TryGetValue(id, out AudioClipData data)) {
            BallSFXPlayer.PlayOneShot(data.Clip, data.Volume * Options.Volume_Master * Options.Volume_SFX);
        }
    }
}
