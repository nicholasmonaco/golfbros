using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public SmoothFollow CameraBallFollow;




    private void Awake() {
        Game.Manager = this;
    }
    


    #region Util

    public void ExitGame() => Game.CloseGame();

    #endregion


    #region Gameplay Management



    #endregion

}
