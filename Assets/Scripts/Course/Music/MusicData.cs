using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class MusicData {
    [SerializeField] private AssetReference Music_Intro;
    [SerializeField] private AssetReference Music_Loop;

    [Space(3)]

    [Range(0, 1)] public float Volume = 1;


    public bool Playing { get; set; } = false;

    private AsyncOperationHandle<AudioClip> _introHandle;
    private AsyncOperationHandle<AudioClip> _loopHandle;
    


    public IEnumerator Load(AudioSource introPlayer, AudioSource loopPlayer, Action doneSet, IEnumerator loadWait) {
        if(introPlayer.isPlaying) introPlayer.Stop();
        if(loopPlayer.isPlaying) loopPlayer.Stop();

        if(Music_Intro.RuntimeKeyIsValid()) {
            // musicPlayer.loop = false;
            yield return LoadSingle(Music_Intro, 0);
            yield return LoadSingle(Music_Loop, 1);

            introPlayer.clip = _introHandle.Result;
            loopPlayer.clip = _loopHandle.Result;

            if(loadWait != null) yield return loadWait;
            
            introPlayer.Play();


            float introDuration = introPlayer.clip.length;
            // while(introPlayer.isPlaying && Playing) yield return null;
            while(introDuration >= Time.deltaTime && Playing) {
                introDuration -= Time.deltaTime;
                yield return null;
            }

            introPlayer.clip = null;

            // musicPlayer.loop = true;
        } else {
            if(loadWait != null) yield return loadWait;

            yield return LoadSingle(Music_Loop, 1);

            loopPlayer.clip = _loopHandle.Result;
        }

        loopPlayer.Play();

        while(Playing) yield return null;

        loopPlayer.Stop();
        loopPlayer.clip = null;

        if(_introHandle.IsValid()) Addressables.Release(_introHandle);
        if(_loopHandle.IsValid()) Addressables.Release(_loopHandle);

        doneSet();
    }


    private IEnumerator LoadSingle(AssetReference assetRef, int handleRef) {
        var curOpHandle = assetRef.LoadAssetAsync<AudioClip>();

        switch(handleRef) {
            default: break;
            case 0: 
                _introHandle = curOpHandle;
                break;
            case 1: 
                _loopHandle = curOpHandle;
                break;
        }

        yield return curOpHandle;
    }



    public void End() {
        Playing = false;
    }


    public void TryForceUnload() {
        if(_introHandle.IsValid()) Addressables.Release(_introHandle);
        if(_loopHandle.IsValid()) Addressables.Release(_loopHandle);
    }
}