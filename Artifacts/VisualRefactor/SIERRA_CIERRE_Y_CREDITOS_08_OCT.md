# Sierra, HUD de combate y cierre — 08/10/2026

## Cambios implementados

- Esbirros de piedra: el banco Actors/pebble/walk tiene orientación nativa izquierda. RunnerFacing ahora la registra, en Sierra y también en el renderer Selva. No se confunde orientación nativa con dirección al objetivo.
- Inca: revisión posterior sustituye la caminata reutilizada por MotionV3: 12 poses nuevas de carrera de embestida y 8 nuevas de retroceso defensivo, dibujadas con imagen generativa y exportadas a paleta Sierra. Estados separados, reloj local del ataque, orientación nativa derecha, registro de pie 124 y referenceExtent 102. Conserva colisiones y bancos anteriores. Fuentes, GIF y prompts en IncaLocomotion/ENTREGA.md.
- Sierra: los 3 segundos de transformación/entrada al portal ahora ocupan los últimos 3 segundos de la canción; fin del recorrido a duration-3, cierre a duration. Ganancia musical baja durante el cierre. Portal sigue centrado en 160 al finalizar; pie corregido 5 unidades lógicas por margen transparente medido (10 píxeles nativos). No se cambian colisiones ni los beats previos al tramo de cierre.
- Cuatro minicuevas: panel compacto compartido bajo cabecera, izquierda (6,26,76,21): corazón pixel, vida por segmentos, icono pistola, BALAS n/5 o RECARGA con segundos según estado; Selva maldita indica armas selladas. Eliminadas las duplicaciones de esos datos sobre el título.
- Final Selva: RunnerFinalMovie, montaje automático 20 segundos Costa/Sierra/Selva usando arte y animaciones existentes; carrera, salto, caída, deslizamiento y disparo, transición negra entre regiones. 10 segundos de créditos verticales: ROMMEL C. V., WILLIAM R. Q., JOE F. C., MG. SULLA TORRES, curso y UNSA. Música proporcionada copiada a Assets/SuyuRun/Audio/Resources/FinalMovie/final_music.mp3, loop y fade final de 2 segundos. Luego retorna a resultados. No nivel secreto aún; no nuevos ejecutables.
- Revisión del montaje: capas de detalle nativo del banco Materials, cámara compartida con acercamiento progresivo, arco balístico continuo y aterrizaje explícito, enemigos regionales y ave/objetivo visible para disparo, impacto sincronizado. Tayta usa bounds/PPU registrados y no una celda estirada fija: sheets 128/192 conservan proporción al cambiar de acción. Guacamayo real de Selva, no halcón Sierra. Sigue pendiente validación visual del montaje, no se certifica calidad cinematográfica final.
- Selva runner y minicuevas: disparar en el aire no sustituye abruptamente el cuerpo por el ciclo terrestre de ráfaga; mantiene ascenso/descenso y el disparo físico. Las acciones cuerpo a cuerpo explícitas siguen teniendo prioridad.

## Auditoría interna

387 hojas / 1445 frames: contratos PNG PASS sin fallos tras MotionV3. Compilación runtime/editor PASS. HUD ocho resoluciones PASS; su revisión visual sigue pendiente. Relojes 30/60/120 FPS PASS. Cuatro minicuevas simuladas a 30/60/120 PASS, derrotan jefe/esbirros y salen. Portales: centros y bloqueo de entradas durante salida PASS; combates continúan tras 65 segundos si faltan enemigos. Escenas Costa/Sierra/Selva habilitadas en EditorBuildSettings. Desbloqueos y traje permanente revisados en RunnerProgress.

## Límites importantes

Esto es auditoría interna, no un recorrido visual cronológico en Unity. Último intento Unity se cerró antes de cargar. Falta comprobar visualmente orientación de cada pose, panel de combate, portal final, cierre musical y montaje/créditos. La cinemática implementada reutiliza el banco existente: no se afirma calidad de película ni máximo detalle nuevo en todos los escenarios. Pulido artístico adicional solo según revisión del usuario. Verificar derechos de música antes de distribuir públicamente.
