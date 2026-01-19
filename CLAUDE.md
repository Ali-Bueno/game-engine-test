# Game3 - Audio Game Engine con Raytracing de Sonido

## Documentación y Referencias
- **vaudio SDK Docs**: https://docs.vercidium.com/raytraced-audio/Getting+Started
- **Vercidium Audio (sitio oficial)**: https://vercidium.com/audio
- **Vercidium Blog**: https://vercidium.com/blog/
- **OpenAL Soft (HRTF)**: https://openal-soft.org/
- **Godot Raytraced Audio (referencia)**: https://github.com/vercidium-patreon/godot_raytraced_audio
- **Godot OpenAL Plugin**: https://github.com/vercidium-patreon/godot_openal

### vaudio SDK - Documentación Detallada
- **Full Code Example**: https://docs.vercidium.com/raytraced-audio/Full+Code+Example
- **Creating a Raytracing Context**: https://docs.vercidium.com/raytraced-audio/Creating+a+Raytracing+Context
- **Updating Raytracing Settings**: https://docs.vercidium.com/raytraced-audio/Updating+Raytracing+Settings
- **Voices**: https://docs.vercidium.com/raytraced-audio/Voices
- **Primitives**: https://docs.vercidium.com/raytraced-audio/Primitives
- **Reverb**: https://docs.vercidium.com/raytraced-audio/Reverb
- **Materials**: https://docs.vercidium.com/raytraced-audio/Materials
- **Raytracing Lifecycle** (Advanced): https://docs.vercidium.com/raytraced-audio/Advanced/Raytracing+Lifecycle
- **Trail-Based Raytracing** (Advanced): https://docs.vercidium.com/raytraced-audio/Advanced/Trail-Based+Raytracing
- **Per-Voice Reverb** (Advanced): https://docs.vercidium.com/raytraced-audio/Advanced/Per-Voice+Reverb

## Resumen
**Engine para crear audiojuegos** (juegos de audio 3D para ciegos) usando MonoGame + vaudio (raytracing de sonido) + OpenAL Soft con HRTF.

El objetivo es proporcionar un framework completo para desarrollar juegos de audio con:
- Audio 3D realista con HRTF y raytracing
- Sistemas de navegación accesibles
- Herramientas para crear narrativas interactivas

## Stack Tecnológico
- **.NET 8.0** (x64)
- **MonoGame 3.8.4.1** - Framework de juego
- **vaudio.dll** - Motor de raytracing de audio (SDK propietario)
- **Silk.NET.OpenAL** - Reproducción de audio con HRTF
- **OpenAL Soft** - DLL Win64 para HRTF (`OpenAL32.dll` en output)
- **NAudio/NVorbis** - Decodificación MP3/OGG
- **Tolk** - Soporte para lectores de pantalla

## Estructura de Carpetas
```
Game3/
├── Audio/
│   ├── AudioManager.cs    # Gestión de OpenAL, EFX reverb, vaudio
│   └── AudioSource.cs     # Fuente de audio 3D con raytracing
├── alisson_da_silva_bueno_raytraced_audio/
│   ├── vaudio.dll         # Motor de raytracing (REQUERIDO)
│   ├── vaudio.license     # Licencia binaria (REQUERIDO)
│   └── resource/          # Recursos de vaudio (shaders, fonts)
├── references/tolk/       # DLLs de Tolk para screen readers
├── bin/Windows/x86/Debug/sounds/  # Archivos de sonido
├── Game1.cs               # Clase principal del juego
├── Map.cs                 # Construcción del mapa y geometría
├── Player.cs              # Movimiento, colisiones, pisadas
├── Door.cs                # Puertas interactivas
├── Stair.cs               # Escaleras automáticas
└── Program.cs             # Entry point y logging
```

## Clases Principales

