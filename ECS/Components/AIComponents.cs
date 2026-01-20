using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Arch.Core;

namespace Game3.ECS.Components
{
    /// <summary>
    /// States for AI behavior.
    /// </summary>
    public enum AIState
    {
        Idle,           // Not doing anything, waiting
        Patrolling,     // Moving between patrol points
        Alerted,        // Heard something, investigating
        Pursuing,       // Actively chasing target
        Attacking,      // In attack range, attacking
        Fleeing,        // Running away from threat
        Returning       // Going back to patrol route
    }

    /// <summary>
    /// Tag component marking an entity as an enemy.
    /// </summary>
    public struct Enemy
    {
        /// <summary>
        /// Type/name of the enemy (e.g., "zombie", "guard", "monster").
        /// </summary>
        public string Type;

        /// <summary>
        /// Sound folder for this enemy type.
        /// </summary>
        public string SoundFolder;

        public Enemy(string type, string soundFolder = null)
        {
            Type = type;
            SoundFolder = soundFolder ?? $"sounds/enemies/{type}";
        }
    }

    /// <summary>
    /// AI agent component with detection and behavior settings.
    /// </summary>
    public struct AIAgent
    {
        /// <summary>
        /// Maximum sight range in units.
        /// </summary>
        public float SightRange;

        /// <summary>
        /// Field of view in degrees (half-angle from forward).
        /// </summary>
        public float FieldOfView;

        /// <summary>
        /// Maximum hearing range for sounds.
        /// </summary>
        public float HearingRange;

        /// <summary>
        /// Current AI state.
        /// </summary>
        public AIState State;

        /// <summary>
        /// Current target entity (usually player).
        /// </summary>
        public Entity Target;

        /// <summary>
        /// Whether target is currently valid.
        /// </summary>
        public bool HasTarget;

        /// <summary>
        /// Last known position of target (for investigation).
        /// </summary>
        public Vector3 LastKnownTargetPosition;

        /// <summary>
        /// Time since last saw target.
        /// </summary>
        public float TimeSinceLastSeen;

        /// <summary>
        /// Time to wait before giving up chase.
        /// </summary>
        public float GiveUpTime;

        /// <summary>
        /// Attack range.
        /// </summary>
        public float AttackRange;

        /// <summary>
        /// Attack damage.
        /// </summary>
        public float AttackDamage;

        /// <summary>
        /// Cooldown between attacks.
        /// </summary>
        public float AttackCooldown;

        /// <summary>
        /// Current attack cooldown timer.
        /// </summary>
        public float AttackTimer;

        public AIAgent(float sightRange = 15f, float hearingRange = 20f)
        {
            SightRange = sightRange;
            FieldOfView = 60f; // 120 degree cone
            HearingRange = hearingRange;
            State = AIState.Idle;
            Target = default;
            HasTarget = false;
            LastKnownTargetPosition = Vector3.Zero;
            TimeSinceLastSeen = 0f;
            GiveUpTime = 5f;
            AttackRange = 1.5f;
            AttackDamage = 10f;
            AttackCooldown = 1f;
            AttackTimer = 0f;
        }
    }

    /// <summary>
    /// Pathfinding component for navigating the map.
    /// </summary>
    public struct Pathfinder
    {
        /// <summary>
        /// Current path as list of waypoints.
        /// </summary>
        public List<Vector3> Path;

        /// <summary>
        /// Current waypoint index.
        /// </summary>
        public int CurrentWaypoint;

        /// <summary>
        /// Distance threshold to consider waypoint reached.
        /// </summary>
        public float WaypointThreshold;

        /// <summary>
        /// Whether pathfinding has a valid path.
        /// </summary>
        public bool HasPath => Path != null && Path.Count > 0 && CurrentWaypoint < Path.Count;

        /// <summary>
        /// Whether we've reached the end of the path.
        /// </summary>
        public bool ReachedDestination => Path != null && CurrentWaypoint >= Path.Count;

