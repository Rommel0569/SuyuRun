# Estado de aprobación de arte

Dirección vigente: `ART_BIBLE.md`. El cambio de estilo NO convierte el arte previo en pixel art aprobado.

## Sierra y Selva — se paran acá, las capas de escena se producen aparte

El usuario decidió parar la producción de capas de Sierra/Selva de este lado: las capa 3 (cerros) y capa 4/Acto 1 (camino) que se habían generado quedaron eliminadas. Lo único que queda de esta entrega:
- Paleta de Sierra aprobada (`sierra_palette.json`) + capas 1 (cielo) y 2 (nevados), ya aprobadas antes de la decisión de parar.
- Paleta de Selva propuesta (`selva_palette.json`, `Assets/SuyuRun/Art/PixelSelva/`), 24 colores según sección 8 de la biblia (verdes profundos, verde lima, marrón río, turquesa, naranja, rojo guacamayo), más azul noche y dorado para el templo final. Pendiente de aprobación del usuario.
- No se genera más arte de capas de Sierra/Selva de este lado hasta nueva indicación.

## Sierra — paleta aprobada, estructura multi-bioma

- `Assets/SuyuRun/Art/PixelSierra/sierra_palette.json`: 24 colores, amanecer dorado. Aprobación recibida: «esta bien la paleta de colores de sierra hazlo».
- Sierra deja de ser un fondo continuo como Costa: son **3 actos que se funden entre sí** (crossfade + parallax blending + niebla degradada + cambio de paleta gradual, técnica pedida por el usuario, aplicada también acá y no solo en Selva):
  1. Camino de Tierra (referencia "sierra 3"): sendero andino, llama, cerros verdes.
  2. Puente Colgante (referencia "sierra 1"): cruce sobre el valle, cóndor, río abajo.
  3. Ruinas Preincas (referencia "sierra 2"): andenes escalonados, remate visual del nivel.
- Cielo (capa 1) y capas de fondo lejano quedan continuas/compartidas para todo el nivel (el arco de amanecer avanza con el tiempo, igual que el atardecer de Costa); las capas medias (estructuras/terreno) sí cambian por acto, mezclándose entre sí en vez de cortar de golpe.

## Pendientes 1 y 2 resueltos (fuente pixel + resultados)

- Fuente pixel real conectada: `PressStart2P-Regular.ttf` (Google Fonts, SIL Open Font License, licencia libre) en `Assets/SuyuRun/Art/Fonts/`, aplicada a los 4 estilos base de texto que usa todo el juego (título, texto, chico, botón) — se propaga automáticamente a inicio, museo, tienda, HUD y pantallas de pausa/muerte/completado.
- `costa_resultados.png` conectado: reemplaza la caja plana en Dead/Complete/Paused, manteniendo su proporción nativa 220x140. En Complete además muestra monedas/puntos/banco reales y las 4 vitrinas de piezas.
- Pantalla de Ajustes nueva (`RunnerSettings.cs`): volumen de música y efectos, funcional y persistente. La calibración de latencia/tap-tempo de la sección 14.4 de la biblia queda pendiente, no se inventó.
- Aviso: `costa_inicio.png` solo tiene 5 botones grabados (JUGAR/NIVELES/MUSEO/TIENDA/SALIR — TIENDA ocupó el lugar que la biblia dibujó como AJUSTES). Ajustes se accede por un link de texto simple en el cielo vacío arriba a la derecha, no por un sexto botón en pixel art.

## Entrega actual

