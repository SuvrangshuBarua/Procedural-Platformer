using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<string, Queue<GameObject>> objectsPool = new Dictionary<string, Queue<GameObject>>();
    public static ObjectPool instance { get; private set; }

    void Awake()
    {
        instance = this;
    }


    public GameObject GetObject(GameObject obj)
    {
        if (objectsPool.TryGetValue(obj.name, out Queue<GameObject> objectList))
        {
            if (objectList.Count == 0)
                return CreateNewObject(obj);

            else
            {
                GameObject requestedObject = objectList.Dequeue();
                return requestedObject;
            }
        }

        else
            return CreateNewObject(obj);



    }

    GameObject CreateNewObject(GameObject obj)
    {
        GameObject newObject = Instantiate(obj);
        newObject.SetActive(false);
        newObject.name = obj.name;
        return newObject;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);

        if (objectsPool.TryGetValue(obj.name, out Queue<GameObject> objectList))
        {
            objectList.Enqueue(obj);
        }
        else
        {
            Queue<GameObject> newObjectQueue = new Queue<GameObject>();
            newObjectQueue.Enqueue(obj);
            objectsPool.Add(obj.name, newObjectQueue);
        }
    }

}
