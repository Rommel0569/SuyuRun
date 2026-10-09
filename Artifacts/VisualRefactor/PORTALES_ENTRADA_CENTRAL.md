# Entrada visual al portal — 04/10/2026

Tayta converge hacia el interior desde el centro real de sus bounds, independiente de pivote o dirección del sprite. Reducción y desvanecimiento únicamente durante absorción, no durante gameplay/disparo. Aplicado al cierre Costa, cierre Sierra, dos cuevas Sierra, dos mininiveles Selva y cierre Selva. Se conservan recompensa, baile previo, cuenta regresiva, condición de enemigos derrotados, colisiones y transición musical.

Reinicio Costa restaura alfa para evitar protagonista invisible tras repetir. Selva restaura color en cada presentación y retira el halo de escudo durante absorción para que no quede flotando fuera del portal.

Compilación runtime/Editor y ocho pruebas de centrado del modelo: PASS. Falta comparar visualmente la nueva absorción en todos los finales en Unity Play; no se declara revisión visual completa.
