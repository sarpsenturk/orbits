using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [SerializeField, Min(float.Epsilon)]
    double mass;
    [SerializeField, Min(float.Epsilon)]
    double radius;

    public double Mass { get { return mass; } }
    public double Radius { get { return radius; } }
    public double Mu { get { return mass * Universe.G; } }

    private void OnValidate()
    {
        transform.localScale = Vector3.one * (float)radius;
    }
}
