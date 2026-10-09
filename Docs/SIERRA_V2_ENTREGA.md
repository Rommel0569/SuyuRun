# SIERRA V2 — paquete artístico propuesto

24/09/2026. Costa cerrado por aprobación y prueba completa del usuario. Esta entrega no cambia Costa ni sus escenas. El pedido vigente permite producir el lote completo; el resultado y su paleta siguen pendientes de aprobación visual.

## Abrir primero

- `Artifacts/SierraV2/SIERRA_TRES_BIOMAS.png`: tres composiciones a escala entera x4.
- `Artifacts/SierraV2/sierra_camino_animacion.gif`
- `Artifacts/SierraV2/sierra_valle_animacion.gif`
- `Artifacts/SierraV2/sierra_ruinas_animacion.gif`
- `Artifacts/SierraV2/sierra_catalogo_1.png` a `sierra_catalogo_7.png`: inventario visual de módulos, personajes y efectos.

Los previews son composiciones OFFLINE de las capas reales, no capturas de Unity ni pruebas de jugabilidad. No importar estas composiciones como fondos planos. Los GIF muestran ejemplos de movimiento/daño y no un recorrido aprobado contra la canción.

## Archivos editables y Unity

- PNG y fichas: `Assets/SuyuRun/Art/PixelSierra/ProductionV2/Resources/SierraV2/`.
- Paleta propuesta: `Assets/SuyuRun/Art/PixelSierra/ProductionV2/sierra_palette_v2.json`.
- Inventario: `sierra_manifest.json`, con rectángulos por frame, FPS, pivotes, bucles, factores y perfiles `surfaceY` cuando aplica.
- Originales independientes de los paisajes: `SourceMedia/SierraV2/`. NO corresponden a un escenario cortado en franjas.
- Generador editable: `Tools/build_sierra_artpack.py`; previews y comprobación: `Tools/preview_sierra_artpack.py`.
- Importador nuevo y acotado: `Assets/SuyuRun/Editor/SierraV2ArtImport.cs`.

En Unity, esperar recompilación y usar **SUYURUN → Sierra V2 → Importar sprites y crear clips**. El importador fija Point, sin compresión, sin mipmaps, PPU32, FullRect, pivotes y slicing de las fichas. El comando genera `.anim` por hoja animada, solo dentro de ProductionV2. NO se ha ejecutado ni validado ese comando en Unity en esta entrega. No modifica escenas. Los clips son curvas discretas de SpriteRenderer; no incluyen todavía controladores, hitboxes ni eventos de daño.

## Recursos incluidos

- Cielo compartido, nubes desplazables, nevados independientes y neblina animada.
- Un panorama propio de camino, uno de valle y uno de ruinas; 640x180 cada uno.
- Capas cercanas animadas para cada acto. Árbol, ichu, roca y cerco separados para variar el recorrido.
- Suelo ondulado de tierra, tierra de descansos y suelo de mampostería escalonado. Cada superficie exporta altura por columna.
- Bloques, rampas, escaleras y pilares de ruinas con perfiles superiores.
- Puente256x80: estable, tensión, dañado y colapso. Tablas, cuerdas, postes y diferencias de estado dibujadas. Reutilizar el módulo varias veces, con descansos entre puentes; no estirar su pixel grid.
- Túnel de entrada/salida con llama animada y máscara opaca independiente. Marca quemada de llegada, humo y anillo de teleportación.
- Llama idle/marcha. Cóndor, halcón y ave de puna: vuelo, anticipación, picada, impacto, caída.
- Guardián fantástico de piedra con huaraca: idle, marcha, ataque, impacto y rotura. No convertir a una cultura o personaje histórico en enemigo.
- Héroe48x48: cuerpo/rostro conservados desde Costa, brazos añadidos; idle, run, jump, land, slide, ráfaga con arma, hit, giro y lanzamiento de huaraca, trepar.
- Piedra giratoria, roca rodante, pinchos de piedra, tronco, farol; cabezas clavas, kero, quipu y moneda con destello.
- Polvo de carrera y aterrizaje, impacto de piedra, chispas, humo de teleportación, escombros de piedra, astillas, rotura de cuerda, salpicaduras.
- Portal ceremonial distinto al de Costa, absorción, aparición/rotura de escudo, transformación e idle del traje protector, reutilización del `?!`.
- Panel, botón, casilla, barra de integridad y retícula pixel art sin texto horneado. Panel/botón con bordes9-slice2px; usar fuente pixel existente.