- Costa, capa 1: cielo de atardecer con sol bajo y nubes con dithering. PNG independiente 640x180 RGBA, sin mar, terreno, edificios, aves, personajes ni UI.
- Factor 0.0. El cielo es la excepción explícita al tile sin costuras. No repetir el sol como capa móvil.
- Cielo de fondo opaco, con canal alfa: no se recorta el cielo para fingir transparencia. La transparencia corresponde al espacio vacío de las capas superpuestas.
- Vista de juego: ventana central 320x180 ampliada x4 nearest-neighbor. Es una previsualización del mismo asset, no otra capa.
- Paleta de Costa APROBADA por el usuario: 24 colores compartidos, `Assets/SuyuRun/Art/PixelCosta/costa_palette.json`. El cielo usa un subconjunto. No cambiar colores sin nueva autorización.
- No sustituir el fondo jugable antes de la aprobación.
- PNG producido con Pillow, verificado 640x180 RGBA, 10 colores usados del conjunto de 24 y ningún color ajeno. Metadata preparada con Point, sin compresión ni mipmaps y PPU candidato 32 para todos los futuros sprites. La cámara del juego aún no se ha migrado.
- Fuentes conservadas: `SourceMedia/El Alcatraz.mid` y `Docs/References/art_bible_style_reference.png` (solo referencia visual, no reutilizada como píxeles del asset).

## Checklist

- [x] Biblia completa guardada y enlazada desde instrucciones del proyecto.
- [x] Capa 1 y paleta aprobadas explícitamente: «si apruebo».
- [x] Capa 2 aprobada: «continua me gusta».
- [x] Capa 3 aprobada: «continua me gusta».
- [x] Capa 4 aprobada: «Esta excelente».
- [x] Capa 5 aprobada: «continua segun la biblia».
- [x] Capa 6 aprobada: «apruebo continua».
- [x] Capa 7 aprobada: «si esta bien aprobado». Las 7 capas obligatorias de parallax están completas.
- [ ] Plano frontal opcional (1.3) — saltado por pedido del usuario, se puede retomar después.
- [x] Plataformas (bloque, rampa, pilar) aprobadas: «esta bien, continua».
- [x] Protagonista aprobado: «esta bien, continua».
- [x] Obstáculos/enemigos vivos aprobados: «esta bien continua».
- [x] Coleccionables aprobados: «listo continua».
- [x] Efectos aprobados: «listo». Arte de mundo de Costa (a)–(e) completo.
- [x] Integración en Unity (Prototype_Costa.unity): cámara, 7 capas, suelo, plataformas, héroe y muestra de obstáculo/coleccionables, verificado funcionando en Play.
- [x] (f) Resultados y animación de logro aprobados, conectados al juego con puerta dimensional, "?!" y absorción: «ya si esta bien todo».
- [x] (g.1) HUD aprobado: «apruebo el hud que hiciste». Sin barra de vida — Costa es muerte instantánea, no tiene sistema de vida. La barra de vida por secciones (referencia: imagen pegada por el usuario en su ChatGPT del 19 sept) corresponde recién al nivel 3 (Selva), no a Costa.
- [x] (g.2) `costa_inicio.png` (320x180): logo SUYURUN piedra/oro, cielo con dithering real, dunas con vetas, guiño frío a Sierra a la derecha (paleta de Costa únicamente), 5 botones, cursor. Aprobado.
- [x] (g.3) `costa_selector.png` (320x180): tarjeta Costa con miniatura real + 2/4 piezas, tarjetas Sierra/Selva bloqueadas con candado y silueta. Aprobado.
- [x] (g.4) Museo: `costa_museo_pared.png`, `costa_museo_vitrina.png`, `costa_museo_panel.png`. Aprobado.
- [x] (g.5) `costa_tienda.png` (280x180): mejora de RÁFAGA, imán de monedas, tercer slot bloqueado (sin stat de vida inventado). Aprobado.
- Aprobación final recibida: «si esta bien todo ok». **Con esto, el checklist de arte (a)-(g) de Costa según la sección 12 de la biblia queda completo.** Falta cablear inicio/selector/museo/tienda al juego (hoy usan cajas OnGUI planas, no estos PNG) — el HUD y el final ya están conectados.
- Pase de "más autenticidad" aplicado a las 5 piezas: dithering real en costuras de cielo (capa1), vetas/estratos cortos en dunas y siluetas, marcas de junta de adobe en bordes de paneles/tarjetas/botones (vocabulario de `costa_plataformas.png`), remaches en marcos de barra, ladrillo con sombreado alterno en el museo. Primer intento se pasó de ruidoso (dithering muy denso, líneas tipo renglón de cuaderno) y se corrigió a algo más sutil.
- [ ] Suelo, plataformas, rampas y pilares.
- [ ] Protagonista y todas sus animaciones.
- [ ] Enemigos, coleccionables y efectos.
- [ ] HUD, inicio, selector, resultados, museo, ajustes y tienda pixel art.
- [ ] Pixel Perfect Camera, importación y parallax integrados tras aprobar assets.
- [ ] Extracción MIDI con mido, notas dudosas y validación auditiva.
- [ ] Recorrido Costa rediseñado y probado contra el MIDI.

