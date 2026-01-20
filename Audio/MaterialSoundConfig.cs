using System.Collections.Generic;

namespace Game3.Audio
{
    /// <summary>
    /// JSON configuration model for material sounds.
    /// Used to define fallbacks and aliases for the MaterialSoundRegistry.
    ///
    /// Example JSON:
    /// {
    ///   "fallbacks": {
    ///     "Cloth": "WoodIndoor",
    ///     "WoodIndoor": "Concrete"
    ///   },
    ///   "aliases": {
    ///     "tile": "Concrete",
    ///     "carpet": "Cloth",
    ///     "stone": "Concrete"
    ///   }
    /// }
    /// </summary>
    public class MaterialSoundConfig
    {
        /// <summary>
        /// Fallback materials: when no sounds exist for a material,
        /// use the sounds from the fallback material.
        /// Key = source material name, Value = fallback material name.
        /// </summary>
        public Dictionary<string, string> Fallbacks { get; set; }

        /// <summary>
        /// Material name aliases: allows using custom names for materials.
        /// Key = alias name, Value = actual MaterialType name.
        /// </summary>
        public Dictionary<string, string> Aliases { get; set; }
    }
}
