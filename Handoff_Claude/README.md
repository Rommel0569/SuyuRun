# SUYURUN — empezar aquí para continuar con Claude

**Actualización 19 septiembre:** el estado de abajo y la copia Proyecto_20260918_194303 son históricos. Claude avanzó capas6/7 y otros assets; ahora se corrigió la integración visual del nivel. Leer primero `../Docs/COSTA_REFERENCE_REENGINEERING.md`, continuar en el proyecto original y NO restaurar la copia antigua encima. Las pruebas recientes están en `../Artifacts/ReferenceCoast/`.

Este relevo fue solicitado por el usuario para poder continuar sin depender de los tokens o del historial de otra herramienta. Está guardado localmente dentro del proyecto, no subido a un servicio remoto.

## Copia completa para traslado

`Proyecto_20260918_194303/` contiene una instantánea del proyecto editable: Assets (incluye escenas, código, arte, música y metadatos), Packages, ProjectSettings, Tools, SourceMedia, Docs, Artifacts y documentos de raíz. Se copiaron 264 archivos (102.58 MB) y se compararon sus hashes SHA256 con los originales, sin diferencias, antes de añadir las instrucciones de continuidad de la copia.

Se excluyeron cachés Library/Temp/Logs, builds antiguos, configuración personal y el historial .git. Unity regenera sus cachés al abrir. No es un ejecutable nuevo ni una publicación remota. Para trabajar en este mismo equipo, usar el proyecto original `SuyuRun/`; esta instantánea es respaldo/traslado, no una segunda fuente de verdad. No copiarla dentro de Assets. Las aprobaciones posteriores deben consultarse en el proyecto activo.

## 1. Lectura obligatoria

1. `../ART_BIBLE.md` COMPLETO antes de cualquier asset: reglas nuevas de pixel art y flujo de aprobación.
2. `../ART_APPROVALS.md`: fuente de verdad de aprobaciones. Tiene prioridad sobre esta instantánea si cambia después.
3. `../PROGRESS.md`: evidencia de avances; los bloques antiguos corresponden al prototipo anterior.
4. `../ASTRA_BUILD_BRIEF.md`: contexto técnico anterior, subordinado a la biblia nueva.
5. `../../AGENTS.md` y `../CLAUDE.md`.

## 2. Punto exacto donde se deja

| Elemento | Estado al guardar este relevo |
| --- | --- |
| Paleta Costa, 24 colores | Aprobada, no cambiar |
| Capa 1: cielo | Aprobada |
| Capa 2: nubes y bandadas | Aprobada |
| Capa 3: Paracas/Candelabro | Aprobada |
| Capa 4: mar, 4 frames, garúa y totora | Aprobada: «Esta excelente» |
| Capa 5: huacas y muros | Generada, revisada y pendiente de aprobación |
| Capa 6: algarrobos, rocas, lobos marinos | No producida |
| Capa 7: suelo y siguientes assets | No producidos bajo la nueva biblia |
| Integración de toda la nueva dirección en Unity | Pendiente |
| MIDI | Copiado, todavía no analizado |

**Siguiente acción:** mostrar/revisar la capa 5 si el usuario todavía no la aprobó. No tomar la aprobación del mar como aprobación de las huacas. Tras aprobación explícita de capa 5, leer de nuevo la biblia completa y producir SOLO capa 6. Al terminar, una línea de resumen, vista previa y esperar aprobación.

La preferencia reiterada del usuario es composición limpia, mar visible, identidad peruana, movimiento ambiental continuo y acabado pixel coherente; no llenar el fondo de objetos amontonados.

## 3. Archivos y fuentes

Todas las siguientes rutas son relativas a la raíz Unity `SuyuRun/`:

- `Assets/SuyuRun/Art/PixelCosta/`: PNG de las capas, metadatos y `costa_palette.json` (lista fija de 24 hexadecimales).
- `Artifacts/ArtApproval/`: vistas PNG x4, demostraciones GIF y validaciones JSON por capa. Para la entrega actual: `costa_capa5_huacas.gif`, `costa_capa5_huacas_preview_x4.png`, `costa_capa5_validation.json`.
- `Tools/draw_costa_layer1.py` hasta `draw_costa_layer5.py`: generadores deterministas Pillow autorizados por el usuario. **No ejecutar los generadores de assets aprobados para regenerarlos sin solicitud.**
- `Tools/pixel_costa_preview.py`: compositor de revisión que solo lee capas previas. No sustituye una prueba en Unity. Actualmente compone capas 1–4 más una nueva; extender para siguientes capas conservando su contenido aprobado.
- `Assets/SuyuRun/Scripts/PixelCostaSkyLayer.cs`: componente aislado para nubes y aves pixel, pendiente de conexión y prueba; no asumir que ya cambia el juego.
- `SourceMedia/El Alcatraz.mid`: MIDI original conservado (mismo SHA256 que el archivo aportado).
- `Assets/SuyuRun/Audio/`: MP3 regionales aportados; `Audio/Resources/Inicio.mp3` para menú. Créditos/licencias por completar antes de distribuir.
- `Docs/References/art_bible_style_reference.png`: referencia SOLO de estilo. No recortar ni copiar sus personajes, UI o arquitectura.

