using System.Collections.Generic;
using Arch.Core;
using Game3.Audio;
using Game3.GameMap;
using Game3.ECS.Components;

namespace Game3.ECS
{
    /// <summary>
    /// Shared resources accessible by all ECS systems.
    /// Contains singletons and data that doesn't belong to individual entities.
    /// </summary>
    public class SharedResources
    {
        /// <summary>
        /// AudioManager singleton - handles OpenAL, EFX reverb, and vaudio raytracing.
        /// </summary>
        public AudioManager AudioManager { get; }

        /// <summary>
        /// ECS World reference for queries.
        /// </summary>
        public World EcsWorld { get; set; }

        /// <summary>
        /// Static colliders from the map (walls, doors, etc.)
        /// Used by collision system to resolve player/entity collisions.
        /// </summary>
        public List<BoxCollider> StaticColliders { get; } = new List<BoxCollider>();

        /// <summary>
        /// Rooms for spatial queries (which room is player in).
        /// These remain as legacy objects since they're complex geometry builders.
        /// </summary>
        public List<GameRoom> Rooms { get; } = new List<GameRoom>();

        // Query descriptions for ECS lookups
        private QueryDescription stairQuery;
        private QueryDescription platformQuery;

        public SharedResources(AudioManager audioManager)
        {
            AudioManager = audioManager;
        }

        /// <summary>
        /// Initialize ECS queries after World is set.
        /// </summary>
        public void InitializeQueries()
        {
            if (EcsWorld != null)
            {
                stairQuery = new QueryDescription().WithAll<StairData>();
                platformQuery = new QueryDescription().WithAll<PlatformData>();
            }
        }

        /// <summary>
        /// Gets the floor height at a given position, considering stairs and platforms.
        /// Uses ECS queries for pure data lookup.
        /// </summary>
        public float GetFloorHeight(Microsoft.Xna.Framework.Vector3 position)
        {
            if (EcsWorld == null) return 0f;

            // Check stairs first (using ECS query)
            float foundHeight = -1f;
            EcsWorld.Query(in stairQuery, (ref StairData stair) =>
            {
                if (foundHeight < 0)
                {
                    float height = stair.GetHeightAtPosition(position);
                    if (height >= 0)
                        foundHeight = height;
                }
            });

            if (foundHeight >= 0)
                return foundHeight;

            // Then check platforms (using ECS query)
            EcsWorld.Query(in platformQuery, (ref PlatformData platform) =>
            {
                if (foundHeight < 0 && platform.Contains(position))
                {
                    foundHeight = platform.Height;
                }
            });

            return foundHeight >= 0 ? foundHeight : 0f;
        }

        /// <summary>
        /// Checks if a position is on any stair.
        /// </summary>
        public bool IsOnStair(Microsoft.Xna.Framework.Vector3 position)
        {
            if (EcsWorld == null) return false;

            bool onStair = false;
            EcsWorld.Query(in stairQuery, (ref StairData stair) =>
            {
                if (!onStair && stair.IsPositionOnStair(position))
                    onStair = true;
            });

            return onStair;
        }

        /// <summary>
        /// Gets the room at the given position, or null if not in any room.
        /// </summary>
        public GameRoom GetRoomAt(Microsoft.Xna.Framework.Vector3 position)
        {
            foreach (var room in Rooms)
            {
                if (room.Contains(position))
                    return room;
            }
            return null;
        }
    }
}
