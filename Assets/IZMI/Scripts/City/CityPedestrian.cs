using UnityEngine;

namespace Izmi
{
    public sealed class CityPedestrian : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<CityPedestrian> All =
            new System.Collections.Generic.List<CityPedestrian>();

        private Vector3 center;
        private float radius;
        private float speed;
        private float angle;
        private bool infected;
        private float reactionTimer;
        private Vector3 panicOffset;
        private Renderer bodyRenderer;
        private Material healthyMaterial;

        public bool IsInfected => infected;

        public void Configure(Vector3 pathCenter, float pathRadius, float walkSpeed, float initialAngle)
        {
            center = pathCenter;
            radius = pathRadius;
            speed = walkSpeed;
            angle = initialAngle;
            bodyRenderer = GetComponent<Renderer>();
            healthyMaterial = bodyRenderer != null ? bodyRenderer.sharedMaterial : null;
        }

        public void SetInfected(Material material)
        {
            if (infected)
            {
                return;
            }

            infected = true;
            speed *= 1.18f;
            radius *= 1.12f;
            if (bodyRenderer != null && material != null)
            {
                bodyRenderer.sharedMaterial = material;
            }
        }

        private void OnEnable()
        {
            if (!All.Contains(this))
            {
                All.Add(this);
            }
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Update()
        {
            var timeScale = Mathf.Clamp(Time.timeScale, 0f, 6f);
            if (timeScale <= 0f)
            {
                return;
            }

            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0f)
            {
                reactionTimer = Random.Range(0.18f, 0.34f);
                panicOffset = EvaluateReaction();
            }

            angle += Time.deltaTime * speed * (infected ? 1.28f : 1f);
            var nextPosition = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0.55f,
                Mathf.Sin(angle) * radius) + panicOffset;

            var direction = nextPosition - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    Time.deltaTime * 12f);
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                nextPosition,
                Time.deltaTime * speed * (infected ? 1.55f : panicOffset == Vector3.zero ? 1f : 2.15f));
        }

        private Vector3 EvaluateReaction()
        {
            CityPedestrian nearest = null;
            var nearestDistance = infected ? 4.2f : 3.4f;

            foreach (var pedestrian in All)
            {
                if (pedestrian == null || pedestrian == this ||
                    pedestrian.infected == infected)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, pedestrian.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = pedestrian;
                    nearestDistance = distance;
                }
            }

            if (nearest == null)
            {
                return Vector3.zero;
            }

            var direction = infected
                ? nearest.transform.position - transform.position
                : transform.position - nearest.transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized * (infected ? 1.2f : 2.2f)
                : Vector3.zero;
        }
    }
}
