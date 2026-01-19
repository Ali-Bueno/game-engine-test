using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Game3.Audio;
using vaudio;

namespace Game3
{
    /// <summary>
    /// Clase base para todos los objetos del mapa que tienen geometría para raytracing.
    /// Proporciona métodos comunes para añadir/quitar primitivas y colliders.
    /// </summary>
    public abstract class MapObject
    {
        protected Map map;
        protected Vector3 position;
        protected bool isActive = true;

        // Primitivas de raytracing (para poder añadir/quitar dinámicamente)
        protected List<PrismPrimitive> primitives = new List<PrismPrimitive>();

        // Colliders asociados
        protected List<BoxCollider> colliders = new List<BoxCollider>();

        public Vector3 Position => position;
        public bool IsActive => isActive;

        protected MapObject(Map map, Vector3 position)
        {
            this.map = map;
            this.position = position;
        }

        /// <summary>
        /// Añade una primitiva al raytracing y la guarda para poder quitarla después
        /// </summary>
        protected PrismPrimitive AddPrimitive(Vector3 pos, Vector3 size, MaterialType material)
        {
            var primitive = new PrismPrimitive()
            {
                material = material,
                size = new Vector3F(size.X, size.Y, size.Z),
                transform = Matrix4F.CreateTranslation(pos.X, pos.Y, pos.Z)
            };
            map.AudioManager.AddPrimitive(primitive);
            primitives.Add(primitive);
            return primitive;
        }

        /// <summary>
        /// Añade un collider y lo guarda
        /// </summary>
        protected BoxCollider AddCollider(Vector3 pos, Vector3 size)
        {
            var collider = new BoxCollider(pos, size);
            map.Colliders.Add(collider);
            colliders.Add(collider);
            return collider;
        }

        /// <summary>
        /// Quita todas las primitivas del raytracing
        /// </summary>
        protected void RemoveAllPrimitives()
        {
            foreach (var primitive in primitives)
            {
                map.AudioManager.RemovePrimitive(primitive);
            }
        }

        /// <summary>
        /// Añade todas las primitivas al raytracing
        /// </summary>
        protected void AddAllPrimitives()
        {
            foreach (var primitive in primitives)
            {
                map.AudioManager.AddPrimitive(primitive);
            }
        }

        /// <summary>
        /// Quita todos los colliders
        /// </summary>
        protected void RemoveAllColliders()
        {
            foreach (var collider in colliders)
            {
                map.Colliders.Remove(collider);
            }
        }

        /// <summary>
        /// Añade todos los colliders
        /// </summary>
        protected void AddAllColliders()
        {
            foreach (var collider in colliders)
            {
                if (!map.Colliders.Contains(collider))
                    map.Colliders.Add(collider);
            }
        }

        /// <summary>
        /// Activa el objeto (añade geometría y colliders)
        /// </summary>
        public virtual void Activate()
        {
            if (isActive) return;
            isActive = true;
            AddAllPrimitives();
            AddAllColliders();
        }

        /// <summary>
        /// Desactiva el objeto (quita geometría y colliders)
        /// </summary>
        public virtual void Deactivate()
        {
            if (!isActive) return;
            isActive = false;
            RemoveAllPrimitives();
            RemoveAllColliders();
        }

        /// <summary>
        /// Actualización por frame (override en subclases si necesario)
        /// </summary>
        public virtual void Update(float deltaTime) { }

        /// <summary>
        /// Construye la geometría del objeto (llamar después de crear)
        /// </summary>
        public abstract void Build();
    }

    // ========================================
    // WINDOW - Ventana con cristal
    // ========================================
    public class Window : MapObject
    {
        private Vector3 size;
        private WallSide wallSide;
        private bool isOpen;
        private AudioSource ambientSound;
        private string ambientSoundPath;

        public bool IsOpen => isOpen;

        public Window(Map map, Vector3 position, Vector3 size, WallSide wallSide, string ambientSound = null)
            : base(map, position)
        {
            this.size = size;
            this.wallSide = wallSide;
            this.isOpen = false;
            this.ambientSoundPath = ambientSound;
        }

        public override void Build()
        {
            // El cristal usa material con alta transmisión (simula vidrio)
            // Nota: vaudio no tiene Glass, usamos un material con propiedades similares
            AddPrimitive(position, size, MaterialType.Metal);  // TODO: configurar transmisión alta

            // Las ventanas no tienen collider (no bloquean movimiento)

            // Sonido ambiente exterior (si se especifica)
            if (!string.IsNullOrEmpty(ambientSoundPath))
            {
                ambientSound = map.AudioManager.Play3D(ambientSoundPath, position.X, position.Y, position.Z, true);
                ambientSound?.SetVolume(0.3f);
            }

            Program.Log($"Window created at ({position.X}, {position.Y}, {position.Z})");
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            // Quitar cristal del raytracing (sonido pasa libremente)
            RemoveAllPrimitives();
            // Aumentar volumen del sonido exterior
            ambientSound?.SetVolume(0.8f);
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            // Poner cristal de vuelta
            AddAllPrimitives();
            // Reducir volumen del sonido exterior
            ambientSound?.SetVolume(0.3f);
        }
    }

