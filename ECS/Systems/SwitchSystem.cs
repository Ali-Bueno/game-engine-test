using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Processes switch activations and performs actions on target entities.
    /// </summary>
    public class SwitchSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription switchQuery;

        // Track previous state to detect changes
        private System.Collections.Generic.Dictionary<Entity, bool> previousStates =
            new System.Collections.Generic.Dictionary<Entity, bool>();

        public SwitchSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.switchQuery = new QueryDescription()
                .WithAll<Position, Switch>()
                .WithNone<Disabled>();
        }

        public void Update(GameTime gameTime)
        {
            world.Query(in switchQuery, (Entity entity, ref Position pos, ref Switch sw) =>
            {
                // Get previous state
                bool wasActivated = previousStates.TryGetValue(entity, out bool prev) && prev;

                // Check for state change
                if (sw.IsActivated != wasActivated)
                {
                    if (sw.IsActivated)
                    {
                        // Just activated
                        OnActivated(ref sw, pos);
                        PerformAction(ref sw);
                    }
                    else
                    {
                        // Just deactivated (toggle switches only)
                        OnDeactivated(ref sw, pos);
                        if (sw.IsToggle)
                        {
                            PerformReverseAction(ref sw);
                        }
                    }
                }

                // Store current state
                previousStates[entity] = sw.IsActivated;
            });
        }

        private void OnActivated(ref Switch sw, Position pos)
        {
            if (!string.IsNullOrEmpty(sw.ActivateSound))
            {
                resources.AudioManager.Play3D(sw.ActivateSound, pos.X, pos.Y, pos.Z, false, 0.8f);
            }
            Program.Log($"Switch activated, action: {sw.Action}");
        }

        private void OnDeactivated(ref Switch sw, Position pos)
        {
            if (!string.IsNullOrEmpty(sw.DeactivateSound))
            {
                resources.AudioManager.Play3D(sw.DeactivateSound, pos.X, pos.Y, pos.Z, false, 0.8f);
            }
            Program.Log($"Switch deactivated, action: {sw.Action}");
        }

        private void PerformAction(ref Switch sw)
        {
            if (!sw.Target.IsAlive())
            {
                Program.Log("Switch target is not alive");
                return;
            }

            switch (sw.Action)
            {
                case SwitchAction.ToggleDoor:
                    if (sw.Target.Has<DoorState>())
                    {
                        ref var door = ref sw.Target.Get<DoorState>();
                        door.ToggleRequested = true;
                    }
                    break;

                case SwitchAction.MovePlatform:
                    if (sw.Target.Has<MovingPlatform>())
                    {
                        ref var platform = ref sw.Target.Get<MovingPlatform>();
                        platform.IsMoving = true;
                    }
                    break;

                case SwitchAction.EnableEntity:
                    if (sw.Target.Has<Disabled>())
                    {
                        sw.Target.Remove<Disabled>();
                    }
                    break;

                case SwitchAction.DisableEntity:
                    if (!sw.Target.Has<Disabled>())
                    {
                        sw.Target.Add(new Disabled());
                    }
                    break;

                case SwitchAction.TriggerEvent:
                    // Future: event system
                    break;

                case SwitchAction.PlaySound:
                    // Sound played in OnActivated
                    break;
            }
        }

        private void PerformReverseAction(ref Switch sw)
        {
            if (!sw.Target.IsAlive())
                return;

            switch (sw.Action)
            {
                case SwitchAction.ToggleDoor:
                    if (sw.Target.Has<DoorState>())
                    {
                        ref var door = ref sw.Target.Get<DoorState>();
                        door.ToggleRequested = true;
                    }
                    break;

                case SwitchAction.MovePlatform:
                    if (sw.Target.Has<MovingPlatform>())
                    {
                        ref var platform = ref sw.Target.Get<MovingPlatform>();
                        platform.IsReversing = true;
                    }
                    break;

                case SwitchAction.EnableEntity:
                    if (!sw.Target.Has<Disabled>())
                    {
                        sw.Target.Add(new Disabled());
                    }
                    break;

                case SwitchAction.DisableEntity:
                    if (sw.Target.Has<Disabled>())
                    {
                        sw.Target.Remove<Disabled>();
                    }
                    break;
            }
        }
    }
}