### AudioManager (`Audio/AudioManager.cs`)
Gestiona todo el sistema de audio:
- **OpenAL Soft**: Precarga `OpenAL32.dll` para HRTF
- **EFX Reverb**: Efectos de reverberación dinámicos
- **vaudio RaytracingContext**: Motor de raytracing
- Métodos clave:
  - `Play2D(path, loop, volume)` - Sonido 2D (UI, pisadas)
  - `Play3D(path, x, y, z, loop, volume)` - Sonido 3D posicional
  - `UpdateListener(x, y, z, yaw)` - Actualizar posición del oyente
  - `AddPrimitive(primitive)` - Añadir geometría para raytracing

### AudioSource (`Audio/AudioSource.cs`)
Fuente de sonido individual:
- Obtiene datos de raytracing de vaudio (oclusión, reverb)
- Aplica filtros EFX basados en materiales
- Actualiza posición 3D para HRTF

### Map (`Map.cs`)
Define la geometría del mundo:
- `BuildMap()` - Construye paredes, puertas, escaleras
- `AddWall(position, size, material)` - Añade pared con colisión
- `AddPrimitive(position, size, material, hasCollision)` - Geometría para raytracing
- `AddSoundSource(position, path, looping)` - Fuente de sonido ambiental
- Listas: `Colliders`, `Doors`, `Stairs`, `Platforms`

### Player (`Player.cs`)
Control del jugador:
- **WASD** - Movimiento
- **Flechas** - Rotación
- **E** - Interactuar con puertas
- **C** - Hablar coordenadas (Tolk)
- Detecta escaleras automáticamente y ajusta altura Z
- Pisadas diferentes según superficie (madera en escaleras)

### Door (`Door.cs`)
Puerta interactiva:
- `Toggle()`, `Open()`, `Close()`
- Sonidos: loop (cerrada), open, close
- Collider dinámico (se quita al abrir)

### Stair (`Stair.cs`)
Escalera con altura automática:
- `GetHeightAtPosition(pos)` - Retorna altura Z según posición
- Direcciones: North, South, East, West
- Añade primitiva de madera para raytracing

## Sistema de Raytracing (vaudio)

### Inicialización
```csharp
var settings = new RaytracingContextSettings()
{
    worldSize = new Vector3F(100, 100, 10),
    reverbRayCount = 1024,      // Rayos para reverb
    occlusionRayCount = 512,    // Rayos para oclusión
    permeationRayCount = 256,   // Rayos para transmisión
    trailBounceCount = 8,       // Rebotes por rayo
    maxVoices = 32,
    metersPerUnit = 1.0f,
};
raytracingContext = new RaytracingContext(settings);
```

### Materiales
```csharp
MaterialType.Concrete  // Duro, muy reflectivo
MaterialType.Brick     // Ligeramente absorbente
MaterialType.Metal     // Muy reflectivo
```

Propiedades de material:
- `AbsorptionLF/HF` - Absorción baja/alta frecuencia
- `ScatteringLF/HF` - Dispersión
- `TransmissionLF/HF` - Transmisión a través del material

### Primitivas
```csharp
var wall = new PrismPrimitive()
{
    material = MaterialType.Brick,
    size = new Vector3F(width, depth, height),
    transform = Matrix4F.CreateTranslation(x, y, z)
};
raytracingContext.AddPrimitive(wall);
```

### Datos de Raytracing por Voz
```csharp
var voice = raytracingContext.GetVoice(voiceId);
voice.occlusionLF      // Oclusión baja frecuencia (0-1)
voice.occlusionHF      // Oclusión alta frecuencia (0-1)
voice.reverbWetness    // Cantidad de reverb (0-1)
voice.processedReverb  // Datos EAX procesados
```

### Reverb Automático
El reverb se calcula automáticamente según la geometría:
- Salas pequeñas → decay corto, reverb íntimo
- Salas grandes → decay largo, reverb tipo catedral
- `raytracingContext.ListenerEAX` contiene parámetros EAX calculados

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

