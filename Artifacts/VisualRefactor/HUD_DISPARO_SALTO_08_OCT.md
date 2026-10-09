# HUD, disparo y salto — 08/10/2026

## Revisión artística y continuidad posterior

- Franjas superior/inferior con marco tallado por capas, esquinas ornamentadas, remaches y relieve de píxel; centro despejado. No se estira una ilustración ni se sustituye el arte de coleccionables. Construcción compartida en código, no un nuevo PNG generado.
- Nombre de región más grande; fuente pixel con sombra dura y contraste, texto centrado ajustado a su casilla.
- Corregido un fallo de la revisión previa: comprimir cuatro poses regionales de caída contra una sola pose Selva congelaba Costa/Sierra en el frame inicial. Ahora se reproducen todos los frames regionales a 12 FPS.
- Reloj de vuelo compartido por Costa/Sierra/Selva y minicuevas: impulso, ascenso, nuevo impulso y descenso independientes de interrupciones de disparo. Conserva los dibujos regionales; NO se presenta como copia literal de todas las poses de Selva ni como producción de nuevos frames.
- Compilación y reloj de vuelo PASS; geometría HUD PASS. El intento de tour real en Unity no arrancó (hud_air_play_08.log, salida 1 antes de cargar); capturas del 04/10 son anteriores y no deben presentarse como resultado actual. Falta validación visual del nuevo marco y de la continuidad de los dibujos.

Actualización de alineación: fuente Press Start 2P común, centrado horizontal/vertical y tamaño entero ajustado al espacio. Controles inferiores en una fila independiente del mensaje y de las casillas obtenidas, que conservan coordenadas. Aplicado en Costa, Sierra, Selva y minicuevas. Las cuevas Sierra muestran tres piezas regionales en las casillas existentes, sin inventar una cuarta pieza. Comprobación de geometría ampliada para controles/mensaje/coleccionables; aceptación visual en Play pendiente.

- Franjas de gameplay unificadas mediante RunnerHud.Chrome: relleno opaco oscuro, borde superior oro y borde inferior piedra. No se estiran texturas con marcas decorativas. Se conserva HUD dentro del viewport y el progreso superior.
- Costa/Sierra comparten etiquetas MON/PTS y separador. Selva y minicuevas utilizan el mismo acabado conservando sus indicadores específicos.
- Sonido compartido del disparo: ganancia 0.40 → 0.09 (reducción del 77.5%). No se modifica volumen musical.
- Costa: E / botón derecho / gatillo mantenido controla la duración. Soltar detiene nuevos proyectiles inmediatamente; los ya disparados siguen su trayectoria. Consumo proporcional al tiempo, recarga al soltar; no agota automáticamente una ráfaga entera por un toque.
- Sierra y las minicuevas ya disparaban mientras se mantiene pulsado; se conserva ese comportamiento. Selva admite click mantenido además de E, sin disparar al seleccionar o utilizar una liana. Conserva cargador y recarga existentes.
- Costa dejó de seleccionar únicamente tres poses fijas por velocidad. Usa reloj local para salto, doble salto y caída. Sierra/cuevas distinguen también doble salto. Cadencia y progreso de clips aéreos tomados de Selva, conservando los dibujos regionales (Costa sin brazos, Sierra con brazos, Selva traje protector). No es una copia del traje de Selva sobre Costa ni una generación de nuevas poses.
- Se mantiene escala de cuerpo y anclaje; el salto no hereda el squash del aterrizaje anterior.

Verificado: compilación runtime/Editor PASS, relojes a 30/60/120 FPS PASS y geometría HUD en ocho resoluciones PASS. Pendiente aceptación visual y comprobación auditiva en Play; estas pruebas no certifican la apariencia ni el balance de sonido.
