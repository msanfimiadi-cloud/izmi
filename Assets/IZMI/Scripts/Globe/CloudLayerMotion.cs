using UnityEngine;

namespace Izmi
{
    public sealed class CloudLayerMotion : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 0.45f;

        private void Update()
        {
            transform.Rotate(
                Vector3.up,
                degreesPerSecond * Time.unscaledDeltaTime,
                Space.Self);
        }
    }
}
