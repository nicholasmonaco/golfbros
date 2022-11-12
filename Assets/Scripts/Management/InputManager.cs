using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    private static bool _set = false;

    private void Awake() {
        if(_set) {
            Destroy(this);
        } else {
            _set = true;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update() {
        // InputHandler.UpdateCurrentState();
    }
}

[System.Flags]
public enum InputState {
    None = (1 << 0),

    Game = (1 << 1),
    GameUI = (1 << 2),
    Chat = (1 << 3),
    Debug = (1 << 4),
    Paused = (1 << 5),
    MainMenu = (1 << 6)
}

public class InputSet {
    public InputState State { get; private set; }

    public InputSet(InputState state) {
        State = state;
        Clear();
    }


    #region Values

    public Vector2 Look { get; private set; }
    public float Zoom { get; private set; }

    public bool Activate { get; private set; }
    public bool Pan { get; private set; }

    public bool Jump { get; private set; }
    public bool Reset { get; private set; }    
    public bool Interact { get; private set; }

    public bool Chat { get; private set; }

    public bool Escape { get; private set; }
    public bool Tab { get; private set; }
    public bool Debug { get; private set; }

    #endregion


    public void Update(InputActions inputActions) {
        Look = inputActions.Player.Look.ReadValue<Vector2>();
        Zoom = inputActions.Player.Zoom.ReadValue<float>();

        Activate = inputActions.Player.Activate.IsPressed();
        Pan = inputActions.Player.Pan.IsPressed();

        Jump = inputActions.Player.Jump.IsPressed();
        Reset = inputActions.Player.Reset.IsPressed();
        Interact = inputActions.Player.Interact.IsPressed();

        Chat = inputActions.Player.Chat.IsPressed();

        Escape = inputActions.Player.Escape.IsPressed();
        Tab = inputActions.Player.Tab.IsPressed();
        Debug = inputActions.Player.Debug.IsPressed();
    }


    internal void Clear() {
        Look = Vector2.zero;
        Zoom = 0;

        Activate = false;
        Pan = false;

        Jump = false;
        Reset = false;
        Interact = false;

        Chat = false;

        Escape = false;
        Tab = false;
        Debug = false;
    }
}


public static class InputHandler {
	private static InputActions InputActions => Game._internalInput;

    public static InputState State { get; private set; } = InputState.None;
    public static InputState LastState { get; private set; } = InputState.None;

    private const InputState CursorLockMask = InputState.None | InputState.Game | InputState.Chat | InputState.Debug; 
    public static bool CursorLocked => (State & CursorLockMask) != 0;

    private static Dictionary<InputState, InputSet> InputMap;
    public static InputSet Sets(InputState state) => InputMap[state];
    public static InputSet Set => InputMap[State];

    static InputHandler() {
        const int inputStateCount = 6;
        InputMap = new Dictionary<InputState, InputSet>(inputStateCount);
        for(int i=0;i<=inputStateCount;i++) {
            InputState state = (InputState)(1 << i);
            InputMap.Add(state, new InputSet(state));
        }

        SwitchState(InputState.None);

        Set.Clear();
        SetCursorLock();
    }


    public static void SwitchState(InputState state) {
        if(state == State) return;

        Set.Clear();
        LastState = State;
        State = state;

        SetCursorLock();
    }


    private static void SetCursorLock() {
        if(CursorLocked) {
            Cursor.visible = false; // we should make a mouse cursor ui element we can toggle when we need to
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    public static void UpdateCurrentState() => InputMap[State].Update(InputActions);

}