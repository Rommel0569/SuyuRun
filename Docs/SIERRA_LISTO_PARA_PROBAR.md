# Sierra — entrega integrada para prueba del jugador

Reglas más recientes de cuevas y claridad visual:
`SIERRA_CIERRE_JUGABILIDAD_26_SEPTIEMBRE.md`. Este documento sustituye la duración
obligatoria del minuto: ahora se puede salir antes con todos derrotados y portal3s.

Actualización: ahora incluye dos cuevas entre biomas. Leer primero
`SIERRA_CUEVAS_25_SEPTIEMBRE.md` para controles, límite musical y estado de pruebas.
Revisión de combate: `SIERRA_CUEVAS_COMBATE_26_SEPTIEMBRE.md` — pistola5balas,
R recarga4s, hasta15esbirros, cinemáticas y ataques propios de ambos jefes.
Los177,55s inferiores corresponden únicamente al runner; las cuevas añaden120,11s
de música más entradas/salidas, pausas y reintentos del jugador.

## Abrir

Escena: `Assets/SuyuRun/Scenes/Level_Sierra_Integration.unity`.
El nombre técnico conserva referencias anteriores; es el recorrido completo, no una escena recortada.
En Unity: `SUYURUN > Sierra V3 > Abrir integración`, después Play. No hay ejecutable nuevo.

## Recorrido

- Camino ondulado, túnel, valle con tres puentes y terrenos intermedios, túnel, ruinas.
- 177.55segundos de música, mapa fijo:60encuentros y21puntos de movilidad.
- Rocas rodantes saltables, vigas, pinchos, barro escapable, guardianes y aves en oleadas.
- Siete secuencias de impulso/doble impulso/descenso para esquivar aves letales y agacharse después.
- Huaraca con carga, giro, lanzamiento y blanco fijado; ráfaga con energía limitada.
- Checkpoint seguro después de la mitad, reintento5monedas y retorno visual.
- Museo permanente, monedas/banco compartidos, mejoras de vida/ráfaga/imán, recompensa protectora y Selva desbloqueable.

## Controles

|Acción|Control|
|Salto / segundo salto|ESPACIO o flecha arriba|
|Impulso de un orbe aéreo|ESPACIO|
|Descenso en punto rojo|S o flecha abajo, pulsación nueva|
|Agacharse al aterrizar|Mantener S o flecha abajo|
|Huaraca|Clic sobre un ave apuntable; el blanco queda fijado durante el lanzamiento|
|Ráfaga|Mantener E mientras quede energía|
|Barro|Pulsar ESPACIO3veces|
|Pausa y servicios|ESC; pestañas RESUMEN, MUSEO, AJUSTES, TIENDA|
|Reiniciar|R fuera de pausa|

## Cierre de integración

- HUD y ventanas comparten RunnerHud/RunnerModal con Costa; los botones Sierra son pixelados.
- Recogida, gasto/progreso y paso de física comparten RunnerPickup/RunnerProgress/RunnerMotion.
- RunnerPresentation comparte cámara de portal y cierre/revelado en píxeles. Acercamiento1x/2x
  por cortes enteros: no se inventan píxeles suavizados mediante zoom fraccional.
- Costa conserva recorrido, arte, balance y música; solo su cierre autorizado se conecta a Sierra.
- ESC también funciona en interludios. Reanudar restaura reloj y audio sin avanzar el mapa durante pausa.
- Ajustes de música, compensación del juicio rítmico y tap-tempo. El ajuste NO desplaza los obstáculos
  ni estira el MIDI; evita presentar una medición estimada como una corrección validada de oído.
- Rachas y puntos por acciones cercanas al pulso; sonidos de acciones/recogidas originales sintetizados.
- No se sustituyeron fondos ni PNG durante este cierre. Halcón y roca ya aprobados.

## Alcance de la prueba

La entrega funcional se comprueba en Unity Play y con pruebas automáticas. No sustituye la
aprobación estética/dificultad del jugador ni una validación auditiva humana de la alineación MIDI/MP3.
Selva NO se implementa en esta entrega: se guarda su desbloqueo y el traje, pero no se carga una escena ficticia.

Resultados25/09/2026:
- Recorrido completo20:58:03:177.55s, errores0,7impulsos/7orbes/7descensos, escape y reintento pagado.
- Interfaces20:54:17: pausa/museo/ajustes/tienda/reanudación, errores0; capturas revisadas.
- Portal21:02:45: Costa Ending→cierre→carga real Sierra→llegada→Running; replay vuelve a Camino; errores0.
- Siete pruebas Python pasan; verificación de gasto, recogida única y aislamiento del guardado pasa.
- Una primera prueba del portal se invalidó por recompilar durante Play (cachés de Costa perdidas).
  Se detuvo y repitió en limpio; el resultado citado es el pase posterior sin errores, no esa ejecución invalidada.
- Arte protegido de Costa:0PNG cambiados. Fondo Sierra conservado.

Reportes reproducibles en `Artifacts/SierraV3`: `unity-play-result.txt`, `services-verification.txt`,
`portal-verification.txt`, `shared-verification.txt`. El resultado no afirma validación manual de oído.

## Fuentes de las fichas culturales

- [Ministerio de Cultura — Museo Nacional de Chavín](https://www.gob.pe/institucion/cultura/noticias/46596-ministerio-de-cultura-celebro-el-v-aniversario-de-la-creacion-del-museo-nacional-de-chavin).
- [Guía Caminos Andinos — cabezas clavas en los muros del templo](https://visitaareasnaturales.sernanp.gob.pe/wp-content/uploads/2022/01/Guia-Caminos-Andinos.pdf).
- [The Metropolitan Museum — Kero](https://www.metmuseum.org/art/collection/search/316839).
- [Harvard Library — registros con quipus](https://library.harvard.edu/about/news/2023-04-12/long-w-2-there-was-quipu-accounting-systems-incan-and-andean-peoples).

Cada ficha puede abrir su fuente desde el juego. Chavín se distingue de las ruinas incas: no se
atribuye la cabeza clava a Machu Picchu. El guardián del puente es un personaje fantástico original.
