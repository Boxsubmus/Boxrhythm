using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Boxsubmus.Boxrhythm
{
    public class TempoFinder : MonoBehaviour
    {
        private bool pressed = false;
        private float timePressed;

        private List<float> pressTimes = new();

        [SerializeField]
        private float tempo;
        public float Tempo => tempo;

        /// <summary>
        /// Reset the BPM
        /// </summary>
        public void Reset()
        {
            pressed = false;
            timePressed = 0.0f;
            pressTimes = new();
            tempo = 0.0f;
        }

        /// <summary>
        /// Call the 'tapped' event.
        /// </summary>
        public void TapBPM()
        {
            pressed = true;
        }

        private void LateUpdate()
        {
            timePressed += Time.deltaTime;
            if (pressed)
            {
                pressed = false;
                tempo = UpdateBPM(timePressed);
                timePressed = 0;
            }
        }

        private float UpdateBPM(float timePressed)
        {
            pressTimes.Add(timePressed);

            // First press is not enough
            if (pressTimes.Count < 2) return 0;

            if (pressTimes.Count > 50)
                pressTimes.RemoveAt(0);

            var averageTime = pressTimes.GetRange(1, pressTimes.Count - 1).Average();

            return 60.0f / averageTime;
        }
    }
}
