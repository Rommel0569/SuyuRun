# Final cinematográfico V4 — SUYURUN

Revisión del montaje a partir de la retroalimentación del 23/10/2026. Conserva V3 completa; no toca gameplay, recorridos, colisiones ni guardado. No genera ejecutable.

Actualización posterior24/10: el banco completo de esta entrega contiene27hojas/225celdas, incluida carrera Costa16poses, entrada al portal16poses y capa de profundidad del abismo. El detalle y la evidencia más reciente están en MOTION_POLISH.md y MOTION_POLISH_VERIFICATION.json. Los recuentos y pruebas de los párrafos originales siguientes describen el estado previo a ese pulido; se preservan como historial, no como prueba de la revisión nueva.

## Dirección aplicada

20 segundos de película más 10 de créditos. Inicio lateral contenido en Costa (zoom1.08→1, no primerísimo plano), carrera/anticipación/salto/absorción/deslizamiento, frenado en Sierra, preparación de piedra/giro/liberación/continuación del brazo, transición de vegetación a Selva, péndulo/suelta balística/absorción/desenfunde/recoil, entrada por el centro de un portal luminoso, recuerdos ganados por región, saludo y vista de victoria que reúne las tres regiones. Planos y cámara tienen tiempos propios, en vez del mismo acercamiento repetido.

Corrección solicitada24/10: Costa ya no utiliza el actor enmascarado y redondo del primer borrador; se generaron24poses nuevas con rostro humano referenciado en Sierra/Selva, brazos ocultos y ropa Costa. Sierra recibió otra edición cromática neutra, sin piel dorada. Los actores de acción utilizan la misma celda lógica68x68y escala uniforme; no se estiran por frame. Versiones anteriores preservadas enBeforeIdentityFix/.

## Arte

Diez hojas nuevas generadas con la herramienta integrada de imágenes: 24 poses Costa, 24 Sierra, 24 Selva y 24 expresiones/gestos de primer plano, más tres capas independientes del mirador que reúne Costa/Sierra/Selva, dos capas de portal y16poses de llegada/victoria. Se conservan en V4 las nueve capas cinematográficas independientes y los efectos, objetos y ambiente de V3; no se recurre a sprites del gameplay. El banco completo contiene24hojas,192celdas (incluidas doce capas estáticas). Exportación256x256por pose y fondos2560x720, hasta24colores regionales, alfa binario, Point/None. El tamaño de los PNG no implica que cada movimiento esté dibujado a24fps.

La referencia de video enviada por el usuario se estudió por16muestras de sus10segundos; no se integró su metraje. Se aplicó continuidad hacia delante, portal cálido, saludo/victoria y apertura a las tres regiones. El acceso al portal llega primero a su centro y después desvanece al actor, sin encogerlo. Sierra tiene una edición mate de las24poses y la exportación evita sus cuatro tonos más pálidos usando únicamente colores ya existentes de la paleta; no cambia el gameplay de Sierra.

Cada hoja usa una única escala de exportación y el actor mantiene proporción1:1en la transformación de dibujo; las poses no se ensanchan mediante escaladoX. El tamaño cambia únicamente por encuadre/cámara o por el plano general de victoria. La comprobación técnica no sustituye la revisión anatómica visual de cada pose.

Fuentes finales: Sources/. Prompts finales: PLAN_COMPLETE.json. Las correcciones de transparencia se hicieron con la herramienta integrada; extracción, registro, cuantización y vistas de revisión son operaciones mecánicas de exportación.

## Continuidad y mezcla

- Ambiente y desplazamiento usan relojes separados: detener los pies no congela aves/plantas.
- Péndulo con velocidad angular variable; la velocidad inicial de la suelta coincide con su tangente. Gravedad y posición de contacto determinan el aterrizaje. La liana continúa oscilando con amortiguación tras soltarla.
- Exposiciones no uniformes en las poses de preparación, liberación, contacto y recuperación; sin estirar al personaje para simular movimiento.
- Piedra separada de la mano, sin explosión contra un objetivo inexistente ni daño a la fauna decorativa.
- Recuerdos organizados por región usando exclusivamente lo recogido; sin modificar PlayerPrefs.
- Música elegida conservada, en loop, con entrada y salida; foley discreto de pasos/aire y un disparo atenuado. EffectsVol se respeta. La pista original y Sfx/shot siguen siendo dependencias del proyecto, no música nueva.
- Análisis PCM offline de energía cerca de los cortes propuestos; MUSIC_LANDMARKS.json registra candidatos. No es transcripción MIDI ni validación de frase musical por oído.
- Tipografía rasterizada a resolución de salida; créditos con jerarquía, lectura y agradecimiento. Máscaras exteriores impiden fugas de fondos fuera del encuadre.
- Se eliminan las franjas negras interiores durante los20segundos de película y la entrada empieza con luz cálida, no con negro. El encuadre no sale de los límites del fondo en Selva. Solo el paso final a créditos y los créditos mantienen negro deliberado. La profundidad se logra con cámara/parallax y portal por capas; no se añadieron modelos3Dni se afirma una escena3Dreal.

## Prueba y reproducción

SUYURUN > Final > Previsualizar pelicula (Play). La vista previa queda abierta al terminar y permite Repetir/Pausar/Continuar/Cerrar. El final real mantiene su salida tras 30 segundos.

SUYURUN > Final > Verificar final V4 (sin cerrar Unity) prueba dentro del Editor abierto sin cerrar Unity ni reemplazar la escena. El modo batch independiente utiliza una escena temporal y termina al completar. Resultado exacto en PlayAudit/result.txt, capturas reales en PlayAudit/. La primera ejecución V4 encontró texturas importadas al tamaño predeterminado y se corrigió el contrato; no cuenta como prueba exitosa.

La aceptación estética del usuario sigue pendiente. Las comprobaciones técnicas no equivalen a aprobar la fluidez de cada fotograma ni a una película de animación tradicional completa.

Prueba final estable del24/10/2026:35capturas,29.79segundos observados y0errores de ejecución en Unity Play. Revisión visual de Costa0.5s, Sierra8.3s, entrada al portal14.2s y victoria19.5s. Las31capturas previas a créditos superan el control de imagen no vacía y bordes negros completos. Evidencia: PLAY_VERIFICATION.json y PlayAudit/FRAME_VALIDATION.json. Esta prueba cubre el epílogo aislado, no una partida completa de los tres niveles.

## Referencias de dirección

- Ending oficial Dragon Ball DAIMA, NAKAMA: https://www.youtube.com/watch?v=7BNJhj5HTwc
- Frieren, dirección del ending y representación de su historia: https://frieren-anime.jp/news/1359/
- Anticipación, staging y follow-through: https://www.adobe.com/creativecloud/animation/discover/principles-of-animation.html
- Puentes de sonido J/L: https://www.adobe.com/uk/creativecloud/video/discover/j-cut-and-l-cut.html

Se usan principios de dirección; no se copiaron diseños o secuencias de esos animes.
