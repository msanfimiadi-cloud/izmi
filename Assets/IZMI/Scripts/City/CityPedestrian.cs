using UnityEngine;

namespace Izmi
{
    public sealed class CityPedestrian : MonoBehaviour
    {
        private Vector3 center;
        private float radius;
        private float speed;
        private float angle;

        public void Configure(Vector3 pathCenter, float pathRadius, float walkSpeed, float initialAngle)
        {
            center = pathCenter;
            radius = pathRadius;
            speed = walkSpeed;
            angle = initialAngle;
        }

        private void Update()
        {
            angle += Time.deltaTime * speed;
            var nextPosition = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0.55f,
                Mathf.Sin(angle) * radius);

            var direction = nextPosition - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            transform.position = nextPosition;
        }
    }
}
