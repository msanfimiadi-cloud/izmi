using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Izmi
{
    public sealed class GlobeCameraController : MonoBehaviour
    {
        [SerializeField] private float dragSensitivity = 0.14f;
        [SerializeField] private float zoomSensitivity = 0.012f;
        [SerializeField] private float zoomSmoothness = 10f;

        private Transform globe;
        private float minimumDistance;
        private float maximumDistance;
        private float targetDistance;
        private Vector2 previousPointer;
        private float previousPinchDistance;
        private bool pointerDragging;
        private CityPrototypeController cityPrototype;

        public void Configure(Transform globeTransform, float minDistance, float maxDistance)
        {
            globe = globeTransform;
            minimumDistance = minDistance;
            maximumDistance = maxDistance;
            targetDistance = transform.position.magnitude;
            cityPrototype = FindAnyObjectByType<CityPrototypeController>();
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (globe == null)
            {
                return;
            }

            if (cityPrototype != null && cityPrototype.IsCityView)
            {
                return;
            }

            HandleTouch();
            HandleMouse();
            ApplyZoom();
        }

        private void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null || Touch.activeTouches.Count > 0)
            {
                return;
            }

            var pointer = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                pointerDragging = true;
                previousPointer = pointer;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                pointerDragging = false;
            }

            if (pointerDragging && mouse.leftButton.isPressed)
            {
                RotateGlobe(pointer - previousPointer);
                previousPointer = pointer;
            }

            var wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                targetDistance -= wheel * zoomSensitivity;

                if (wheel > 0f && targetDistance <= minimumDistance + 0.08f)
                {
                    cityPrototype?.EnterCity();
                }
            }
        }

        private void HandleTouch()
        {
            var touches = Touch.activeTouches;

            if (touches.Count == 1)
            {
                RotateGlobe(touches[0].delta);
                previousPinchDistance = 0f;
            }
            else if (touches.Count >= 2)
            {
                var currentDistance = Vector2.Distance(
                    touches[0].screenPosition,
                    touches[1].screenPosition);

                if (previousPinchDistance > 0f)
                {
                    targetDistance -= (currentDistance - previousPinchDistance) * zoomSensitivity;

                    if (targetDistance <= minimumDistance + 0.08f)
                    {
                        cityPrototype?.EnterCity();
                    }
                }

                previousPinchDistance = currentDistance;
            }
            else
            {
                previousPinchDistance = 0f;
            }
        }

        private void RotateGlobe(Vector2 delta)
        {
            var idleRotation = globe.GetComponent<PlanetIdleRotation>();
            if (idleRotation != null)
            {
                idleRotation.NotifyInteraction();
            }

            globe.Rotate(Vector3.up, -delta.x * dragSensitivity, Space.World);
            globe.Rotate(transform.right, delta.y * dragSensitivity, Space.World);
        }

        private void ApplyZoom()
        {
            targetDistance = Mathf.Clamp(targetDistance, minimumDistance, maximumDistance);
            var currentDistance = transform.position.magnitude;
            var distance = Mathf.Lerp(
                currentDistance,
                targetDistance,
                1f - Mathf.Exp(-zoomSmoothness * Time.unscaledDeltaTime));

            transform.position = transform.position.normalized * distance;
            transform.LookAt(Vector3.zero);
        }
    }
}
