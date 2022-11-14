using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {
    public AudioSource MusicIntroSource;
    public AudioSource MusicLoopSource;

    private Coroutine _lastCoroutine = null;
    private MusicData _curData = null;


    private void Awake() {
        if(Game.Manager.MusicPlayer != null) {
            Destroy(this.gameObject);
            return;
        }

        Game.Manager.MusicPlayer = this;

        // Options.VolumeChanged_Master += ReadjustVolume;
    }

    private void Start() {
        _lastCoroutine = null;
        _curData = null;

        // DontDestroyOnLoad(this.gameObject);
    }


    public IEnumerator LoadMusic(MusicData data, bool force = false, IEnumerator loadWait = null) {
        if(force) {
            if(_lastCoroutine != null) {
                if(_curData != null) {
                    _curData.TryForceUnload();
                }
                StopCoroutine(_lastCoroutine);
                _lastCoroutine = null;
            }
        } else {
            if(_curData == null && _lastCoroutine != null) {
                StopCoroutine(_lastCoroutine);
            } else if(_curData != null && _curData.Playing) {
                _curData.End();
            }
        }

        while(_lastCoroutine != null) {
            yield return null;
        }

        _curData = data;

        ReadjustVolume();

        data.Playing = true;
        _lastCoroutine = StartCoroutine(data.Load(MusicIntroSource, MusicLoopSource, DoneSet, loadWait));
    }


    private void ReadjustVolume() {
        if(_curData != null) {
            float volume = _curData.Volume ; // * Options.Volume_Master;
            MusicIntroSource.volume = volume;
            MusicLoopSource.volume = volume;
        }
    }


    private void DoneSet() {
        _lastCoroutine = null;
    }
}
