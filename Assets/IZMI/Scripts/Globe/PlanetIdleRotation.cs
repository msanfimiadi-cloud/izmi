using UnityEngine;

namespace Izmi
{
    public sealed class PlanetIdleRotation : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 0.18f;
        [SerializeField] private float resumeDelay = 2.5f;

        private float lastInteractionTime = float.NegativeInfinity;

        public void NotifyInteraction()
        {
            lastInteractionTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime - lastInteractionTime < resumeDelay)
            {
                return;
            }

            transform.Rotate(
                Vector3.up,
                degreesPerSecond * Time.unscaledDeltaTime,
                Space.World);
        }
    }
}
