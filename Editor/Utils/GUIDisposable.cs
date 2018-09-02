using System;

public abstract class GUIDisposable : IDisposable
{
    public GUIDisposable()
    {
    }

    public abstract void Dispose();
}