        /// <summary>
        /// Current waypoint position.
        /// </summary>
        public Vector3? CurrentTarget => HasPath ? Path[CurrentWaypoint] : null;

        public Pathfinder(float threshold = 0.5f)
        {
            Path = null;
            CurrentWaypoint = 0;
            WaypointThreshold = threshold;
        }

        /// <summary>
        /// Sets a new path.
        /// </summary>
        public void SetPath(List<Vector3> newPath)
        {
            Path = newPath;
            CurrentWaypoint = 0;
        }

        /// <summary>
        /// Advances to the next waypoint.
        /// </summary>
        public void NextWaypoint()
        {
            CurrentWaypoint++;
        }

        /// <summary>
        /// Clears the current path.
        /// </summary>
        public void ClearPath()
        {
            Path = null;
            CurrentWaypoint = 0;
        }
    }

    /// <summary>
    /// Patrol route for patrolling enemies.
    /// </summary>
    public struct PatrolRoute
    {
        /// <summary>
        /// List of patrol waypoints.
        /// </summary>
        public List<Vector3> Waypoints;

        /// <summary>
        /// Current waypoint index.
        /// </summary>
        public int CurrentIndex;

        /// <summary>
        /// Whether to loop or ping-pong the route.
        /// </summary>
        public bool Loop;

        /// <summary>
        /// Current direction (for ping-pong).
        /// </summary>
        public bool Forward;

        /// <summary>
        /// Wait time at each waypoint.
        /// </summary>
        public float WaitTime;

        /// <summary>
        /// Current wait timer.
        /// </summary>
        public float WaitTimer;

        public PatrolRoute(List<Vector3> waypoints, bool loop = true, float waitTime = 1f)
        {
            Waypoints = waypoints ?? new List<Vector3>();
            CurrentIndex = 0;
            Loop = loop;
            Forward = true;
            WaitTime = waitTime;
            WaitTimer = 0f;
        }

        /// <summary>
        /// Gets the current patrol target position.
        /// </summary>
        public Vector3? CurrentTarget => Waypoints != null && Waypoints.Count > 0
            ? Waypoints[CurrentIndex]
            : null;

        /// <summary>
        /// Advances to the next waypoint in the patrol route.
        /// </summary>
        public void NextWaypoint()
        {
            if (Waypoints == null || Waypoints.Count == 0) return;

            if (Loop)
            {
                CurrentIndex = (CurrentIndex + 1) % Waypoints.Count;
            }
            else
            {
                // Ping-pong
                if (Forward)
                {
                    CurrentIndex++;
                    if (CurrentIndex >= Waypoints.Count)
                    {
                        CurrentIndex = Waypoints.Count - 2;
                        Forward = false;
                    }
                }
                else
                {
                    CurrentIndex--;
                    if (CurrentIndex < 0)
                    {
                        CurrentIndex = 1;
                        Forward = true;
                    }
                }
            }
            WaitTimer = WaitTime;
        }
    }

    /// <summary>
    /// Movement settings for AI entities.
    /// </summary>
    public struct AIMovement
    {
        /// <summary>
        /// Normal movement speed.
        /// </summary>
        public float WalkSpeed;

        /// <summary>
        /// Speed when chasing/fleeing.
        /// </summary>
        public float RunSpeed;

        /// <summary>
        /// Rotation speed in degrees per second.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Whether currently running.
        /// </summary>
        public bool IsRunning;

        public AIMovement(float walkSpeed = 2f, float runSpeed = 4f)
        {
            WalkSpeed = walkSpeed;
            RunSpeed = runSpeed;
            RotationSpeed = 180f;
            IsRunning = false;
        }

        /// <summary>
        /// Gets the current movement speed.
        /// </summary>
        public float CurrentSpeed => IsRunning ? RunSpeed : WalkSpeed;
    }
}
