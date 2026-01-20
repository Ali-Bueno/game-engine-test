# Game3 - Audio Game Engine con Raytracing de Sonido

## Documentación de vaudio
La documentación completa del SDK de vaudio está en la carpeta local:
```
alisson_da_silva_bueno_raytraced_audio/guides/
```

Archivos disponibles:
- `getting started.txt` - Inicio rápido
- `full code example.txt` - Ejemplo completo
- `creating a raitrace context.txt` - Configuración del contexto
- `primitives.txt` - Tipos de primitivas (PrismPrimitive, SpherePrimitive, etc.)
- `materials.txt` - Materiales y sus propiedades acústicas
- `voices.txt` - Sistema de voces (fuentes de sonido)
- `reverb.txt` - Sistema de reverberación
- `trail based raitracing.txt` - Cómo funciona el raytracing
- `raitracing lifecycle.txt` - Ciclo de vida del raytracing
- `threading architecture.txt` - Arquitectura de hilos
- `air absortion.txt` - Absorción del aire
- `per voice reverb.txt` - Reverb por voz
- `custom eax formulas.txt` - Fórmulas EAX personalizadas
- `voice energy formula.txt` - Fórmula de energía de voz
- `updating raitrace settings.txt` - Actualizar configuración

## Resumen
**Engine para crear audiojuegos** (juegos de audio 3D para ciegos) usando MonoGame + vaudio (raytracing de sonido) + OpenAL Soft con HRTF.

El objetivo es proporcionar un framework completo para desarrollar juegos de audio con:
- Audio 3D realista con HRTF y raytracing
- Sistemas de navegación accesibles
- Herramientas para crear narrativas interactivas

## Stack Tecnológico
- **.NET 8.0** (x64)
- **MonoGame 3.8.4.1** - Framework de juego
- **Arch ECS 2.0.0** - Entity Component System
- **vaudio.dll** - Motor de raytracing de audio (SDK propietario)
- **Silk.NET.OpenAL** - Reproducción de audio con HRTF
- **OpenAL Soft** - DLL Win64 para HRTF (`OpenAL32.dll` en output)
- **NAudio/NVorbis** - Decodificación MP3/OGG
- **Tolk** - Soporte para lectores de pantalla

## Arquitectura ECS

El engine usa **Arch ECS** para separar datos de lógica. Toda la lógica de gameplay está en sistemas ECS.

### Componentes (`ECS/Components/`)
- **TransformComponents.cs**: `Position`, `Rotation`, `Scale`
- **PhysicsComponents.cs**: `Velocity`, `Gravity`, `CollisionShape`
- **AudioComponents.cs**: `AudioSourceRef`, `FootstepSounds`, `AudioListener`, `AmbientSound`
- **GameplayComponents.cs**: `PlayerControlled`, `PlayerMovement`, `DoorState`, `DoorAudio`, `Interactable`, `StairData`, `PlatformData`

### Sistemas (`ECS/Systems/`)
| Sistema | Responsabilidad |
|---------|-----------------|
| `PlayerInputSystem` | Procesa WASD, flechas, Espacio, E, C |
| `MovementSystem` | Aplica velocidad a posición |
| `GravitySystem` | Salto, caída, detección de suelo |
| `CollisionSystem` | Colisiones con paredes |
| `FootstepSystem` | Sonidos de pisadas |
| `AudioListenerSystem` | Actualiza posición del listener |
| `DoorInteractionSystem` | Detecta puertas cercanas, solicita toggle |
| `DoorSystem` | Abre/cierra puertas, gestiona primitivas vaudio |
| `AmbientSoundSystem` | Inicializa fuentes de sonido ambiental |

### Game Loop
```csharp
PlayerInputSystem → MovementSystem → GravitySystem → CollisionSystem
→ FootstepSystem → AudioListenerSystem → DoorInteractionSystem
→ DoorSystem → AmbientSoundSystem → AudioManager.Update()
```

### Recursos Compartidos (`ECS/SharedResources.cs`)
- `AudioManager` - Singleton para audio
- `StaticColliders` - Lista de colliders del mapa
- `Rooms` - Lista de habitaciones (para queries espaciales)
- `GetFloorHeight(pos)` - Consulta ECS para altura del suelo
- `IsOnStair(pos)` - Consulta ECS para detección de escaleras

