using System.Collections.Generic;
using UnityEngine;

namespace Izmi
{
    public sealed class CityPedestrian : MonoBehaviour
    {
        private static readonly List<CityPedestrian> All =
            new List<CityPedestrian>();

        private Vector3 home;
        private Vector3 destination;
        private float roamRadius;
        private float speed;
        private bool infected;
        private float scanTimer;
        private float destinationTimer;
        private float attackCooldown;
        private CityPedestrian threatOrTarget;
        private Renderer[] renderers;
        private Material[] healthyMaterials;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private float gait;

        public bool IsInfected => infected;

        public void Configure(Vector3 pathCenter, float pathRadius, float walkSpeed, float initialAngle)
        {
            home = pathCenter;
            roamRadius = Mathf.Max(2f, pathRadius * 2.6f);
            speed = walkSpeed;
            destination = home + new Vector3(
                Mathf.Cos(initialAngle),
                0f,
                Mathf.Sin(initialAngle)) * roamRadius;
            renderers = GetComponentsInChildren<Renderer>(true);
            healthyMaterials = new Material[renderers.Length];
            for (var index = 0; index < renderers.Length; index++)
            {
                healthyMaterials[index] = renderers[index].sharedMaterial;
            }

            leftArm = transform.Find("Left Arm");
            rightArm = transform.Find("Right Arm");
            leftLeg = transform.Find("Left Leg");
            rightLeg = transform.Find("Right Leg");
        }

        public void SetInfected(Material material)
        {
            if (infected) return;
            infected = true;
            speed *= 1.32f;
            if (material != null && renderers != null)
            {
                foreach (var bodyPart in renderers)
                {
                    if (bodyPart != null) bodyPart.sharedMaterial = material;
                }
            }
            transform.localScale = new Vector3(1.06f, 1.1f, 1.06f);
        }

        public void SetHealthy()
        {
            if (!infected) return;
            infected = false;
            speed /= 1.32f;
            transform.localScale = Vector3.one;
            if (renderers != null && healthyMaterials != null)
            {
                for (var index = 0; index < renderers.Length && index < healthyMaterials.Length; index++)
                {
                    if (renderers[index] != null)
                        renderers[index].sharedMaterial = healthyMaterials[index];
                }
            }
        }

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            scanTimer -= Time.deltaTime;
            destinationTimer -= Time.deltaTime;
            attackCooldown -= Time.deltaTime;

            if (scanTimer <= 0f)
            {
                scanTimer = Random.Range(0.12f, 0.24f);
                threatOrTarget = FindNearestOpponent(infected ? 8f : 10f);
            }

            var movementSpeed = speed;
            if (threatOrTarget != null)
            {
                var offset = threatOrTarget.transform.position - transform.position;
                offset.y = 0f;
                var distance = offset.magnitude;

                if (infected)
                {
                    destination = threatOrTarget.transform.position;
                    movementSpeed *= 2.15f;
                    if (distance < 0.72f && attackCooldown <= 0f)
                    {
                        attackCooldown = 0.75f;
                        PrototypeInfectionSystem.Active?.TryInfectFromAttack(threatOrTarget);
                    }
                }
                else
                {
                    var escape = distance > 0.01f
                        ? -offset.normalized
                        : Random.insideUnitSphere;
                    escape.y = 0f;
                    destination = transform.position + escape.normalized * 8f;
                    movementSpeed *= 2.7f;
                }
            }
            else if (destinationTimer <= 0f ||
                     Vector3.Distance(transform.position, destination) < 0.35f)
            {
                destinationTimer = Random.Range(1.6f, 4.2f);
                var wander = Random.insideUnitCircle * roamRadius;
                destination = home + new Vector3(wander.x, 0f, wander.y);
            }

            destination.x = Mathf.Clamp(destination.x, -33f, 29f);
            destination.z = Mathf.Clamp(destination.z, -34f, 34f);
            destination.y = 0f;

            var direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.002f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    1f - Mathf.Exp(-12f * Time.deltaTime));
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    Time.deltaTime * movementSpeed);
                AnimateWalk(movementSpeed);
            }
        }

        private CityPedestrian FindNearestOpponent(float radius)
        {
            CityPedestrian nearest = null;
            var nearestSquared = radius * radius;
            foreach (var pedestrian in All)
            {
                if (pedestrian == null || pedestrian == this ||
                    pedestrian.infected == infected ||
                    !pedestrian.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var squared = (pedestrian.transform.position - transform.position).sqrMagnitude;
                if (squared < nearestSquared)
                {
                    nearest = pedestrian;
                    nearestSquared = squared;
                }
            }
            return nearest;
        }

        private void AnimateWalk(float movementSpeed)
        {
            gait += Time.deltaTime * movementSpeed * (infected ? 9f : 7f);
            var swing = Mathf.Sin(gait) * (infected ? 42f : 28f);
            if (infected)
            {
                var attackSwing = swing * 0.28f;
                if (leftArm != null) leftArm.localRotation = Quaternion.Euler(68f + attackSwing, 0f, -10f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Euler(68f - attackSwing, 0f, 10f);
                if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
                if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
            }
            else
            {
                if (leftArm != null) leftArm.localRotation = Quaternion.Euler(swing, 0f, 0f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-swing, 0f, 0f);
                if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
                if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
            }
        }
    }
}
