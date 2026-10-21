using UnityEngine;

// A load describes resistance and acceleration; it never moves the hook itself.
public struct HookLoadSample
{
    public float ReelResistance { get; }
    public Vector2 Acceleration { get; }
    public float LinePullStrength { get; }

    public HookLoadSample(float reelResistance, Vector2 acceleration, float linePullStrength = 0f)
    {
        ReelResistance = Mathf.Max(0f, reelResistance);
        Acceleration = acceleration;
        LinePullStrength = Mathf.Max(0f, linePullStrength);
    }
}

public interface IHookLoadProvider
{
    // Called once per physics tick by HookController, not per substep.
    HookLoadSample SampleLoad(float deltaTime);
}
