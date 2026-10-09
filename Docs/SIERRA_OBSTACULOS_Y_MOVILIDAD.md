# Sierra — variedad de obstáculos y movilidad aérea

Se conserva el fondo. Revisión individual de roca rodante (ocho fotogramas), anterior respaldado.
No se generó otro ejecutable. Las secciones inferiores de validación son registros históricos.

## Controles y secuencias nuevas

- Base con marca dorada: impulso automático al pisarla.
- Punto aéreo dorado: recoge una carga; ESPACIO libera el segundo impulso.
- Punto aéreo rojo, flecha abajo: habilita S/↓ durante0.45s para un descenso rápido diagonal.
- El avance horizontal continúa a224px/s; no hay teleport ni inversión de gravedad.
- Marca horizontal de llegada: suelo existente, no plataforma falsa sin colisión.
- Un ave cruza de derecha a izquierda a800px/s en mundo (1024px/s relativos al corredor),
  con aviso0.65s antes. El contacto es letal, sin inmunidad por daño previo; no es objetivo de disparo.
- Tras el descenso aparece una viga en t+1.8s para mantener S/↓ al aterrizar.
- Rocas rodantes: ya NO se destruyen con ráfaga, exigen saltar. Cruzan su posición original
  al tiempo del mapa y ruedan hacia el corredor. Cada una acompaña un ave apuntable para huaraca aérea.
- Agacharse copia la proporción de Costa0.48/1.06 y mantiene los pies apoyados; pose solo en suelo.

Siete secuencias completas distribuidas en los tres biomas. Cada una reserva desde2s antes del inicio
hasta3s después: se retiran los encuentros anteriores incompatibles, no se superponen indiscriminadamente.
El mapa contiene32 encuentros fijos,7 aves veloces,7 vigas posteriores,3 aves de roca,
8 aves de oleadas y3 de puentes (60 encuentros). Los puntos y la recogida reutilizan
`RunnerPickup`, con los tipos4/5 de carga aérea/base impulsora del diseño existente de Costa.

## Tiempo y validación

El inicio de cada ejercicio conserva un acento del MIDI y su sourceIndex. Los desplazamientos de0.36s,
0.70s,0.78s (salida del ave veloz) y1.8s (viga) son coreografía diseñada, NO notas musicales inventadas ni análisis en vivo.
Los tiempos principales no se estiraron; la validación musical de oído continúa pendiente.

`python Tools/test_sierra_mobility.py`: tres pruebas pasan; comprueban corredores libres, correspondencia
de los tiempos con los acentos originales y alcance/aterrizaje de las7 secuencias a30/60/144fps.
La simulación Python verifica las trayectorias matemáticas; Unity Play debe comprobar la integración real.
El reporte automático de Unity ahora incluye Pads/Orbs/Dives para comprobar las acciones, no solo llegar al final.

Resultado real Unity Play: Complete177.55; errors=0; Pads=7; Orbs=7; Dives=7;
25/09/2026 06:38:50-05:00. Las siete secuencias se ejecutaron y el recorrido terminó.
Es autoplay diagnóstico, no una validación manual de dificultad ni musical. Se mantiene pendiente el
acabado general de Sierra indicado en el documento de integración. Costa:0PNG modificados según hashes.
Después de la prueba se añadió protección de respawn para no volver a premiar aves de cruce ya derrotadas;
esa rama de reintento requiere comprobación manual.
