using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FishingCastClipBuilder
{
    private const float WindupEnd = 5f / 6f;
    private const float HoldPoseTime = 3f / 6f;
    private const float SourceEnd = 1.5f;
    private const float Epsilon = 0.00001f;

    [MenuItem("Tools/Fishing/Create Cast Clips From Selected")]
    private static void CreateClips()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Hay thoat Play Mode truoc.");
            return;
        }

        AnimationClip source = Selection.activeObject as AnimationClip;

        if (source == null)
        {
            Debug.LogError(
                "Chon PlayerCastRelease.anim trong Project truoc.");
            return;
        }

        // Công cụ này dành cho clip đầy đủ đã kiểm tra.
        if (!HasCompleteSpriteSequence(source))
        {
            Debug.LogError(
                "Clip duoc chon khong co du 5 Sprite hop le. " +
                "Hay chon ban Release day du, khong chon Hold.",
                source);
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(source);
        string folder = AssetDatabase.GenerateUniqueAssetPath(
            "Assets/FishingCastRecovered");

        AssetDatabase.CreateFolder(
            "Assets",
            System.IO.Path.GetFileName(folder));

        try
        {
            // Giữ thêm một bản nguồn đầy đủ.
            if (!AssetDatabase.CopyAsset(
                    sourcePath, folder + "/PlayerCastFullBackup.anim"))
            {
                throw new Exception("Khong tao duoc ban sao clip nguon.");
            }

            AnimationClip windup = BuildClip(
                source, "PlayerCastWindup",
                0f, WindupEnd, false, false);

            AnimationClip hold = BuildClip(
                source, "PlayerCastHold",
                HoldPoseTime, HoldPoseTime + 1f / 6f,
                true, true);

            AnimationClip release = BuildClip(
                source, "PlayerCastRelease",
                WindupEnd, SourceEnd, false, false);

            // Đặt Event sau thời điểm bắt đầu một nhịp nhỏ.
            // Pose thứ 4 đã có từ thời điểm 0 của Release.
            float releaseDuration = SourceEnd - WindupEnd;

            AnimationUtility.SetAnimationEvents(
                release,
                new[]
                {
                    new AnimationEvent
                    {
                        time = 1f / 60f,
                        functionName = "OnCastRelease"
                    },
                    new AnimationEvent
                    {
                        time = releaseDuration - 1f / 60f,
                        functionName = "OnCastFinished"
                    }
                });

            AssetDatabase.CreateAsset(
                windup, folder + "/PlayerCastWindup.anim");

            AssetDatabase.CreateAsset(
                hold, folder + "/PlayerCastHold.anim");

            AssetDatabase.CreateAsset(
                release, folder + "/PlayerCastRelease.anim");

            AssetDatabase.SaveAssets();

            Selection.activeObject = release;
            EditorGUIUtility.PingObject(release);

            Debug.Log(
                "Da tao 3 clip va ban sao nguon trong: " + folder +
                "\nCac file cu van duoc giu nguyen.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static bool HasCompleteSpriteSequence(AnimationClip source)
    {
        foreach (EditorCurveBinding binding in
                 AnimationUtility.GetObjectReferenceCurveBindings(source))
        {
            if (binding.type != typeof(SpriteRenderer) ||
                binding.propertyName != "m_Sprite")
            {
                continue;
            }

            var keys = AnimationUtility.GetObjectReferenceCurve(
                source, binding);

            var sprites = new HashSet<UnityEngine.Object>();

            foreach (var key in keys)
            {
                if (key.value == null)
                    return false;

                sprites.Add(key.value);
            }

            if (sprites.Count == 5 &&
                keys.Length > 0 &&
                keys[keys.Length - 1].time >= 1.3f &&
                source.length >= SourceEnd - Epsilon)
            {
                return true;
            }
        }

        return false;
    }

    private static AnimationClip BuildClip(
        AnimationClip source,
        string clipName,
        float start,
        float end,
        bool freezePose,
        bool loop)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60f,
            legacy = false
        };

        float duration = end - start;

        // Sao chép các thuộc tính số: vị trí tay, góc cần...
        foreach (EditorCurveBinding binding in
                 AnimationUtility.GetCurveBindings(source))
        {
            AnimationCurve original =
                AnimationUtility.GetEditorCurve(source, binding);

            var keys = new List<Keyframe>
            {
                StepKey(0f, original.Evaluate(start))
            };

            if (!freezePose)
            {
                foreach (Keyframe key in original.keys)
                {
                    if (key.time > start + Epsilon &&
                        key.time < end - Epsilon)
                    {
                        keys.Add(StepKey(
                            key.time - start, key.value));
                    }
                }
            }

            float lastValue = original.Evaluate(
                freezePose ? start : end - Epsilon);

            keys.Add(StepKey(duration, lastValue));

            var curve = new AnimationCurve(keys.ToArray());

            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(
                    curve, i, AnimationUtility.TangentMode.Constant);

                AnimationUtility.SetKeyRightTangentMode(
                    curve, i, AnimationUtility.TangentMode.Constant);
            }

            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        // Sprite là object reference, phải xử lý riêng.
        foreach (EditorCurveBinding binding in
                 AnimationUtility.GetObjectReferenceCurveBindings(source))
        {
            var original = AnimationUtility.GetObjectReferenceCurve(
                source, binding);

            if (original.Length == 0)
                continue;

            var keys = new List<ObjectReferenceKeyframe>
            {
                new ObjectReferenceKeyframe
                {
                    time = 0f,
                    value = ValueAt(original, start)
                }
            };

            if (!freezePose)
            {
                foreach (var key in original)
                {
                    if (key.time > start + Epsilon &&
                        key.time < end - Epsilon)
                    {
                        keys.Add(new ObjectReferenceKeyframe
                        {
                            time = key.time - start,
                            value = key.value
                        });
                    }
                }
            }

            keys.Add(new ObjectReferenceKeyframe
            {
                time = duration,
                value = ValueAt(
                    original, freezePose ? start : end - Epsilon)
            });

            AnimationUtility.SetObjectReferenceCurve(
                clip, binding, keys.ToArray());
        }

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.startTime = 0f;
        settings.stopTime = duration;
        settings.loopTime = loop;

        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }

    private static Keyframe StepKey(float time, float value)
    {
        return new Keyframe(
            time, value,
            float.PositiveInfinity,
            float.PositiveInfinity);
    }

    private static UnityEngine.Object ValueAt(
        ObjectReferenceKeyframe[] keys, float time)
    {
        UnityEngine.Object value = keys[0].value;

        foreach (var key in keys)
        {
            if (key.time > time + Epsilon)
                break;

            value = key.value;
        }

        return value;
    }
}