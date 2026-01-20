using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using vaudio;

namespace Game3.Audio
{
    /// <summary>
    /// Categories of sounds associated with materials.
    /// </summary>
    public enum SoundCategory
    {
        Steps,      // Footstep sounds
        Impact,     // Impact/collision sounds
        Break,      // Destruction sounds
        Scrape      // Dragging/scraping sounds
    }

    /// <summary>
    /// Registry for material-based sounds with auto-discovery and fallback chain.
    /// Singleton pattern for global access.
    /// </summary>
    public class MaterialSoundRegistry
    {
        private static MaterialSoundRegistry _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static MaterialSoundRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new MaterialSoundRegistry();
                    }
                }
                return _instance;
            }
        }

        // Cache: MaterialType -> SoundCategory -> List of sound paths
        private Dictionary<MaterialType, Dictionary<SoundCategory, List<string>>> soundCache;

        // Fallback chain for materials without sounds
        private Dictionary<MaterialType, MaterialType> fallbacks;

        // Material name aliases (e.g., "tile" -> Concrete)
        private Dictionary<string, MaterialType> aliases;

        private Random random;
        private bool isInitialized;
        private string basePath;

        // Supported audio extensions
        private static readonly string[] SupportedExtensions = { ".wav", ".ogg", ".mp3" };

        private MaterialSoundRegistry()
        {
            soundCache = new Dictionary<MaterialType, Dictionary<SoundCategory, List<string>>>();
            fallbacks = new Dictionary<MaterialType, MaterialType>();
            aliases = new Dictionary<string, MaterialType>(StringComparer.OrdinalIgnoreCase);
            random = new Random();
            isInitialized = false;

            // Default fallback chain
            SetupDefaultFallbacks();
        }

        private void SetupDefaultFallbacks()
        {
            // Soft materials fall back to wood, then concrete
            fallbacks[MaterialType.Cloth] = MaterialType.WoodIndoor;
            fallbacks[MaterialType.WoodIndoor] = MaterialType.Concrete;

            // Hard materials fall back to concrete
            fallbacks[MaterialType.Brick] = MaterialType.Concrete;
            fallbacks[MaterialType.Metal] = MaterialType.Concrete;
        }

        /// <summary>
        /// Initializes the registry by scanning the sounds folder for material sounds.
        /// </summary>
        /// <param name="soundsBasePath">Base path to the sounds folder (e.g., "sounds")</param>
        public void Initialize(string soundsBasePath)
        {
            if (isInitialized)
            {
                Program.Log("MaterialSoundRegistry: Already initialized");
                return;
            }

            basePath = soundsBasePath;
            string materialsPath = Path.Combine(soundsBasePath, "materials");

            // Load configuration if exists
            string configPath = Path.Combine(materialsPath, "material_sounds.json");
            if (File.Exists(configPath))
            {
                LoadConfiguration(configPath);
            }

            // Auto-discover sounds
            if (Directory.Exists(materialsPath))
            {
                ScanMaterialFolders(materialsPath);
            }
            else
            {
                Program.Log($"MaterialSoundRegistry: Materials folder not found at {materialsPath}");
            }

            isInitialized = true;
            LogRegistryState();
        }

        private void LoadConfiguration(string configPath)
        {
            try
            {
                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<MaterialSoundConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config != null)
                {
                    // Load fallbacks
                    if (config.Fallbacks != null)
                    {
                        foreach (var kvp in config.Fallbacks)
                        {
                            if (TryParseMaterial(kvp.Key, out var fromMaterial) &&
                                TryParseMaterial(kvp.Value, out var toMaterial))
                            {
                                fallbacks[fromMaterial] = toMaterial;
                            }
                        }
                    }

                    // Load aliases
                    if (config.Aliases != null)
                    {
                        foreach (var kvp in config.Aliases)
                        {
                            if (TryParseMaterial(kvp.Value, out var material))
                            {
                                aliases[kvp.Key] = material;
                            }
                        }
                    }

                    Program.Log($"MaterialSoundRegistry: Loaded config from {configPath}");
                }
            }
            catch (Exception ex)
            {
                Program.Log($"MaterialSoundRegistry: Error loading config: {ex.Message}");
            }
        }

        private void ScanMaterialFolders(string materialsPath)
        {
            foreach (var materialDir in Directory.GetDirectories(materialsPath))
            {
                string materialName = Path.GetFileName(materialDir);

                if (!TryParseMaterial(materialName, out var materialType))
                {
                    // Check aliases
                    if (aliases.TryGetValue(materialName, out materialType))
                    {
                        Program.Log($"MaterialSoundRegistry: Using alias {materialName} -> {materialType}");
                    }
                    else
                    {
                        Program.Log($"MaterialSoundRegistry: Unknown material folder: {materialName}");
                        continue;
                    }
                }

                ScanCategoryFolders(materialDir, materialType);
            }
        }

        private void ScanCategoryFolders(string materialDir, MaterialType materialType)
        {
            if (!soundCache.ContainsKey(materialType))
            {
                soundCache[materialType] = new Dictionary<SoundCategory, List<string>>();
            }

            foreach (var categoryDir in Directory.GetDirectories(materialDir))
            {
                string categoryName = Path.GetFileName(categoryDir);

                if (!TryParseCategory(categoryName, out var category))
                {
                    Program.Log($"MaterialSoundRegistry: Unknown category folder: {categoryName}");
                    continue;
                }

                var sounds = ScanSoundFiles(categoryDir);
                if (sounds.Count > 0)
                {
                    soundCache[materialType][category] = sounds;
                    Program.Log($"MaterialSoundRegistry: Found {sounds.Count} {category} sounds for {materialType}");
                }
            }
        }

        private List<string> ScanSoundFiles(string folderPath)
        {
            var sounds = new List<string>();

            foreach (var extension in SupportedExtensions)
            {
                foreach (var file in Directory.GetFiles(folderPath, $"*{extension}"))
                {
                    sounds.Add(file);
                }
            }

            // Sort for consistent ordering
            sounds.Sort();
            return sounds;
        }

        /// <summary>
        /// Gets a random sound path for a material and category.
        /// Falls back through the fallback chain if no sounds found.
        /// </summary>
        /// <param name="material">The material type</param>
        /// <param name="category">The sound category</param>
        /// <returns>Path to a random sound, or null if none available</returns>
        public string GetRandomSound(MaterialType material, SoundCategory category)
        {
            var sounds = GetSoundsWithFallback(material, category);

            if (sounds == null || sounds.Count == 0)
                return null;

            int index = random.Next(sounds.Count);
            return sounds[index];
        }

        /// <summary>
        /// Gets all sound paths for a material and category.
        /// Falls back through the fallback chain if no sounds found.
        /// </summary>
        public List<string> GetSounds(MaterialType material, SoundCategory category)
        {
            return GetSoundsWithFallback(material, category);
        }

        private List<string> GetSoundsWithFallback(MaterialType material, SoundCategory category)
        {
            // Try to get sounds for this material
            if (soundCache.TryGetValue(material, out var categories) &&
                categories.TryGetValue(category, out var sounds) &&
                sounds.Count > 0)
            {
                return sounds;
            }

            // Try fallback material
            if (fallbacks.TryGetValue(material, out var fallbackMaterial))
            {
                return GetSoundsWithFallback(fallbackMaterial, category);
            }

            return null;
        }

        /// <summary>
        /// Checks if sounds are available for a material and category.
        /// </summary>
        public bool HasSounds(MaterialType material, SoundCategory category)
        {
            return GetSoundsWithFallback(material, category) != null;
        }

        /// <summary>
        /// Gets the number of sound variations available for a material and category.
        /// </summary>
        public int GetSoundCount(MaterialType material, SoundCategory category)
        {
            var sounds = GetSoundsWithFallback(material, category);
            return sounds?.Count ?? 0;
        }

        /// <summary>
        /// Manually registers a sound for a material and category.
        /// Useful for adding sounds at runtime or overriding auto-discovered sounds.
        /// </summary>
        public void RegisterSound(MaterialType material, SoundCategory category, string soundPath)
        {
            if (!soundCache.ContainsKey(material))
            {
                soundCache[material] = new Dictionary<SoundCategory, List<string>>();
            }

            if (!soundCache[material].ContainsKey(category))
            {
                soundCache[material][category] = new List<string>();
            }

            if (!soundCache[material][category].Contains(soundPath))
            {
                soundCache[material][category].Add(soundPath);
            }
        }

        /// <summary>
        /// Sets a custom fallback for a material type.
        /// </summary>
        public void SetFallback(MaterialType from, MaterialType to)
        {
            fallbacks[from] = to;
        }

        /// <summary>
        /// Registers an alias for material name parsing.
        /// </summary>
        public void RegisterAlias(string alias, MaterialType material)
        {
            aliases[alias] = material;
        }

        private bool TryParseMaterial(string name, out MaterialType material)
        {
            // Try direct enum parse (case-insensitive)
            if (Enum.TryParse<MaterialType>(name, true, out material))
            {
                return true;
            }

            // Try aliases
            if (aliases.TryGetValue(name, out material))
            {
                return true;
            }

            material = default;
            return false;
        }

        private bool TryParseCategory(string name, out SoundCategory category)
        {
            return Enum.TryParse<SoundCategory>(name, true, out category);
        }

        private void LogRegistryState()
        {
            Program.Log("=== MaterialSoundRegistry State ===");
            Program.Log($"  Initialized: {isInitialized}");
            Program.Log($"  Base path: {basePath}");
            Program.Log($"  Materials with sounds: {soundCache.Count}");

            foreach (var material in soundCache)
            {
                foreach (var category in material.Value)
                {
                    Program.Log($"    {material.Key}/{category.Key}: {category.Value.Count} sounds");
                }
            }

            Program.Log($"  Fallbacks configured: {fallbacks.Count}");
            foreach (var fb in fallbacks)
            {
                Program.Log($"    {fb.Key} -> {fb.Value}");
            }
        }
    }
}
