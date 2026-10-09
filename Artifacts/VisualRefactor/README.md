# Banco de refactor visual — avance 01, 03/10/2026

## Actualización: paquete técnico Avance02

91 hojas / 418 frames exportados: incluye las 65 hojas del primer banco,
12 hojas MotionV2 de los cuatro bosses (96 poses adicionales), y 14 hojas
core Costa/Sierra (64 poses). Archivo Banco_Refactor_Avance02.zip.
Catálogo unity_catalog.json con rectángulos, pivote, PPU, tiempos y hashes.
Menú de importación a staging compilado sin errores con las referencias del
Editor instalado. No ejecutado en Unity; no activado en gameplay.

Core Costa/Sierra se reexportó desde nuevas fuentes por componentes completos,
no por cortes nominales que recortaban la primera celda. Escala corporal única
según idle, celdas 192x128 para alojar poses anchas sin encoger al actor;
offset explícito respecto a celda 96. El banco necesita revisión de identidad,
sombrero, apoyos y registro en Play antes de activarse. Estados extra de huaraca,
recarga, recompensa y transformaciones aún pendientes de exportación final.
No es todavía el banco completo de TODOS los personajes/enemigos del juego.

Autorizado en lote por el usuario. Calidad de reposo/carrera/salto de Tayta
protector aprobada como dirección. Fuentes originales y sprites anteriores
conservados; este banco NO está integrado ni probado en Unity Play.

## Exportado

- Bank/TaytaSelva: 25 hojas, 98 frames nativos 96x96. Todos los estados actuales
  del protector: reposo, carrera, saltos, caída, aterrizaje, deslizamiento,
  armas/recarga, guardia, lianas, combate, daño, derrota y celebración.
- Bank/stone, inca, treant, chulla: ocho estados por jefe, cuatro frames por
  estado, 128x128. Total 32 hojas / 128 frames. Incluye reposo, despertar,
  caminar, anticipación, ataque, carga, daño y derrota.
- Bank/Effects: ocho efectos / 32 frames 96x96, separados de los personajes.
  Raíces, ataduras, maldición, invocación, impactos, escudo.
- Cada hoja tiene PNG transparente, JSON y GIF de revisión; manifest y preview.
- Exportación mecánica verificada: dimensiones, <=24 colores, alpha binaria,
  márgenes. Esto NO verifica anatomía, interpolación, timing de gameplay o bugs.

## Fuentes nuevas guardadas, no listas para integración

- Heroes/costa_source.png: el generador hizo dos filas de ocho poses de carrera,
  no cuatro; requiere extracción adaptada. Conservar brazos ocultos hasta final.
- Heroes/sierra_source.png y sierra_extra_source.png: movimientos y huaraca,
  incluida transformación al traje. Falta normalizar a escala corporal común;
  la pose de brillo incluye un panel no deseado que debe corregirse/separarse.
- Borradores opacos de stone/treant conservados en Bosses, NO usados en Bank.
  Exportación usa stone_source_v2 y treant_source_v3 transparentes.

## Pendiente: NO es todavía el banco completo de todo el juego

1. Terminar exportación y QA visual de Costa/Sierra sin recortes, sin variación
   de tamaño al disparar y con huaraca completa sin reducir al personaje.
2. Esbirros de piedra, esqueletos, brotes, ecos y demás enemigos activos.
3. Caimanes, aves de combate y enemigos de Costa; revisar fauna/personajes NPC.
4. Efectos adicionales de cada ataque, proyectiles, transiciones/recompensas.
5. Ajustar actuación y duración de animaciones de jefes contra sus ataques
   reales; separar jab/uppercut y fases de carga donde el código lo requiera.
6. Verificar anclajes de lianas (estimados) y siluetas frame por frame.
7. Integrar tras QA, revisar render 640x360 conservando encuadre lógico 320x180,
   pivotes, tamaño corporal, colisiones, portales y sonido. Sin nuevo ejecutable.
8. Pulido secundario de escenarios: no cambiar recorrido ni mecánicas.

Scripts reproducibles en Tools/build_tayta96_bank.py, build_boss128_bank.py,
build_effect96_bank.py. Prompts en bank_prompts.md. No se promete cero bugs.
# Estado vigente — 03/10/2026

Banco ampliado e integración visual en el proyecto. Consultar ESTADO_ACTUAL.md
y los informes runtime_audit.txt / play_smoke.txt. Las secciones siguientes
conservan el historial; sus avisos de «no integrado» corresponden a avances anteriores.

