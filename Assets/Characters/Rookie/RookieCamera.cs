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

    public GameObject FirstPersonCameraPosition;
    public GameObject ThirdPersonCameraPosition;
    public GameObject VirtualRealityCameraPosition;

    public Transform Pivot;
    public Camera PlayerCamera;
    public Transform Head;
    public Transform LookAtTarget;
    public Transform LeftHandTarget;
    public Transform RightHandTarget;
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
            if (value.Equals(CameraPosition.VirtualReality) || VirtualRealityCameraPosition.activeInHierarchy)
            {
                // TODO: Allow switching into and from VR
                Debug.LogError("Not able to set VR camera right now.");
                return;
            }

            var mainCamera = Camera.main;
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

    private void Start()
    {
        _animator = GetComponent<Animator>();

        VRSettings.enabled = !ForceDisableVr;
        if (VRSettings.enabled && VRDevice.isPresent)
        {
            Camera.main.gameObject.SetActive(false);
            VirtualRealityCameraPosition.SetActive(true);
        }
        else
        {
            VirtualRealityCameraPosition.SetActive(false);
            Camera.main.gameObject.SetActive(true);
        }
    }

    public void OnTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        Debug.Log("---");
        Debug.LogFormat("Controller: {0}", e.controllerIndex);
        Debug.LogFormat("Head: {0}, {1}", InputTracking.GetLocalPosition(VRNode.Head), InputTracking.GetLocalRotation(VRNode.Head).eulerAngles);
        Debug.LogFormat("CenterEye: {0}, {1}", InputTracking.GetLocalPosition(VRNode.CenterEye), InputTracking.GetLocalRotation(VRNode.CenterEye).eulerAngles);
        Debug.LogFormat("LeftHand: {0}, {1}", InputTracking.GetLocalPosition(VRNode.LeftHand), InputTracking.GetLocalRotation(VRNode.LeftHand).eulerAngles);
        Debug.LogFormat("RightHand: {0}, {1}", InputTracking.GetLocalPosition(VRNode.RightHand), InputTracking.GetLocalRotation(VRNode.RightHand).eulerAngles);
        Debug.Log("---");
    }

    private void Update()
    {
        if (CurrentCameraPosition.Equals(CameraPosition.VirtualReality))
        {
            var headTarget = PlayerCamera.transform;
            var playerPosition = headTarget.position;
            var modelPosition = Pivot.position;
            modelPosition.x = playerPosition.x;
            modelPosition.z = playerPosition.z;
            Pivot.position = modelPosition;

            var playerRotation = headTarget.rotation.eulerAngles;
            var modelRotation = Pivot.rotation.eulerAngles;
            Pivot.rotation = Quaternion.Euler(modelRotation.x, playerRotation.y, modelRotation.z);

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

        if (Input.GetButtonUp("Fire3"))
        {
            CurrentCameraPosition = CurrentCameraPosition.Equals(CameraPosition.ThirdPerson) ? CameraPosition.FirstPerson : CameraPosition.ThirdPerson;
        }

        var forwardInput = Input.GetAxis("Vertical"); // Vertical axis is back and forth tied to up and down.
        var yawInput = (Input.GetAxis("Mouse X") + Input.GetAxis("Horizontal")) / 2.0f; // Take average of mouse and stick/keyboard input
        var pitchInput = Input.GetAxis("Mouse Y"); // up and down

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

    private void LateUpdate()
    {
        if (CurrentCameraPosition.Equals(CameraPosition.VirtualReality))
        {
        }
    }

    void OnAnimatorIK()
    {
        if (CurrentCameraPosition.Equals(CameraPosition.VirtualReality))
        {
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.rotation);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandTarget.rotation);
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