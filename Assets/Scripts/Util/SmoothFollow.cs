using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {
    public Transform TrackedTransform;
    public float LerpSpeed = 20;
    public float SnapDistance = 0.01f;


    public void LateUpdate() {
        if(TrackedTransform == null) return;
        
        transform.position = Vector3.Lerp(transform.position, TrackedTransform.position, Time.deltaTime * LerpSpeed);

        float dist = Vector3.Distance(transform.position, TrackedTransform.position);
        if(dist <= SnapDistance) {
            transform.position = TrackedTransform.position;
        }
    }

}
