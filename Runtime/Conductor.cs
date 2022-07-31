using Boxsubmus.Boxrhythm.Models;
using Boxsubmus.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Boxsubmus.Boxrhythm
{
    [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        /// <summary>
        /// Sn AudioSource attached to this GameObject that will play the music.
        /// </summary>
        public AudioSource musicSource;

        [Space(10)]

        /// <summary>
        /// The tempo the song will start at.
        /// </summary>
        public float initialTempo = 145.0f;

        /// <summary>
        /// Song beats per minute.
        /// This is determined by the song you're trying to sync up to.
        /// </summary>
        public float songTempo => 60.0f / secPerBeat;
        
        /// <summary>
        /// The real time in seconds per beat.
        /// </summary>
        private float secPerBeat;

        /// <summary>
        /// The number of seconds for each song beat, inversely scaled to song pitch (higher pitch = shorter time)
        /// </summary>
        public float pitchedSecPerBeat => (secPerBeat / musicSource.pitch);

        private float songPos; // for Conductor use only
        /// <summary>
        /// Current song position, in seconds.
        /// </summary>
        public float songPosition;

        private float songPosBeat; // for Conductor use only
        /// <summary>
        /// Current song position, in beats.
        /// </summary>
        public float songPositionInBeats;

        /// <summary>
        /// Current time of the song.
        /// </summary>
        private float time;

        /// <summary>
        /// The offset to the first beat of the song in seconds.
        /// </summary>
        public float firstBeatOffset;

        [Space(10)]

        /// <summary>
        /// Conductor is currently playing song.
        /// </summary>
        public bool isPlaying;

        /// <summary>
        /// Conductor is currently paused, but not fully stopped.
        /// </summary>
        public bool isPaused;

        /// <summary>
        /// Conductor's song is muted.
        /// </summary>
        public bool muted;

        /// <summary>
        /// Plays the song when the Awake() method is called.
        /// </summary>
        public bool playOnAwake;

        [Space(10)]
        /// <summary>
        /// Optional range where if enabled the song loops between the X value and the Y value. (Uses seconds)
        /// NOTE: I would recommend against using this and writing your own looper. As this can be pretty inconsistent.
        /// </summary>
        public Optional<Vector2> LoopRange = new Optional<Vector2>(new Vector2((60.0f/145f) * 1, (60.0f/145f)*16.97f));

        [Space(10)]
        /// <summary>
        /// List of TempoChanges that changes the tempo using the beat it starts and length of the change.
        /// </summary>
        public List<TempoChange> tempoChanges = new();

        private void Awake()
        {
            secPerBeat = SecondsPerBeat();

            musicSource.playOnAwake = false;
            if (playOnAwake)
                Play();
        }

        private void Reset()
        {
            LoopRange.Enabled = false;
            playOnAwake = true;

            var audioSource = GetComponent<AudioSource>();
            musicSource = audioSource;
            musicSource.clip = Resources.Load<AudioClip>("ExampleSong");
            musicSource.playOnAwake = false;
        }

        private void Update()
        {
            // tempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat));

            secPerBeat = SecondsPerBeat();

            musicSource.mute = muted;

            if (isPlaying)
            {
                var dt = Time.unscaledDeltaTime * musicSource.pitch;
                time += dt;

                songPos = time;
                songPosition = songPos;

                songPosBeat += dt / secPerBeat;
                songPositionInBeats = songPosBeat;
            }

            if (LoopRange.Enabled)
            {
                if (songPosition > LoopRange.Value.y)
                {
                    Play(LoopRange.Value.x, true);
                }
            }
        }

        /// <summary>
        /// The proper way of setting the beat of the Conductor.
        /// </summary>
        /// <param name="beat">Beat to skip to.</param>
        public void SetBeat(float beat)
        {
            float secFromBeat = GetSongPosFromBeat(beat);

            if (musicSource.clip != null)
            {
                if (secFromBeat < musicSource.clip.length)
                    musicSource.time = secFromBeat;
                else
                    if (beat < 0)
                    musicSource.time = 0;
                else
                    musicSource.time = 0;
            }

            songPos = secFromBeat;
            songPosition = songPos;
            songPosBeat = beat;
            songPositionInBeats = songPosBeat;
        }

        /// <summary>
        /// Plays the Conductor at a certain beat.
        /// </summary>
        /// <param name="beat">Beat to start at.</param>
        public void Play(float beat = 0.0f, bool secs = false)
        {
            bool negativeOffset = firstBeatOffset < 0f;
            bool negativeStartTime = false;

            var startPos = GetSongPosFromBeat(beat);
            if (secs)
                startPos = beat;

            if (negativeOffset)
            {
                time = startPos;
            }
            else
            {
                negativeStartTime = startPos - firstBeatOffset < 0f;

                if (negativeStartTime)
                    time = startPos - firstBeatOffset;
                else
                    time = startPos;
            }

            songPosBeat = time / secPerBeat;

            isPlaying = true;
            isPaused = false;

            if (SongPosLessThanClipLength(startPos))
            {
                if (negativeOffset)
                {
                    var musicStartTime = startPos + firstBeatOffset;

                    if (musicStartTime < 0f)
                    {
                        musicSource.time = startPos;
                        musicSource.PlayScheduled(AudioSettings.dspTime - firstBeatOffset / musicSource.pitch);
                    }
                    else
                    {
                        musicSource.time = musicStartTime;
                        musicSource.PlayScheduled(AudioSettings.dspTime);
                    }
                }
                else
                {
                    if (negativeStartTime)
                    {
                        musicSource.time = startPos;
                    }
                    else
                    {
                        musicSource.time = startPos + firstBeatOffset;
                    }

                    musicSource.PlayScheduled(AudioSettings.dspTime);
                }
            }

            musicSource.Play();
        }

        /// <summary>
        /// Pauses the Conductor.
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
            isPaused = true;

            musicSource.Pause();
        }

        /// <summary>
        /// Stops the Conductor.
        /// </summary>
        /// <param name="time"></param>
        public void Stop(float time)
        {
            this.time = time;

            songPosBeat = 0;
            songPositionInBeats = 0;

            isPlaying = false;
            isPaused = false;

            musicSource.Stop();
        }

        /// <summary>
        /// Reports a beat to the Conductor.
        /// </summary>
        public bool ReportBeat(ref float lastReportedBeat, float offset = 0, bool shiftBeatToOffset = false)
        {
            bool result = songPosition > (lastReportedBeat + offset) + secPerBeat;
            if (result == true)
            {
                lastReportedBeat = (songPosition - (songPosition % secPerBeat));

                if (!shiftBeatToOffset)
                    lastReportedBeat += offset;
            }
            return result;
        }

        /// <summary>
        /// Get the normalized position of a loop from beats.
        /// </summary>
        public float GetLoopPositionFromBeat(float beatOffset, float length)
        {
            return Mathf.Repeat((songPositionInBeats / length) + beatOffset, 1);
        }

        /// <summary>
        /// Get the normalized position of a loop from time.
        /// </summary>
        public float GetLoopPositionFromTime(float timeOffset, float length)
        {
            return Mathf.Repeat((songPosition / length) + timeOffset, 1);
        }

        /// <summary>
        /// Get the normalized position from a beat to length. (For example on Beat 3 between a startBeat of 2 and a length of 2, the position will be 0.5)
        /// </summary>
        public float GetPositionFromBeat(float startBeat, float length)
        {
            return MathHelper.Normalize(songPositionInBeats, startBeat, startBeat + length);
        }

        /// <summary>
        /// Get the normalized position from time to length. (For example on Second 3 between a startTime of 2 and a length of 2, the position will be 0.5)
        /// </summary>
        public float GetPositionFromTime(float startTime, float length)
        {
            return MathHelper.Normalize(songPosition, startTime, startTime + length);
        }

        /// <summary>
        /// Gets the normalized position from the target beat minus the margin. (For example if your target beat is 4 and your margin is 1, while your beat position is 3.5, this will return 0.5)
        /// </summary>
        public float GetPositionFromMargin(float targetBeat, float margin)
        {
            return GetPositionFromBeat(targetBeat - margin, margin);
        }

        /// <summary>
        /// Uses the tempo changes to accurately get what the song position would be at this beat.
        /// </summary>
        public float GetSongPosFromBeat(float beat)
        {
            float counter = 0.0f;
            float lastTempoChangeBeat = 0.0f;

            for (int i = 0; i < tempoChanges.Count; i++)
            {
                var tempoChange = tempoChanges[i];

                if (tempoChange.beat > beat)
                    break;

                counter += (tempoChange.beat - lastTempoChangeBeat) * secPerBeat;

                lastTempoChangeBeat = tempoChange.beat;
            }

            counter += (beat - lastTempoChangeBeat) * secPerBeat;

            return counter;
        }

        /// <summary>
        /// Uses the tempo changes to accurately get what the beat position would be at this second.
        /// </summary>
        public float GetBeatFromSongPos(float seconds)
        {
            float lastTempoChange = 0.0f;
            float lastTempo = initialTempo;
            float counterSeconds = -firstBeatOffset;

            for (int i = 0; i < tempoChanges.Count; i++)
            {
                var tempoChange = tempoChanges[i];

                float beatToNext = tempoChange.beat - lastTempoChange;
                float secToNext = BeatsToSecs(beatToNext, lastTempo);
                float nextSecs = counterSeconds + secToNext;

                if (nextSecs >= seconds)
                    break;

                lastTempoChange = tempoChange.beat;
                lastTempo = tempoChange.tempo;
                counterSeconds = nextSecs;
            }

            return lastTempoChange + SecsToBeats(seconds - counterSeconds, lastTempo);
        }

        /// <summary>
        /// Sets the volume of the Music Source.
        /// </summary>
        public void SetVolume(int percent)
        {
            musicSource.volume = percent / 100f;
        }

        /// <summary>
        /// Gets the length of the song in beats.
        /// </summary>
        public float SongLengthInBeats()
        {
            if (!musicSource.clip) return 0;
            return musicSource.clip.length / secPerBeat;
        }

        /// <summary>
        /// Converts beats to realtime seconds. (General help function, Conductor parameters have nothing to do with what this returns)
        /// </summary>
        public static float BeatsToSecs(float beats, float tempo)
        {
            return beats / tempo * 60.0f;
        }

        /// <summary>
        /// Converts seconds to beats. (General help function, Conductor parameters have nothing to do with what this returns)
        /// </summary>
        public static float SecsToBeats(float secs, float tempo)
        {
            return secs / 60.0f * tempo;
        }

        /// <summary>
        /// Checks if the time is less than or equal to the length of the song.
        /// </summary>
        public bool SongPosLessThanClipLength(float t)
        {
            if (musicSource.clip != null)
                return t < musicSource.clip.length;
            else
                return false;
        }

        /// <summary>
        /// Returns true if the song is currently playing or paused.
        /// </summary>
        public bool NotStopped()
        {
            return isPlaying == true || isPaused == true;
        }


        /// <summary>
        /// Gets the current seconds per beat.
        /// </summary>
        private float SecondsPerBeat()
        {
            var tempo = initialTempo;
            float lastTempo = tempo;

            for (int i = 0; i < tempoChanges.Count; i++)
            {
                var tempoChange = tempoChanges[i];
                var normalized = GetPositionFromBeat(tempoChange.beat, tempoChange.length);

                if (tempoChange.beat <= songPositionInBeats)
                {
                    tempo = Mathf.Lerp(lastTempo, tempoChange.tempo, normalized);
                    lastTempo = tempoChange.tempo;
                }
            }

            return 60.0f / tempo;
        }

        /// <summary>
        /// Toggle that mutes and un-mutes the song.
        /// </summary>
        public void Mute()
        {
            muted = (muted) ? muted = false : muted = true;
        }
    }
}