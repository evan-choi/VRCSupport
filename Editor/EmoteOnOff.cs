using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EmoteOnOff
{
    public const string OnSuffix = "_ON";
    public const string OffSuffix = "_OFF";

    static readonly float frame = 1 / 60f;

    public GameObject TargetObject { get; set; }

    public string OnName { get; set; }

    public string OffName { get; set; }

    public AnimationClip OnClip { get; set; }

    public AnimationClip OffClip { get; set; }

    public bool FixedJoint { get; set; }

    string _sceneDirectory;

    GameObject _avatar;
    GameObject _targetParent;

    GameObject _containerOn;
    GameObject _containerOff;

    public EmoteOnOff()
    {
        OnName = "";
        OffName = "";
    }

    public bool Apply()
    {
        try
        {
            _sceneDirectory = Path.GetDirectoryName(TargetObject.scene.path);
            _avatar = GetRoot(TargetObject);

            _targetParent = TargetObject.transform.parent.gameObject;
            var position = TargetObject.transform.position;

            _containerOn = new GameObject(TargetObject.name + OnSuffix);
            _containerOff = new GameObject(TargetObject.name + OffSuffix);

            _containerOn.SetParent(_targetParent);
            _containerOff.SetParent(_containerOn);
            TargetObject.SetParent(_containerOff);

            TargetObject.transform.position = Vector3.zero;
            _containerOn.transform.position = position;

            TargetObject.SetActive(false);

            CreateToggleAnimation();
            CreateToggleEmoteAnimation();

            if (FixedJoint)
                CreateFixedJoint();

            AssetDatabase.Refresh();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return false;
    }

    void CreateFixedJoint()
    {
        var rigidbody = TargetObject.GetOrAddComponent<Rigidbody>();
        var joint = TargetObject.GetOrAddComponent<FixedJoint>();

        rigidbody.mass = 1;
        rigidbody.drag = 0;
        rigidbody.angularDrag = 0;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        rigidbody.interpolation = RigidbodyInterpolation.None;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigidbody.constraints = RigidbodyConstraints.None;

        joint.connectedBody = null;
        joint.breakForce = float.PositiveInfinity;
        joint.breakTorque = float.PositiveInfinity;
        joint.enableCollision = false;
        joint.enablePreprocessing = true;

        var animation = _containerOff.GetComponent<Animation>().clip;
        string path = AnimationUtility.CalculateTransformPath(
            TargetObject.transform, 
            _containerOff.transform);

        animation.SetCurve(
            path,
            typeof(Transform),
            "localPosition.x",
            AnimationCurve.Linear(0, 0, frame, 0));

        animation.SetCurve(
            path,
            typeof(Transform),
            "localPosition.y",
            AnimationCurve.Linear(0, 0, frame, 0));

        animation.SetCurve(
            path,
            typeof(Transform),
            "localPosition.z",
            AnimationCurve.Linear(0, 0, frame, 0));
    }

    void CreateToggleAnimation()
    {
        var containerOnAnim = new AnimationClip()
        {
            legacy = true,
            wrapMode = WrapMode.Loop
        };

        var containerOffAnim = new AnimationClip()
        {
            legacy = true,
            wrapMode = WrapMode.Loop
        };

        containerOnAnim.SetCurve(
            AnimationUtility.CalculateTransformPath(
                TargetObject.transform,
                _containerOn.transform),
            TargetObject.GetType(),
            "m_IsActive",
            AnimationCurve.Linear(0, 1, frame, 1));

        containerOffAnim.SetCurve(
            AnimationUtility.CalculateTransformPath(
                TargetObject.transform,
                _containerOff.transform),
            TargetObject.GetType(),
            "m_IsActive",
            AnimationCurve.Linear(0, 0, frame, 0));

        var onAnim = _containerOn.AddComponent<Animation>();
        var offAnim = _containerOff.AddComponent<Animation>();

        onAnim.enabled = false;
        offAnim.enabled = false;

        onAnim.clip = containerOnAnim;
        offAnim.clip = containerOffAnim;

        AssetDatabase.CreateAsset(containerOnAnim, _sceneDirectory + "/" + TargetObject.name + OnSuffix + ".anim");
        AssetDatabase.CreateAsset(containerOffAnim, _sceneDirectory + "/" + TargetObject.name + OffSuffix + ".anim");
    }

    void CreateToggleEmoteAnimation()
    {
        var emoteOnAnim = OnClip ?? new AnimationClip();
        var emoteOffAnim = OffClip ?? new AnimationClip();

        emoteOnAnim.SetCurve(
            AnimationUtility.CalculateTransformPath(
                _containerOn.transform, 
                _avatar.transform),
            typeof(Animation),
            "m_Enabled",
            AnimationCurve.Linear(0, 1, frame * 10, 1));

        emoteOffAnim.SetCurve(
            AnimationUtility.CalculateTransformPath(
                _containerOff.transform, 
                _avatar.transform),
            typeof(Animation),
            "m_Enabled",
            AnimationCurve.Linear(0, 1, frame * 10, 1));

        if (OnClip == null)
            AssetDatabase.CreateAsset(emoteOnAnim, _sceneDirectory + "/" + OnName + ".anim");

        if (OffClip == null)
            AssetDatabase.CreateAsset(emoteOffAnim, _sceneDirectory + "/" + OffName + ".anim");
    }

    GameObject GetRoot(GameObject obj)
    {
        while (obj.transform.parent != null)
            obj = obj.transform.parent.gameObject;

        return obj;
    }
}