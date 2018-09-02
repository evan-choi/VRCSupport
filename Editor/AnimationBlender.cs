using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationBlender
{
    public AnimationClip SourceClip
    {
        get
        {
            return _sourceClip;
        }
        set
        {
            _sourceClip = value;
            InvalidateConflictCurves();
        }
    }

    public AnimationClip DestinationClip
    {
        get
        {
            return _destinationClip;
        }
        set
        {
            _destinationClip = value;
            InvalidateConflictCurves();
        }
    }

    public CurveBlendData[] ConflictCurves { get; private set; }

    string _cache;
    AnimationClip _sourceClip;
    AnimationClip _destinationClip;

    public bool Blend()
    {
        if (_sourceClip == null || _destinationClip == null)
            return false;

        var sourceCurves = AnimationUtility.GetCurveBindings(_sourceClip)
            .ToDictionary(c => GetUniqueKey(c));

        var conflictCurves = ConflictCurves
            .ToDictionary(c => GetUniqueKey(c.Source), c => c);

        foreach (var kv in sourceCurves)
        {
            EditorCurveBinding editorCurve = kv.Value;
            CurveBlendData data;

            if (conflictCurves.TryGetValue(kv.Key, out data))
            {
                if (data.BlendType == BlendType.None)
                    continue;
            }

            var curve = AnimationUtility.GetEditorCurve(_sourceClip, editorCurve);

            AnimationUtility.SetEditorCurve(_destinationClip, editorCurve, curve);
        }

        return true;
    }

    void InvalidateConflictCurves()
    {
        if (_sourceClip == null || _destinationClip == null)
        {
            ConflictCurves = null;
            _cache = null;
            return;
        }

        string key = string.Format("{0}{1}", _sourceClip.GetInstanceID(), _destinationClip.GetInstanceID());

        if (_cache == key)
            return;

        var bindings = new List<CurveBlendData>();

        var sourceCurves = AnimationUtility.GetCurveBindings(_sourceClip)
            .ToDictionary(c => GetUniqueKey(c));

        var destinationCurves = AnimationUtility.GetCurveBindings(_destinationClip)
            .ToDictionary(c => GetUniqueKey(c));

        foreach (var kv in sourceCurves)
        {
            EditorCurveBinding dstCurve;

            if (destinationCurves.TryGetValue(kv.Key, out dstCurve))
                bindings.Add(new CurveBlendData(kv.Value, dstCurve));
        }

        _cache = key;
        ConflictCurves = bindings.ToArray();
    }

    string GetUniqueKey(EditorCurveBinding binding)
    {
        return binding.path + ":" + binding.type.GUID + "." + binding.propertyName;
    }

    public class CurveBlendData
    {
        public readonly EditorCurveBinding Source;

        public readonly EditorCurveBinding Destination;

        public BlendType BlendType { get; set; }

        public CurveBlendData()
        {
            BlendType = BlendType.Overwrite;
        }

        public CurveBlendData(EditorCurveBinding source, EditorCurveBinding destination) : this()
        {
            Source = source;
            Destination = destination;
        }
    }
}