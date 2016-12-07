using UnityEngine;
using System.Collections;

public class RookieCamera : MonoBehaviour
{
    public float AnimationSpeed = 1.5f;
    public float ForwardsSpeed = 7.0f;
    public float BackwardsSpeed = 7.0f;
    public float TurningSpeed = 2.0f;

    public GameObject FirstPersonCameraPosition;
    public GameObject ThirdPersonCameraPosition;

    private Vector3 _velocity;

    public enum CameraPosition
    {
        FirstPerson,
        ThirdPerson,
        Unknown,
    }

    public CameraPosition CurrentCameraPosition
    {
        get
        {
            var mainCameraParent = Camera.main.transform.parent.gameObject;
            if (mainCameraParent.Equals(FirstPersonCameraPosition))
            {
                return CameraPosition.FirstPerson;
            }
            else if (mainCameraParent.Equals(ThirdPersonCameraPosition))
            {
                return CameraPosition.ThirdPerson;
            }
            else
            {
                Debug.LogAssertionFormat("Unknown main camera position on {0}. Camera is child of {1}", name, mainCameraParent.name);
                return CameraPosition.Unknown;

            }
        }

        set
        {
            if (value.Equals(CameraPosition.ThirdPerson))
            {
                Camera.main.transform.SetParent(ThirdPersonCameraPosition.transform, false);
            }
            else
            {
                Debug.AssertFormat(value.Equals(CameraPosition.FirstPerson),
                    "Unexpected camera position {0} supplied on {1}. Setting camera to first-person camera.",
                    value,
                    name);
                Camera.main.transform.SetParent(FirstPersonCameraPosition.transform, false);
            }
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetButtonUp("Fire3"))
        {
            CurrentCameraPosition = CurrentCameraPosition.Equals(CameraPosition.ThirdPerson) ? CameraPosition.FirstPerson : CameraPosition.ThirdPerson;
        }

        float forwardInput = Input.GetAxis("Vertical"); // Vertical axis is back and forth tied to up and down.
        float yawInput = Input.GetAxis("Mouse X") + Input.GetAxis("Horizontal"); // Horizontal axis is side to side (turning)
        float pitchInput = Input.GetAxis("Mouse Y"); // up and down

        var velocityVector = forwardInput * Vector3.forward;
        velocityVector = transform.TransformDirection(velocityVector); // Get world-space velocity
        if (forwardInput > 0.1)
        {
            velocityVector *= ForwardsSpeed;
        }
        else if (forwardInput < -0.1)
        {
            velocityVector *= BackwardsSpeed;

        }
        else
        {
            velocityVector = Vector3.zero;
        }

        transform.localPosition += velocityVector * Time.deltaTime;
        transform.Rotate(0, yawInput * TurningSpeed, 0);

        if (CurrentCameraPosition.Equals(CameraPosition.FirstPerson))
        {
            Camera.main.transform.Rotate(-pitchInput * TurningSpeed, 0, 0);
        }
        else
        {
            Camera.main.transform.localRotation = Quaternion.identity;
        }

        // TODO: Move to Idle Animation when input has not been received for a while/And then move out when it finishes. This should likely be a trigger.
    }
}