### WorldBuilder (`ECS/WorldBuilder.cs`)
Crea entidades ECS puras:
```csharp
worldBuilder.CreatePlayer(position, angle);
worldBuilder.CreateDoor(position, size, side, roomName, soundFolder);
worldBuilder.CreateStair(startPos, length, width, height, direction);
worldBuilder.CreatePlatform(minX, minY, maxX, maxY, height);
worldBuilder.CreateAmbientSound(position, soundPath, looping, volume);
```

## Estructura de Carpetas
```
Game3/
├── Audio/
│   ├── AudioManager.cs      # Gestión de OpenAL, EFX reverb, vaudio (SINGLETON)
│   └── AudioSource.cs       # Fuente de audio 3D con raytracing
├── ECS/
│   ├── Components/
│   │   ├── TransformComponents.cs   # Position, Rotation, Scale
│   │   ├── PhysicsComponents.cs     # Velocity, Gravity, CollisionShape
│   │   ├── AudioComponents.cs       # AudioSourceRef, FootstepSounds, etc.
│   │   └── GameplayComponents.cs    # PlayerControlled, DoorState, StairData, etc.
│   ├── Systems/
│   │   ├── PlayerInputSystem.cs     # Input WASD/flechas/espacio
│   │   ├── MovementSystem.cs        # Aplica velocidad
│   │   ├── GravitySystem.cs         # Salto y caída
│   │   ├── CollisionSystem.cs       # Colisiones con paredes
│   │   ├── FootstepSystem.cs        # Sonidos de pisadas
│   │   ├── AudioListenerSystem.cs   # Actualiza listener
│   │   ├── DoorInteractionSystem.cs # Interacción con puertas (E)
│   │   ├── DoorSystem.cs            # Lógica de puertas
│   │   └── AmbientSoundSystem.cs    # Sonidos ambientales
│   ├── SharedResources.cs   # Recursos compartidos (AudioManager, colliders)
│   └── WorldBuilder.cs      # Crea entidades ECS desde datos
├── GameMap/
│   ├── Common.cs            # BoxCollider, Platform, enums
│   ├── GameMap.cs           # Mapa + DoorData/SoundSourceData structs
│   ├── GameRoom.cs          # Habitación con auto-construcción
│   ├── GameStair.cs         # Datos de escaleras para Build()
│   ├── MapRenderer.cs       # Renderizado 3D (usa ECS para queries)
│   └── ExampleMap.cs        # Ejemplo
├── alisson_da_silva_bueno_raytraced_audio/
│   ├── vaudio.dll           # Motor de raytracing (REQUERIDO)
│   ├── vaudio.license       # Licencia binaria (REQUERIDO)
│   ├── guides/              # Documentación del SDK
│   └── resource/            # Recursos de vaudio
├── references/tolk/         # DLLs de Tolk
├── bin/x86/Debug/net8.0/sounds/  # Archivos de sonido
├── Game1.cs                 # Clase principal (orquesta ECS)
├── Input.cs                 # Gestión de entrada
└── Program.cs               # Entry point y logging
```

## Clases Principales

### AudioManager (`Audio/AudioManager.cs`)
Gestiona todo el sistema de audio:
- **OpenAL Soft**: Precarga `OpenAL32.dll` para HRTF
- **EFX Reverb**: Efectos de reverberación dinámicos
- **vaudio RaytracingContext**: Motor de raytracing
- **World Bounds Dinámicos**: Se calculan automáticamente según el mapa

Métodos clave:
- `AudioManager(enableDebugWindow)` - Constructor sin vaudio (bounds dinámicos)
- `InitializeVaudioWithBounds(min, max)` - Inicializa vaudio con bounds calculados
- `Play2D(path, loop, volume)` - Sonido 2D (UI)
- `Play3D(path, x, y, z, loop, volume)` - Sonido 3D posicional
- `UpdateListener(x, y, z, yaw)` - Actualizar posición del oyente
- `AddPrimitive(primitive)` - Añadir geometría para raytracing

### AudioSource (`Audio/AudioSource.cs`)
Fuente de sonido individual:
- Obtiene datos de raytracing de vaudio (oclusión, reverb)
- Aplica filtros EFX basados en materiales
- Actualiza posición 3D para HRTF

