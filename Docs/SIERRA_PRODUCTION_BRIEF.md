# Sierra — encargo vigente, 24 septiembre 2026

**Corrección posterior de estilo:** el usuario comparó los previews con Costa. No basta que ambos sean pixel art: Sierra debe conservar la misma densidad de detalle, masas de color planas y siluetas limpias. V2 queda preservado pero no recomendado; la propuesta vigente se guarda en `ProductionV3`, con previews en `Artifacts/SierraV3`. No usar los paisajes imagegen más texturizados de V2 como arte aprobado. Generador V3 nativo: `Tools/build_sierra_coast_style.py`.

Costa está terminado y aprobado tras prueba completa MANUAL del usuario. No modificar arte, código, recorrido ni cierre de Costa. Los resultados automáticos anteriores ya no son el criterio de aceptación de Costa.

## Entrega de esta sesión

Paquete visual completo de Sierra: capas independientes, módulos, sprites, animaciones y efectos, con manifiesto de importación. El usuario autoriza explícitamente el lote completo en esta sesión; es una excepción al flujo de un asset por aprobación. No equivale a aprobación automática de los nuevos resultados. Conservar los assets viejos; producir en `Assets/SuyuRun/Art/PixelSierra/ProductionV2/`.

Una cuadrícula nativa, vista320x180, escala entera x4, PPU32, PNG RGBA, 24 colores compartidos por Sierra, contorno1px. Sin blur ni alfa gradual. PaletaV2 propuesta para cubrir los tres actos, no sobrescribe la paleta anterior. Imágenes de paisaje generadas de forma independiente, jamás un fondo completo cortado en franjas. Originales generados y prompts conservados. Derivados técnicos y sprites con Pillow, autorizado en la biblia.

## Tres actos y conexiones

1. Camino ondulado de tierra: entrada por teleportación con suelo chamuscado, hierba, ichu, árboles de altura, cercos de MADERA, llamas. Subidas/bajadas reales: perfil de superficie exportado para colisión. Ataques con huaraca y obstáculos originales.
2. Túnel de roca → valle de puentes: varios puentes colgantes separados por tierra, descansos y encuentros. Puentes con cuerda, tablas, balanceo, tensión, daño visible, rotura y caída. Aves anuncian ataque a soportes; el jugador debe interrumpirlas con huaraca. Los estados visuales no sustituyen la futura lógica de integridad/colisión.
3. Otro túnel → ruinas INCAS inspiradas en Machu Picchu (el nombre previo «preincas» queda corregido). Recorrido sobre bloques, terrazas, escaleras y claros entre estructuras. Portal final ceremonial fantástico, distinto al de Costa. Concede traje protector integral de apariencia simiesca/gorila, conserva el rostro del protagonista visible; no presentarlo como traje histórico ni como fauna peruana. Reutilizar el efecto ?! existente sin modificar Costa.

Transiciones por OCLUSIÓN de túnel: cambiar el bioma cuando la pantalla queda cubierta, no crossfade/degradado suave. Cielo y nevados coherentes a lo largo de todo el nivel.

## Animación requerida

Viento en nubes, niebla, hierba y copas; río/agua y espuma; llama idle/marcha; cóndor, halcón y ave de puna con anticipación, ataque, impacto y caída; piedras girando; héroe con brazos y huaraca; polvo/aterrizaje; humo/chispas de teleport; tablas/cuerdas vibrando y rompiéndose; escombros; luces de túnel; portal y transformación del traje/escudo. Cada sheet tendrá tamaños, FPS, pivotes, bucle y finalidad en manifiesto.

## Ritmo y límites de integración

Música de Sierra177.5543s. Repartir tercios orientativos a59.1848s y118.3695s; túneles ocultan conexiones. Conservar SierraSoundtrack.asset y JSON existentes. Los325 pulsos de audio y174 acentos MIDI son base de diseño, NO sincronía validada de oído. No modificar Costa ni recalcular sus pistas. La entrega artística no debe describirse como un nivel completo jugable si faltan course, colisiones, controlador de huaraca, daño a puentes y transición entre escenas.

Referencias locales: `Artifacts/ArtApproval/sierra 3.png`=camino, `sierra 1.png`=puente, `sierra 2.png`=ruinas. Sus textos y héroe no se copian. Son guías de composición y detalle.
