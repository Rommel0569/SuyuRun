# Sierra: correcciones de jugabilidad y huaraca

## Alcance de esta entrega

Costa se conserva sin cambios. Se trabaja en `Level_Sierra_Integration.unity`, sin generar ejecutables.
La regla vigente de revisión individual se respeta con un solo asset animado entregado: la huaraca
(cargar, girar y lanzar). El rediseño gráfico de enemigos queda pendiente de esa aprobación.

## Implementado

- Velocidad 224 px/s (7 unidades/s a PPU32, igual a Costa); antes era72.
- Salto y segundo salto aéreo por pulsación, se recuperan al aterrizar.
-29 encuentros conservan sus tiempos y procedencia MIDI; dos obstáculos elevados requieren doble salto.
- Clic válido sobre el cuerpo de un ave selecciona objetivo; clic en vacío no dispara.
- Secuencia de huaraca: carga0.2s, giro0.5s, lanzamiento desde0.7s; piedra sale en0.8s.
- Objetivo retenido durante la secuencia y piedra dirigida al ave en movimiento. No autofuego de huaraca normal.
- Ráfaga E independiente. Mira intermitente y fija durante selección.
- Vuelo con deambulación continua, aviso de aproximación y persecución; variación determinista por ave para permitir reintentos consistentes.
- Aves decorativas de Costa reutilizadas en bandadas, sin modificar sus archivos.
- Se retira la franja cercana que hacía flotar cercas: módulos independientes siguen el perfil físico del suelo.
- Huaraca48x48,12frames, dos cuerdas, cuna roja, piedra, flexión/extensión del brazo, piernas en movimiento.
- Respaldo de sprites anteriores en `Artifacts/SierraV3/BeforeHuaracaCorrection`.

## Verificación y pendientes

La prueba automática anterior llegó a177.55s sin errores, ANTES de estas correcciones; no demuestra
que el nuevo balance sea correcto. Hay que ejecutar nuevamente Unity Play y probar manualmente la selección.
No se ha validado de oído la diferencia de duración MIDI170.767s / MP3177.554s. No se estiró el MIDI.
Pendientes: aprobación visual de huaraca, mejorar pixel art de enemigos, acabado adicional del fondo,
calibración musical, balance de ataques/puentes a la nueva velocidad y recorrido manual completo.

Generador incremental: `Tools/build_sierra_huaraca.py`; después de regenerar todoV3, volver a ejecutarlo.
Vista de revisión: `Artifacts/SierraV3/huaraca_accion.gif` y `huaraca_secuencia_x4.png`.
