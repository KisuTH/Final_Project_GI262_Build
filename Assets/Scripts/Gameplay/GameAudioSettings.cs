using System;
using UnityEngine;

namespace ColorShiftKnight.Gameplay
{
    public static class GameAudioSettings
    {
        public enum Channel { Main, Music, Effect }
        public static event Action Changed;

        public static float Get(Channel channel)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat("Audio." + channel, 1f));
        }

        public static void Set(Channel channel, float value)
        {
            PlayerPrefs.SetFloat("Audio." + channel, Mathf.Clamp01(value));
            AudioListener.volume = Get(Channel.Main);
            Changed?.Invoke();
        }

        public static void Save() { PlayerPrefs.Save(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() { Changed = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() { AudioListener.volume = Get(Channel.Main); }
    }
}
