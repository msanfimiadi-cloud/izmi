using UnityEngine;

namespace Izmi
{
    public sealed class CityVehicle : MonoBehaviour
    {
        private Vector3 start;
        private Vector3 end;
        private float speed;
        private float progress;

        public void Configure(Vector3 routeStart, Vector3 routeEnd, float routeSpeed, float initialProgress)
        {
            start = routeStart;
            end = routeEnd;
            speed = routeSpeed;
            progress = initialProgress;
            transform.position = Vector3.Lerp(start, end, progress);
            transform.LookAt(end);
        }

        private void Update()
        {
            var routeLength = Mathf.Max(Vector3.Distance(start, end), 0.01f);
            progress += Time.deltaTime * speed / routeLength;

            if (progress >= 1f)
            {
                progress -= 1f;
            }

            transform.position = Vector3.Lerp(start, end, progress);
        }
    }
}
