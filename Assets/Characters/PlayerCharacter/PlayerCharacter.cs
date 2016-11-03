using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Animator))]
public class PlayerCharacter : MonoBehaviour
{
    public float AnimationSpeed = 1.5f;
    public float ForwardsSpeed = 7.0f;
    public float BackwardsSpeed = 7.0f;
    public float TurningSpeed = 2.0f;

    private static int NoInputState = Animator.StringToHash("Base Layer.Idle");
    private static int MovingState = Animator.StringToHash("Base Layer.Locomotion");
    private static int IdleAnimationState = Animator.StringToHash("Base Layer.Rest");

    private Vector3 _velocity;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        float forwardInput = Input.GetAxis("Vertical"); // Vertical axis is back and forth tied to up and down.
        float turningInput = Input.GetAxis("Horizontal"); // Horizontal axis is side to side (turning)

        _animator.SetFloat("Speed", forwardInput);
        _animator.SetFloat("Direction", turningInput);
        _animator.speed = AnimationSpeed;

        var currentBaseState = _animator.GetCurrentAnimatorStateInfo(0); // 0 should correspond to "Base Layer"

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

        transform.localPosition += velocityVector * Time.fixedDeltaTime;
        transform.Rotate(0, turningInput * TurningSpeed, 0);

        // TODO: Move to Idle Animation when input has not been received for a while/And then move out when it finishes. This should likely be a trigger.
    }
}
