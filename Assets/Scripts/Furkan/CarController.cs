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

        // Stores the current movement input (x: turn, y: forward)
        [SerializeField] private Vector2 movementVector;

        /// <summary>
        /// Initializes the Rigidbody reference.
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
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
            // Apply forward force if under max speed
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(movementVector.y * power * transform.forward);
            }

            // Apply turning torque (only when moving forward/backward)
            rb.AddTorque(movementVector.x * movementVector.y * torque * Vector3.up);
        }
    }
}
