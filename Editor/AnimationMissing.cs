using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationMissing
{
    public GameObject Target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            Invalidate();
        }
    }

    public AnimationClip Clip
    {
        get
        {
            return _clip;
        }
        set
        {
            _clip = value;
            Invalidate();
        }
    }

    public EditorCurveBinding[] MissingCurves { get; private set; }

    GameObject _target;
    AnimationClip _clip;

    string _cache;

    void Invalidate()
    {
        if (_target == null || _clip == null)
        {
            MissingCurves = null;
            _cache = null;
            return;
        }

        MissingCurves = AnimationUtility.GetCurveBindings(_clip)
            .Where(b => AnimationUtility.GetAnimatedObject(Target, b) == null)
            .ToArray();
    }

    public bool Clean()
    {
        try
        {
            foreach (var curveBinding in MissingCurves)
                AnimationUtility.SetEditorCurve(_clip, curveBinding, null);

            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return false;
    }
}