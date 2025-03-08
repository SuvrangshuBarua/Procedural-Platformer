using UnityEngine;
using UnityEngine.EventSystems;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private FloatingJoystick joyStick;
    [SerializeField] private Rigidbody rb;

    [Space]
    [SerializeField] private float movementSpeed = 5;

    [SerializeField] private float multiplier = 50;
    [SerializeField] private float rotationSpeed = 360;


    private Transform selfTransform;
    private Vector3 moveVector;
    private Matrix4x4 matrix;
    private Vector3 skewedInput;
    private float inputOffsetAngle = -45f;

    private UIManager uiManager;
    private bool isRunning = false;
    private Transform buildingToLookAt;
    public Animator animator;

    private void Start()
    {
        selfTransform = transform;

        uiManager = UIManager.Instance;

        rb.linearVelocity = Vector3.zero;

        matrix = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
    }
    public void Initialize()
    {
        joyStick = UIManager.Instance.joyStick;
    }
    private void Update()
    {
        // #if UNITY_EDITOR
        //         if (Input.GetKeyDown(KeyCode.H))
        //         {
        //             Cursor.visible = false;
        //         }
        //         if (Input.GetKeyDown(KeyCode.U))
        //         {
        //             Cursor.visible = true;
        //         }
        // #endif
        //         //if (uiManager.IsOverlayUIEnabled())
        //         //    return;

        

        Move();
    }

    private void Move()
    {
        moveVector = Vector3.zero;
        moveVector.x = joyStick.Horizontal;
        moveVector.z = joyStick.Vertical;

        if (Mathf.Abs(joyStick.Horizontal) > 0.01f || Mathf.Abs(joyStick.Vertical) > 0.01f)
        {

            skewedInput = matrix.MultiplyPoint3x4(moveVector);
            Vector3 relativeDirection = (selfTransform.position + skewedInput) - selfTransform.position;
            Quaternion rot = Quaternion.LookRotation(relativeDirection, transform.up);


            selfTransform.rotation = Quaternion.RotateTowards(selfTransform.rotation, rot, rotationSpeed * Time.deltaTime);


            if (!isRunning)
            {

                isRunning = true;
            }
        }
        else if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
        {
            if (isRunning)
            {
                rb.linearVelocity = Vector3.zero;
                skewedInput = Vector3.zero;

                isRunning = false;
            }
        }
    }

    private void FixedUpdate()
    {
       
        //rb.MovePosition(rb.position + skewedInput * Time.fixedDeltaTime * movementSpeed);

        rb.linearVelocity = skewedInput * Time.fixedDeltaTime * movementSpeed * multiplier;
        animator.SetFloat("speed", rb.linearVelocity.sqrMagnitude);

        //Debug.Log($"veloctiy : {rb.velocity.sqrMagnitude}");
    }

    public void SetPlayerMovementSpeed(float speed)
    {
        movementSpeed = speed;
    }

}
