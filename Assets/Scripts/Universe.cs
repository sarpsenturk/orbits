using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour
{
    /// <summary>
    /// Gravitational constant
    /// </summary>
    public const double G = 6.67430e-11;

    /// <summary>
    /// Universe epoch
    /// </summary>
    public const double Epoch = 0;

    /// <summary>
    /// Finds the Universe inside the scene
    /// </summary>
    /// <returns>Universe instance in scene</returns>
    public static Universe GetInstance()
    {
        GameObject universe = GameObject.Find("Universe");
        Debug.Assert(universe != null);
        return universe.GetComponent<Universe>();
    }

    public double UniversalTime = Epoch;
    public double TimeScale { get; private set; } = 1.0;

    public void SetTimeScale(double timeScale)
    {
        TimeScale = timeScale;
    }

    private void Update()
    {
        UniversalTime += Time.unscaledDeltaTime * TimeScale;
    }
}
