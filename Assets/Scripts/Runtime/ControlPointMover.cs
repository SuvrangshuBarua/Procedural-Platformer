using PathTool;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ControlPointMover : MonoBehaviour
{
    private List<Transform> controlPoints = new List<Transform>(); // Assign control points in the Inspector
    public LayerMask controlLayer; // Layer for detecting the XZ plane
    public LayerMask platformLayer;
    private Transform selectedPoint; // Currently selected control point
    private Transform selectedPlatform;
    private Camera mainCamera;
    private bool enable = true;
    public bool invert = false;
    public bool Enable
    {
        get { return enable; }
        set { enable = value; }
    }

    void Start()
    {
        mainCamera = Camera.main;
        controlPoints = BeizerPathManager.Instance.GetControlPoints();
    }

    void Update()
    {
        if (!enable) return;
        HandleMouseInput();
        
    }

    private void HandleMouseInput()
    {
        // Detect left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            SelectControlPoint();
            DetectPlatform();

        }

        // Detect left mouse button held down
        if (Input.GetMouseButton(0) && selectedPoint != null)
        {
            MoveControlPoint();
        }

        // Detect left mouse button released
        if (Input.GetMouseButtonUp(0))
        {
            selectedPoint = null;
        }
    }

    private void SelectControlPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, controlLayer))
        {
            selectedPoint = hit.transform;
        }
       
    }

    private void MoveControlPoint()
    {

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, controlLayer))
        {
            Vector3 newPosition = hit.point;
            if(invert)
            {
                newPosition.z = selectedPoint.position.z; // Keep the height (Y-axis) fixed
                newPosition.x = selectedPoint.position.x;
                selectedPoint.position = newPosition;
            }
            else
            {
                newPosition.y = selectedPoint.position.y; // Keep the height (Y-axis) fixed
                selectedPoint.position = newPosition;
            }
           
        }
       
    }
    private void DetectPlatform()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, platformLayer))
        {
            selectedPlatform = hit2.transform;
            Debug.Log("Platform selected" + selectedPlatform.name);
            UIManager.Instance.AssignPlatform(selectedPlatform.GetComponent<PlatformGroup>());
        }
    }

    private void OnDrawGizmos()
    {
        // Draw control point positions in the Scene view for clarity
        Gizmos.color = Color.yellow;
        foreach (Transform point in controlPoints)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}
