using UnityEngine;

namespace Izmi
{
    public sealed class SunOrbit : MonoBehaviour
    {
        [SerializeField] private float longitudeOffset = -28f;
        private SimulationClock simulationClock;

        private void Start()
        {
            simulationClock = FindAnyObjectByType<SimulationClock>();
        }

        private void LateUpdate()
        {
            if (simulationClock == null)
            {
                return;
            }

            var hours = (float)simulationClock.CurrentDate.TimeOfDay.TotalHours;
            var dailyAngle = hours / 24f * 360f - 90f;
            transform.rotation = Quaternion.Euler(dailyAngle, longitudeOffset, 7f);
        }
    }
}
