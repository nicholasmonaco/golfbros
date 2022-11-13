using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class PlayerLobbyEntry : MonoBehaviour {
    public RectTransform RT;
    [SerializeField] private Image ColorImage;
    [SerializeField] private GameObject ColorPickerContainer;
    [SerializeField] private ColorPickerControl ColorPicker;
    [SerializeField] private TMP_InputField NameField;
    [SerializeField] private TMP_Text ReadyGraphic;

    [Space(5)]
    [SerializeField] private Color Color_NonReady = Color.white;
    [SerializeField] private Color Color_Ready = Color.red;

    [NonSerialized] public int Id = -1;

    private Action<Color, string> _updateCallback = null;


    public void Load(int id, string name, Color c) {
        Id = id;

        ColorImage.color = c;
        NameField.text = name;
    }

    
    public void Update() {
        if(ColorPickerContainer.activeSelf && InputHandler.Sets(InputState.GameUI).Escape) {
            SetColor();
            ToggleColorPicker(false);
        }
    }


    public void SetUpdateCallback(Action<Color, string> callback) {
        _updateCallback = callback;
    }


    public void UpdateValues_Name(string newName) {
        _updateCallback?.Invoke(ColorImage.color, newName);
    }

    public void UpdateValues_Color(Color newColor) {
        _updateCallback?.Invoke(newColor, NameField.text);
    }


    public void SetInteractable(bool interactable) {
        NameField.interactable = interactable;
    }

    public void ToggleReady(bool ready) {
        ReadyGraphic.color = ready ? Color_Ready : Color_NonReady;
    }


    public void ToggleColorPicker(bool open) {
        InputHandler.SwitchState(open ? InputState.GameUI : InputHandler.LastState);

        ColorPickerContainer.SetActive(open);
    }

    public void SetColor() {
        UpdateValues_Color(ColorPicker.CurrentColor);
    }

}
