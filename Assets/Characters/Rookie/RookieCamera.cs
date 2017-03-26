using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using VRTK;

[RequireComponent(typeof(Animator))]
public class RookieCamera : MonoBehaviour
{
    public float TurningSpeed = 2.0f;

    [Range(0.0f, 90.0f)]
    public float PitchLimit = 70.0f;

    public Camera NonVrCamera;
    public GameObject FirstPersonCameraPosition;
    public GameObject ThirdPersonCameraPosition;
    public GameObject VirtualRealityCameraPosition;

    public Transform Pivot;
    public Camera PlayerCamera;
    public Transform Head;
    public Transform MeshTransform;
    public Vector3 HeadOffset;
    public Transform LookAtTarget;
    public GameObject LeftHandTarget;
    public GameObject RightHandTarget;
    public Transform LeftFootTarget;
    public Transform RightFootTarget;

    public bool ForceDisableVr = false;

    private const int ThirdPersonOnlyLayer = 1 << 8;
    private const int FirstPersonLayer = ~ThirdPersonOnlyLayer;

    private Animator _animator;
    Vector3 _leftFootPos;
    Vector3 _rightFootPos;
    Vector3 _leftFootRot;
    Vector3 _rightFootRot;

    public enum CameraPosition
    {
        FirstPerson,
        ThirdPerson,
        VirtualReality,
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
            else if (VirtualRealityCameraPosition.activeInHierarchy)
            {
                return CameraPosition.VirtualReality;
            }
            else
            {
                Debug.LogAssertionFormat("Unknown main camera position on {0}. Camera is child of {1}", name, mainCameraParent.name);
                return CameraPosition.Unknown;

            }
        }