    // ========================================
    // PILLAR - Columna/Pilar
    // ========================================
    public class Pillar : MapObject
    {
        private float radius;
        private float height;
        private int segments;
        private MaterialType material;

        public Pillar(Map map, Vector3 position, float radius, float height, MaterialType material = MaterialType.Concrete, int segments = 8)
            : base(map, position)
        {
            this.radius = radius;
            this.height = height;
            this.material = material;
            this.segments = segments;
        }

        public override void Build()
        {
            // Aproximar cilindro con prismas
            // Por simplicidad, usamos un prisma cuadrado/octogonal
            float side = radius * 1.41f; // Aproximación cuadrada

            AddPrimitive(
                new Vector3(position.X, position.Y, position.Z + height / 2),
                new Vector3(side, side, height),
                material
            );

            AddCollider(
                new Vector3(position.X, position.Y, position.Z + height / 2),
                new Vector3(side, side, height)
            );

            Program.Log($"Pillar created at ({position.X}, {position.Y}, {position.Z}), radius={radius}, height={height}");
        }
    }

    // ========================================
    // FURNITURE - Mueble genérico
    // ========================================
    public class Furniture : MapObject
    {
        private Vector3 size;
        private MaterialType material;
        private bool hasCollision;

        public Furniture(Map map, Vector3 position, Vector3 size, MaterialType material = MaterialType.Brick, bool hasCollision = true)
            : base(map, position)
        {
            this.size = size;
            this.material = material;
            this.hasCollision = hasCollision;
        }

        public override void Build()
        {
            AddPrimitive(position, size, material);

            if (hasCollision)
            {
                AddCollider(position, size);
            }

            Program.Log($"Furniture created at ({position.X}, {position.Y}, {position.Z}), size=({size.X}, {size.Y}, {size.Z})");
        }
    }

    // ========================================
    // SOUNDEMITTER - Emisor de sonido sin geometría visible
    // ========================================
    public class SoundEmitter : MapObject
    {
        private string soundPath;
        private bool looping;
        private float volume;
        private AudioSource audioSource;

        public AudioSource AudioSource => audioSource;

        public SoundEmitter(Map map, Vector3 position, string soundPath, bool looping = true, float volume = 1.0f)
            : base(map, position)
        {
            this.soundPath = soundPath;
            this.looping = looping;
            this.volume = volume;
        }

        public override void Build()
        {
            audioSource = map.AudioManager.Play3D(soundPath, position.X, position.Y, position.Z, looping, volume);
            Program.Log($"SoundEmitter created at ({position.X}, {position.Y}, {position.Z}), sound={soundPath}");
        }

        public void SetVolume(float vol)
        {
            volume = vol;
            audioSource?.SetVolume(vol);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            audioSource?.Stop();
        }

        public override void Activate()
        {
            base.Activate();
            audioSource?.Play();
        }
    }

    // ========================================
    // PLATFORM - Plataforma elevada
    // ========================================
    public class PlatformObject : MapObject
    {
        private Vector3 size;
        private MaterialType material;
        private bool addToPlayerPlatforms;

        public PlatformObject(Map map, Vector3 position, Vector3 size, MaterialType material = MaterialType.Concrete, bool addToPlayerPlatforms = true)
            : base(map, position)
        {
            this.size = size;
            this.material = material;
            this.addToPlayerPlatforms = addToPlayerPlatforms;
        }

        public override void Build()
        {
            // Suelo de la plataforma
            AddPrimitive(position, size, material);

            // Añadir a la lista de plataformas del jugador si es necesario
            if (addToPlayerPlatforms)
            {
                map.Platforms.Add(new Platform(
                    position.X - size.X / 2,
                    position.Y - size.Y / 2,
                    position.X + size.X / 2,
                    position.Y + size.Y / 2,
                    position.Z + size.Z / 2
                ));
            }

            Program.Log($"Platform created at ({position.X}, {position.Y}, {position.Z}), size=({size.X}, {size.Y}, {size.Z})");
        }
    }

    // ========================================
    // WALL SEGMENT - Segmento de pared independiente
    // ========================================
    public class WallSegment : MapObject
    {
        private Vector3 size;
        private MaterialType material;

        public WallSegment(Map map, Vector3 position, Vector3 size, MaterialType material = MaterialType.Brick)
            : base(map, position)
        {
            this.size = size;
            this.material = material;
        }

        public override void Build()
        {
            AddPrimitive(position, size, material);
            AddCollider(position, size);
            Program.Log($"WallSegment created at ({position.X}, {position.Y}, {position.Z})");
        }
    }

    // ========================================
    // RAILING - Barandilla
    // ========================================
    public class Railing : MapObject
    {
        private Vector3 size;
        private MaterialType material;

        public Railing(Map map, Vector3 position, Vector3 size, MaterialType material = MaterialType.Metal)
            : base(map, position)
        {
            this.size = size;
            this.material = material;
        }

        public override void Build()
        {
            // Las barandillas tienen geometría pero collider más pequeño (solo bloquean a altura de cintura)
            AddPrimitive(position, size, material);
            AddCollider(position, size);
            Program.Log($"Railing created at ({position.X}, {position.Y}, {position.Z})");
        }
    }
}
