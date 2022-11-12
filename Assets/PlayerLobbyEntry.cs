using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLobbyEntry : MonoBehaviour {
    public RectTransform RT;
    [SerializeField] private Image ColorImage;
    [SerializeField] private TMP_InputField NameField;

    [NonSerialized] public int Id = -1;

    private Action _updateCallback = null;


    public void Load(int id, string name, Color c) {
        Id = id;

        ColorImage.color = c;
        NameField.text = name;
    }

    
    public void SetUpdateCallback(Action callback) {
        _updateCallback = callback;
    }

    public void UpdateValues() {
        _updateCallback?.Invoke();
    }


    public void SetInteractable(bool interactable) {
        NameField.interactable = interactable;
    }


    public string PlayerName => NameField.name;
    public Color PlayerColor => ColorImage.color;
}
