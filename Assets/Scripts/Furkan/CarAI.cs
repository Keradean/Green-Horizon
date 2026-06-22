using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Furkan
{
    /// <summary>
    /// Controls the movement and behavior of a car agent along a given path.
    /// Handles path following, turning, collision detection, and drive events.
    /// </summary>
    public class CarAI : MonoBehaviour
    {
        // The path the car will follow (list of world positions)
        [SerializeField] private List<Vector3> path = null;

        // Distance thresholds for considering the car "arrived" at a path point or the final point
        [SerializeField] private float arriveDistance = .3f, lastPointArriveDistance = .1f;

        // Minimum angle (in degrees) before the car starts turning
        [SerializeField] private float turningAngleOffset = 5;

        // The current target position on the path
        [SerializeField] private Vector3 currentTargetPosition;

        // The point from which collision raycasts are cast (usually the car's front)
        [SerializeField] private GameObject raycastStartingPoint = null;

        // Length of the raycast used for collision detection
        [SerializeField] private float collisionRaycastLength = 0.1f;

        /// <summary>
        /// Returns true if the car is at the last index of the path.
        /// </summary>
        internal bool IsThisLastPathIndex()
        {
            return index >= path.Count - 1;
        }

        // Current index in the path
        private int index = 0;

        // Flags to control movement: stop (manual) and collisionStop (automatic)
        private bool stop;
        private bool collisionStop = false;

        /// <summary>
        /// Property to get/set the stop flag. Car stops if either stop or collisionStop is true.
        /// </summary>
        public bool Stop
        {
            get { return stop || collisionStop; }
            set { stop = value; }
        }

        /// <summary>
        /// Event invoked every frame to control the car's movement (e.g., for a car controller script).
        /// Vector2: (turn direction, move forward)
        /// </summary>
        [field: SerializeField]
        public UnityEvent<Vector2> OnDrive { get; set; }

        /// <summary>
        /// Initializes the car at start. Stops if no path is set.
        /// </summary>
        private void Start()
        {
            if (path == null || path.Count == 0)
            {
                Stop = true;
            }
            else
            {
                currentTargetPosition = path[index];
            }
        }

        /// <summary>
        /// Sets the path for the car to follow and resets its state.
        /// </summary>
        public void SetPath(List<Vector3> path)
        {
            if (path.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            this.path = path;
            index = 0;
            currentTargetPosition = this.path[index];

            // Instantly rotate the car to face the next path point
            Vector3 relativepoint = transform.InverseTransformPoint(this.path[index + 1]);
            float angle = Mathf.Atan2(relativepoint.x, relativepoint.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
            Stop = false;
        }

        /// <summary>
        /// Called once per frame. Handles arrival checks, driving logic, and collision detection.
        /// </summary>
        private void Update()
        {
            CheckIfArrived();
            Drive();
            CheckForCollisions();
        }

        /// <summary>
        /// Checks for obstacles in front of the car using a raycast.
        /// If an obstacle is detected, sets collisionStop to true.
        /// </summary>
        private void CheckForCollisions()
        {
            if (Physics.Raycast(raycastStartingPoint.transform.position, transform.forward, collisionRaycastLength,
                    1 << gameObject.layer))
            {
                collisionStop = true;
            }
            else
            {
                collisionStop = false;
            }
        }

        /// <summary>
        /// Handles the car's driving logic: turning and moving forward.
        /// Invokes the OnDrive event with the appropriate control values.
        /// </summary>
        private void Drive()
        {
            if (Stop)
            {
                OnDrive?.Invoke(Vector2.zero);
            }
            else
            {
                Vector3 relativepoint = transform.InverseTransformPoint(currentTargetPosition);
                float angle = Mathf.Atan2(relativepoint.x, relativepoint.z) * Mathf.Rad2Deg;
                var rotateCar = 0;
                if (angle > turningAngleOffset)
                {
                    rotateCar = 1;
                }
                else if (angle < -turningAngleOffset)
                {
                    rotateCar = -1;
                }

                OnDrive?.Invoke(new Vector2(rotateCar, 1));
            }
        }

        /// <summary>
        /// Checks if the car has arrived at the current target position.
        /// If so, advances to the next path point or destroys the car if finished.
        /// </summary>
        private void CheckIfArrived()
        {
            if (Stop == false)
            {
                var distanceToCheck = arriveDistance;
                if (index == path.Count - 1)
                {
                    distanceToCheck = lastPointArriveDistance;
                }

                if (Vector3.Magnitude(currentTargetPosition - transform.position) < distanceToCheck)
                {
                    SetNextTargetIndex();
                }
            }
        }

        /// <summary>
        /// Advances to the next target in the path. Destroys the car if the path is finished.
        /// </summary>
        private void SetNextTargetIndex()
        {
            index++;
            if (index >= path.Count)
            {
                Stop = true;
                Destroy(gameObject);
            }
            else
            {
                currentTargetPosition = path[index];
            }
        }
    }
}
