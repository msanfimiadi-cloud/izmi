using UnityEngine;

namespace Izmi
{
    public sealed class CityAircraft : MonoBehaviour
    {
        private Vector3 center;
        private float radius;
        private float altitude;
        private float speed;
        private float angle;
        private Transform rotor;
        private bool helicopter;

        public void Configure(Vector3 routeCenter, float routeRadius, float flightAltitude,
            float flightSpeed, float initialAngle, bool isHelicopter, Transform mainRotor)
        {
            center = routeCenter;
            radius = routeRadius;
            altitude = flightAltitude;
            speed = flightSpeed;
            angle = initialAngle;
            helicopter = isHelicopter;
            rotor = mainRotor;
        }

        private void Update()
        {
            angle += Time.deltaTime * speed;
            var next = center + new Vector3(
                Mathf.Cos(angle) * radius,
                altitude + Mathf.Sin(angle * 2.3f) * (helicopter ? 0.22f : 0.55f),
                Mathf.Sin(angle) * radius);

            var direction = next - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    Time.deltaTime * 5f);
            }

            transform.position = next;
            if (rotor != null)
            {
                rotor.Rotate(Vector3.up, Time.deltaTime * 1100f, Space.Self);
            }
        }
    }
}
