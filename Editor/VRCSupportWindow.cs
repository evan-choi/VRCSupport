using System;
using System.Text;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class VRCSupportWindow : EditorWindow
{
    const string name = "VRCSupport";

    public static VRCSupportWindow Window { get; private set; }

    public readonly EmoteOnOff EmoteOnOff;

    public readonly AnimationBlender AnimationBlender;

    public readonly AnimationMissing AnimationMissing;

    bool _animationBlenderFoldout;

    Vector2 _scrollPos;

    #region Constructor
    public VRCSupportWindow()
    {
        EmoteOnOff = new EmoteOnOff();
        AnimationBlender = new AnimationBlender();
        AnimationMissing = new AnimationMissing();
    }

    [MenuItem("오챠★/VRCSupport")]
    public static void Init()
    {
        Window = GetWindow<VRCSupportWindow>(name, true);
        Window.Show();
    }
    #endregion

    #region GUI
    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        InitializeEmoteOnOff();
        AddSpace();

        InitializeAnimationBlender();
        AddSpace();

        InitializeAnimationMissing();

        EditorGUILayout.EndScrollView();
    }

    void AddSpace(int space = 12)
    {
        GUILayout.Space(space);
    }

    void InitializeEmoteOnOff()
    {
        GUILayout.Label("Emote On/Off", EditorStyles.boldLabel);

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Target");
            EmoteOnOff.TargetObject = EditorGUILayout.ObjectField(EmoteOnOff.TargetObject, typeof(GameObject), true) as GameObject;
        }

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Fixed Joint (World)");
            EmoteOnOff.FixedJoint = EditorGUILayout.Toggle(EmoteOnOff.FixedJoint);
        }

        using (this.Enabled(EmoteOnOff.OnClip == null))
        {
            using (this.BeginScope(Scope.Horizontal))
            {
                EditorGUILayout.PrefixLabel("ON Name");
                EmoteOnOff.OnName = GUILayout.TextField(EmoteOnOff.OnName);
            }
        }

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("ON Animation");
            EmoteOnOff.OnClip = EditorGUILayout.ObjectField(EmoteOnOff.OnClip, typeof(AnimationClip), true) as AnimationClip;
        }

        using (this.Enabled(EmoteOnOff.OffClip == null))
        {
            using (this.BeginScope(Scope.Horizontal))
            {
                EditorGUILayout.PrefixLabel("OFF Name");
                EmoteOnOff.OffName = GUILayout.TextField(EmoteOnOff.OffName);
            }
        }

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("OFF Animation");
            EmoteOnOff.OffClip = EditorGUILayout.ObjectField(EmoteOnOff.OffClip, typeof(AnimationClip), true) as AnimationClip;
        }

        using (this.Enabled(ValidateEmoteOnOff()))
        {
            if (GUILayout.Button("Create"))
            {
                Execute(
                    EmoteOnOff.Apply,
                    () =>
                    {
                        EmoteOnOff.TargetObject = null;
                        EmoteOnOff.OnClip = null;
                        EmoteOnOff.OffClip = null;
                        EmoteOnOff.OnName = "";
                        EmoteOnOff.OffName = "";
                    });
            }
        }
    }

    void InitializeAnimationBlender()
    {
        GUILayout.Label("Animation Blender", EditorStyles.boldLabel);

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Source");
            AnimationBlender.SourceClip = EditorGUILayout.ObjectField(AnimationBlender.SourceClip, typeof(AnimationClip), true) as AnimationClip;
        }

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Destination");
            AnimationBlender.DestinationClip = EditorGUILayout.ObjectField(AnimationBlender.DestinationClip, typeof(AnimationClip), true) as AnimationClip;
        }

        bool validate = ValidateAnimationBlender();

        if (validate)
        {
            AnimationBlender.CurveBlendData[] curves = AnimationBlender.ConflictCurves;

            if (curves.Length > 0)
            {
                AddSpace(6);

                _animationBlenderFoldout = EditorGUILayout.Foldout(
                    _animationBlenderFoldout, 
                    "Conflict (" + curves.Length + ")");

                if (_animationBlenderFoldout)
                {
                    for (int i = 0; i < curves.Length; i++)
                    {
                        var curve = curves[i];
                        
                        if (i > 0)
                            AddSpace(2);

                        using (this.BeginScope(Scope.Horizontal))
                        {
                            EditorGUILayout.PrefixLabel("Path");
                            EditorGUILayout.LabelField(curve.Source.GetFriendlyName());
                        }

                        using (this.BeginScope(Scope.Horizontal))
                        {
                            EditorGUILayout.PrefixLabel("Blend Action");
                            curve.BlendType = (BlendType)EditorGUILayout.EnumPopup(curve.BlendType);
                        }
                    }
                }

                EditorGUILayout.HelpBox("충돌되는 애니메이션 속성이 있습니다", MessageType.Warning);
            }
        }

        using (this.Enabled(validate))
        {
            if (GUILayout.Button("Blend"))
            {
                Execute(
                    AnimationBlender.Blend,
                    () =>
                    {
                        AnimationBlender.SourceClip = null;
                        AnimationBlender.DestinationClip = null;
                    });
            }
        }
    }

    void InitializeAnimationMissing()
    {
        GUILayout.Label("Animation Missing", EditorStyles.boldLabel);

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Target");
            AnimationMissing.Target = EditorGUILayout.ObjectField(AnimationMissing.Target, typeof(GameObject), true) as GameObject;
        }

        using (this.BeginScope(Scope.Horizontal))
        {
            EditorGUILayout.PrefixLabel("Animation");
            AnimationMissing.Clip = EditorGUILayout.ObjectField(AnimationMissing.Clip, typeof(AnimationClip), true) as AnimationClip;
        }

        bool validate = ValidateAnimationMissing();

        if (validate)
        {
            EditorCurveBinding[] missings = AnimationMissing.MissingCurves;

            if (missings.Length > 0)
            {
                var builder = new StringBuilder();

                builder.AppendFormat("※ Found {0} missings\r\n", missings.Length);

                for (int i = 0; i < missings.Length; i++)
                {
                    builder.AppendLine();
                    builder.AppendFormat(missings[i].GetFriendlyName());
                }

                EditorGUILayout.HelpBox(builder.ToString(), MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("There are no missings", MessageType.Info);
                return;
            }
        }

        using (this.Enabled(validate))
        {
            if (GUILayout.Button("Clean"))
            {
                Execute(
                    AnimationMissing.Clean,
                    () =>
                    {
                        AnimationMissing.Target = null;
                        AnimationMissing.Clip = null;
                    });
            }
        }
    }

    bool ValidateEmoteOnOff()
    {
        var buffer = new StringBuilder();

        bool ready = true;
        bool clipOverride = (EmoteOnOff.OnClip != null || EmoteOnOff.OffClip != null);

        if (EmoteOnOff.TargetObject == null)
        {
            buffer.AppendLine("대상객체가 설정되지 않았습니다");
            ready = false;
        }

        if ((string.IsNullOrEmpty(EmoteOnOff.OnName) && EmoteOnOff.OnClip == null) ||
            (string.IsNullOrEmpty(EmoteOnOff.OffName) && EmoteOnOff.OffClip == null))
        {
            buffer.AppendLine("이모트 이름 혹은 Animation을 설정해주세요");
            ready = false;
        }
        
        if (ready && !clipOverride && EmoteOnOff.OnName == EmoteOnOff.OffName)
        {
            buffer.AppendLine("이모트 On/Off 이름이 같습니다");
            ready = false;
        }

        if (ready && clipOverride && EmoteOnOff.OnClip == EmoteOnOff.OffClip)
        {
            buffer.AppendLine("이모트 On/Off 클립이 같습니다");
            ready = false;
        }

        if (ready)
        {
            string onClipName = EmoteOnOff.TargetObject.name + EmoteOnOff.OnSuffix;
            string offClipName = EmoteOnOff.TargetObject.name + EmoteOnOff.OffSuffix;

            ready = (EmoteOnOff.OnClip != null || !onClipName.EqualsIgnoreCase(EmoteOnOff.OnName));
            ready &= (EmoteOnOff.OffClip != null || !onClipName.EqualsIgnoreCase(EmoteOnOff.OffName));

            if (!ready)
            {
                buffer.AppendLine("예약된 애니메이션 파일 이름입니다");
                ready = false;
            }
        }

        FlushHelpBox(buffer);

        return ready;
    }

    bool ValidateAnimationBlender()
    {
        var buffer = new StringBuilder();
        bool ready = true;

        if (AnimationBlender.SourceClip == null)
        {
            buffer.AppendLine("Source 애니메이션이 설정되지 않았습니다");
            ready = false;
        }

        if (AnimationBlender.DestinationClip == null)
        {
            buffer.AppendLine("Destination 애니메이션이 설정되지 않았습니다");
            ready = false;
        }

        if (ready && AnimationBlender.SourceClip == AnimationBlender.DestinationClip)
        {
            buffer.AppendLine("애니메이션 파일이 같습니다");
            ready = false;
        }

        FlushHelpBox(buffer);

        return ready;
    }

    bool ValidateAnimationMissing()
    {
        var buffer = new StringBuilder();
        bool ready = true;

        if (AnimationMissing.Target == null)
        {
            buffer.AppendLine("대상객체가 설정되지 않았습니다");
            ready = false;
        }

        if (AnimationMissing.Clip == null)
        {
            buffer.AppendLine("애니메이션이 설정되지 않았습니다");
            ready = false;
        }

        FlushHelpBox(buffer);

        return ready;
    }

    void FlushHelpBox(StringBuilder buffer)
    {
        if (buffer.Length > 0)
            EditorGUILayout.HelpBox(buffer.ToString().TrimEnd('\r', '\n'), MessageType.Error);
    }
    #endregion

    void Execute(Func<bool> run, Action clear)
    {
        if (run())
        {
            clear();
            EditorUtility.DisplayDialog(name, "성공!", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog(name, "왠지 모르게 실패했으니 5ちゃ에게 연락 ㄱㄱ", "확인");
        }
    }
}