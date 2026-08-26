using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Izmi
{
    public sealed class CityCameraController : MonoBehaviour
    {
        [SerializeField] private float panSensitivity = 0.018f;
        [SerializeField] private float zoomSensitivity = 0.025f;

        private CityPrototypeController cityPrototype;
        private Vector2 previousPointer;
        private float previousPinchDistance;
        private bool dragging;
        private bool wasInCity;

        private void Start()
        {
            cityPrototype = FindAnyObjectByType<CityPrototypeController>();
        }

        private void Update()
        {
            var inCity = cityPrototype != null && cityPrototype.IsCityView;
            if (!inCity)
            {
                wasInCity = false;
                return;
            }

            if (!wasInCity)
            {
                dragging = false;
                previousPinchDistance = 0f;
                wasInCity = true;
            }

            HandleMouse();
            HandleTouch();
            ClampPosition();
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
                dragging = true;
                previousPointer = pointer;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
            }

            if (dragging && mouse.leftButton.isPressed)
            {
                Pan(pointer - previousPointer);
                previousPointer = pointer;
            }

            var wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                Zoom(wheel * zoomSensitivity);
            }
        }

        private void HandleTouch()
        {
            var touches = Touch.activeTouches;
            if (touches.Count == 1)
            {
                Pan(touches[0].delta);
                previousPinchDistance = 0f;
            }
            else if (touches.Count >= 2)
            {
                var distance = Vector2.Distance(
                    touches[0].screenPosition,
                    touches[1].screenPosition);

                if (previousPinchDistance > 0f)
                {
                    Zoom((distance - previousPinchDistance) * zoomSensitivity);
                }

                previousPinchDistance = distance;
            }
            else
            {
                previousPinchDistance = 0f;
            }
        }

        private void Pan(Vector2 delta)
        {
            var planarForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var planarRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            transform.position +=
                (-planarRight * delta.x - planarForward * delta.y) *
                panSensitivity;
        }

        private void Zoom(float amount)
        {
            transform.position += transform.forward * amount;
        }

        private void ClampPosition()
        {
            var position = transform.position;
            position.x = Mathf.Clamp(position.x, -18f, 18f);
            position.y = Mathf.Clamp(position.y, 10f, 42f);
            position.z = Mathf.Clamp(position.z, -34f, 18f);
            transform.position = position;
        }
    }
}
