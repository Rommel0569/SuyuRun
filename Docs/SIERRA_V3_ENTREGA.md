# Sierra V3 — misma dirección gráfica que Costa

## Avance posterior: acabado e integración en Unity

El usuario acepta esta dirección y pide pulido y continuación. Se añadieron acentos de material y vegetación sin cambiar paleta ni siluetas; respaldo de93PNG anteriores en `Artifacts/SierraV3_BeforePolish`. El estado histórico más abajo se conserva, pero las afirmaciones «no compilado/importado» quedan sustituidas por este avance: **Unity compiló los scripts y generó69clips; la escena `Assets/SuyuRun/Scenes/Level_Sierra_Integration.unity` existe**. No es todavía la entrega definitiva.

Abrir con **SUYURUN → Sierra V3 → Abrir integración**, luego Play. No se modificaron el nivel ni la escena de Costa. El menú fija la escena de inicio dePlay para que no se abra Costa por la configuración anterior. No se generó ejecutable.

En `SierraStageDirector.cs`: relojDSP con MP3 completo, tres actos, dos túneles de oclusión, pendientes/terrazas a partir de perfiles del arte, tres puentes separados por tierra, salto/deslizamiento, disparos dirigidos al mouse, daño/colapso de puente por aves, vida/reintento/pausa, registro permanente de3piezas con claves nuevas deSierra y secuencia de traje al terminar. Es una integración inicial: faltan balance humano, montaje del museo general, conexión desde Costa y continuidad real hacia Selva, entre otros puntos.

Controles: ESPACIO/↑ saltar, S/↓ deslizar, click huaraca, E disparo repetido, ESC pausa, R reiniciar. Actualmente E no tiene aún el recurso limitado/recarga final. El guardián terrestre usa la animación de marcha y colisión; su ataque de huaraca todavía no está conectado. No describir estas mecánicas pendientes como completas.

La prueba automática usa autopuntería y salto de diagnóstico, NO representa dificultad humana y NO modifica los coleccionables guardados. Evidencia: `Artifacts/SierraV3/unity-play-result.txt` cuando exista, `unity-live.txt` y `unity-play-0/1/2.png`. El primer ensayo fue interrumpido por recompilación enPlay, que invalidó cachés no serializables; se añadió reinicio seguro tras hot reload y se repite desde cero. Solo el reporte posterior con0errores permite decir que completó el recorrido técnico.

### MIDI: auditoría verificada, no sincronía de oído inventada

- Fuente: `Assets/sierra.midi`,1pista/1canal,13.835note-on, tempo declarado120BPM,43ticks/quarter, sin canal de batería separado.
- Duración real MIDI170.76744186s; MP3177.5543s. Diferencia6.78685814s. **No se aplicó stretch**: podría ser una cola sin transcripción; no se demostró desfase global.
- Ingeniería previa:174acentos, todos con notas en su ventana correspondiente. Clasificación ground/air por intensidad, NO identificación de instrumento. Ventanas120ms, no los onsets exactos.
- Exportación robusta de todas las pistas y mapa de tempo por intervalos: `Tools/audit_sierra_midi.py`. Datos: `Artifacts/Audio/SierraAudit/sierra_midi_exact.json`, `sierra_hit_review.json`, `summary.json`.
-3pruebas de extracción pasadas: cambios de tempo, múltiples pistas/canal9/velocity0, tempo implícito. Ejecutar `python Tools/test_sierra_midi.py`.
- Runtime conserva el original `SierraSoundtrack.asset` con325pulsos estimados deMP3. Clona temporalmente el soundtrack para usar la grilla declarada del MIDI con BeatToSeconds/SecondsToBeat; los174acentos no se estiran ni cambian. El filtro inicial separa encuentros al menos2.8s y despeja túneles/puentes; es una selección provisional, no un mapa manual final aprobado.
- La tabla de174acentos queda marcada para validar de oído: transcripción densa, clasificación por intensidad y cuantización de120ms. Los saltos todavía necesitan coreografía/balance musical por sección; no afirmar aterrizajes sincronizados perfectos.

24/09/2026. El usuario señaló correctamente que los paisajes V2 tenían más textura que Costa. La coincidencia de resolución/paleta no bastaba para afirmar mismo estilo. V3 corrige la dirección visual: planos de color grandes, siluetas legibles, sombras reducidas y dithering solo cielo/niebla. **Pendiente de aprobación visual**, no declarar aprobado por el «continúa» condicional.

## Ver el resultado

