using UnityEngine;

namespace Izmi
{
    public sealed class MarkerPulse : MonoBehaviour
    {
        private Vector3 initialScale;
        private float phase;

        private void Awake()
        {
            initialScale = transform.localScale;
            phase = Mathf.Abs(transform.position.GetHashCode() % 1000) / 1000f * Mathf.PI * 2f;
        }

        private void Update()
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.4f + phase) * 0.16f;
            transform.localScale = initialScale * pulse;
        }
    }
}
