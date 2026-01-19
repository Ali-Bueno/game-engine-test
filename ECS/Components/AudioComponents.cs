using Game3.Audio;

namespace Game3.ECS.Components
{
    /// <summary>
    /// Reference to an OpenAL audio source.
    /// </summary>
    public struct AudioSourceRef
    {
        /// <summary>
        /// The AudioSource wrapper from AudioManager.
        /// </summary>
        public AudioSource Source;

        /// <summary>
        /// Whether this is a 3D positional sound.
        /// </summary>
        public bool Is3D;

        public AudioSourceRef(AudioSource source, bool is3D)
        {
            Source = source;
            Is3D = is3D;
        }
    }

    /// <summary>
    /// Footstep sounds for walking entities.
    /// </summary>
    public struct FootstepSounds
    {
        /// <summary>
        /// Sound folder path for normal footsteps.
        /// </summary>
        public string NormalSoundFolder;

        /// <summary>
        /// Sound folder path for stair footsteps.
        /// </summary>
        public string StairSoundFolder;

        /// <summary>
        /// Number of sound variations for normal steps.
        /// </summary>
        public int NormalSoundCount;

        /// <summary>
        /// Number of sound variations for stair steps.
        /// </summary>
        public int StairSoundCount;

        /// <summary>
        /// Time between footsteps in seconds.
        /// </summary>
        public float Interval;

        /// <summary>
        /// Volume of footstep sounds.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Current timer for footstep scheduling.
        /// </summary>
        public float Timer;

        /// <summary>
        /// Whether currently on stairs (affects sound selection).
        /// </summary>
        public bool IsOnStair;
    }

    /// <summary>
    /// Marks an entity as the audio listener (usually the player).
    /// </summary>
    public struct AudioListener
    {
        /// <summary>
        /// Height offset for ears (from position Z).
        /// </summary>
        public float EarHeight;

        public AudioListener(float earHeight)
        {
            EarHeight = earHeight;
        }
    }

    /// <summary>
    /// Ambient sound source that plays continuously - pure ECS component.
    /// </summary>
    public struct AmbientSound
    {
        /// <summary>
        /// Path to the sound file.
        /// </summary>
        public string SoundPath;

        /// <summary>
        /// Volume of the sound.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Whether the sound loops.
        /// </summary>
        public bool Looping;

        /// <summary>
        /// Reference to the AudioSource wrapper (created by AmbientSoundSystem).
        /// </summary>
        public AudioSource Source;

        /// <summary>
        /// Whether the sound has been initialized.
        /// </summary>
        public bool IsInitialized;
    }
}
