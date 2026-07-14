using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Furkan
{

    /// <summary>
    /// Controls the movement and turning of a car using physics (Rigidbody).
    /// Applies forward force and turning torque based on input.
    /// </summary>

    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        // Reference to the car's Rigidbody component
        Rigidbody rb;

        // Forward movement power
        [SerializeField] private float power = 10;

        // Turning strength
        [SerializeField] private float torque = 0.5f;

        // Maximum allowed speed
        [SerializeField] private float maxSpeed = 5;

        // How quickly sideways velocity is damped to prevent drift
        [SerializeField] private float lateralGrip = 8f;

        // Helps stabilize spin while cornering
        [SerializeField] private float angularDamping = 4f;

        // Caps and smooths yaw rotation to avoid spin-outs on corners.
        [SerializeField] private float maxYawRate = 2.2f;
        [SerializeField] private float yawStability = 7f;

        // Stores the current movement input (x: turn, y: forward)
        [SerializeField] private Vector2 movementVector;

        /// <summary>
        /// Initializes the Rigidbody reference.
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = Mathf.Max(rb.maxAngularVelocity, maxYawRate);
        }

        /// <summary>
        /// Sets the movement input vector (x: turn, y: forward).
        /// </summary>
        public void Move(Vector2 movementInput)
        {
            this.movementVector = movementInput;
        }

        /// <summary>
        /// Applies movement and turning forces in the physics update loop.
        /// Limits speed and applies torque for turning.
        /// </summary>
        private void FixedUpdate()
        {
            rb.angularDamping = angularDamping;

            // Apply forward force if under max speed
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(movementVector.y * power * transform.forward);
            }

            // Apply turning torque (only when moving forward/backward)
            rb.AddTorque(movementVector.x * movementVector.y * torque * Vector3.up);

            // Keep yaw stable and prevent odd rotations around X/Z.
            var angularVelocity = rb.angularVelocity;
            angularVelocity.x = 0f;
            angularVelocity.z = 0f;

            var targetYaw = movementVector.x * movementVector.y * maxYawRate;
            angularVelocity.y = Mathf.Lerp(angularVelocity.y, targetYaw, yawStability * Time.fixedDeltaTime);
            angularVelocity.y = Mathf.Clamp(angularVelocity.y, -maxYawRate, maxYawRate);
            rb.angularVelocity = angularVelocity;

            // Remove sideways slip in local space so cars hold their lane.
            var localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, lateralGrip * Time.fixedDeltaTime);
            rb.linearVelocity = transform.TransformDirection(localVelocity);
        }
    }
}
