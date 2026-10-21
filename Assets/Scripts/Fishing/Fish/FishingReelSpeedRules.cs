using System;

// Pure rules: normalized resistance sets a ceiling on manual reeling speed.
public static class FishingReelSpeedRules
{
    public static float BandCeiling(float ratio, float greenThreshold, float redThreshold,
        float mediumMultiplier, float redMultiplier)
    {
        float green = Math.Max(0f, greenThreshold);
        float red = Math.Max(green + 0.01f, redThreshold);
        float mediumSpeed = Clamp(mediumMultiplier, 0.05f, 1f);
        float redSpeed = Clamp(redMultiplier, 0.05f, mediumSpeed);
        if (ratio <= green) return 1f;
        if (ratio >= red) return redSpeed;

        float middle = (green + red) * 0.5f;
        return ratio <= middle
            ? Smooth(1f, mediumSpeed, (ratio - green) / (middle - green))
            : Smooth(mediumSpeed, redSpeed, (ratio - middle) / (red - middle));
    }

    public static float Combine(float phaseResistance, float speedCeiling)
    {
        float phaseSpeed = 1f / (1f + Math.Max(0f, phaseResistance));
        // Use a ceiling, not two multiplied penalties. A stronger individual
        // struggle can still slow below the ceiling, but recovery cannot bypass it.
        return Math.Min(phaseSpeed, Clamp(speedCeiling, 0.05f, 1f));
    }

    private static float Smooth(float from, float to, float t)
    {
        t = Clamp(t, 0f, 1f);
        return from + (to - from) * (t * t * (3f - 2f * t));
    }

    private static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
