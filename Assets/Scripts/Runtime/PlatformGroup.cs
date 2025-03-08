using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace PathTool
{
    public enum PlatformType
    {
        Straight,
        Hinge,
        UpwardSlope,
        DownwardSlope
    }
    [ExecuteInEditMode]
    public class PlatformGroup : MonoBehaviour
    {
        public PlatformType platformType;
        public List<Transform> objects = new List<Transform>();
        public Transform playerSpawnPoint;
        private float localScaleX;
        private float localScaleZ;


        private void Start()
        {
            localScaleX = objects[0].localScale.x;
            localScaleZ = objects[0].localScale.z;
        }
        public float GetTotalLength()
        {
            float totalLength = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                float bound = objects[i].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * objects[i].transform.localScale.z;
                totalLength += bound;
            }
            return totalLength;
        }
        public void ChangeWidth(float width)
        {
            for (int i = 0; i < objects.Count; i++)
            {

                objects[i].localScale = new Vector3(width * localScaleX, objects[i].localScale.y, objects[i].localScale.z);
            }
        }
        public void ChangeLength(float length)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].localScale = new Vector3(objects[i].localScale.x, objects[i].localScale.y, length * localScaleZ);
            }
        }
        public void UpdateVisual(Material material)
        {
            foreach (Transform obj in objects)
            {
                obj.GetComponent<MeshRenderer>().material = material;
            }
        }
    }
}