## Controles
| Tecla | Acción |
|-------|--------|
| W/S | Adelante/Atrás |
| A/D | Izquierda/Derecha (strafe) |
| ←/→ | Girar |
| E | Abrir/cerrar puerta |
| C | Hablar coordenadas |
| Esc | Salir |

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
- Ruta de sonidos: `bin/Windows/x86/Debug/sounds/`

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

### Debug Window
Activar ventana de debug de vaudio:
```csharp
new AudioManager(worldSize, enableDebugWindow: true);
```

### MapObject (`MapObject.cs`)
Sistema base para objetos del mapa con geometría de raytracing:
- `MapObject` - Clase base abstracta
- `Window` - Ventana con cristal (sonido exterior)
- `Pillar` - Columna/pilar
- `Furniture` - Mueble genérico
- `SoundEmitter` - Emisor de sonido sin geometría
- `PlatformObject` - Plataforma elevada
- `WallSegment` - Segmento de pared independiente
- `Railing` - Barandilla

```csharp
// Los objetos gestionan sus propias primitivas y colliders
var furniture = new Furniture(map, position, size, MaterialType.Brick);
furniture.Build();
furniture.Deactivate();  // Quita primitivas y colliders
furniture.Activate();    // Los añade de vuelta
```

### Door (hereda de MapObject)
Las puertas ahora heredan de `MapObject` y gestionan dinámicamente sus primitivas:
- Al abrir: `RemoveAllPrimitives()` + `RemoveAllColliders()` → sonido pasa libremente
- Al cerrar: `AddAllPrimitives()` + `AddAllColliders()` → sonido bloqueado

## Problemas Conocidos

1. **HRTF no funciona**: Verificar que `OpenAL32.dll` sea la versión Win64 de OpenAL Soft
2. **Jugador atrapado**: Verificar colliders, reducir `PlayerRadius` si es necesario
3. **Sin reverb**: El mapa debe tener techo y paredes cerradas
4. **Sonido cortado**: Verificar que los archivos de sonido existen en la ruta correcta

## Problemas Resueltos

### Oclusión de sonido no funcionaba (raytracing)
**Síntoma**: Los sonidos no se "camuflaban" a través de paredes. `gainLF/gainHF ≈ 1.0` en el log.

**Causa**: Huecos en la geometría de las paredes. Los segmentos de pared a cada lado de las puertas no cubrían completamente hasta el borde de las habitaciones, dejando espacios de 0.5m a 4m donde los rayos pasaban libremente.

**Solución**: Recalcular el ancho de cada segmento de pared correctamente:
```csharp
// Antes (incorrecto - dejaba huecos)
AddWall(center, new Vector3(4f - doorWidth/2, ...));  // = 3m, dejaba hueco

// Ahora (correcto - sin huecos)
float wallWidth = (doorCenter - roomLeftEdge) - doorWidth/2;  // Cálculo preciso
AddWall(new Vector3(roomLeft + wallWidth/2, ...), new Vector3(wallWidth, ...));
```

**Importante para futuros mapas**: Siempre verificar que los segmentos de pared cubran desde el borde de la habitación hasta el borde de la abertura de la puerta, sin dejar huecos.

---

## Roadmap - Próximas Funcionalidades

### 1. Sistema de Objetivos (Quests)
Sistema para crear misiones, historias y progresión del jugador.

**Características planificadas:**
- `Objective` - Objetivo individual con condiciones de completado
- `Quest` - Agrupación de objetivos con recompensas
- `QuestManager` - Gestión de quests activas, completadas, fallidas
- Tipos de objetivos:
  - Llegar a una ubicación
  - Interactuar con objeto/NPC
  - Recoger items
  - Eliminar enemigos
  - Tiempo límite
- Eventos: `OnObjectiveComplete`, `OnQuestComplete`, `OnQuestFailed`
- Diálogos asociados a quests (narración con Tolk)
- Persistencia de progreso (guardar/cargar)

