using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen_Main : MenuScreen {
    public override void OnLoad() {
        // Switch input to main menu
        InputHandler.SwitchState(InputState.MainMenu);
    }

    // public virtual void OnExit() { }
}
