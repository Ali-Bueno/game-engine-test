using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Updates the audio listener position based on entities with AudioListener component.
    /// Typically attached to the player entity.
    /// </summary>
    public class AudioListenerSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription listenerQuery;
        private int logCounter = 0;

        public AudioListenerSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.listenerQuery = new QueryDescription()
                .WithAll<Position, Rotation, AudioListener>();
        }

        public void Update(GameTime gameTime)
        {
            world.Query(in listenerQuery, (ref Position pos, ref Rotation rot, ref AudioListener listener) =>
            {
                float listenerZ = pos.Z + listener.EarHeight;
                float yaw = rot.YawRadians;

                // Log every ~1 second (60 frames)
                logCounter++;
                if (logCounter >= 60)
                {
                    logCounter = 0;
                    Program.Log($"ECS Listener: X={pos.X:F1}, Y={pos.Y:F1}, Z={listenerZ:F1} (playerZ={pos.Z:F1})");
                }

                resources.AudioManager.UpdateListener(pos.X, pos.Y, listenerZ, yaw);
            });
        }
    }
}
