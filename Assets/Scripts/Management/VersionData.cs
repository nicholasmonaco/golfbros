using System;

[Serializable]
public class VersionData {
    public string VersionId { get; private set; }
    public string VersionHash { get; private set; }

    public VersionData(string id, string hash) {
        VersionId = id;
        VersionHash = hash;
    }
}