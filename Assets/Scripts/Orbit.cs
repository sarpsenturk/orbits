using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Orbit : MonoBehaviour
{
    [Header("Orbital Elements")]
    [SerializeField]
    double semiMajorAxis;
    [SerializeField, Range(0f, 1f - float.Epsilon)]
    double eccentricity;
    [SerializeField, Range(0f, 180f)]
    double inclination;
    [SerializeField, Range(0f, 360f)]
    double longitudeOfAscendingNode;
    [SerializeField, Range(0f, 360f)]
    double argumentOfPeriapsis;
    [SerializeField, Range(0f, 360f)]
    double trueAnomalyEpoch;
    [Header("Scene References")]
    [SerializeField]
    Universe universe;
    [SerializeField]
    CelestialBody centralBody;
    [Header("Orbit Visualization")]
    [SerializeField, Min(0)]
    int lineSegments;
    [SerializeField]
    bool drawOrbit = true;
    LineRenderer lineRenderer;
    [SerializeField]
    TMPro.TMP_Text periapsisText;
    [SerializeField]
    TMPro.TMP_Text apoapsisText;

    public double SemiMajorAxis { get { return semiMajorAxis; } }
    public double SemiMajorAxisCubed { get { return semiMajorAxis * semiMajorAxis * semiMajorAxis; } }
    public double Eccentricity { get { return eccentricity; } }
    public double Inclination { get { return inclination * Mathd.Deg2Rad; } }
    public double LongitudeOfAscendingNode { get { return longitudeOfAscendingNode * Mathd.Deg2Rad; } }
    public double ArgumentOfPeriapsis { get { return argumentOfPeriapsis * Mathd.Deg2Rad; } }
    public double TrueAnomalyEpoch { get { return trueAnomalyEpoch * Mathd.Deg2Rad; } }
    public double Period
    {
        get
        {
            Debug.Assert(centralBody != null);
            return 2 * Mathd.PI * Mathd.Sqrt(SemiMajorAxisCubed / centralBody.Mu);
        }
    }
    public double Apoapsis { get { return SemiMajorAxis * (1 + Eccentricity); } }
    public double Periapsis { get { return SemiMajorAxis * (1 - Eccentricity); } }

    public Vector3d GetPositionAtTime(double time)
    {
        Debug.Assert(centralBody != null);
        Debug.Assert(time >= 0);
        double meanAnomaly = GetMeanAnomaly(time);
        double eccentricAnomaly = GetEccentricAnomaly(meanAnomaly);
        double trueAnomaly = GetTrueAnomaly(eccentricAnomaly);
        double distance = GetDistance(eccentricAnomaly);
        Vector3d positionVector = GetPositionVector(trueAnomaly, distance);
        return GetInertialBodyCentricCoordinates(positionVector) + new Vector3d(centralBody.transform.position);
    }

    public Vector3d GetPositionAtMeanAnomaly(double meanAnomaly)
    {
        Debug.Assert(centralBody != null);
        Debug.Assert(meanAnomaly >= 0);
        double eccentricAnomaly = GetEccentricAnomaly(meanAnomaly);
        double trueAnomaly = GetTrueAnomaly(eccentricAnomaly);
        double distance = GetDistance(eccentricAnomaly);
        Vector3d positionVector = GetPositionVector(trueAnomaly, distance);
        return GetInertialBodyCentricCoordinates(positionVector) + new Vector3d(centralBody.transform.position);
    }

    public Vector3d GetInertialBodyCentricCoordinates(Vector3d positionVector)
    {
        // Precalculate cos & sin values
        double cos_w = Mathd.Cos(ArgumentOfPeriapsis);
        double sin_w = Mathd.Sin(ArgumentOfPeriapsis);
        double cos_omega = Mathd.Cos(LongitudeOfAscendingNode);
        double sin_omega = Mathd.Sin(LongitudeOfAscendingNode);
        double cos_i = Mathd.Cos(Inclination);
        double sin_i = Mathd.Sin(Inclination);

        double x =
            positionVector.x * (cos_w * cos_omega - sin_w * cos_i * sin_omega) -
            positionVector.y * (sin_w * cos_omega + cos_w * cos_i * sin_omega);
        double y =
            positionVector.x * (cos_w * sin_omega + sin_w * cos_i * cos_omega) +
            positionVector.y * (cos_w * cos_i * cos_omega - sin_w * sin_omega);
        double z =
            positionVector.x * (sin_w * sin_i) +
            positionVector.y * (cos_w * sin_i);
        return new(x, y, z);
    }

    public Vector3d GetPositionVector(double trueAnomaly, double distance)
    {
        return new Vector3d(Mathd.Cos(trueAnomaly), Mathd.Sin(trueAnomaly), 0) * distance;
    }

    public double GetDistance(double eccentricAnomaly)
    {
        return SemiMajorAxis * (1 - Eccentricity * Mathd.Cos(eccentricAnomaly));
    }

    public double GetTrueAnomaly(double eccentricAnomaly)
    {
        double half_e = eccentricAnomaly / 2;
        return 2 * Mathd.Atan2(
            Mathd.Sqrt(1 + Eccentricity) * Mathd.Sin(half_e),
            Mathd.Sqrt(1 - Eccentricity) * Mathd.Cos(half_e));
    }

    public double GetEccentricAnomaly(double meanAnomaly)
    {
        double e = meanAnomaly;
        const int iterations = 2;
        for (int i = 0; i < iterations; i++)
        {
            e = e - (e - Eccentricity * Mathd.Sin(e) - meanAnomaly) / (1 - Eccentricity * Mathd.Cos(e));
        }
        return e;
    }

    public double GetMeanAnomaly(double time)
    {
        double meanAnomaly = TrueAnomalyEpoch + time * Mathd.Sqrt(centralBody.Mu / SemiMajorAxisCubed);
        return meanAnomaly % (2 * Mathd.PI);
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        Debug.Assert(universe != null);
        transform.position = (Vector3)GetPositionAtTime(universe.UniversalTime);
    }

    private void OnValidate()
    {
        ValidateOrbitElements();
        SetPositionAtEpoch();
        DrawOrbit();
        UpdateUI();
    }

    private void UpdateUI()
    {
        periapsisText.transform.position = (Vector3)GetPositionAtMeanAnomaly(0);
        periapsisText.text = string.Format("Periapsis\n{0:0.00}m", Periapsis);
        apoapsisText.transform.position = (Vector3)GetPositionAtMeanAnomaly(Mathd.PI);
        apoapsisText.text = string.Format("Apoapsis\n{0:0.00}m", Apoapsis);
    }

    // TODO: The way positions for the line renderer is currently generated
    //  creates issues with highly eccentric orbits since more of the positions
    //  are towards the apoapsis. Should use a better solution to create equidistant points
    //  around the ellipse
    private void DrawOrbit()
    {
        if (!centralBody) return;
        if (!lineRenderer && !(lineRenderer = GetComponent<LineRenderer>())) return;
        if (!drawOrbit)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3[] positions = new Vector3[lineSegments];
        double time = Universe.Epoch;
        double segmentOffset = Period / lineSegments;
        for (int i = 0; i < lineSegments; i++)
        {
            positions[i] = (Vector3)GetPositionAtTime(time);
            time += segmentOffset;
        }
        lineRenderer.loop = true;
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private void SetPositionAtEpoch()
    {
        if (!centralBody) return;
        transform.position = (Vector3)GetPositionAtTime(Universe.Epoch);
    }

    private void ValidateOrbitElements()
    {
        if (semiMajorAxis == 0)
        {
            Debug.LogWarning("Semi-major axis cannot be 0");
            semiMajorAxis += float.Epsilon;
        }
        if (eccentricity < 0 || eccentricity >= 1)
        {
            Debug.LogWarning("Eccentricity must be in range [0-1)");
            eccentricity = Mathd.Clamp(eccentricity, 0, 1 - float.Epsilon);
        }
        if (inclination < 0 || inclination > 180)
        {
            Debug.LogWarning("Inclination must be in range [0, 180]");
            inclination = Mathd.Clamp(inclination, 0, 180);
        }
        if (longitudeOfAscendingNode < 0 || longitudeOfAscendingNode > 360)
        {
            Debug.LogWarning("Longitude of ascending node must be in range [0, 360]");
            longitudeOfAscendingNode = Mathd.Clamp(longitudeOfAscendingNode, 0, 360);
        }
        if (argumentOfPeriapsis < 0 || argumentOfPeriapsis > 360)
        {
            Debug.LogWarning("Argument of periapsis must be in range [0, 360]");
            argumentOfPeriapsis = Mathd.Clamp(argumentOfPeriapsis, 0, 360);
        }
    }
}
