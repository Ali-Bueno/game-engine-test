using vaudio;

namespace Game3.ECS.Components
{
    /// <summary>
    /// Health component for entities that can take damage.
    /// </summary>
    public struct Health
    {
        /// <summary>
        /// Maximum health value.
        /// </summary>
        public float Max;

        /// <summary>
        /// Current health value.
        /// </summary>
        public float Current;

        /// <summary>
        /// Whether this entity is dead (Current <= 0).
        /// </summary>
        public bool IsDead => Current <= 0;

        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        /// <summary>
        /// Apply damage to this entity.
        /// </summary>
        public void TakeDamage(float amount)
        {
            Current = System.Math.Max(0, Current - amount);
        }

        /// <summary>
        /// Heal this entity.
        /// </summary>
        public void Heal(float amount)
        {
            Current = System.Math.Min(Max, Current + amount);
        }
    }

    /// <summary>
    /// Marks an entity as destructible with associated material for sounds.
    /// </summary>
    public struct Destructible
    {
        /// <summary>
        /// Material type for destruction sounds.
        /// </summary>
        public MaterialType Material;

        /// <summary>
        /// Whether this object has been broken.
        /// </summary>
        public bool IsBroken;

        /// <summary>
        /// Whether to remove the entity after breaking.
        /// </summary>
        public bool RemoveOnBreak;

        /// <summary>
        /// Volume for destruction sounds.
        /// </summary>
        public float SoundVolume;

        public Destructible(MaterialType material, bool removeOnBreak = true, float soundVolume = 1f)
        {
            Material = material;
            IsBroken = false;
            RemoveOnBreak = removeOnBreak;
            SoundVolume = soundVolume;
        }
    }

    /// <summary>
    /// Marks an entity for delayed removal from the world.
    /// </summary>
    public struct ToBeDestroyed
    {
        /// <summary>
        /// Time in seconds before the entity is removed.
        /// </summary>
        public float Delay;

        /// <summary>
        /// Current timer counting down to removal.
        /// </summary>
        public float Timer;

        public ToBeDestroyed(float delay)
        {
            Delay = delay;
            Timer = delay;
        }
    }

    /// <summary>
    /// Damage event component - added temporarily to apply damage.
    /// </summary>
    public struct DamageEvent
    {
        /// <summary>
        /// Amount of damage to apply.
        /// </summary>
        public float Amount;

        /// <summary>
        /// Source of the damage (optional).
        /// </summary>
        public Arch.Core.Entity? Source;

        /// <summary>
        /// Position where the damage occurred (for impact sounds).
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 ImpactPosition;

        public DamageEvent(float amount, Microsoft.Xna.Framework.Vector3 impactPosition)
        {
            Amount = amount;
            Source = null;
            ImpactPosition = impactPosition;
        }
    }
}
