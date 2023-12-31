using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Orbit))]
public class OrbitRunner : MonoBehaviour
{
    Orbit orbit_;
    Universe universe_;

    private void Start()
    {
        orbit_ = GetComponent<Orbit>();
        universe_ = Universe.GetInstance();
    }

    private void Update()
    {
        Debug.Assert(orbit_ != null);
        Debug.Assert(universe_ != null);
        transform.position = (Vector3)orbit_.GetPositionAtTime(universe_.UniversalTime);
    }

    private void OnGUI()
    {
        double meanAnomaly = orbit_.GetMeanAnomaly(universe_.UniversalTime);
        GUILayout.Label(string.Format("M(t): {0}", meanAnomaly));
        double eccentricAnomaly = orbit_.GetEccentricAnomaly(meanAnomaly);
        GUILayout.Label(string.Format("E(t): {0}", eccentricAnomaly));
        double trueAnomaly = orbit_.GetTrueAnomaly(universe_.UniversalTime);
        GUILayout.Label(string.Format("v(t): {0}", trueAnomaly));
        double distance = orbit_.GetDistance(universe_.UniversalTime);
        GUILayout.Label(string.Format("r(t): {0}", distance));
        Vector3d positionVector = orbit_.GetPositionVector(trueAnomaly, distance);
        GUILayout.Label(string.Format("o(t): {0}", positionVector));
    }
}
