using Microsoft.Xna.Framework;

namespace Game3.ECS.Components
{
    /// <summary>
    /// Moving platform that travels between two positions.
    /// </summary>
    public struct MovingPlatform
    {
        /// <summary>
        /// Starting position.
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// Ending position.
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// Movement speed in units per second.
        /// </summary>
        public float Speed;

        /// <summary>
        /// Current progress (0 = at Start, 1 = at End).
        /// </summary>
        public float Progress;

        /// <summary>
        /// Whether the platform ping-pongs (true) or loops (false).
        /// </summary>
        public bool PingPong;

        /// <summary>
        /// Whether the platform is currently moving forward (towards End).
        /// </summary>
        public bool MovingForward;

        /// <summary>
        /// Whether the platform is currently moving.
        /// </summary>
        public bool IsMoving;

        /// <summary>
        /// Whether to reverse direction (set by SwitchSystem).
        /// </summary>
        public bool IsReversing;

        /// <summary>
        /// Wait time at each end point.
        /// </summary>
        public float WaitTime;

        /// <summary>
        /// Current wait timer.
        /// </summary>
        public float WaitTimer;

        /// <summary>
        /// Sound to play while moving.
        /// </summary>
        public string MoveSound;

        /// <summary>
        /// Sound to play when stopping.
        /// </summary>
        public string StopSound;

        public MovingPlatform(Vector3 start, Vector3 end, float speed, bool pingPong = true)
        {
            Start = start;
            End = end;
            Speed = speed;
            Progress = 0f;
            PingPong = pingPong;
            MovingForward = true;
            IsMoving = false;
            IsReversing = false;
            WaitTime = 0f;
            WaitTimer = 0f;
            MoveSound = null;
            StopSound = null;
        }

        /// <summary>
        /// Gets the current world position of the platform.
        /// </summary>
        public Vector3 CurrentPosition => Vector3.Lerp(Start, End, Progress);
    }

    /// <summary>
    /// Platform that appears and disappears on a timer.
    /// </summary>
    public struct DisappearingPlatform
    {
        /// <summary>
        /// Time the platform stays visible.
        /// </summary>
        public float VisibleDuration;

        /// <summary>
        /// Time the platform stays invisible.
        /// </summary>
        public float InvisibleDuration;

        /// <summary>
        /// Current timer counting down.
        /// </summary>
        public float Timer;

        /// <summary>
        /// Whether the platform is currently visible/solid.
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// Time before disappearing after player steps on it (0 = uses regular timer).
        /// </summary>
        public float CollapseDelay;

        /// <summary>
        /// Whether player is currently on this platform.
        /// </summary>
        public bool PlayerOnPlatform;

        /// <summary>
        /// Whether the platform is collapsing (triggered by player).
        /// </summary>
        public bool IsCollapsing;

        /// <summary>
        /// Sound to play when appearing.
        /// </summary>
        public string AppearSound;

        /// <summary>
        /// Sound to play when disappearing.
        /// </summary>
        public string DisappearSound;

        public DisappearingPlatform(float visibleDuration, float invisibleDuration)
        {
            VisibleDuration = visibleDuration;
            InvisibleDuration = invisibleDuration;
            Timer = visibleDuration;
            IsVisible = true;
            CollapseDelay = 0f;
            PlayerOnPlatform = false;
            IsCollapsing = false;
            AppearSound = null;
            DisappearSound = null;
        }
    }

    /// <summary>
    /// Falling platform that drops when stepped on.
    /// </summary>
    public struct FallingPlatform
    {
        /// <summary>
        /// Delay before falling after player steps on.
        /// </summary>
        public float FallDelay;

        /// <summary>
        /// Current fall delay timer.
        /// </summary>
        public float FallTimer;

        /// <summary>
        /// Whether player has triggered this platform to fall.
        /// </summary>
        public bool Triggered;

        /// <summary>
        /// Whether the platform is currently falling.
        /// </summary>
        public bool IsFalling;

        /// <summary>
        /// Fall speed (gravity).
        /// </summary>
        public float FallSpeed;

        /// <summary>
        /// Whether to respawn after falling.
        /// </summary>
        public bool Respawns;

        /// <summary>
        /// Time to respawn after falling off screen.
        /// </summary>
        public float RespawnDelay;

        /// <summary>
        /// Original position for respawning.
        /// </summary>
        public Vector3 OriginalPosition;

        public FallingPlatform(float fallDelay, float fallSpeed = 15f)
        {
            FallDelay = fallDelay;
            FallTimer = 0f;
            Triggered = false;
            IsFalling = false;
            FallSpeed = fallSpeed;
            Respawns = false;
            RespawnDelay = 3f;
            OriginalPosition = Vector3.Zero;
        }
    }

    /// <summary>
    /// Bouncy platform that launches the player upward.
    /// </summary>
    public struct BouncyPlatform
    {
        /// <summary>
        /// Bounce force applied to player.
        /// </summary>
        public float BounceForce;

        /// <summary>
        /// Sound to play on bounce.
        /// </summary>
        public string BounceSound;

        /// <summary>
        /// Cooldown between bounces.
        /// </summary>
        public float Cooldown;

        /// <summary>
        /// Current cooldown timer.
        /// </summary>
        public float CooldownTimer;

        public BouncyPlatform(float bounceForce)
        {
            BounceForce = bounceForce;
            BounceSound = null;
            Cooldown = 0.2f;
            CooldownTimer = 0f;
        }
    }
}
