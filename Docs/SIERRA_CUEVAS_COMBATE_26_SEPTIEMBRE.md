# Cuevas de Sierra — revisión de combate

## Implementado

- Hasta15apariciones de esbirros por cueva, máximo5simultáneos. Reaparecen en
  plataformas superiores/inferiores a distancia prudente, con aviso de1s.
  Persiguen al héroe mediante desplazamiento, saltos y caída entre plataformas.
  Durante el jefe el máximo baja a3esbirros vivos. Cada3esbirros derrotados se
  recupera1vida, además de las monedas, para sostener el combate con recarga.
- Pistola:5balas, recarga de4s al vaciarse o con R. E/clic dispara; durante recarga
  se conservan movimiento y doble salto. Retroceso reinicia la cueva; R también
  reintenta cuando se pierde. Munición y cuenta atrás visibles.
- Cinemáticas de6s al acercarse a cada jefe, cámara enfocada, despertar y texto
  en dos partes. El combate se congela, la música continúa dentro de60,0555s.
- Calavera de Piedra: guardia articulada, directo, uppercut ascendente y golpe
  al suelo con ondas. Aviso previo y recuperación; no puede encerrar al jugador
  contra la barrera sin espacio de retirada.
- Pachacútec, el Inca caído: maza, proyectiles solares y embestida, indumentaria
  roja/dorada y esqueleto visible; identidad fantástica, no reconstrucción histórica.
-10hojas nuevas,8frames por animación corporal y4por efecto. Se conservan las
  anteriores, paleta24y píxel nativo. Previews: `Artifacts/SierraCaves/*_combos.gif`.
- Anclaje de túneles al suelo real, tanto transición exterior como salida de cueva.
- Cámara de combate encuadra al héroe y al jefe; la introducción enfoca al jefe.
  Al volver al runner se limita la vida a su máximo, sin transferir vida extra
  de la reserva de5vidas de exploración.
- Cada intento mantiene la duración del MP3 seleccionado. El runner exterior y
  su grilla MIDI se pausan y reanudan en el mismo segundo. Costa no se modifica.

## Contexto histórico y límites

Chavín se relaciona con escultura de piedra y galerías subterráneas. Pachacútec
expandió el poder inca en el sigloXV. El despertar, poderes, calavera y cripta
son ficción y aparecen etiquetados como tal en la cinemática.

- Ministerio de Cultura, Chavín: https://www.gob.pe/institucion/cultura/noticias/126679-la-milenaria-cultura-chavin-lista-para-ser-vista-de-manera-virtual-desde-tu-casa
- Dirección Desconcentrada de Cultura Cusco: https://www.culturacusco.gob.pe/?p=1769
- UNESCO, Machu Picchu: https://whc.unesco.org/en/list/274

## Verificación

La corrida completa anterior de Sierra terminó con0errores y2/2cuevas, pero NO
valida esta revisión de combate. Las nuevas pruebas y su resultado se registran
en `Artifacts/SierraCaves/model-verification.txt` e `integration-verification.txt`.
Resultado26/09/2026 a07:18:50: PASS ambas cuevas completas,0errores, pausa/reanudación
y retorno al runner. Segunda cueva:15apariciones,16bajas contando al jefe,6recargas.
Modelo: controles30/60/144FPS; cargador/recarga; presupuesto15y alturas distintas;
combate completo en ambas cuevas con pasos fijos y DSP variables. Arte:3pruebas
Python aprobadas; movilidad exterior:7aprobadas.51archivos protegidos de Costa,
0cambios. Limpieza del HUD de introducción comprobada después en Play a07:20:11:
`Artifacts/SierraCaves/intro-0.png`, sin controles, munición ni retícula sobre el relato.
Esta última corrida fue detenida deliberadamente tras revisar la introducción;
la prueba completa válida sigue siendo la de07:18:50, no una corrida inconclusa.
Pruebas visuales/de dificultad del usuario pendientes; no declarar aprobación.
No se genera otro ejecutable.
