using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {
    public Transform TrackedTransform;
    public float LerpSpeed = 20;
    public float SnapDistance = 0.01f;
    public float MaxSnapDistance = 5;


    public void Update() {
        if(TrackedTransform == null) return;
        
        Vector3 testPos = Vector3.Lerp(transform.position, TrackedTransform.position, Time.deltaTime * LerpSpeed);

        float dist = Vector3.Distance(testPos, TrackedTransform.position);
        if(dist <= SnapDistance) {
            transform.position = TrackedTransform.position;
        
        } 
        // else if (dist >= MaxSnapDistance) {
        //     Vector3 d = (TrackedTransform.position - transform.position).normalized * MaxSnapDistance * 0.95f;
        //     transform.position = d;

        // } 
        else {
            transform.position = testPos;
        }
    }

}
