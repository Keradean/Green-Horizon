using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Furkan
{
    public class CarAI : MonoBehaviour
    {
        [SerializeField] private List<Vector3> path = null;
        [SerializeField] private float arriveDistance = .3f, lastPointArriveDistance = .1f;
        [SerializeField] private float turningAngleOffset = 5;
        [SerializeField] private float cornerCutDistance = 0.45f;
        [SerializeField] private float preCornerBrakeDistance = 1.9f;
        [SerializeField] private float preCornerBrakeStrength = 0.6f;
        [SerializeField] private Vector3 currentTargetPosition;
        [SerializeField] private GameObject raycastStartingPoint = null;
        [SerializeField] private float collisionRaycastLength = 0.1f;
        [SerializeField] private float trafficDetectionDistance = 3f;
        [SerializeField] private float trafficStopDistance = 1.2f;
        [SerializeField] private float trafficCheckRadius = 0.35f;

        private float _stuckTimer = 0f;
        private Vector3 _lastPosition;
        private const float StuckTimeout = 4f;
        private const float StuckDistanceThreshold = 0.1f;

        internal bool IsThisLastPathIndex()
        {
            return index >= path.Count - 1;
        }

        private int index = 0;
        private bool stop;
        private bool collisionStop = false;

        public bool Stop
        {
            get { return stop || collisionStop; }
            set { stop = value; }
        }

        [field: SerializeField]
        public UnityEvent<Vector2> OnDrive { get; set; }

        private void Start()
        {
            if (path == null || path.Count == 0)
            {
                Stop = true;
            }
            else
            {
                currentTargetPosition = path[index];
                _lastPosition = transform.position;
            }
        }

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
            _lastPosition = transform.position;
            _stuckTimer = 0f;

            var nextPoint = this.path[1];
            var lookDirection = nextPoint - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

            Stop = false;
        }

        private void Update()
        {
            CheckIfArrived();
            Drive();
            CheckForCollisions();
            CheckIfStuck();
        }

        private void CheckIfStuck()
        {
            if (Stop) return;

            if (Vector3.Distance(transform.position, _lastPosition) < StuckDistanceThreshold)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= StuckTimeout)
                    Destroy(gameObject);
            }
            else
            {
                _stuckTimer = 0f;
                _lastPosition = transform.position;
            }
        }

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
                    rotateCar = Mathf.Clamp(angle / 45f, -1f, 1f);
                }

                float turnStrength = Mathf.Clamp01(Mathf.Abs(angle) / 90f);
                float throttle = Mathf.Lerp(1f, 0.45f, turnStrength);
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