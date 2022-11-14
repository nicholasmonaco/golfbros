using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallUtil : MonoBehaviour {
    [SerializeField] private MeshRenderer BallRenderer;
    [SerializeField] private Outline BallOutline;
    public AudioSource BallSFXPlayer;

    [Space(5)]

    [SerializeField, Range(0, 1f)] private float OutlineIntensity = 0.8f;
    [SerializeField, Range(0, 1f)] private float OutlineAlpha = 1;



    public void SetColor(Color c) {
        BallRenderer.material.SetColor("_Color", c);

        Color outlineColor = c * OutlineIntensity;
        outlineColor.a = OutlineAlpha;
        BallOutline.OutlineColor = outlineColor;
    }
}