- `Artifacts/SierraV3/SIERRA_TRES_BIOMAS.png`: composiciones estáticas de camino, valle y ruinas, 320x180 ampliado x4.
- `Artifacts/SierraV3/sierra_camino_animacion.gif`, `sierra_valle_animacion.gif`, `sierra_ruinas_animacion.gif`: previews de movimiento de capas, personajes y efectos. Son demos offline de8s, NO gameplay de Unity ni sincronía probada con MIDI.
- `Artifacts/SierraV3/sierra_catalogo_1.png` a `sierra_catalogo_7.png`: módulos y sprites disponibles.

## Paquete

`Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/`: **93 PNG / 346 fotogramas**, cada archivo con ficha `.sprite.json`; inventario `sierra_manifest.json`. Mismo PPU32, filtros Point, base320x180, paleta24, alfa binario. No hay fotogramas extra de relleno; el total incluye celdas estáticas y repeticiones necesarias de ciclos.

Incluye cielo/nubes/nevados,3paisajes independientes,3capas cercanas con viento, suelos y perfiles, río animado, niebla, puentes con4estados, túneles, llegada por teleport, módulos de ruinas, llamas,3tipos de aves y sus ataques, guardián de piedra con huaraca, animaciones del protagonista, proyectil, obstáculos, coleccionables, partículas, portal, transformación del traje y componentes UI. Detalle funcional y eventos recomendados: `SIERRA_V2_ENTREGA.md`, con las excepciones de abajo.

## Qué cambia respecto a V2

- Cielo, nubes, nevados y los tres paisajes redibujados directamente a640x180. Se usa el mismo enfoque de dibujo nativo que los generadores aprobados de Costa; no hay reescalado de una ilustración detallada ni recortes de un escenario completo para fingir capas.
- El río ahora es una capa completa independiente de6frames y coincide con las dos aperturas del valle. Desplazar paisaje y agua con **el mismo offset y factor0.30**. No hay cauce estático duplicado en el fondo.
- Nevados a offset vertical0; eliminar el ajuste−25 de la propuesta V2.
- Las vistas V3 reutilizan módulos/sprites nativos del generador común con brazos del slide corregidos y ráfaga con arma visible. Se conservan el cuerpo y rostro originales de Costa.
- Se retiraron los frames duplicados innecesarios de tronco y pinchos (son estáticos). 346frames, no352.
- Los anteriores PNG de V2 y fuentes generadas permanecen guardados, sin reemplazar arte aprobado ni Costa.

## Unity

El importador `Assets/SuyuRun/Editor/SierraV2ArtImport.cs` admite ambos paquetes, de forma acotada. Para esta versión elegir **SUYURUN → Sierra V3 → Importar sprites y crear clips**. Genera clips solo dentro deProductionV3, curvas discretas deSpriteRenderer, frame-rate/bucles de las fichas; paneles/botones9-slice. No modifica ninguna escena. **No ejecutado ni compilado/verificado en Unity en esta entrega.**

No consumir ambos paquetes por un nombre genérico: usar `Resources.LoadAll<Sprite>("SierraV3/nombre")`. Cada escena de capa necesita dos copias para scroll. Los archivos de preview NUNCA son assets de fondo; montar las capas reales.

Para regenerar desde SuyuRun:

```
python Tools/build_sierra_coast_style.py
python Tools/preview_sierra_artpack.py --v3
```

Leer antes ART_BIBLE.md y SIERRA_PRODUCTION_BRIEF.md. El primer script reutiliza rutinas nativas de `build_sierra_artpack.py`, redirigidas aV3. No carga los paisajes generados deV2 ni modifica sus archivos.

## Validación y límites

`Artifacts/SierraV3/validation.json`:93assets/346frames,0errores de paleta, alfa, dimensiones de frames y columnas de unión. Igualdad de bordes no es aprobación artística de la repetición. `costa_protected_hashes.json`:SHA256 de51PNG de Costa, verificados idénticos antes/después.

Pendientes de implementación: escena de Sierra,2túneles/varios puentes distribuidos durante la canción, colisiones de pendientes y puentes, IA/daño/huaraca, animaciones conectadas a eventos, integración de portal/progreso/recompensa, validación del recorrido contra MIDI y de oído y prueba completa enPlay. Conservar música y JSON actuales. No se creó ningún ejecutable.

El traje de aspecto de gorila es protección fantástica con rostro visible; no fauna peruana ni vestimenta histórica. Ruinas inspiradas en Machu Picchu son INCAS. Ninguna ficha histórica nueva queda validada por generar un sprite.
