using Game3.Audio;
using vaudio;

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
    /// Now uses MaterialSoundRegistry for dynamic material-based sounds.
    /// </summary>
    public struct FootstepSounds
    {
        /// <summary>
        /// Current material type detected by position (from SurfaceMaterial entities).
        /// </summary>
        public MaterialType CurrentMaterial;

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
        /// Whether currently on stairs (affects sound selection and interval).
        /// </summary>
        public bool IsOnStair;

        /// <summary>
        /// Multiplier for footstep interval when on stairs (slower steps).
        /// </summary>
        public float StairIntervalMultiplier;
    }

    /// <summary>
    /// Defines a surface area with a specific material for footstep sounds.
    /// Used to detect which material the player is walking on.
    /// </summary>
    public struct SurfaceMaterial
    {
        /// <summary>
        /// The material type of this surface.
        /// </summary>
        public MaterialType Material;

        /// <summary>
        /// Minimum X coordinate of the surface bounds.
        /// </summary>
        public float MinX;

        /// <summary>
        /// Minimum Y coordinate of the surface bounds.
        /// </summary>
        public float MinY;

        /// <summary>
        /// Maximum X coordinate of the surface bounds.
        /// </summary>
        public float MaxX;

        /// <summary>
        /// Maximum Y coordinate of the surface bounds.
        /// </summary>
        public float MaxY;

        /// <summary>
        /// Minimum Z (height) for this surface.
        /// </summary>
        public float MinZ;

        /// <summary>
        /// Maximum Z (height) for this surface.
        /// </summary>
        public float MaxZ;

        /// <summary>
        /// Priority for overlap resolution (higher priority wins).
        /// </summary>
        public int Priority;

        /// <summary>
        /// Checks if a position is within this surface's bounds.
        /// </summary>
        public bool Contains(float x, float y, float z)
        {
            return x >= MinX && x <= MaxX &&
                   y >= MinY && y <= MaxY &&
                   z >= MinZ && z <= MaxZ;
        }
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
