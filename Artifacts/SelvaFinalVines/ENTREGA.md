# Selva: regreso a las lianas antes del portal final

Pedido del 27/10/2026: tras Chullachaqui, caminar brevemente, volver al balanceo y regresar al camino del portal.

- Se conserva la ruta y la sección musical existente: no se agrega otro bioma ni se mueve el portal.
- Entrada de 480 unidades (3 s a velocidad de carrera); ocho estanques de 704 unidades con tres lianas cada uno, orillas de descanso, victorias regias y caimanes existentes.
- Camino continuo desde la última salida del agua hasta el portal (aproximadamente 693 unidades con el mapa actual). Los tiempos de balanceo dependen del jugador.
- Se reutilizan arte, péndulo, agarre, impulso de suelta, traje y animaciones existentes. Los nudos del tramo final se dibujan delante del follaje para que sean visibles.
- Los eventos de suelo del segmento sustituido no se renderizan ni colisionan ni interceptan disparos; entrada y salida conservan los suyos. Las recuperaciones tras caer buscan una orilla real.
- El río original mantiene sus 27 lianas y nueve estanques. El nuevo tramo tiene 24 lianas y ocho estanques.

Ver `result.txt`: prueba controlada en Unity de recursos/geometría, 24 agarres/sueltas y parada sobre suelo del portal; no certifica un recorrido humano completo ni la dificultad. Capturas de terreno sin HUD OnGUI en `river.png` y `final_path.png`. Compilación runtime/Editor comprobada; no se genera ejecutable. Aceptación del usuario pendiente.

## Corrección del cierre musical y celebración

Tras probar las lianas, el usuario confirmó el recorrido y reportó que la música terminaba antes de llegar y se perdía la celebración. Se reserva la cadencia final; únicamente si el jugador todavía está recorriendo el bioma se repite una frase de ocho intervalos de la grilla MIDI, con fundido de 240 ms. Esto puede extender la canción al demorarse en el balanceo; no impone un límite fijo de duración al jugador. Al llegar, se garantiza al menos 2,5 s de celebración antes de los 1,4 s de entrada y fundido. No cambia el epílogo cinematográfico aprobado. Prueba controlada de cursor musical, selección de baile y secuencia final en Unity: PASS; no constituye aprobación de oído del empalme.
