using System;
using UnityEngine;
using UnityEngine.Events;

namespace Boxsubmus.Boxrhythm
{
    public class Metronome : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        private float lastReportedBeat = 0.0f;

        /// <summary>
        /// Called when a beat occurs.
        /// </summary>
        public Action OnTick;

        private void Update()
        {
            if (ReportBeat(ref lastReportedBeat))
            {
                // Play Sound Here
                if (OnTick != null)
                    OnTick();
            }
            else if (conductor.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(conductor.songPositionInBeats);
            }
        }

        private bool ReportBeat(ref float lastReportedBeat, float offset = 0, bool shiftBeatToOffset = true)
        {
            bool result = conductor.songPositionInBeats + (shiftBeatToOffset ? offset : 0f) >= (lastReportedBeat) + 1f;
            if (result)
            {
                lastReportedBeat += 1f;
                if (lastReportedBeat < conductor.songPositionInBeats)
                {
                    lastReportedBeat = Mathf.Round(conductor.songPositionInBeats);
                }
            }
            return result;
        }
    }
}
