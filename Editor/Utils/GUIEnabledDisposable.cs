using UnityEngine;

public class GUIEnabledDisposable : GUIDisposable
{
    bool _enabledBackup;

    public GUIEnabledDisposable(bool enabeld)
    {
        _enabledBackup = GUI.enabled;
        GUI.enabled = enabeld;
    }

    public override void Dispose()
    {
        GUI.enabled = _enabledBackup;
    }
}