El MIDI aportado se conserva como fuente; todavía no se analizó, por lo que no hay una lista de notas dudosas. No modificar el timing en esta entrega de cielo.

## Capa 2 — aprobada

- Solicitud posterior: pájaros que se muevan o alejen de forma continua y continuar con la siguiente capa. Se mantienen cielo y paleta aprobados sin cambios visuales.
- `costa_capa2_nubes.png`: capa transparente independiente 640x180, borde izquierdo/derecho idéntico, factor 0.05; formas altas y alargadas, sin sol ni paisaje.
- `costa_capa2_aves.png`: sprites auxiliares de ESTA capa (no otra capa de paisaje). Hoja 96x48: 6 frames de 16x16 por fila, 3 distancias dibujadas sobre la misma cuadrícula. Separados de las nubes para volar independientemente; no una captura de la escena troceada.
- `PixelCostaSkyLayer.cs`: componente aislado preparado para Unity, dos copias de nubes, 4 bandadas de 5 aves, planeo/aleteo, reciclaje fuera de pantalla, posiciones enteras y reloj DSP con pausa. Recibe distancia mediante SetTravelPixels. Texturas a asignar después de aprobar; no conectado aún al nivel ni probado en Play.
- Previsualización animada `Artifacts/ArtApproval/costa_capa2_movimiento.gif`: muestra 16 segundos, luego reinicia la demostración. No representa el límite del vuelo continuo del componente. Vista 320x180 escalada x4, sobre una copia de previsualización del cielo aprobado.
- Validación: paleta sin cambios, transparencia binaria, bordes coincidentes, cielo intacto por SHA256 y al menos 10 aves visibles en cada frame muestreado. Informe `costa_capa2_validation.json`.
- Confirmación recibida: «continua me gusta». Se autoriza producir capa 3, sin cambios a los assets aprobados. No se ha integrado el nuevo fondo completo ni se ha modificado el ritmo MIDI.

## Capa 3 — aprobada

