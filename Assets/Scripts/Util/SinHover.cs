using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinHover : MonoBehaviour {
    public bool Bobbing = true;
    public Vector3 MovementVector = Vector3.up;

    private Vector3 _origPosition;
    private Vector3 _finalPosition;


    private Coroutine PulseRoutine;

    [SerializeField] private float UpTime = 1;
    [SerializeField] private float DownTime = 1;

    private bool _set = false;


    private void Start() {
        _origPosition = transform.localPosition;
        _finalPosition = _origPosition + MovementVector;

        _set = true;

        PulseRoutine = StartCoroutine(Pulse());
    }


    private void OnEnable() {
        if(_set) PulseRoutine = StartCoroutine(Pulse());
    }

    private void OnDisable() {
        if(PulseRoutine != null) StopCoroutine(PulseRoutine);
    }


    private IEnumerator Pulse() {
        transform.localPosition = _origPosition;

        float timer = 0;
        bool up = true;

        while(true) {
            yield return new WaitForFixedUpdate();

            if(!Bobbing) continue;

            if(up) {
                timer += Time.fixedDeltaTime;
                
                if(timer > UpTime) {
                    timer = UpTime;
                    transform.localPosition = Vector3.Lerp(_origPosition, _finalPosition, SinLerp(timer / UpTime));
                    timer = DownTime;
                    up = false;
                } else {
                    transform.localPosition = Vector3.Lerp(_origPosition, _finalPosition, SinLerp(timer / UpTime));
                }

            } else {
                timer -= Time.fixedDeltaTime;

                if(timer < 0) {
                    timer = 0;
                    up = true;
                }

                transform.localPosition = Vector3.Lerp(_origPosition, _finalPosition, SinLerp(timer / DownTime));
            }
        }
    }


    private float SinLerp(float frac) {
        return (Mathf.Sin(Mathf.PI * frac - (Mathf.PI / 2f)) + 1f) / 2f;
    }
}
