using UnityEngine;

namespace ColorShiftKnight.Gameplay
{
    // Add to a music/effect AudioSource to opt it into the corresponding volume control.
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioVolumeChannel : MonoBehaviour
    {
        [SerializeField] private GameAudioSettings.Channel channel = GameAudioSettings.Channel.Effect;
        private AudioSource source;
        private float originalVolume;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            originalVolume = source.volume;
        }

        private void OnEnable()
        {
            GameAudioSettings.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            GameAudioSettings.Changed -= Apply;
            source.volume = originalVolume;
        }

        private void Apply()
        {
            // Main gain is already applied globally by AudioListener.
            source.volume = originalVolume * (channel == GameAudioSettings.Channel.Main ? 1f : GameAudioSettings.Get(channel));
        }
    }
}
