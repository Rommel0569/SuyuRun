# SUYURUN — continuidad para Sierra y Selva (para Astra)

Costa está 100% completo (capas, plataformas, héroe, obstáculos, coleccionables, efectos, final con puerta dimensional, HUD, inicio, selector, museo, tienda, fuente pixel real). Este documento es el prompt de arranque para lo que sigue: Sierra y Selva.

## Regla de oro (viene de ART_BIBLE.md y CLAUDE.md)

Leer `ART_BIBLE.md` completo antes de tocar cualquier asset. Un asset o capa por mensaje, esperar confirmación explícita antes de seguir. Paleta de 24 colores fija por nivel, sin degradados/anti-aliasing/blur, contorno de 1px, dithering solo en cielo/niebla. No reescribir assets ya aprobados. Archivos: `nivel_capa_descripcion.png`.

## Lo que falta de la biblia, en general

- **Costa**: plano frontal opcional (capa 1.3) — el usuario lo saltó a propósito, se puede retomar si algún día lo pide. Notas MIDI dudosas de Costa sin validar de oído.
- **Sierra**: todo el contenido visual y jugable (ver abajo). Ya tiene paleta, capa 1 (cielo) y capa 2 (nevados) aprobadas.
- **Selva**: todo el contenido visual y jugable. Ya tiene paleta aprobada, ninguna capa todavía.
- **Final**: plaza festiva que une Costa/Sierra/Selva, 3 personajes bailando (sección 9 de la biblia) — no empezado.
- **Museo y tienda de Sierra/Selva**: no existen todavía (solo Costa los tiene).
- **Sistema de vida**: no existe en Costa (muerte instantánea, decisión ya tomada). Arranca recién en Sierra/Selva — el usuario mostró una referencia de barra de vida por secciones (pegada en su ChatGPT del 19 sept) para implementar ahí.

## Sierra — lo que ya se definió

**Paleta**: `Assets/SuyuRun/Art/PixelSierra/sierra_palette.json`, 24 colores, amanecer dorado (opuesto al atardecer de Costa). Aprobada.

**Capas aprobadas**: `sierra_capa1_cielo.png`, `sierra_capa2_nevados.png`. Compartidas por todo el nivel (no cambian por acto).

**Estructura — multi-bioma, NO un fondo continuo como Costa**: 3 actos que se funden entre sí (crossfade + parallax blending + niebla degradada + cambio gradual de qué colores dominan, dentro de la misma paleta — no cambio de paleta real). Basado en las referencias que pasó el usuario (sierra 1/2/3):

| Acto | Referencia | Contenido | Tiempo real | Distancia (v=7) |
|---|---|---|---|---|
| 1 | Camino de Tierra | sendero de tierra, cerco de MADERA (no piedra — error ya corregido una vez), llama, árboles en la cresta cercana | 0s–57s | x=0–399 |
| *(fusión)* | | | 51s–57s | x=357–399 |
| 2 | Puente Colgante | cruce sobre el valle, cóndor, río abajo | 51s–114s | x=357–798 |
| *(fusión)* | | | 108s–114s | x=756–798 |
| 3 | Ruinas Preincas | andenes escalonados, remate visual (puerta dimensional a Selva) | 108s–171s* | x=756–1197* |

*Ojo: estos tiempos se calcularon sobre 171s (duración del MIDI). La duración REAL del audio es 177.5543s — reescalar los 3 actos proporcionalmente antes de construir el course.

**Poder de Sierra**: brazos ya obtenidos al final de Costa; en Sierra se usa la HUARACA — click apunta y lanza piedras, principalmente contra enemigos aéreos (aves). Al final de Sierra se gana el traje de Selva.

**Enemigos** (de la biblia, sección 7): cóndor en picada 64x48 (4 frames + 1 daño/caída), halcón 32x32 (4 frames), ave de puna (2 de ataque). **Coleccionables**: cabezas clavas, keros ceremoniales, quipus.

**Música lista**: `SierraSoundtrack.asset` (BPM real medido 109.95, 325 pulsos reales del audio), `Artifacts/Audio/sierra_rhythm.json` (174 golpes clasificados suelo/aire). Ver `Artifacts/Audio/sierra-selva-engineering-notes.txt` para el detalle completo de cómo usarlos.

## Selva — lo que ya se definió

**Paleta**: `Assets/SuyuRun/Art/PixelSelva/selva_palette.json`, 24 colores: verde profundo/lima (dosel), marrón río, turquesa (río/cascadas), naranja (antorchas), rojo guacamayo, azul noche, dorado (templo final). Propuesta, sin capas todavía.

**Estructura — 6 etapas que se funden entre sí** (misma técnica de crossfade que Sierra), basada en la referencia que pasó el usuario:

1. Bosque Inicial — cocodrilos, pinchos, dosel con luces colgantes
2. Río y Paso en Bote — cascadas, loros, bote remando
3. Cueva en la Selva — antorchas, murciélagos, puente de cuerda sobre agua
4. Ruinas Antiguas — templo piramidal de fondo, serpiente
5. Bosque de Noche — luna, cabaña, puente de cuerda
6. Templo Final — ídolo dorado, remate del nivel (poder/transición al Final del juego)

**Mecánicas pedidas explícitamente por el usuario**: trepar con lianas, esquivar cocodrilos en pantanos/río — multi-bioma, multi-obstáculo, multi-acción (no un solo set de obstáculos repetido).

**Enemigos**: caimanes (ya en la biblia), murciélagos (nuevo, de la Cueva), serpiente (nuevo, de las Ruinas). **Coleccionables**: cerámica Shipibo, instrumentos de viento.

**Poder de Selva**: traje protector amazónico, escudo proporcional a la vida — primer nivel con sistema de vida real (ver referencia del usuario del 19 sept).

**Música lista**: `SelvaSoundtrack.asset` (BPM real medido 114.85, 345 pulsos), `Artifacts/Audio/selva_rhythm.json` (260 golpes). Mismo detalle en `sierra-selva-engineering-notes.txt`.

## Por qué las capas de escena (bosques/puentes/ruinas) no están hechas

El usuario decidió que ese trabajo específico de arte de escena lo hace Astra, no esta sesión («eso hare que haga el astra porque tu no sabes»). Se generaron y se borraron dos intentos (capa 3 "cerros" y capa 4 "camino" de Sierra) — quedaron solo las paletas y las 2 capas compartidas de Sierra que ya estaban aprobadas antes de esa decisión.
