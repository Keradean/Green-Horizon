using System.Collections.Generic;
using UnityEngine;

namespace Furkan
{
    /// <summary>
    /// Simple path follower for pedestrian agents.
    /// </summary>
    public class PedestrianAI : MonoBehaviour
    {
        [SerializeField] private List<Vector3> path = null;
        [SerializeField] private float moveSpeed = 0.3f;
        [SerializeField] private float arriveDistance = 0.1f;
        [SerializeField] private float rotateSpeed = 8f;

        private int currentIndex;

        private void Start()
        {
            if (path == null || path.Count < 2)
            {
                enabled = false;
            }
        }

        public void SetPath(List<Vector3> newPath)
        {
            if (newPath == null || newPath.Count < 2)
            {
                Destroy(gameObject);
                return;
            }

            path = newPath;
            currentIndex = 0;
            transform.position = path[0];
            FaceTarget(path[1]);
            enabled = true;
        }

        private void Update()
        {
            if (path == null || path.Count < 2 || currentIndex >= path.Count)
                return;

            var target = path[currentIndex];
            var toTarget = target - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude <= arriveDistance)
            {
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    Destroy(gameObject);
                    return;
                }

                target = path[currentIndex];
                toTarget = target - transform.position;
                toTarget.y = 0f;
            }

            if (!(toTarget.sqrMagnitude > 0.0001f)) return;
            FaceTarget(target);
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }

        private void FaceTarget(Vector3 target)
        {
            var direction = target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }
}