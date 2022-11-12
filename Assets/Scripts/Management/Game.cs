using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game {
	public static VersionData VersionData { get; private set; }

    public static GameManager Manager;






    static Game() {
		Application.targetFrameRate = 60; // This should be set in settings, but I'm doing it now because this is stupid.

		LoadVersionData();
	}

	public static void LoadVersionData() {
		string versionName = "0.1_b03";
		string versionHash = versionName;

		VersionData = new VersionData(versionName, versionHash);
	}


    #region Helper Methods

    public static void CloseGame() {
		#if UNITY_EDITOR
 		if(UnityEditor.EditorApplication.isPlaying) {
 		     UnityEditor.EditorApplication.isPlaying = false;
 		}
		#else
		Application.Quit();
 		#endif
	}

    #endregion


    #region Input
    private static InputActions InputActions { get; set; } = new _InputActions();
	internal static InputActions _internalInput => InputActions;

    // Modified controls class to enable on construction
	private class _InputActions : InputActions {
		public _InputActions() : base() {
			Enable();
		}
	}

	#endregion
}
