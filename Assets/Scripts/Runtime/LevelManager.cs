using PathTool;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private PlatformGroup initialPlatform;
    public CinemachineBrain brain;
    public CinemachineCamera baseCamera;
    public CinemachineCamera followCamera;
    public CinemachineSplineDolly splineCamera;
    public Camera overlayCamera;
    [Header("Player Properties")]
    public GameObject playerPrefab;
    public ControlPointMover controlPointMover;
    GameObject player;

    public delegate void CameraBlendStarted();
    public static event CameraBlendStarted onCameraBlendStarted;

    public delegate void CameraBlendFinished();
    public static event CameraBlendFinished onCameraBlendFinished;

    private CinemachineBrain cineMachineBrain;

    private bool wasBlendingLastFrame;
    public bool isDragging = false;
    private float previousMouseX;
    public float dragSpeed = 0.5f;

    private void OnEnable()
    {
        onCameraBlendStarted += OnCameraInstantActivated;
        onCameraBlendFinished += OnCameraDelayedActivated;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Update()
    {
        if (brain.IsBlending)
        {
            if (!wasBlendingLastFrame)
            {
                onCameraBlendStarted?.Invoke();
            }
            wasBlendingLastFrame = true;
        }
        else
        {
            if (wasBlendingLastFrame)
            {
                onCameraBlendFinished?.Invoke();
            }
            wasBlendingLastFrame = false;
        }

        if(brain.ActiveVirtualCamera.Equals(splineCamera.VirtualCamera))
        {
            // Check if the left mouse button is pressed
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                previousMouseX = Input.mousePosition.x;
            }

            // Check if the left mouse button is released
            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
            }

            // Handle drag movement while holding the mouse button
            if (isDragging)
            {
                float currentMouseX = Input.mousePosition.x;
                float deltaX = currentMouseX - previousMouseX;


                splineCamera.SplineSettings.Position += -deltaX * dragSpeed * Time.deltaTime;

                // Clamp the position to stay within the path bounds
                splineCamera.SplineSettings.Position = Mathf.Clamp(splineCamera.SplineSettings.Position, 0f, splineCamera.Spline.CalculateLength());

                previousMouseX = currentMouseX;
            }
        }
    }
    private void Start()
    {
        wasBlendingLastFrame = false;
        UIManager.Instance.joyStick.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        onCameraBlendStarted -= OnCameraInstantActivated;
        onCameraBlendFinished -= OnCameraDelayedActivated;

    }
    private void OnCameraInstantActivated()
    {

        switch(brain.ActiveVirtualCamera)
        {
            case CinemachineCamera virtualCamera:
                if (virtualCamera == followCamera)
                {
                    Debug.Log("Switched to Follow Camera");
                    OnSwitchToFollowCamera();
                    //Spawn Player and Enable Joystick
                    //Disable Path Buttons and control points
                    UIManager.Instance.DisablePlatformScaling();
                }
                else if(virtualCamera == baseCamera)
                {
                    UIManager.Instance.joyStick.gameObject.SetActive(false);
                    UIManager.Instance.EnablePlatformScaling();
                }
                else if (virtualCamera == splineCamera.VirtualCamera)
                {
                    UIManager.Instance.joyStick.gameObject.SetActive(false);
                    UIManager.Instance.DisablePlatformScaling();
                }

                break;
        }
    }
    private void OnCameraDelayedActivated()
    {
        if (brain.ActiveVirtualCamera.Equals(baseCamera))
        {
            Debug.Log("Switched to Base Camera");
            OnSwitchToBaseCamera();
           

        }
    }

    private void OnSwitchToFollowCamera()
    {
        UIManager.Instance.joyStick.gameObject.SetActive(true);
        if (initialPlatform == null)
        {
            initialPlatform = BeizerPathManager.Instance.spawnedPlatformsList[0].GetComponent<PlatformGroup>();
        }
        PlayerMovement playerMovement = SpawnPlayer().GetComponent<PlayerMovement>();
        followCamera.Target.TrackingTarget = playerMovement.transform;
        playerMovement.Initialize();
        UIManager.Instance.DisableAllInteractableButton();
        BeizerPathManager.Instance.DisablePathVisual();
        overlayCamera.enabled = false;

    }
    private void OnSwitchToBaseCamera()
    {
        DespawnPlayer();
        UIManager.Instance.EnableAllInteractableButton();
        BeizerPathManager.Instance.EnablePathVisual();
        overlayCamera.enabled = true;
    }
    private GameObject SpawnPlayer()
    {
        player = ObjectPool.instance.GetObject(playerPrefab);
        player.SetActive(true); 
        player.transform.position = initialPlatform.playerSpawnPoint.position;
        return player;
    }
    private void DespawnPlayer()
    {
        if(player != null) ObjectPool.instance.ReturnToPool(player);

    }
    public void SwitchPriority()
    {

        if (brain.ActiveVirtualCamera.Equals(baseCamera))
        {
            splineCamera.VirtualCamera.Priority = 10;
            followCamera.Priority = 0;
            baseCamera.Priority = 0;
            overlayCamera.cullingMask = 1;
            Camera.main.cullingMask = 1 | 1 << 6 | 1<<7;
            controlPointMover.invert = true;
        }
        else if(brain.ActiveVirtualCamera.Equals(splineCamera.VirtualCamera))
        {
            splineCamera.VirtualCamera.Priority = 0;
            followCamera.Priority = 0;
            baseCamera.Priority = 10;
            overlayCamera.cullingMask = 1<<6;
            Camera.main.cullingMask = 1 | 1 << 7;
            controlPointMover.invert = false;
        }

    }
    public void ToggleCamera()
    {
        if(BeizerPathManager.Instance.spawnedPlatformsList.Count == 0)
        {
            return;
        }
        if (brain.ActiveVirtualCamera.Equals(baseCamera))
        {
            splineCamera.VirtualCamera.Priority = 0;
            baseCamera.Priority = 0;
            followCamera.Priority = 10;
        }
        else if (brain.ActiveVirtualCamera.Equals(followCamera))
        {
            splineCamera.VirtualCamera.Priority = 0;
            followCamera.Priority = 0;
            baseCamera.Priority = 10;
        }
    }

}