        set
        {
            if (value.Equals(CameraPosition.VirtualReality))
            {
                VRSettings.enabled = true;
                VirtualRealityCameraPosition.SetActive(true);
                if (!VRDevice.isPresent)
                {
                    Debug.LogWarning("No VR Device Present. Not switching camera.");
                    VRSettings.enabled = false;
                    VirtualRealityCameraPosition.SetActive(false);
                    return;
                }

                NonVrCamera.gameObject.SetActive(false);
                return;
            }

            var mainCamera = NonVrCamera;
            if (value.Equals(CameraPosition.ThirdPerson))
            {
                mainCamera.cullingMask |= ThirdPersonOnlyLayer;
                mainCamera.transform.SetParent(ThirdPersonCameraPosition.transform, false);
            }
            else
            {
                Debug.AssertFormat(value.Equals(CameraPosition.FirstPerson),
                    "Unexpected camera position {0} supplied on {1}. Setting camera to first-person camera.",
                    value,
                    name);
                mainCamera.cullingMask &= FirstPersonLayer;
                mainCamera.transform.SetParent(FirstPersonCameraPosition.transform, false);
            }
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        VRSettings.enabled = !ForceDisableVr;
        if (VRSettings.enabled && VRDevice.isPresent)
        {
            NonVrCamera.gameObject.SetActive(false);
            VirtualRealityCameraPosition.SetActive(true);
        }
        else
        {
            VirtualRealityCameraPosition.SetActive(false);
            NonVrCamera.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.WaitingForPlayer)
        {
            return;
        }

        if (CurrentCameraPosition.Equals(CameraPosition.VirtualReality))
        {
            if (Input.GetButtonUp("ToggleVR"))
            {
                VRSettings.enabled = false;
                VirtualRealityCameraPosition.SetActive(false);
                NonVrCamera.gameObject.SetActive(true);
                CurrentCameraPosition = CameraPosition.ThirdPerson;
            }

            var headTarget = PlayerCamera.transform;
            var playerPosition = headTarget.position;
            var modelPosition = Pivot.position;
            modelPosition.x = playerPosition.x;
            modelPosition.z = playerPosition.z;
            Pivot.position = modelPosition;

            var playerRotation = headTarget.rotation.eulerAngles;
            var modelRotation = Pivot.rotation.eulerAngles;
            Pivot.rotation = Quaternion.Euler(modelRotation.x, playerRotation.y, modelRotation.z);

            var playerCameraPosition = VirtualRealityCameraPosition.transform.position;
            var playerHeadPosition = InputTracking.GetLocalPosition(VRNode.CenterEye);
            var modelHeadPosition = Head.position;
            playerCameraPosition.y = modelHeadPosition.y - playerHeadPosition.y + HeadOffset.y;
            VirtualRealityCameraPosition.transform.position = playerCameraPosition;

            MeshTransform.localPosition = new Vector3(HeadOffset.x, 0.0f, HeadOffset.z);

            _leftFootPos = LeftFootTarget.position;
            _leftFootPos.y = 0f;
            _rightFootPos = RightFootTarget.position;
            _rightFootPos.y = 0f;

            _leftFootRot = LeftFootTarget.eulerAngles;
            _leftFootRot.x = 0f;
            _leftFootRot.z = 0f;

            _rightFootRot = RightFootTarget.eulerAngles;
            _rightFootRot.x = 0f;
            _rightFootRot.z = 0f;

            return;
        }


        if (Input.GetButtonUp("ChangeView"))
        {
            CurrentCameraPosition = CurrentCameraPosition.Equals(CameraPosition.ThirdPerson) ? CameraPosition.FirstPerson : CameraPosition.ThirdPerson;
        }

        if (Input.GetButtonUp("ToggleVR"))
        {
            CurrentCameraPosition = CameraPosition.VirtualReality;
        }

        var forwardInput = Input.GetAxis("Vertical"); // Vertical axis is back and forth tied to up and down.
        var yawInput = (Input.GetAxis("Horizontal") + Input.GetAxis("MouseX")) / 2.0f;
        var pitchInput = Input.GetAxis("LookPitch"); // up and down

        _animator.SetFloat("VSpeed", forwardInput);

        transform.Rotate(0, yawInput * TurningSpeed, 0);

        var mainCameraTransform = Camera.main.transform;
        if (CurrentCameraPosition.Equals(CameraPosition.FirstPerson))
        {
            var currentRotation = mainCameraTransform.localEulerAngles.x;
            if (currentRotation >= 180.0f)
            {
                currentRotation -= 360.0f; // Makes it so 180 to 359 become negative, which corresponds to looking up
            }

            var nextRotation = currentRotation - pitchInput * TurningSpeed;
            nextRotation = Mathf.Clamp(nextRotation, -PitchLimit, PitchLimit);
            if (nextRotation < 0.0f)
            {
                nextRotation = 360.0f + nextRotation; // Adding a negative is like subtracting
            }
            mainCameraTransform.localEulerAngles = new Vector3(nextRotation, 0.0f, 0.0f);
        }
        else
        {
            mainCameraTransform.localRotation = Quaternion.identity;
        }
    }

    void OnAnimatorIK()
    {
        if (CurrentCameraPosition.Equals(CameraPosition.VirtualReality))
        {
            var cameraTransform = VirtualRealityCameraPosition.transform;
            var leftPosition = cameraTransform.TransformPoint(InputTracking.GetLocalPosition(VRNode.LeftHand));
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftPosition);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, InputTracking.GetLocalRotation(VRNode.LeftHand));
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            var rightPosition = cameraTransform.TransformPoint(InputTracking.GetLocalPosition(VRNode.RightHand));
            _animator.SetIKPosition(AvatarIKGoal.RightHand, rightPosition);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, InputTracking.GetLocalRotation(VRNode.RightHand));
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

            _animator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootPos);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(_rightFootRot));
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootPos);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.Euler(_leftFootRot));
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

            _animator.SetLookAtPosition(LookAtTarget.position);
            _animator.SetLookAtWeight(1f);
        }
    }
}