### GameMap (`GameMap/GameMap.cs`)
Clase principal que coordina todo el mapa:
- `CreateRoom(name, center, size)` - Crea una habitación
- `CreateDoor(room, openingId, soundFolder)` - Crea una puerta
- `CreateStair(start, length, width, height, direction)` - Crea escalera
- `AddSoundSource(position, path, loop, volume)` - Fuente de sonido (datos para ECS)
- `SetSpawnPoint(position, angle)` - Define posición inicial del jugador
- `CalculateBounds()` - Calcula world bounds automáticamente
- `Build()` - Construye geometría vaudio del mapa

### Constantes del Jugador (`WorldBuilder`)
```csharp
public const float PlayerHeight = 1.75f;  // Altura del jugador en metros
public const float EyeHeight = 1.65f;     // Altura de ojos/oídos
```

### MapRenderer (`GameMap/MapRenderer.cs`)
Renderizador 3D para visualizar el mapa:
- **Vista en tercera persona** con cámara que sigue al jugador
- **Colisión de cámara** que evita atravesar paredes
- **Jugador humanoid** con cuerpo, cabeza, brazos, piernas
- **Puertas** con panel y pomo dorado (desaparecen al abrir)
- **Escaleras** con peldaños individuales y barandillas
- **Suelo con patrón** de baldosas ajedrezado
- **Paredes con sombreado** según orientación

Características visuales:
- Jugador: camisa azul, pantalones grises, piel, pelo marrón
- Flecha roja en el suelo indicando dirección
- Colores diferenciados por tipo de superficie
- Sin techo renderizado para mejor visibilidad

### Common (`GameMap/Common.cs`)
Tipos compartidos:
- `WallSide` - Enum: North, South, East, West
- `StairDirection` - Enum: North, South, East, West
- `BoxCollider` - Colisionador de caja con resolución de colisiones
- `Platform` - Área elevada para caminar

## Controles
| Tecla | Acción |
|-------|--------|
| W/S | Adelante/Atrás |
| A/D | Izquierda/Derecha (strafe) |
| ←/→ | Girar |
| Espacio | Saltar |
| E | Abrir/cerrar puerta |
| C | Hablar coordenadas |
| Esc | Salir |

## Uso del Sistema de Mapas

### Flujo de Inicialización (World Bounds Dinámicos)
```csharp
// 1. Crear AudioManager sin vaudio
audioManager = new AudioManager(enableDebugWindow: true);

// 2. Crear elementos del mapa
gameMap = ExampleMap.Create();

// 3. Calcular bounds automáticamente
var (min, max) = gameMap.CalculateBounds();

// 4. Inicializar vaudio con los bounds
audioManager.InitializeVaudioWithBounds(
    new Vector3F(min.X, min.Y, min.Z),
    new Vector3F(max.X, max.Y, max.Z));

// 5. Asignar AudioManager y construir
gameMap.SetAudioManager(audioManager);
gameMap.Build();
```

### Crear un Mapa
```csharp
var map = new GameMap();

// Crear habitaciones
var room1 = map.CreateRoom("Sala Pequeña",
    center: new Vector3(5f, 5f, 0f),
    size: new Vector3(10f, 10f, 5f))
    .AddDoorOpening("puerta1", WallSide.North, 0.5f, 2f);

var room2 = map.CreateRoom("Sala Grande",
    center: new Vector3(5f, 20f, 0f),
    size: new Vector3(20f, 15f, 5f))
    .AddDoorOpening("puerta1_sur", WallSide.South, 0.5f, 2f);

// Crear puerta
map.CreateDoor(room1, "puerta1", "sounds/doors/door1");

// Añadir escalera
map.CreateStair(
    startPosition: new Vector3(0f, 10f, 0f),
    length: 10f, width: 3f, heightChange: 4f,
    direction: StairDirection.North
).WithSideWalls();

// Añadir fuente de sonido
map.AddSoundSource(new Vector3(5f, 5f, 1.5f), "sounds/ambient.mp3", true, 0.5f);

// Definir spawn point (la entidad jugador se crea con WorldBuilder)
map.SetSpawnPoint(new Vector3(5f, 5f, 0f), angle: 0f);
```

## Sistema de Raytracing (vaudio)