- `Assets/SuyuRun/Art/PixelCosta/costa_capa3_paracas.png`: PNG independiente RGBA 640x180. Siluetas lejanas separadas por espacio abierto, Candelabro interpretado como grabado discreto en ladera. Cinco colores de la paleta aprobada, sin dithering de terreno ni colores nuevos.
- Factor de parallax 0.15; PPU 32, Point, sin compresión ni mipmaps. Dos copias desplazadas se pueden reciclar; los bordes izquierdo/derecho son idénticos y transparentes. No es una franja extraída del cielo.
- Línea de agua prevista en y=146; zona inferior y espacios entre penínsulas transparentes para no tapar el futuro mar. Esta entrega NO crea el mar ni cambia capas 1/2.
- Vista `Artifacts/ArtApproval/costa_capa3_parallax.gif`: demostración de 12 s sobre copias de las capas aprobadas. Muestra 0.15 frente al 0.05 de nubes; al acabar se reinicia la demostración. No integrado aún al nivel Unity.
- Verificación: tamaño, alfa binario, paleta, coincidencia de bordes y hashes de cielo, nubes, aves y paleta sin cambios. Informe `Artifacts/ArtApproval/costa_capa3_validation.json`.
- Referencia cultural de ubicación en una duna: [PROMPERÚ / Paracas e Islas Ballestas](https://www.peru.travel/es/atractivos/paracas-e-islas-ballestas). Dibujo original estilizado, no reconstrucción arqueológica; no se atribuye un significado ni datación al geoglifo.
- Aprobación recibida: «continua me gusta». Se autoriza capa 4 sin alterar las anteriores.

## Capa 4 — aprobada

- Archivo único `Assets/SuyuRun/Art/PixelCosta/costa_capa4_mar.png`, RGBA 640x720: cuatro frames de 640x180 apilados de arriba abajo, sin padding. Es una hoja de animación de una sola capa, no un escenario cortado en franjas.
- Mar turquesa oscuro en planos de color, crestas pixeladas con ciclo suave, garúa localizada en el horizonte y un caballito de totora lejano con silueta sentada y remo. Balanceo de un píxel, sin rotación interpolada ni blur. Dithering exclusivamente en la garúa.
- Parallax 0.30; reproducción propuesta de cuatro frames por segundo (ciclo de un segundo). Para Unity, rectángulos (x,y,w,h): (0,540,640,180), (0,360,640,180), (0,180,640,180), (0,0,640,180), en ese orden. Preparado Point/None, sin mipmaps, PPU 32. Recorte de frames, animador y conexión al nivel pendientes tras aprobación.
- Línea del mar y=146; transparencias superiores conservan el paisaje, el sol y el geoglifo. Se repite con dos copias en cada frame; los extremos laterales coinciden exactamente.
- Vista `Artifacts/ArtApproval/costa_capa4_mar.gif`: 12 segundos de composición con las capas aprobadas y movimiento de aves; reinicia la demostración. No es captura de Unity ni integración del nivel.
- Validación `costa_capa4_validation.json`: cuatro frames distintos, dimensiones, alfa binario, colores de paleta, bordes y hashes de todos los assets previos sin cambios.
- Aprobación recibida: «Esta excelente». Se produce la capa 5 manteniendo las anteriores intactas.

## Capa 5 — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_capa5_huacas.png`, PNG RGBA independiente 640x180. Huaca de cuatro terrazas con escalera frontal y juntas de adobe, más dos muros bajos aislados con frisos originales de peces y olas inspirados en Chan Chan. Interpretación artística, no reconstrucción arqueológica.
- Factor 0.50, Point/None, sin mipmaps, PPU 32. Bordes transparentes idénticos. Sin dithering de arquitectura ni colores nuevos; contornos duros de un píxel.
- Es arquitectura decorativa sin colisión, distinta de las futuras plataformas jugables. Las bases llegan al borde inferior y se asentaran visualmente con las capas de orilla/suelo que aún faltan. No añade otra plataforma ni cambia la física.
- La geometría ocupa 5,65 % del lienzo; incluso en la ventana horizontal más densa deja 155 de 320 columnas sin arquitectura. El mar sigue visible a través de los espacios; no es una pared continua.
- Vista `Artifacts/ArtApproval/costa_capa5_huacas.gif`, 12 segundos de composición, reinicio de demostración; no es una captura de Unity. Validación en `costa_capa5_validation.json`: dimensiones, paleta, alfa binario, bordes y hashes de capas 1–4 sin cambios.
- Aprobación recibida: «continua segun la biblia». Se produce la capa 6 manteniendo las anteriores intactas.

## Capa 6 — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_capa6_algarrobos.png`, PNG RGBA independiente 640x180. Dos algarrobos secos (tronco nudoso, ramas angulares desnudas, sin follaje lleno), dos grupos de rocas y dos lobos marinos echados sobre las rocas, en silueta oscura de la paleta.
- Factor 0.75, Point/None, sin mipmaps, PPU 32. Bordes transparentes idénticos (columna 0 = columna 639). Sin dithering (reservado a cielo/niebla); contornos duros de un píxel.
- Elemento de plano medio decorativo, sin colisión; distinto de las futuras plataformas jugables. No se atribuye a una playa o reserva específica; interpretación original de fauna y flora costera peruana.
- `Tools/pixel_costa_preview.py` se extendió para incluir también la capa 5 (huacas, factor 0.50) en la composición de revisión, conservando sin cambios cómo se dibujan las capas 1–4; antes solo mostraba capas 1–4 más la nueva.
- Vista `Artifacts/ArtApproval/costa_capa6_algarrobos.gif` y `costa_capa6_algarrobos_preview_x4.png`, composición de 12 s, reinicio de demostración; no es captura de Unity. Validación en `costa_capa6_validation.json`: dimensiones, paleta, alfa binario, bordes y hashes de capas 1–5 sin cambios.
- Aprobación recibida: «apruebo continua». Se produce la capa 7 manteniendo las anteriores intactas.

## Capa 7 — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_capa7_suelo.png`, PNG RGBA único de 32x32 (no un panel de 640x180 como las capas anteriores: la biblia pide esta como **tile** de suelo jugable). Curva de adobe en las filas superiores, friso original inspirado en textiles Paracas (chevrón escalonado, periodo 8px, interpretación estilizada — no copia de una pieza documentada) y relleno de arena con grano disperso en posiciones fijas.
- Factor 1.0 (se mueve con el jugador, es la superficie jugable inmediata). Sin transparencia parcial (opaco, alfa 255 en todo el tile). Costura izquierda = derecha y también arriba = abajo, para poder apilarse verticalmente si hace falta suelo más grueso. Sin colisión incluida en el PNG; eso lo define el nivel.
- Para la vista de revisión, el tile se repitió 20 veces a lo ancho (640px) formando una franja y se compuso igual que las demás capas — el compositor no cambió su forma de dibujar capas 1–6.
- Vista `Artifacts/ArtApproval/costa_capa7_suelo.gif` y `costa_capa7_suelo_preview_x4.png`. Validación en `costa_capa7_validation.json`: tamaño 32x32, paleta, alfa, costuras y hashes de capas 1–6 sin cambios.
- Aprobación recibida: «si esta bien aprobado». Con esto quedan completas las 7 capas de parallax obligatorias de Costa.

## Plataformas (bloque, rampa, pilar) — aprobado

- El usuario pidió seguir con la biblia hasta tener todo listo para el nivel Costa. Se salta el plano frontal opcional (1.3, decorativo) y se pasa directo a lo jugable, según el orden de la sección 11: capas → plataformas → protagonista → obstáculos → coleccionables → efectos → HUD → inicio. Se sigue entregando un asset por vez y esperando confirmación.
- `Assets/SuyuRun/Art/PixelCosta/costa_plataformas.png`, PNG RGBA 128x64: bloque de roca/adobe escalonado (48x32, dos terrazas, cara iluminada y cara en sombra), rampa ascendente (32x32) y pilar con base/fuste/capitel (16x48). Contorno de 1px, sin dithering, paleta intacta.
- Es arte jugable: la colisión real (qué tan alto se puede aterrizar, física) se define aparte en Unity/datos del nivel, no está codificada en el PNG.
- Vista de contexto `Artifacts/ArtApproval/costa_plataformas_preview_x4.png`: las tres piezas paradas sobre el suelo y mar ya aprobados (no es una franja que se desplaza como las capas de fondo, porque las plataformas son objetos colocados, no una capa de parallax). Validación en `costa_plataformas_validation.json`: paleta, alfa binario y hashes de las 7 capas sin cambios.
- Aprobación recibida: «esta bien, continua».

## Protagonista (héroe de Costa) — aprobado

- Resuelta la ambigüedad de los brazos (ver sección de ambigüedades resueltas): nunca hay brazos en ninguna animación de Costa.
- `Assets/SuyuRun/Art/PixelCosta/costa_heroe.png`, hoja RGBA 384x336, 48x48 nativo por fotograma (sin escalado incluido en el PNG; el x4 es solo la vista de revisión, igual que las capas de fondo). Filas: idle (4), run (8), jump (3), land (2), slide (3), ráfaga (4), daño (2) — 26 fotogramas en total.
- Ráfaga: en vez de una animación de brazo/arma (no tiene brazos), es un aro de destellos alrededor de la cabeza que crece de intensidad, coherente con el «aro de energía» ya descrito para Suyu en la dirección de arte.
- Primera generación tuvo dos errores que se corrigieron antes de mostrarla: la vista previa se guardaba con fondo negro en vez de transparente (nada más que un bug de la previsualización, no del sprite), y los dos fotogramas de "daño" mezclaban colores fuera de la paleta con un blend suave que rompía la regla de alfa binario/sin degradados. Se corrigió con un remapeo fijo paleta-a-paleta (cada tono salta a un tono de la misma paleta) en vez de mezclar colores.
- Validación en `costa_heroe_validation.json`: cada píxel opaco pertenece a la paleta de 24 colores o es completamente transparente (alfa binario estricto), hashes de las 7 capas y las plataformas sin cambios.
- Es solo arte: el controlador, el estado de animación y el collider se conectan aparte en Unity, no en esta entrega.
- Nota honesta: es un personaje deliberadamente simple (silueta redondeada, sin detalle de tela/ornamento) comparado con el nivel de detalle del fondo (huacas, frisos). Si preferís más ornamento/textura en la ropa, decilo y lo reviso antes de aprobar.
- Aprobación recibida: «esta bien, continua». Se produce a continuación obstáculos/enemigos vivos, manteniendo todo lo anterior intacto.

## Obstáculos/enemigos vivos — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_obstaculos.png`, hoja RGBA 384x96, tres tipos en filas separadas: hexágono con patas (4 fotogramas, patas en X que alternan), pájaro atacante (6 fotogramas, aleteo completo, distinto de las gaviotas decorativas de la capa 2), bote con pescador (4 fotogramas, todo el bote se balancea, caña visible).
- Contorno de 1px, sin dithering, paleta intacta, alfa binario estricto (validado píxel por píxel). Solo arte: hitboxes, movimiento y reglas de aparición se conectan aparte en Unity.
- Vista `Artifacts/ArtApproval/costa_obstaculos_preview_x4.png`. Validación en `costa_obstaculos_validation.json`: paleta, alfa y hashes de capas/plataformas/héroe sin cambios.
- Aprobación recibida: «esta bien continua». Se produce a continuación coleccionables, manteniendo todo lo anterior intacto.

## Coleccionables — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_coleccionables.png`, hoja RGBA 32x64: huaco retrato moche, textil Paracas, cerámica Nazca y moneda de oro moche, cada uno 16x16 con 2 fotogramas (brillo). Interpretaciones originales estilizadas, no reproducciones de piezas documentadas.
- Moneda de oro moche tratada como ficha lúdica del juego, sin atribuirle circulación histórica (ver ambigüedad ya registrada).
- Contorno de 1px, paleta intacta, alfa binario estricto. Solo arte: la lógica de recolección se conecta aparte en Unity.
- Vista `Artifacts/ArtApproval/costa_coleccionables_preview_x4.png`. Validación en `costa_coleccionables_validation.json`: paleta, alfa y hashes de todo lo anterior sin cambios.
- Aprobación recibida: «listo continua». Se producen a continuación los efectos, manteniendo todo lo anterior intacto.

## Efectos — pendiente de aprobación

- `Assets/SuyuRun/Art/PixelCosta/costa_efectos.png`, hoja RGBA 64x80: polvo (4 fotogramas, se dispersa), salpicadura de agua (3), brillo de monedas (2), destello de coleccionable (2, cruz que se abre a estrella), farol/antorcha (2, apagado/encendido con chispas).
- Deliberadamente mínimos (puntos y trazos sueltos): es el tratamiento habitual de partículas en pixel art a esta escala, no un recorte de calidad.
- Paleta intacta, alfa binario. Vista `Artifacts/ArtApproval/costa_efectos_preview_x4.png`. Validación en `costa_efectos_validation.json`: hashes de todo lo anterior sin cambios.
- Con esto quedan completos los puntos (a)–(e) de la sección 12 de la biblia (capas, plataformas, protagonista, obstáculos, coleccionables, efectos). Faltan (f) pantalla de resultados + animación de logro de fin de nivel, y (g) HUD, panel de inicio, selector de nivel, museo y tienda — todo pixel art, todavía sin producir.
- Aprobación recibida: «listo». El usuario pide ahora acoplar todo lo aprobado al nivel de Costa en Unity, y después continuar con (f) y (g).

## Integración en Unity — hecha en Prototype_Costa.unity, no en Level_Costa.unity

Por seguridad se armó en `Prototype_Costa.unity` (antes casi vacía, solo tenía un escenario "SuyuRun Prototype" heredado que se desactivó, no se borró) en vez de en `Level_Costa.unity`, que sigue siendo el nivel activo/probado del rediseño vectorial anterior (fases 1-6, ya con su propio commit). Nada de esto reemplaza ese trabajo todavía; es la base para decidir después cómo se reconcilian ambos.

- Paquete `com.unity.2d.pixel-perfect` instalado. Pixel Perfect Camera a 320x180, PPU 32, sin recorte de fotograma.
- `PixelCostaImport.cs` (Editor): postprocesador que aplica el contrato de importación de la biblia (Point, sin compresión, sin mipmaps, PPU 32, pivote esquina superior izquierda) a todo `Art/PixelCosta/`, y recorta en sprites individuales las hojas de múltiples fotogramas (mar, héroe, obstáculos, coleccionables, efectos, plataformas) con los rectángulos exactos que generaron los scripts de Python.
- `PixelParallaxLayer.cs`: capa de scroll genérica (cielo/tierra con cualquier ancho de tile, no solo capas de 640px como `PixelCostaSkyLayer`), usada para Paracas, huacas, algarrobos y el suelo jugable.
- `FrameAnimator.cs`: reproduce una hoja de sprites a fps fijo, usada para el mar (4 fps), el héroe (idle) y el hexágono.
- `AssembleCostaPixelLevel.cs` (menú `SuyuRun > Pixel Art > Ensamblar nivel Costa pixel art`): ensambla cámara, las 7 capas, suelo, plataformas, héroe, un hexágono, un farol y dos coleccionables de muestra en la escena, de forma repetible (idempotente, se puede volver a correr).
- Dos errores de posicionamiento encontrados y corregidos antes de dar por buena la integración: la cámara no estaba centrada en la ventana de 0–10 unidades que asume la convención "origen esquina superior izquierda" (el mar solo se veía en la mitad izquierda), y el suelo/plataformas/héroe duplicaban el desplazamiento vertical del contenedor raíz (aparecían flotando cerca del cielo en vez de sobre el suelo).
- Verificado en Play: cámara, las 7 capas, suelo, plataformas, héroe (animación idle) y obstáculo visibles y bien compuestos: el mar cubre todo el ancho, nada flota fuera de lugar. El scroll de demostración (`CostaPixelSceneDriver`) se probó real (no con `Thread.Sleep`, que congela el Editor): el suelo avanzó de X=10.85 a X=1.68 en 11 segundos reales. Sin errores ni advertencias en consola durante la corrida.
- Pendiente explícito: esto es una integración VISUAL de demostración, no el controlador del juego. El input, la física de salto/colisión reales, el estado de animación completo del héroe (solo se probó idle) y las reglas de aparición de obstáculos/coleccionables no están conectados. Reconciliar esto con `Level_Costa.unity` (cuál gana, o cómo se fusionan) es una decisión aparte, todavía no tomada.

## (f) Pantalla de resultados y animación de logro — aprobado y conectado

- `costa_resultados.png` (220x140): panel 9-slice adobe/terracota, barra de título, 3 filas de estadística (moneda/pieza/puntos, cada una con ícono de color e barra en blanco para el número real), 4 casillas de piezas vacías (marco de adobe, matching HUD), 2 botones (REINTENTAR/SIGUIENTE) sin texto. Generado, todavía sin conectar como reemplazo del panel de resultados actual (ese reemplazo queda para (g), UI general).
- `costa_heroe_logro.png` (192x48, 4 fotogramas 48x48): reutiliza el cuerpo base del héroe; los brazos aparecen progresivamente con un aro de brillo dorado que crece, terminando en un destello — la animación de fin de nivel que desbloquea el poder de Sierra. **Conectada al juego.**
- `costa_puerta_dimensional.png` (64x96 x2 frames): portal trapezoidal tipo puerta inca al final del recorrido, brillo turquesa-a-dorado con destellos que convergen al centro (efecto de absorción). Paleta de Costa únicamente. **Conectada al juego**, parada sobre el suelo al final exacto del nivel.
- `costa_efecto_sorpresa.png` (32x32 x3 frames): "?!" sin cuadro, crece de chico a grande. **Conectado.**
- `costa_efecto_absorcion.png` (40x40 x4 frames): remolino turquesa-dorado que converge al centro, mismo lenguaje que el portal. **Conectado.**
- Secuencia de cierre de Costa implementada en `RunnerCoastalEnding.cs`: llegar a la puerta → brazos aparecen (aro dorado) → "?!" → remolino de absorción encoge y desvanece al héroe → pantalla "SIERRA DESBLOQUEADA". Validado con corrida automática completa del nivel: 0 muertes, 9/9 orbes, secuencia final sin errores.
- Bug corregido en el camino: un obstáculo de salto pegado a la entrada de una plataforma (89% del recorrido) no dejaba aterrizar antes del siguiente bloque para agacharse — retirado, esa plataforma ya tenía suficiente contenido. También: agacharse ahora reduce el hitbox en el aire, no solo en el suelo (afectaba varios bloques cerca de saltos de escalón).
- Paleta intacta, alfa binario en las 4 piezas. Vistas en `Artifacts/ArtApproval/`. Aprobación recibida: «ya si esta bien todo».
- Corresponde ahora (g): HUD, panel de inicio, selector de nivel, museo y tienda.

## Ambigüedades resueltas

- Protagonista Costa: resuelto por el usuario. NUNCA tiene brazos visibles durante Costa (ni idle ni run — corre todo el tiempo, así que en la práctica nunca hay brazos en el gameplay de Costa). Los brazos son la habilidad que se gana al terminar Costa: animación de logro con brillo dorado (sección 12.f de la biblia), que da paso a Sierra, donde el personaje ya tiene brazos y usa la HUARACA — apunta con el mouse y dispara con click a objetivos aéreos (aves u otros). Esto es trabajo de Sierra, no de esta entrega; aquí solo aplica a que ninguna animación de Costa (idle/run/jump/land/slide/ráfaga/daño) dibuja brazos.

## Ambigüedades conservadas para entregas futuras
- Moneda de oro Moche: tratar como ficha lúdica salvo validación histórica; no atribuirle circulación histórica sin evidencia.
- Logo textual frente a «sin texto dentro del arte»: excepción para logo/UI cuando llegue su entrega.
- La entrega de Costa está condicionada a aprobaciones una a una: no omitirlas para cumplir un plazo.
