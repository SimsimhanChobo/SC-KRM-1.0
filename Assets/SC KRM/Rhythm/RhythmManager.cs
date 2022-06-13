using Newtonsoft.Json;
using SCKRM.Easing;
using SCKRM.Json;
using SCKRM.SaveLoad;
using SCKRM.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCKRM.Rhythm
{
    [AddComponentMenu("SC KRM/Rhythm/Rhythm Manager")]
    public sealed class RhythmManager : Manager<RhythmManager>
    {
        [GeneralSaveLoad]
        public sealed class SaveData
        {
            [JsonProperty] public static double screenOffset { get; set; } = 0;
            [JsonProperty] public static double soundOffset { get; set; } = 0;
        }

        public static Map map { get; private set; }
        public static SoundPlayerParent soundPlayer { get; private set; }



        public static bool isPlaying { get; private set; } = false;



        public static bool dropPart { get; set; } = false;



        public static double bpm { get; private set; }
        public static float bpmFpsDeltaTime { get; private set; }
        public static float bpmUnscaledFpsDeltaTime { get; private set; }



        public static float time => (float)(soundPlayer?.time);
        public static double currentBeat { get; private set; }
        public static double currentBeatSound { get; private set; }
        public static double currentBeatScreen { get; private set; }
        public static double currentBeat1Beat { get; private set; }

        static double bpmOffsetBeat;
        static double bpmOffsetTime;



        [Obsolete("Use currentBeat1Beat instead")] public static event Action oneBeat;
        [Obsolete("Use currentBeat1Beat instead")] public static event Action oneBeatDropPart;



        void Awake() => SingletonCheck(this);



        static int tempCurrentBeat = 0;
        void Update()
        {
            if (isPlaying)
            {
                if (soundPlayer == null || map == null)
                    Stop();
                else
                {
                    SetCurrentBeat();

                    {
                        bpm = map.effect.bpm.GetValue(currentBeat, out double beat, out bool isValueChanged);

                        if (isValueChanged)
                        {
                            BPMChange(bpm, beat);
                            SetCurrentBeat();
                        }
                    }

                    {
                        bpmFpsDeltaTime = (float)(bpm * 0.01f * Kernel.fpsDeltaTime * soundPlayer.speed);
                        bpmUnscaledFpsDeltaTime = (float)(bpm * 0.01f * Kernel.fpsUnscaledDeltaTime * soundPlayer.speed);
                    }

                    dropPart = map.effect.dropPart.GetValue();

                    if (tempCurrentBeat != (int)currentBeat && currentBeat >= 0)
                    {
                        oneBeat?.Invoke();
                        if (dropPart)
                            oneBeatDropPart?.Invoke();

                        tempCurrentBeat = (int)currentBeat;
                    }

                    /*Debug.Log("time: " + time);
                    Debug.Log("currentBeat: " + currentBeat);
                    Debug.Log("bpm: " + bpm);
                    Debug.Log("dropPart: " + dropPart);*/
                }
            }
        }

        static void SetCurrentBeat()
        {
            double soundTime = (double)time - map.info.offset - bpmOffsetTime;
            double bpmDivide60 = bpm / 60d;

            currentBeat = (soundTime * bpmDivide60) + bpmOffsetBeat;
            currentBeatSound = ((soundTime - SaveData.soundOffset) * bpmDivide60) + bpmOffsetBeat;
            currentBeatScreen = ((soundTime - SaveData.screenOffset) * bpmDivide60) + bpmOffsetBeat;

            if (currentBeat >= 0)
                currentBeat1Beat = currentBeat - (int)currentBeat;
            else
                currentBeat1Beat = 1 - ((int)currentBeat - currentBeat);
        }

        static void BPMChange(double bpm, double offsetBeat)
        {
            bpmOffsetBeat = offsetBeat;

            bpmOffsetTime = 0;
            double tempBeat = 0;
            for (int i = 0; i < map.effect.bpm.Count; i++)
            {
                if (map.effect.bpm[0].beat >= offsetBeat)
                    break;

                double tempBPM;
                if (i - 1 < 0)
                    tempBPM = map.effect.bpm[0].value;
                else
                    tempBPM = map.effect.bpm[i - 1].value;

                bpmOffsetTime += (map.effect.bpm[i].beat - tempBeat) * (60d / tempBPM);
                tempBeat = map.effect.bpm[i].beat;

                if (map.effect.bpm[i].beat >= offsetBeat)
                    break;
            }

            RhythmManager.bpm = bpm;
        }

        public static void Play(SoundPlayerParent soundPlayer, Map map)
        {
            currentBeat = 0;

            RhythmManager.soundPlayer = soundPlayer;
            RhythmManager.map = map;

            soundPlayer.timeChanged += SoundPlayerTimeChange;
            isPlaying = true;
        }

        public static void Stop()
        {
            currentBeat = 0;

            if (soundPlayer != null)
                soundPlayer.timeChanged -= SoundPlayerTimeChange;

            soundPlayer = null;
            map = null;

            isPlaying = false;
        }

        static void SoundPlayerTimeChange()
        {
            for (int i = 0; i < map.effect.bpm.Count; i++)
            {
                {
                    BeatValuePair<double> bpm = map.effect.bpm[i];
                    BPMChange(bpm.value, bpm.beat);
                    SetCurrentBeat();
                }

                if (bpmOffsetTime >= time)
                {
                    if (i - 1 >= 0)
                    {
                        BeatValuePair<double> bpm = map.effect.bpm[i - 1];
                        SetCurrentBeat();
                        BPMChange(bpm.value, bpm.beat);
                    }

                    break;
                }
            }
        }
    }
}