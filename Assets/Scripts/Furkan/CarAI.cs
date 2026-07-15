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

        // Allows switching to next waypoint slightly before corner nodes.
        [SerializeField] private float cornerCutDistance = 0.45f;

        // Reduces throttle shortly before a turn so corners are smoother.
        [SerializeField] private float preCornerBrakeDistance = 1.9f;
        [SerializeField] private float preCornerBrakeStrength = 0.6f;

        // The current target position on the path
        [SerializeField] private Vector3 currentTargetPosition;

        // The point from which collision raycasts are cast (usually the car's front)
        [SerializeField] private GameObject raycastStartingPoint = null;

        // Length of the raycast used for collision detection
        [SerializeField] private float collisionRaycastLength = 0.1f;

        // Distance and radius used to detect cars ahead in traffic
        [SerializeField] private float trafficDetectionDistance = 3f;
        [SerializeField] private float trafficStopDistance = 1.2f;
        [SerializeField] private float trafficCheckRadius = 0.35f;

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
            if (path == null || path.Count < 2)
            {
                Destroy(gameObject);
                return;
            }

            this.path = path;
            index = 0;
            currentTargetPosition = this.path[index];

            // Instantly rotate the car to face the next path point
            var nextPoint = this.path[1];
            var lookDirection = nextPoint - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

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
        /// Checks for obstacles in front of the car using raycasts and traffic detection.
        /// If another car is detected ahead, the car slows down or stops to preserve distance.
        /// </summary>
        private void CheckForCollisions()
        {
            collisionStop = false;

            if (Physics.Raycast(raycastStartingPoint.transform.position, transform.forward, collisionRaycastLength,
                    1 << gameObject.layer))
            {
                collisionStop = true;
                return;
            }

            if (TryFindVehicleAhead(out var otherCar, out var distance))
            {
                collisionStop = distance <= trafficStopDistance || (distance <= trafficDetectionDistance && otherCar.Stop);
            }
        }

        private bool TryFindVehicleAhead(out CarAI otherCar, out float distance)
        {
            otherCar = null;
            distance = float.MaxValue;

            var origin = raycastStartingPoint != null
                ? raycastStartingPoint.transform.position
                : transform.position + transform.forward * 0.5f;

            if (Physics.SphereCast(origin, trafficCheckRadius, transform.forward, out var hit, trafficDetectionDistance))
            {
                var carAhead = hit.collider.GetComponentInParent<CarAI>();
                if (carAhead != null && carAhead != this)
                {
                    var directionToCar = carAhead.transform.position - transform.position;
                    if (Vector3.Dot(directionToCar, transform.forward) > 0)
                    {
                        otherCar = carAhead;
                        distance = directionToCar.magnitude;
                        return true;
                    }
                }
            }

            return false;
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
                float rotateCar = 0f;
                if (Mathf.Abs(angle) > turningAngleOffset)
                {
                    // Proportional steering prevents binary oversteer at corners.
                    rotateCar = Mathf.Clamp(angle / 45f, -1f, 1f);
                }

                // Slow down while taking sharp turns to stay on lane.
                float turnStrength = Mathf.Clamp01(Mathf.Abs(angle) / 90f);
                float throttle = Mathf.Lerp(1f, 0.45f, turnStrength);

                // Additional lookahead braking before the next corner.
                throttle *= GetPreCornerBrakeFactor();

                OnDrive?.Invoke(new Vector2(rotateCar, throttle));
            }
        }

        private float GetPreCornerBrakeFactor()
        {
            if (path == null || index >= path.Count - 2)
                return 1f;

            var currentPosition = transform.position;
            var toCurrentTarget = currentTargetPosition - currentPosition;
            toCurrentTarget.y = 0f;

            var distanceToCurrentTarget = toCurrentTarget.magnitude;
            if (distanceToCurrentTarget > preCornerBrakeDistance)
                return 1f;

            var currentSegment = currentTargetPosition - currentPosition;
            currentSegment.y = 0f;
            if (currentSegment.sqrMagnitude < 0.0001f)
                return 1f;

            var nextSegment = path[index + 1] - currentTargetPosition;
            nextSegment.y = 0f;
            if (nextSegment.sqrMagnitude < 0.0001f)
                return 1f;

            var cornerAngle = Vector3.Angle(currentSegment.normalized, nextSegment.normalized);
            var cornerStrength = Mathf.InverseLerp(10f, 90f, cornerAngle);
            if (cornerStrength <= 0f)
                return 1f;

            var approachStrength = 1f - Mathf.Clamp01(distanceToCurrentTarget / preCornerBrakeDistance);
            var brake = cornerStrength * approachStrength * Mathf.Clamp01(preCornerBrakeStrength);

            return Mathf.Clamp01(1f - brake);
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

                if (ShouldAdvanceToNextTarget(distanceToCheck))
                {
                    SetNextTargetIndex();
                }
            }
        }

        private bool ShouldAdvanceToNextTarget(float distanceToCheck)
        {
            var toTarget = currentTargetPosition - transform.position;
            toTarget.y = 0f;
            var distanceToTarget = toTarget.magnitude;

            if (distanceToTarget < distanceToCheck)
                return true;

            if (path == null || index >= path.Count - 1)
                return false;

            var nextSegment = path[index + 1] - currentTargetPosition;
            nextSegment.y = 0f;
            if (nextSegment.sqrMagnitude < 0.0001f)
                return false;

            // If the car has crossed the node plane, switch target immediately.
            if (Vector3.Dot(toTarget, nextSegment.normalized) <= 0f)
                return true;

            if (index < path.Count - 2)
            {
                var upcomingSegment = path[index + 2] - path[index + 1];
                upcomingSegment.y = 0f;
                if (upcomingSegment.sqrMagnitude > 0.0001f)
                {
                    var cornerAngle = Vector3.Angle(nextSegment.normalized, upcomingSegment.normalized);
                    var cornerStrength = Mathf.InverseLerp(15f, 90f, cornerAngle);
                    if (cornerStrength > 0f)
                    {
                        var earlyDistance = Mathf.Lerp(distanceToCheck, cornerCutDistance, cornerStrength);
                        if (distanceToTarget < earlyDistance)
                            return true;
                    }
                }
            }

            return false;
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
