using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EmoteOnOff
{
    public const string NameContainer = "CONTAINER";
    public const string SuffixOn = "_TOGGLE";
    public const string NameOff = "OFF";

    static readonly float frame = 1 / 60f;

    public GameObject TargetObject { get; set; }

    public string OnName { get; set; }

    public string OffName { get; set; }

    public AnimationClip OnClip { get; set; }

    public AnimationClip OffClip { get; set; }

    public bool FixedJoint { get; set; }

    string _sceneDirectory;
    string _assetDirectory;

    GameObject _avatar;
    GameObject _targetParent;

    GameObject _containerOn;
    GameObject _containerOff;
    GameObject _container;

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
            _assetDirectory = "Assets/VrcSupport/Animation";

            _avatar = GetRoot(TargetObject);

            if (TargetObject.transform.parent != null)
                _targetParent = TargetObject.transform.parent.gameObject;

            var position = TargetObject.transform.position;

            _containerOn = new GameObject(TargetObject.name + SuffixOn);
            _containerOff = new GameObject(NameOff);
            _container = new GameObject(NameContainer);

            _container.SetActive(false);

            if (_targetParent != null)
                _containerOn.SetParent(_targetParent);

            _containerOff.SetParent(_containerOn);
            _container.SetParent(_containerOff);
            TargetObject.SetParent(_container);

            TargetObject.transform.position = Vector3.zero;
            _containerOn.transform.position = position;

            CreateToggleAnimator();
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

    void CreateToggleAnimator()
    {
        var containerOnAnimator = _containerOn.AddComponent<Animator>();
        var containerOffAnimator = _containerOff.AddComponent<Animator>();

        containerOnAnimator.enabled = false;
        containerOffAnimator.enabled = false;

        containerOnAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(_assetDirectory + "/OnController.controller");
        containerOffAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(_assetDirectory + "/OffController.controller");
    }

    void CreateToggleEmoteAnimation()
    {
        var emoteOnAnim = OnClip ?? new AnimationClip();
        var emoteOffAnim = OffClip ?? new AnimationClip();

        var onPath = AnimationUtility.CalculateTransformPath(_containerOn.transform, _avatar.transform);
        var offPath = AnimationUtility.CalculateTransformPath(_containerOff.transform, _avatar.transform);
        var curveEnable = AnimationCurve.Linear(0, 1, frame * 10, 1);
        var curveDisable = AnimationCurve.Linear(0, 0, frame * 10, 0);

        emoteOnAnim.SetCurve(onPath, typeof(Behaviour), "m_Enabled", curveEnable);
        emoteOnAnim.SetCurve(offPath, typeof(Behaviour), "m_Enabled", curveDisable);

        emoteOffAnim.SetCurve(onPath, typeof(Behaviour), "m_Enabled", curveDisable);
        emoteOffAnim.SetCurve(offPath, typeof(Behaviour), "m_Enabled", curveEnable);

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