## 4. Contrato técnico del arte

PNG originales a resolución nativa; no imágenes grandes reducidas para fingir pixel art. Vista base 320x180, escala x4 nearest-neighbor, una misma cuadrícula. PPU propuesto 32; Point, sin compresión ni mipmaps. No cambiar la paleta ni añadir alfa parcial. Dithering solo cielo/niebla.

Capas 1, 2, 3 y 5: lienzo 640x180. Aves auxiliares de capa 2: 96x48, seis frames 16x16 y tres filas de distancia. Capa 4: PNG único 640x720, cuatro frames 640x180 de arriba abajo; 4 FPS, ciclo 1 s. El cielo es la excepción al tile; las demás capas tienen bordes coincidentes. Factores: 0, .05, .15, .30, .50, .75, 1, opcional 1.3.

Las vistas GIF combinan copias de los assets SOLO para revisión y reinician una demostración de 12/16 segundos. No son una imagen de producción recortada en capas, ni prueban integración en el juego. El parallax real aún debe conectarse con dos copias por capa y coordenadas enteras.

Línea de mar actual y=146. El cielo usa ventana central x=160..480 en las vistas. La capa 5 es decoración sin colisión; sus pies quedarán asentados por las siguientes capas. Cuidar que orilla, suelo y edificios no oculten todo el mar al componer el resultado.

## 5. Unity y alcance real

Proyecto: Unity 6000.3.17f1, URP 2D. Escenas existentes: `Level_Costa.unity`, `Prototype_Legacy.unity`, `Prototype_Costa.unity`, en `Assets/SuyuRun/Scenes/`. Conservar el prototipo antiguo de cerros separado.

Hay un juego previo con código parcial `RunnerPrototype`, arte procedural/ilustrado y otros cambios del usuario. NO es todavía el juego nuevo 100 % pixel art. Inspeccionar estado actual antes de tocarlo: existen cambios previos y archivos sin seguimiento de Git. No resetear, borrar carpetas ni sobrescribir implementaciones ajenas.

El usuario permite pruebas en Play; pidió no generar otro ejecutable externo en este avance. No ejecutar comandos antiguos de build. Las pruebas históricas de recorrido no validan los assets ni la integración nuevos. La última entrega valida archivos e inspección visual del preview, no Play.

## 6. Ritmo y pendientes que no se deben inventar

Extraer MIDI con Python+mido respetando ticks_per_beat y TODO el mapa set_tempo. Note-on de batería/bajo a segundos; exportar beatTimes y notas, conservar pistas/pitch. No usar `costa_music_map.json` para timing (solo ambiente). Ver sección 14 completa de biblia. No atribuir precisión musical al análisis MP3 anterior. No existe aún lista de notas MIDI dudosas: producirla tras analizar, para validación de oído del usuario.

Pendientes después de capas: suelo/plataformas, héroe y todas sus animaciones, enemigos, objetos/efectos, HUD, inicio SUYURUN (una sola palabra), selector, museo/tienda/ajustes/resultados, integración Pixel Perfect y pruebas. Sierra/Selva/Final quedan para después de Costa. No prometer terminar todo en un plazo sin aprobaciones.

Antes del héroe aclarar la contradicción «sin brazos en Costa» frente a «brazos en run». Las representaciones culturales son interpretativas; no presentar decoración fantástica como reconstrucción histórica ni la moneda lúdica como moneda Moche real sin validación.

## 7. Prompt para retomar

«Continúa SUYURUN desde Handoff_Claude/README.md. Lee ART_BIBLE.md completo y ART_APPROVALS.md. Conserva los assets aprobados y trabaja un asset por entrega. Primero identifica si capa 5 ya fue aprobada; no avances a capa 6 sin esa aprobación. No regeneres archivos aprobados ni generes otro EXE.»
