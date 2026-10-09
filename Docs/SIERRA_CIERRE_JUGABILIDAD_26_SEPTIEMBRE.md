# Sierra — revisión de claridad, combate y salida

## Reglas vigentes del usuario

Esta revisión sustituye la espera obligatoria al final de la canción de las notas
anteriores. Cada cueva conserva su pista de aproximadamente60s como límite normal.

- Derrotar al jefe y a TODOS los esbirros que aparecieron. No se generan nuevas
  oleadas después de vencer al jefe. Los que queden siguen persiguiendo al héroe.
- Llegar al portal con todos derrotados: cuenta de3s y salida anticipada, con
  desvanecimiento de volumen. Alejarse del portal reinicia la cuenta.
- Si termina el minuto y está derrotado el jefe o ya no quedan esbirros, hay una
  única prórroga de5s para completar el objetivo pendiente. Si ambos siguen vivos,
  no se concede prórroga. Con todo derrotado se permite llegar al portal durante
  esos5s. Una cuenta de salida ya iniciada puede terminar sus3s.
- El HUD muestra vida por segmentos, daño reciente, munición, enemigos vivos,
  temporizador normal/prórroga y aviso de quién falta al acercarse al portal.

## Combate y presentación

La embestida inca se prepara1,1s: retroceso inicial, seguimiento del blanco durante
0,6s y fijación. Después carga al punto marcado sin perseguir automáticamente los
movimientos posteriores. Recuperación y retirada a paso visible; zona de impacto
permisiva para esquivar con salto/doble salto. Marca de apuntado/fijación en pantalla.

Las transiciones Sierra→cueva bajan el volumen durante1,6s antes de pausar Sierra;
la cueva entra progresivamente durante1,2s. La salida anticipada desvanece la cueva
durante los3s; Sierra recupera volumen en1,2s desde su segundo congelado. La pista
también baja al acercarse al límite musical. Se cambia volumen, NUNCA tono/pitch ni
velocidad del MIDI. Pausa detiene los relojes y ambas fuentes cuando corresponde.
El final de Sierra baja el volumen antes de detenerse. Los cambios se circunscriben
a Sierra y sus cuevas; no se han modificado las pistas ni el nivel Costa.

Barreras de agacharse/doble salto: silueta biselada, terracota oscura, borde claro y
bandas diagonales; no se confunden con los bloques arquitectónicos. Conservan las
colisiones y posiciones musicales. Arte generado en píxel nativo por el código,
sin sobrescribir PNG aprobados. Impulsos: rombos concéntricos como los de Costa;
azul claro para subir, rojo/naranja para descender, flechas y controles explícitos.

## Verificación

`Artifacts/SierraCaves/model-verification.txt`: reglas de prórroga, objetivo fijado,
movimiento30/60/144FPS, munición/recarga, presupuesto de oleadas, esbirros obligatorios,
cuenta3s/reinicio al alejarse, ambas cuevas superables con pasos fijos y variables.
`Tools/test_sierra_mobility.py`:7pruebas. `Tools/test_sierra_caves.py`:3pruebas.
El resultado completo de Play se actualiza en `Artifacts/SierraV3/unity-play-result.txt`.
Resultado26/09/2026 a12:11:37: COMPLETE177,55s,0errores,7pads/7orbes/7descensos,
2/2cuevas completas,0esbirros pendientes, salida3s, escape y reintento pagado.
La segunda cueva finalizó en45,55s con15esbirros+jefe derrotados. El final de Sierra
llegó con ambas fuentes musicales a volumen0. Capturas de HUD revisadas en Play.
51archivos protegidos de Costa comparados porSHA256:0cambios.
Control adicional a12:12:39: deslizamiento capturado y1pad/1orbe/1descenso,
0errores. Último ajuste de decoración: símbolo de peligro «!» en la barrera;
no afecta colisiones, temporización ni dificultad.
La aceptación manual de dificultad, diseño y mezcla musical sigue siendo del usuario.
No se genera otro ejecutable y no se desarrolla Selva en esta revisión.
