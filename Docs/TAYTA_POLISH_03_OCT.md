# Tayta: carrera, disparo y propietario del renderer — 03/10/2026

## Revisión de carrera V3

V3 aceptada posteriormente por el usuario. Ajuste de estabilidad solicitado:
frameOrder sustituye la celda7 extraída del borde derecho de la fuente por la
pose de vuelo3 completa; no redibuja ni cambia el diseño aprobado. Son ocho
pasos temporales con siete dibujos distintos. Selva conserva fase de piernas
entre run/burst, evitando reinicios por cada disparo. Prueba de márgenes laterales
y superiores añadida; cuerpo completo y escala se verifican para run/burst.

V2 rechazada por el usuario: seguía pareciendo trote. Se redibujan únicamente
los ciclos de carrera de las tres formas usando las dos referencias de zancada
que entregó: torso adelantado, rodilla elevada, impulso con pierna trasera y
talón recogido. Tres PNG `run_v3`, ocho frames cada uno; V1/V2 conservados.
Registro horizontal con cabeza como referencia estable, no centro variable de
las piernas. TaytaArt selecciona V3 automáticamente para run. No cambia velocidad
del runner ni física. Imagen fuente `Artifacts/Tayta/run-v3-source.png`, prompt
en `Artifacts/Tayta/Polish/run_v3_prompt.md`; GIF regionales `*_run_v3.gif`.
Compilación runtime y auditoría efectiva48hojas/323frames sin fallos.
Revisión visual del usuario y Unity Play pendientes; no declarado aprobado.

Lote autorizado expresamente por el usuario. No cambia física, obstáculos, música,
armas disponibles, poderes regionales ni hitboxes. No se crea ejecutable del juego.

## Cambios

- Carrera: tres hojas nuevas `run_v2`, ocho poses de contacto/compresión/paso/vuelo,
  rodillas flexionadas y piernas alternadas. 14fps; Costa antes reproducía solo
  2,8fps. Dos píxeles de elevación visual en las poses de vuelo; collider intacto.
- Sierra/Selva: nuevas `burst_v2`, cuerpo completo en las ocho poses, una misma
  pistola compacta gris sostenida por las manos. Se elimina el arma antigua
  flotante en cuevas cuando Tayta está disponible. Costa conserva la ráfaga de
  energía sin brazos, conforme a la progresión aprobada.
- Selva: el Animator antiguo queda desactivado, sin controller; no puede volver
  a escribir los sprites del protagonista anterior. Un solo renderer controlado
  por TaytaArt, escala unitaria y compensación al mirar a izquierda.
- Selva muestra el disparo también al correr y saltar, incluidos mininiveles;
  la maldición mantiene puños/patadas/guardia y bloquea la pistola como antes.
- Auditoría adicional encontró celdas incompletas en revelación de Costa,
  lanzamiento de huaraca y salto/caída/doble salto/daño Selva. JSON `_v2`
  reutilizan la pose completa vecina sin borrar los originales. No son nuevas
  poses dibujadas ni una certificación de suavidad perfecta de todos los ciclos.

## Fuentes y validación

Herramienta imagegen, prompts de carrera y disparo conservados en
`Artifacts/Tayta/Polish/prompts.md`. Fuentes originales generadas:
`Artifacts/Tayta/run-polish-source.png`, `burst-polish-source.png`.
Exportación mecánica: `Tools/normalize_tayta_polish.py`: bandas alfa reales,
escala común por secuencia (no ajuste individual por frame), eliminación de
fragmentos de celdas vecinas, nearest-neighbour y paleta regional sin cambios.
48x48, alfa binario; originales conservados. Costa/Sierra 24 colores permitidos,
Selva usa su paleta V2 existente de28; no se propone otra paleta.

GIF y hojas x4 en `Artifacts/Tayta/Polish/`. Auditoría de los frames que selecciona
realmente TaytaArt: `Tools/validate_tayta_polish.py`. Comprueba cuerpo completo,
paleta, alfa, variación de alturas y poses de piernas de carrera/disparo.
Compilación runtime se ejecuta aparte. Revisión visual en Unity Play pendiente;
no se afirma una partida completa ni ausencia absoluta de bugs.
