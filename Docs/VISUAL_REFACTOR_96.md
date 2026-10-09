# Refactor visual — 03/10/2026

Autorizado: aumentar resolución de personajes y dar más acabado a personajes,
jefes y efectos de habilidades. Escenarios: pulido secundario, sin cambiar
recorrido, ritmo, transiciones, música ni mecánicas. Entrega uno por uno.

Primera entrega: Tayta protector Selva, reposo de cuatro frames 96x96, fuente
imagegen y exportación Point/transparente con 24 colores regionales existentes.
Pendiente de aprobación: Artifacts/VisualRefactor/Tayta/selva_idle_96.png.
NO integrado y NO implica que el resto de animaciones esté terminado.

Reposo aprobado por el usuario. Segunda entrega: carrera 96x96, ocho poses,
14 fps, 23 colores, alpha binaria y márgenes verificados. Preparada sin
reemplazar recursos runtime; aprobación visual de carrera pendiente.
TaytaArt ahora acepta dimensiones, PPU y conversión de anclajes desde JSON;
los recursos antiguos sin esos campos mantienen 48x48/PPU32. No se ha
activado una mezcla de trajes viejos/nuevos ni cambiado el render.

Carrera aprobada. Tercera entrega: hoja de salto de ocho poses, con JSON de
selección para jump, double_jump, fall y land. El GIF incluye un arco ilustrativo
para revisar el movimiento; NO es una prueba en Unity ni modifica la trayectoria
física. Fuentes, PNG y metadatos guardados sin reemplazar los sprites actuales.

## Integración prevista, no aplicada todavía

- Leer frameWidth/frameHeight desde metadatos, no constantes 48.
- PPU 64 para celdas 96: mismo tamaño en mundo que 48 a PPU32.
- Conservar pivote, apoyos, hitboxes y posición de portales.
- Convertir anclajes de mano de píxeles de textura a coordenadas lógicas.
- Para mostrar el detalle real hay que revisar el render a 640x360 sin
  cambiar el encuadre lógico de 320x180: renderizar a 320x180 y reducir
  los sprites a 48 de pantalla volvería a perder el detalle añadido.
- No cambiar el renderer hasta aprobar esta primera hoja y verificar el
  resultado de escala con una captura en Unity. Sin nuevo ejecutable.

## Secuencia

1. Tayta: reposo, carrera, saltos, caída/aterrizaje, deslizamiento, armas,
   recarga, lianas, boxeo, daño/muerte, baile/victoria.
2. Transformación Sierra y escudo.
3. Guardián de piedra, Pachacútec y esbirros: cada personaje con sus efectos.
4. Treant, Chullachaqui, invocaciones y esbirros: cada personaje con efectos.
5. Enemigos restantes; después acabado de escenarios, sin alterar flujo.
