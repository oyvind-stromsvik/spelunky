using UnityEngine;

// TODO: Create a github UPM package from this so I can share it between my projects.
public static class ExtensionMethods {

    /// <summary>
    /// Reset a transform the same way you can in the inspector.
    /// </summary>
    /// <param name="transform"></param>
    public static void Reset(this Transform transform) {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Remaps a value from a current minimum a1 and maximum a2 to a new minimum b1 and maximum b2.
    /// </summary>
    /// <param name="value">The value to remap.</param>
    /// <param name="a1">The current minimum</param>
    /// <param name="a2">The current maximum</param>
    /// <param name="b1">The new minimum</param>
    /// <param name="b2">The new maximum</param>
    /// <returns></returns>
    public static float Remap(this float value, float a1, float a2, float b1, float b2) {
        return b1 + (value - a1) * (b2 - b1) / (a2 - a1);
    }
}
