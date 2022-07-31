// This isn't clean code, this is just an example of what you can do with Boxrhythm.
// Not what you're "SUPPOSED" to do.

using Boxsubmus.Boxrhythm;
using UnityEngine;

namespace Boxsubmus.Boxrhythm.Tests
{
    public class TestBehaviour : MonoBehaviour
    {
        public Conductor conductor;
        public Metronome metronome;

        public TempoFinder tempoFinder;
        public RectTransform content;
        public Transform TempoChangesHolder;
        public Transform CurrentBeatMarker;

        private void Start()
        {
            TempoChangesHolder.transform.GetChild(0).gameObject.SetActive(false);
            metronome.OnTick += Tick;
        }

        private void Tick()
        {
            Debug.Log("Tick");
        }

        private void Update()
        {
            content.sizeDelta = new Vector2(conductor.GetBeatFromSongPos(conductor.musicSource.clip.length), content.sizeDelta.y);

            for (int i = 1; i < TempoChangesHolder.transform.childCount; i++)
                Destroy(TempoChangesHolder.transform.GetChild(i).gameObject);

            for (int i = 0; i < conductor.tempoChanges.Count; i++)
            {
                var tempo = conductor.tempoChanges[i];

                GameObject tempoChangeObj = Instantiate(TempoChangesHolder.transform.GetChild(0).gameObject, TempoChangesHolder);
                tempoChangeObj.SetActive(true);
                var rect = tempoChangeObj.GetComponent<RectTransform>();

                rect.anchoredPosition = new Vector2(tempo.beat, rect.position.y);
                rect.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(tempo.length, rect.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
            }

            if (Input.GetMouseButtonDown(0))
                tempoFinder.TapBPM();

            CurrentBeatMarker.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(conductor.songPositionInBeats, 0);
        }

        void OnGUI()
        {
            GUI.TextArea(new Rect(20, 20, 80, 20), tempoFinder.Tempo.ToString());
            GUI.TextArea(new Rect(20, 40, 80, 20), conductor.songPositionInBeats.ToString());
            GUI.TextArea(new Rect(20, 60, 80, 20), conductor.songTempo.ToString());
        }
    }
}