**Ejemplo de uso futuro:**
```csharp
var quest = new Quest("escape_building", "Escapa del Edificio");
quest.AddObjective(new LocationObjective("door_exit", new Vector3(10, 50, 0), radius: 2f));
quest.OnComplete += () => audioManager.Play2D("sounds/victory.ogg");
questManager.StartQuest(quest);
```

### 2. Carga Dinámica de Mapas y Geometría Avanzada
Sistema para cargar mapas y crear geometría compleja.

**Características planificadas:**

#### Carga de Mapas
- Formato de mapa (JSON o binario)
- `MapLoader` - Carga mapas desde archivo
- `MapManager` - Gestión de transiciones entre mapas
- Transiciones suaves (fade de audio, carga asíncrona)
- Editor de mapas (futuro)

#### Escaleras Avanzadas
- **Escalera recta** (actual) - Dirección única (N/S/E/W)
- **Escalera en L** - Dos tramos con giro de 90°
- **Escalera en U** - Tres tramos con dos giros
- **Escalera en espiral/caracol** - Giro continuo alrededor de un eje central

```csharp
// Escalera en L
var stairL = new LShapedStair(
    start: new Vector3(0, 0, 0),
    firstLength: 6f,           // Primer tramo
    secondLength: 4f,          // Segundo tramo tras el giro
    width: 2f,
    totalHeight: 4f,
    firstDirection: StairDirection.North,
    turnDirection: TurnDirection.Right  // Gira a la derecha
);

// Escalera en espiral
var spiral = new SpiralStair(
    center: new Vector3(0, 0, 0),
    innerRadius: 1f,           // Radio interior (hueco central)
    outerRadius: 3f,           // Radio exterior
    totalHeight: 8f,           // Altura total
    turns: 1.5f,               // Vueltas completas (1.5 = 540°)
    direction: SpiralDirection.Clockwise
);
```

#### Salas con Formas Variadas
- **Rectangular** (actual) - Paredes rectas
- **Circular/Cilíndrica** - Paredes curvas, aproximadas con segmentos
- **Poligonal** - Hexagonal, octogonal, formas irregulares
- **Abovedada** - Techos curvos para acústica especial

```csharp
// Sala circular
var circularRoom = new CircularRoom(
    center: new Vector3(20, 20, 0),
    radius: 8f,
    height: 5f,
    segments: 16,              // Más segmentos = más suave
    material: MaterialType.Concrete,
    hasCeiling: true
);

// Sala poligonal (hexagonal)
var hexRoom = new PolygonalRoom(
    center: new Vector3(40, 20, 0),
    radius: 6f,
    sides: 6,                  // 6 = hexágono, 8 = octágono
    height: 4f,
    material: MaterialType.Brick
);

// Cúpula (techo abovedado)
var dome = new DomeRoom(
    center: new Vector3(60, 20, 0),
    radius: 10f,
    maxHeight: 8f,             // Altura en el centro
    segments: 12,
    material: MaterialType.Concrete
);
```

#### Conectores entre Salas
- **Pasillos rectos** - Conexión simple
- **Pasillos curvos** - Arcos entre salas
- **Túneles** - Pasillos con techo bajo

```csharp
// Pasillo curvo entre dos salas
var corridor = new CurvedCorridor(
    start: room1.GetDoorPosition(),
    end: room2.GetDoorPosition(),
    width: 2f,
    height: 3f,
    curvature: 0.3f            // 0 = recto, 1 = muy curvo
);
```

