using PathTool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public enum CurveMode
{
    Linear = 0,
    Bezier
}
[ExecuteAlways]
[RequireComponent(typeof(SplineContainer))]
public class BeizerPathManager : MonoBehaviour
{
    public static BeizerPathManager Instance { get; private set; }

    [Header("References")]
    public CurveMode curveMode = CurveMode.Bezier; // Curve mode
    private SplineContainer splineContainer;             // Line Renderer to display the curve
    public GameObject controlPointPrefab;
    public List<Transform> controlPoints = new List<Transform>(); // List of control points

    [Header("Curve Settings")]
    [Range(10, 100)] public int curveResolution = 50; // Number of curve segments

    [Header("Platform Properties")]
    [Range(0, 20)]
    public float plarformSpacing = 5f;
    private float lastPlatformSpacing;
    public List<PlatformGroup> objectGroups = new List<PlatformGroup>();
    private List<(GameObject platform, float startDistance, float groupLength, Vector3 lastTangent)> spawnedPlatforms = new List<(GameObject, float, float, Vector3)>();
    public List<GameObject> spawnedPlatformsList
    {
        get { return spawnedPlatforms.Select(x => x.platform).ToList(); }
    }
    private float lastPlatformEnd = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B)) curveMode = CurveMode.Bezier;
        if(Input.GetKeyDown(KeyCode.L)) curveMode = CurveMode.Linear;

        UpdateCurve();
        ManagePlatforms();
    }
    private void Awake()
    {
        Instance = this;
        if (splineContainer == null)
        {
            splineContainer = gameObject.GetOrAddComponent<SplineContainer>();
        }
    }
    public List<Transform> GetControlPoints()
    {
        return controlPoints;
    }
    public void UpdateCurve()
    {
        if (controlPoints.Count < 2) return;
        splineContainer.Spline.Clear();

        for (int i = 0; i < controlPoints.Count; i++)
        {
            splineContainer.Spline.Add(controlPoints[i].position);
        }

        splineContainer.Spline.SetTangentMode(curveMode == CurveMode.Bezier ? TangentMode.AutoSmooth : TangentMode.Linear);

       ReadjustPlatforms();
    }
   
  
    public void AddControlPoint(Vector3 position)
    {
        //GameObject newPoint = new GameObject("Control Point");
        //GameObject newPoint = Instantiate(controlPointPrefab, position, Quaternion.identity);
        GameObject newPoint = ObjectPool.instance.GetObject(controlPointPrefab);
        newPoint.gameObject.SetActive(true);
        newPoint.transform.position = position;
        newPoint.name = "Control Point";
        newPoint.transform.position = position;
        newPoint.transform.parent = transform;
        newPoint.AddComponent<ControlPoint>();
        controlPoints.Add(newPoint.transform);
        SpawnHingePlatformAtControlPoint(position);
        UpdateCurve();
    }

    public void RemoveAllControlPoints()
    {
        
        foreach (Transform cp in controlPoints)
        {
            ObjectPool.instance.ReturnToPool(cp.gameObject);
        }
        controlPoints.Clear();
        splineContainer.Spline.Clear();
    }
    public void RemoveLastControlPoint()
    {
        if (controlPoints.Count > 0)
        {
            Transform lastPoint = controlPoints[controlPoints.Count - 1];
            controlPoints.Remove(lastPoint);
            ObjectPool.instance.ReturnToPool(lastPoint.gameObject);
            //DestroyImmediate(lastPoint.gameObject);
            UpdateCurve();
        }
    }
    #region Platform Generation
    public void DisablePathVisual()
    {
        splineContainer.GetComponent<SplineExtrude>().Radius = 0;
    }
    public void EnablePathVisual()
    {
        splineContainer.GetComponent<SplineExtrude>().Radius = 0.15f;
    }
    /* public void GeneratePlatforms()
     {
         if (objectGroups.Count == 0)
         {
             Debug.LogError("Object Groups should be assigned");
             return;
         }

         // Clear existing platforms
         foreach (GameObject group in spawnedPlatforms)
         {
             if (group != null)
             {
                 DestroyImmediate(group);
             }
         }
         spawnedPlatforms.Clear();

         float totalSplineLength = splineContainer.Spline.GetLength();
         float totalPlatformLength = 0;

         // Spawn platform groups along the spline
         while (totalPlatformLength < totalSplineLength)
         {
             PlatformGroup selectedGroup = objectGroups[Random.Range(0, objectGroups.Count)];
             float groupLength = selectedGroup.GetTotalLength();

             if (totalPlatformLength + groupLength > totalSplineLength)
             {
                 break;
             }

             SpawnGroupOnSpline(selectedGroup, totalPlatformLength, groupLength);
             totalPlatformLength += groupLength + plarformSpacing;
         }
     }*/
    private void ManagePlatforms()
    {
        float totalSplineLength = splineContainer.Spline.GetLength();

        bool comparsion = Mathf.Approximately(lastPlatformSpacing, plarformSpacing) ? false : true;
        if(comparsion)
        {
            ReadjustPlatforms();
        }

        // Spawn new platforms if there's enough spline length
        while (lastPlatformEnd + plarformSpacing < totalSplineLength)
        {
            PlatformGroup selectedGroup = objectGroups.Find(x => x.platformType == PlatformType.Straight); ;
            float groupLength = selectedGroup.GetTotalLength();

            if (lastPlatformEnd + groupLength + plarformSpacing > totalSplineLength)
            {
                break;
            }

            SpawnGroupOnSpline(selectedGroup, lastPlatformEnd, groupLength);
            lastPlatformEnd += groupLength + plarformSpacing;
        }

        // Remove platforms if the spline length has shortened
        for (int i = spawnedPlatforms.Count - 1; i >= 0; i--)
        {
            var (platform, startDistance, groupLength, direction) = spawnedPlatforms[i];

            if (startDistance + groupLength > totalSplineLength)
            {
                //DestroyImmediate(platform); // Destroy the platform
                ObjectPool.instance.ReturnToPool(platform);
                spawnedPlatforms.RemoveAt(i); // Remove from the list
            }
        }

        lastPlatformEnd = spawnedPlatforms.Count > 0
        ? spawnedPlatforms[spawnedPlatforms.Count - 1].startDistance + spawnedPlatforms[spawnedPlatforms.Count - 1].groupLength + plarformSpacing
        : 0;
    }
    private void ReadjustPlatforms()
    {
        float currentDistance = 0;
        float splineLength = splineContainer.Spline.GetLength();
        if(splineLength == 0) return;   

        for (int i = 0; i < spawnedPlatforms.Count; i++)
        {
            var (platform, _, groupLength, lastTangent) = spawnedPlatforms[i];

            float tStart = Mathf.Clamp01(currentDistance / splineLength);
            float tEnd = Mathf.Clamp01((currentDistance + groupLength) / splineLength);

            Vector3 startPosition = splineContainer.Spline.EvaluatePosition(tStart);
            Vector3 endPosition = splineContainer.Spline.EvaluatePosition(tEnd);

            Vector3 currentTangent = (endPosition - startPosition).normalized;

            // Only update position/rotation if tangent or position has changed significantly
            if (Vector3.Distance(startPosition, platform.transform.position) > 0.01f ||
                Vector3.Angle(currentTangent, lastTangent) > 1f)
            {
                platform.transform.position = startPosition;

                if (currentTangent != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(currentTangent);
                    platform.transform.rotation = rotation;
                }

                // Update the last tangent
                spawnedPlatforms[i] = (platform, currentDistance, groupLength, currentTangent);
            }
            currentDistance += groupLength + plarformSpacing;
        }
        lastPlatformEnd = currentDistance;
    }
    private void SpawnHingePlatformAtControlPoint(Vector3 position)
    {
        PlatformGroup platformGroup = objectGroups.Find(x => x.platformType == PlatformType.Hinge);
        if (platformGroup == null)
        {
            Debug.LogError("Hinge platform group not found");
            return;
        }
        GameObject hinge = ObjectPool.instance.GetObject(platformGroup.gameObject);
        hinge.SetActive(true);
        hinge.transform.parent = transform;
        hinge.transform.position = position;
        hinge.transform.rotation = Quaternion.identity;
        float groupLength = platformGroup.GetTotalLength();
        lastPlatformEnd += groupLength;

        spawnedPlatforms.Add((hinge, 0, groupLength, Vector3.zero));
    }
    private void SpawnGroupOnSpline(PlatformGroup platformGroup, float startDistance, float groupLength)
    {
        float splineLength = splineContainer.Spline.GetLength();

        float tStart = Mathf.Clamp01(startDistance / splineLength);
        float tEnd = Mathf.Clamp01((startDistance + groupLength) / splineLength);

        Vector3 startPosition = splineContainer.Spline.EvaluatePosition(tStart);
        Vector3 endPosition = splineContainer.Spline.EvaluatePosition(tEnd);

        Vector3 direction = (endPosition - startPosition).normalized;

        //GameObject groupInstance = Instantiate(platformGroup.gameObject, transform);
        GameObject groupInstance = ObjectPool.instance.GetObject(platformGroup.gameObject);
        groupInstance.SetActive(true);
        groupInstance.transform.parent = transform;

        groupInstance.transform.position = startPosition;

        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            groupInstance.transform.rotation = rotation;
        }

        spawnedPlatforms.Add((groupInstance, startDistance, groupLength, direction));
    }
    /*public void RemoveAllSpawnedPlatforms()
    {
        foreach (GameObject group in spawnedPlatforms)
        {
            if (group != null)
            {
                DestroyImmediate(group);
            }
        }
        spawnedPlatforms.Clear();
    }*/
    public void ClearAllPlatforms()
    {
        foreach (var (platform, _, _, _) in spawnedPlatforms)
        {
            if (platform != null)
            {
                ObjectPool.instance.ReturnToPool(platform); // Destroy platform GameObject
                
            }
        }
        spawnedPlatforms.Clear(); // Clear the list
        lastPlatformEnd = 0; // Reset the tracking variable
    }
    #endregion
}
