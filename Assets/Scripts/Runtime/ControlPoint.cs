using UnityEngine;
using UnityEngine.EventSystems;

public class ControlPoint : MonoBehaviour
{
    private bool isFirstControlPoint = false;    

    public void SetFirstControlPoint(bool value)
    {
        isFirstControlPoint = value;
    }
}