**Estructura de archivo de mapa (expandida):**
```json
{
  "name": "nivel_1",
  "worldSize": [100, 100, 20],
  "rooms": [
    {
      "type": "rectangular",
      "position": [5, 5, 0],
      "size": [10, 10, 5],
      "material": "brick"
    },
    {
      "type": "circular",
      "center": [30, 30, 0],
      "radius": 8,
      "height": 6,
      "segments": 16,
      "material": "concrete"
    },
    {
      "type": "polygonal",
      "center": [50, 30, 0],
      "radius": 6,
      "sides": 6,
      "height": 4,
      "material": "metal"
    }
  ],
  "stairs": [
    {
      "type": "straight",
      "position": [-10, 47, 0],
      "length": 14,
      "width": 4,
      "height": 4,
      "direction": "north"
    },
    {
      "type": "L-shaped",
      "start": [20, 0, 0],
      "firstLength": 6,
      "secondLength": 4,
      "width": 2,
      "height": 4,
      "firstDirection": "north",
      "turn": "right"
    },
    {
      "type": "spiral",
      "center": [40, 40, 0],
      "innerRadius": 1,
      "outerRadius": 3,
      "height": 8,
      "turns": 1.5,
      "direction": "clockwise"
    }
  ],
  "corridors": [
    {
      "type": "curved",
      "start": [15, 5, 0],
      "end": [22, 30, 0],
      "width": 2,
      "height": 3,
      "curvature": 0.3
    }
  ],
  "doors": [
    {"position": [5, 10, 1.1], "rotation": 0, "sounds": "door1"}
  ],
  "soundSources": [
    {"position": [5, 5, 1.5], "sound": "ambient/rain.ogg", "loop": true}
  ],
  "playerSpawn": {"position": [5, 5, 0], "angle": 0}
}
```

#### Consideraciones de Audio para Geometría Curva
- Las paredes curvas se aproximan con múltiples primitivas rectas
- Más segmentos = mejor simulación acústica pero más CPU
- Salas circulares tienen reverb característico (ecos focalizados)
- Cúpulas crean reflexiones únicas desde el centro

### 3. Sistema de Cutscenes en Tiempo Real
Cutscenes usando el engine de audio en tiempo real (sin vídeos pre-renderizados).

**Características planificadas:**
- `Cutscene` - Secuencia de eventos cinematográficos
- `CutsceneAction` - Acción individual (base class)
- Tipos de acciones:
  - `DialogueAction` - Diálogo narrado (Tolk o audio)
  - `SoundAction` - Reproducir sonido 3D/2D
  - `MoveListenerAction` - Mover cámara/oyente suavemente
  - `WaitAction` - Pausa temporal
  - `FadeAction` - Fade in/out de audio global
  - `SpawnSoundAction` - Crear fuente de sonido temporal
  - `TriggerAction` - Ejecutar código personalizado
- Timeline con eventos paralelos y secuenciales
- Control del jugador deshabilitado durante cutscene
- Skippable (opcional)

**Ejemplo de uso futuro:**
```csharp
var intro = new Cutscene("intro");
intro.AddAction(new FadeAction(FadeType.In, duration: 2f));
intro.AddAction(new DialogueAction("Despierto en la oscuridad...", voice: "narrator"));
intro.AddAction(new SoundAction("sounds/ambient/drip.ogg", new Vector3(5, 10, 2), delay: 1f));
intro.AddAction(new MoveListenerAction(from: Vector3.Zero, to: new Vector3(5, 5, 0), duration: 3f));
intro.AddAction(new DialogueAction("¿Dónde estoy?", voice: "player"));
intro.AddAction(new FadeAction(FadeType.Out, duration: 1f));
intro.OnComplete += () => EnablePlayerControl();
cutsceneManager.Play(intro);
```

**Flujo de cutscene:**
```
[Fade In] → [Diálogo 1] → [Sonido ambiente] → [Mover oyente] → [Diálogo 2] → [Fade Out]
     ↓           ↓              ↓                   ↓              ↓            ↓
   2 seg      narrador      goteo agua          3 seg          jugador      1 seg
```

### Prioridad de Implementación
1. **Sistema de Objetivos** - Fundamental para crear juegos con propósito
2. **Cutscenes** - Necesario para narrativa y tutoriales
3. **Carga Dinámica de Mapas** - Para juegos más grandes y modulares