El traje de aspecto simiesco es fantasía, con rostro visible, no una afirmación sobre fauna o indumentaria peruana. Las ruinas son INCAS, no preincas. La ficha de cabeza clava debe distinguir Chavín del escenario inspirado en Machu Picchu. Ninguna ficha histórica nueva se presenta como investigada/validada en esta entrega.

## Composición y animación

Base320x180; escalar a enteros, preferentemente4. Capas640x180; hojas animadas de capas apiladas verticalmente, siempre640px de ancho. Cada capa tiene contenido independiente y transparencia binaria, salvo cielo opaco. Las hojas de personajes se disponen horizontalmente, celdas completas sin separadores.

Orden: cielo0 → nubes0.05 → nevados0.12 → paisaje0.30 → niebla0.25 / río0.30 → cercano0.75 → suelo/módulos1.0 → personajes/FX/UI. Factores de esta propuesta se registran en el manifiesto, no aplicar los factores de Costa automáticamente. Dos copias por capa con repetición de640px; no recalcular zoom según altura de los sprites. El preview sube los nevados25px para composición; replicar ese offset en revisión de Unity.

El río animado es una capa auxiliar; ajustar su ubicación a la garganta seleccionada, **no colocar una segunda masa de agua sobre el cauce ya pintado sin enmascarar sus orillas**. La previsualización usa destellos discretos sobre el paisaje. Es un punto pendiente de ajuste de integración.

Colisión: convertir `surfaceY[x]` desde coordenada superior a Unity: `y=(180-surfaceY[x])/32` para capas. En un módulo de altoH, usar `(H-surfaceY[x])/32`. Los perfiles se muestrean cada píxel; simplificar sin perder los escalones. El alfa del PNG NO es un collider.

Puentes: estable → aviso del ave → tensión → dañado → colapso. Primera ventana de aviso siempre antes de daño real. Al colapso, retirar soporte en frame2 y activar caída, no mantener collider invisible. Las otras versiones exportan la curva estable; balanceo visual requiere ajustar también la superficie o limitarlo a cuerda/decoración. Añadir período de recuperación entre encuentros; mantener aterrizajes libres de enemigos.

Huaraca: giro previo, lanzamiento en frame1 de `huaraca_lanzar`, proyectil apuntado con mouse, impacto/retroalimentación. Guardián lanza en frame4. Estas son instrucciones de conexión, NO eventos ya implementados. Conectar `AudioSettings.dspTime` al avance musical y al timeline; no usar reloj independiente para recolocar obstáculos.

Túneles: ocultar la pantalla con máscara opaca; cambiar capas y recorrido solo cuando esté tapada; salir por otro arco. No crossfade ni transparencia suave. Llegada: anillo → humo → aparición → marca en superficie. Final: detenerse → transformación completa → `?!` → escudo → absorción → transición a Selva.

## Pendiente antes de declarar Sierra terminado

- Aprobación visual del paquete/paleta por el usuario, especialmente héroe, enemigos y traje.
- Montaje de la escena Sierra y sus tres actos con varios puentes y dos conexiones de túnel.
- Física de pendientes/escalones, vulnerabilidad de puentes, IA de aves/guardianes, huaraca y daño.
- Conexión al museo/progreso, pantalla final y acceso real desde el portal existente, sin rediseñar Costa.
- Diseñar el recorrido contra los174 acentos MIDI y los325 pulsos existentes, conservar fuentes. Resolver diferencia de duración MIDI171s frente a MP3177.5543s; no estirar notas automáticamente ni afirmar sincronización exacta.
- Validación de oído, latencia, salto/aterrizaje y ventanas de ataque en toda la canción.
- Ajuste visual de río/costuras/capas en movimiento dentro de Unity, revisión de colisión con animaciones y pruebas completas en Play. Ningún EXE nuevo.

## Comprobaciones de esta entrega

`Artifacts/SierraV2/validation.json`: comprueba paleta24, alfa binario, frames no vacíos y coincidencia de columnas de los bordes de capas. La igualdad de bordes es una comprobación técnica, NO garantiza por sí sola una composición artística continua en todas las repeticiones. El generador compara SHA256 antes/después de los51PNG de Costa. No se han tocado música ni mapas MIDI.

Reproducir: `python Tools/build_sierra_artpack.py` y `python Tools/preview_sierra_artpack.py`, desde SuyuRun. Leer primero ART_BIBLE.md y SIERRA_PRODUCTION_BRIEF.md; los generadores solo regeneran ProductionV2 y sus previews, no trabajo anterior.
