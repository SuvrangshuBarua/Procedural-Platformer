using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PathTool
{
    [ExecuteInEditMode]
    public class PathGenerator : MonoBehaviour
    {
        [Header("Spline Properties")]
        public List<Transform> points = new List<Transform>();

        [Header("Platform Properties")]
        public List<PlatformGroup> objectGroups = new List<PlatformGroup>();

        private SplineContainer splineContainer;
        private List<GameObject> spawnedPlatforms = new List<GameObject>();

        #region Spline Properties
        public void GenerateSpline()
        {
            if (points.Count == 0)
            {
                Debug.LogError("Points should be assigned");
                return;
            }
            if (splineContainer == null)
            {
                splineContainer = gameObject.GetOrAddComponent<SplineContainer>();
            }
            InitializeSpline();
        }
        private void InitializeSpline()
        {
            
            splineContainer.Spline.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                splineContainer.Spline.Add(points[i].position);
            }

            splineContainer.Spline.SetTangentMode(TangentMode.Linear);
        }
        #endregion

        #region Platform Properties
        public void GeneratePlatform()
        {
            if (objectGroups.Count == 0)
            {
                Debug.LogError("Object Groups should be assigned");
                return;
            }
            foreach (GameObject group in spawnedPlatforms)
            {
                if(group != null)
                {
                    DestroyImmediate(group);
                }
            }
            spawnedPlatforms.Clear();   

            float totalSplineLength = splineContainer.Spline.GetLength();
            float totalPlatformLength = 0;

            while(totalPlatformLength < totalSplineLength)
            {
                PlatformGroup selectedGroup = objectGroups[Random.Range(0, objectGroups.Count)];
                float groupLength = selectedGroup.GetTotalLength();

                if (totalPlatformLength + groupLength > totalSplineLength)
                {
                    break;
                }

                /*float positionOnSpline = totalPlatformLength / totalSplineLength;
                Vector3 position = splineContainer.Spline.EvaluatePosition(positionOnSpline);

                GameObject groupInstance = Instantiate(selectedGroup.gameObject, position, Quaternion.identity, transform);
                spawnedPlatforms.Add(groupInstance);

                totalPlatformLength += groupLength;*/

                SpawnGroupOnSpline(selectedGroup, totalPlatformLength, groupLength);
                totalPlatformLength += groupLength;
            }
        }
        private void SpawnGroupOnSpline(PlatformGroup platformGroup, float startDistance, float groupLength)
        {
            float splineLength = splineContainer.Spline.GetLength();

            float tStart = Mathf.Clamp01(startDistance / splineLength);
            float tEnd = Mathf.Clamp01((startDistance + groupLength) / splineLength);


            Vector3 startPosition = splineContainer.Spline.EvaluatePosition(tStart);
            Vector3 endPosition = splineContainer.Spline.EvaluatePosition(tEnd);

            Vector3 direction = (endPosition - startPosition).normalized;

            GameObject groupInstance = Instantiate(platformGroup.gameObject, transform);
            groupInstance.transform.position = startPosition;

            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                groupInstance.transform.rotation = rotation;
            }


            spawnedPlatforms.Add(groupInstance);
        }
        #endregion
    }
}
