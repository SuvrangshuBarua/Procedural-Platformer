using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using PathTool;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public FloatingJoystick joyStick;
    public Button AddControlPointButton;
    public Button RemoveControlPointButton;
    public Button ResetPathButton;
    public Button ChangeCamera;
    public Button RemovePlatformsButton;
    public TMP_Dropdown platformDropdown;
    public Slider spacingSlider;
    public Slider platformWidth;
    public Slider platformLength;
    public Button color1Button;
    public Button color2Button;


    private PlatformGroup currentPlatformGroup;

    private void Awake()
    {
        Instance = this;
        spacingSlider.minValue = 0f;
        spacingSlider.maxValue = 20f;
        platformWidth.minValue = 0f;
        platformWidth.maxValue = 2f;

        platformLength.minValue = 0f;

        platformLength.maxValue = 2f;
        

    }
    private void Start()
    {
        spacingSlider.value = BeizerPathManager.Instance.plarformSpacing;
        platformDropdown.value = (int)BeizerPathManager.Instance.curveMode;
    }
    private void OnEnable()
    {
        AddControlPointButton.onClick.AddListener(AddControlPointBehavior);
        RemoveControlPointButton.onClick.AddListener(RemoveControlPointBehavior);
        ResetPathButton.onClick.AddListener(()=> BeizerPathManager.Instance.RemoveAllControlPoints());
        //ChangeCamera.onClick.AddListener(()=> LevelManager.Instance.ToggleCamera());
        RemovePlatformsButton.onClick.AddListener(()=> BeizerPathManager.Instance.ClearAllPlatforms());
        spacingSlider.onValueChanged.AddListener(UpdateSpacing);
        platformDropdown.onValueChanged.AddListener(OnDropDownValueChanged);
        platformWidth.onValueChanged.AddListener((value) => currentPlatformGroup?.ChangeWidth(value));
        platformLength.onValueChanged.AddListener((value) => currentPlatformGroup?.ChangeLength(value));
        color1Button.onClick.AddListener(() => currentPlatformGroup?.UpdateVisual(color1Button.GetComponent<ColorMaterialHolder>().colorMaterial));
        color2Button.onClick.AddListener(() => currentPlatformGroup?.UpdateVisual(color2Button.GetComponent<ColorMaterialHolder>().colorMaterial));

    }
    private void OnDisable()
    {
        AddControlPointButton.onClick.RemoveAllListeners();
        RemoveControlPointButton.onClick.RemoveAllListeners();
        ResetPathButton.onClick.RemoveAllListeners();
        //ChangeCamera.onClick.RemoveListener(() => LevelManager.Instance.ToggleCamera());
        RemovePlatformsButton.onClick.RemoveAllListeners();
        spacingSlider.onValueChanged.RemoveListener(UpdateSpacing);
        platformDropdown.onValueChanged.RemoveListener(OnDropDownValueChanged);
        platformWidth.onValueChanged.RemoveListener((value) => currentPlatformGroup?.ChangeWidth(value));
        platformLength.onValueChanged.RemoveListener((value) => currentPlatformGroup?.ChangeLength(value));
        color1Button.onClick.RemoveListener(() => currentPlatformGroup?.UpdateVisual(color1Button.GetComponent<ColorMaterialHolder>().colorMaterial));
        color2Button.onClick.RemoveListener(() => currentPlatformGroup?.UpdateVisual(color2Button.GetComponent<ColorMaterialHolder>().colorMaterial));
    }
    private void AddControlPointBehavior()
    {
        Vector3 newPointPosition = BeizerPathManager.Instance.controlPoints.Count > 0
                ? BeizerPathManager.Instance.controlPoints[BeizerPathManager.Instance.controlPoints.Count - 1].position + Vector3.forward * 5f
                : BeizerPathManager.Instance.transform.position;
        BeizerPathManager.Instance.AddControlPoint(newPointPosition);
    }
    private void RemoveControlPointBehavior() {
        BeizerPathManager.Instance.RemoveLastControlPoint();
    }
    private void UpdateSpacing(float value)
    {
        BeizerPathManager.Instance.plarformSpacing = value;
    }
    public void OnDropDownValueChanged(int value)
    {
        BeizerPathManager.Instance.curveMode = (CurveMode)value;
    }

    public void DisableAllInteractableButton()
    {
        AddControlPointButton.interactable = false;
        RemoveControlPointButton.interactable = false;
        ResetPathButton.interactable = false;
        RemovePlatformsButton.interactable = false;
        spacingSlider.interactable = false;
        platformDropdown.interactable = false;
    }
    public void EnableAllInteractableButton()
    {
        AddControlPointButton.interactable = true;
        RemoveControlPointButton.interactable = true;
        ResetPathButton.interactable = true;
        RemovePlatformsButton.interactable = true;
        spacingSlider.interactable = true;
        platformDropdown.interactable = true;
    }
    public void EnablePlatformScaling()
    {
        platformWidth.interactable = true;
        platformLength.interactable = true;
    }
    public void DisablePlatformScaling()
    {
        platformWidth.interactable = false;
        platformLength.interactable = false;
    }
    public void AssignPlatform(PlatformGroup plaform)
    {
        
       currentPlatformGroup = plaform;
    }
}