### Materiales
Los valores de **Transmission** son en **dB/metro** (NO 0-1):
```csharp
// Configuración correcta de materiales
concrete.TransmissionLF = 100f;  // dB/m
concrete.TransmissionHF = 150f;  // dB/m
brick.TransmissionLF = 80f;      // dB/m
brick.TransmissionHF = 120f;     // dB/m
metal.TransmissionLF = 150f;     // dB/m
metal.TransmissionHF = 250f;     // dB/m
```

### Reverb Automático
El reverb se calcula automáticamente según la geometría:
- Salas pequeñas → decay corto, reverb íntimo
- Salas grandes → decay largo, reverb tipo catedral
- **IMPORTANTE**: Las habitaciones deben estar cerradas (suelo, techo, paredes)

## OpenAL y HRTF

### Requisitos HRTF
1. Copiar `soft_oal.dll` (Win64) como `OpenAL32.dll` al output
2. Crear `alsoft.ini` en output:
```ini
[general]
hrtf = true
stereo-mode = headphones
```

### Verificar HRTF
El log muestra:
```
HRTF extension present: True
HRTF status: Enabled
```

## Compilación
```bash
dotnet build
```
Output: `bin/x86/Debug/net8.0/Game3.dll`

## Logging
Todo se registra en `game_log.txt`:
```csharp
Program.Log("mensaje");
```

## Notas Importantes

### Sonidos
- Los sonidos 3D deben ser **MONO** (se convierten automáticamente)
- Formatos soportados: WAV, OGG, MP3
- Ruta de sonidos: `bin/x86/Debug/net8.0/sounds/`

### Coordenadas
- X = Este/Oeste
- Y = Norte/Sur
- Z = Arriba/Abajo
- Ángulo 0° = Norte (+Y)

### Performance
Si el raytracing es lento, reducir en `AudioManager.cs`:
```csharp
reverbRayCount = 512,    // Era 1024
occlusionRayCount = 256, // Era 512
```

### Debug Window de vaudio
Activar ventana de debug de vaudio:
```csharp
new AudioManager(enableDebugWindow: true);
```
**IMPORTANTE**: La ventana de debug de vaudio interfiere con el renderizado de MonoGame.
Si usas `MapRenderer` para visualización 3D, desactiva el debug:
```csharp
new AudioManager(enableDebugWindow: false);
```

## Problemas Conocidos

1. **HRTF no funciona**: Verificar que `OpenAL32.dll` sea la versión Win64 de OpenAL Soft
2. **Jugador atrapado**: Verificar colliders, reducir `PlayerRadius` si es necesario
3. **Sin reverb**: El mapa debe tener techo y paredes cerradas
4. **Sonido cortado**: Verificar que los archivos de sonido existen en la ruta correcta

## Problemas Resueltos

### Oclusión de sonido no funcionaba
**Causa**: Los valores de Transmission estaban en rango 0-1 cuando deberían ser dB/metro.
**Solución**: Usar valores como 80-250 dB/m según el material.

### Reverb no funcionaba en balcones/plataformas
**Causa**: El espacio no estaba cerrado (faltaban paredes).
**Solución**: Crear balcones como GameRoom completas con paredes.

### Pantalla negra con MapRenderer
**Causa**: La ventana de debug de vaudio (`enableDebugWindow: true`) interfiere con el renderizado de MonoGame.
**Solución**: Desactivar el debug de vaudio con `enableDebugWindow: false`.

### Cámara dentro de paredes
**Causa**: La cámara en tercera persona se posicionaba detrás del jugador sin verificar colisiones.
**Solución**: Implementado `ApplyCameraCollision()` en MapRenderer que acerca la cámara al jugador si hay paredes en medio.

---

## Roadmap - Próximas Funcionalidades

### 1. Sistema de Objetivos (Quests)
- `Objective`, `Quest`, `QuestManager`
- Tipos: ubicación, interacción, recolección, tiempo límite
- Eventos y persistencia

### 2. Carga Dinámica de Mapas
- Formato JSON para mapas
- `MapLoader`, `MapManager`
- Transiciones entre mapas

### 3. Geometría Avanzada
- Escaleras en L, U, espiral
- Salas circulares, poligonales
- Pasillos curvos

### 4. Sistema de Cutscenes
- Secuencias de audio en tiempo real
- Diálogos, movimiento de cámara, efectos
