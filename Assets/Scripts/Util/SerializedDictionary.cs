using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {

    [SerializeField] protected bool AutoSerialize = true; 

    [SerializeField]
    protected List<SerializableKVP<TKey, TValue>> data = new List<SerializableKVP<TKey, TValue>>();


    public virtual void OnBeforeSerialize() {
        if(!AutoSerialize) return;

        data.Clear();

        foreach(var kvp in this) {
            data.Add(new SerializableKVP<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }

    public virtual void OnAfterDeserialize() {
        if(!AutoSerialize) return;

        this.Clear();

        foreach(var skvp in data) {
            this.Add(skvp.Key, skvp.Value);
        }
    }
}


[Serializable]
public class SerializableKVP<TKey, TValue> {
    public TKey Key;
    public TValue Value;

    public SerializableKVP(TKey key, TValue value) {
        Key = key;
        Value = value;
    }
}



// Instances

[Serializable] public class CourseBank : SerializableDictionary<CourseType, CourseData> { }
[Serializable] public class MenuScreenBank : SerializableDictionary<MenuScreenId, MenuScreen> { }
[Serializable] public class SFXBank : SerializableDictionary<string, AudioClipData> { }


[Serializable]
public class AudioClipData {
    public AudioClip Clip;
    public float Volume = 1;
}