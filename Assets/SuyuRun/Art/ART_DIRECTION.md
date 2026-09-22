# SuyuRun — dirección artística, avance Costa

## Referencia aprobada por el usuario

Captura adjunta y enlace https://youtu.be/v9JsLfmJuqg. El video no pudo recuperarse mediante la herramienta web; las decisiones de este avance se basan en la captura visible, no en una supuesta revisión del video completo.

Conservar el acabado de ilustración 2D luminosa: cielo saturado, nubes voluminosas simplificadas, planos de color limpios, siluetas angulares, profundidad atmosférica y estructuras arquitectónicas legibles. No copiar composición, personajes ni recursos existentes. El protagonista, poderes, historia y música siguen siendo de SuyuRun.

Petición explícita adicional: las bases de aterrizaje deben parecer estructuras, no cajas flotantes. Las bases actuales tienen cornisa iluminada alineada al collider, cara sombreada, paneles hundidos, relieves geométricos originales, contrafuertes y decoración escalonada. Las escaleras de la fachada son decorativas; el borde superior claro es la superficie real de aterrizaje. Evitar confundir adornos con apoyo físico.

- Costa: adobe ocre, arena, piedra, Pacífico turquesa y vegetación costera.
- Sierra, pendiente: terrazas y estructuras pétreas, montañas y vegetación de altura.
- Selva, pendiente: vegetación estratificada, río, madera y estructuras acordes al entorno, sin presentar construcciones inventadas como patrimonio real.

## Fondo integrado

Archivo: `Resources/CoastPanorama.png`. Generado con la herramienta integrada ImageGen, no CLI. Imagen original generada por IA; paisaje estilizado, no reconstrucción arqueológica documentada. Se carga en `RunnerCoastalArt.cs`, detrás de las estructuras jugables. Conservar la copia dentro del proyecto.

Prompt de generación:

> Use case: stylized-concept. Asset type: production background panorama for an original Unity 2D side-scrolling runner SuyuRun, Peru COAST region. Generate a wide 3:1 landscape illustration, preferably 3072x1024. Style: very clean flat vector-like 2D cartoon environment, vivid cyan blue sky, huge elegant rounded cream and pale cyan clouds, strong sculpted angular silhouettes with 2-3 flat shading tones per shape, atmospheric layered depth, polished rhythmic platformer aesthetic, no outlines, no texture grain, no realistic rendering. Scene: Peruvian Pacific coast with turquoise ocean, warm ochre and terracotta desert cliffs, sweeping sand dunes, distant original stepped adobe coastal ruins and a few coastal shrubs. Broad open sky in upper half, distant landscape middle, quiet sand and turquoise water low foreground. Side-on panoramic game background, not a perspective road. No playable platforms, no hazards, no character, no text, no UI, no logos. Original landscape not copying an existing game composition. Match the user's reference's crisp shapes and saturated joyful daylight but replace European fantasy mountains/castle with Peru's coastal landscape. Image will be a distant background behind real foreground platforms. No black borders. Deliver image file.

Semilla no expuesta por la herramienta. Las plataformas y relieves son mallas originales reproducibles desde código. El fondo ilustrado es una capa panorámica con desplazamiento suave; vegetación y estructuras se desplazan por separado. Separar más capas ilustradas y refinar la coherencia de los obstáculos sigue pendiente del arte final.

## Diagnóstico y limpieza de fondo (18 de septiembre, rediseño Costa)

Petición del usuario: revisar el fondo actual antes de tocar nada, por capas duplicadas, bordes y repeticiones, y dejarlo como una sola escena limpia con parallax por capas, previo a reconstruir mar, barcos, nadador, montañas/nubes y texturas en pixel art por fases.

Diagnóstico encontrado en `RunnerCoastalArt.cs`/`RunnerParallax.cs`:

- `CoastPanorama.png` es una ilustración de perspectiva única (una sola bahía, una sola ruina de adobe, rocas de primer plano asimétricas), no una tira horizontal pensada para repetirse. El código anterior la mostraba en 4 copias en mosaico, alternando espejo horizontal (`flipX`), para cubrir el ancho del nivel. Resultado: la bahía, la ruina y las rocas se veían duplicadas y reflejadas de forma evidente cada pocas unidades — la causa concreta de "repeticiones" percibidas.
- La misma imagen incluye su propio mar y playa pintados en la mitad inferior, pero un plano procedural independiente ("Open Pacific") se dibujaba encima cubriéndolos por completo. El borde recto de ese plano (arista superior en y=.35) no coincidía en color con la ilustración justo encima, generando una línea de corte visible — la causa concreta de "bordes".
- Los tres acantilados de distancia media (capa procedural) eran copias idénticas sin variación, separadas de forma regular — se leían como recortes repetidos.

Cambios aplicados:

- `CoastPanorama.png` se recortó (script Python/PIL) para conservar solo cielo, nubes y acantilados/ruina distantes, descartando la mitad inferior pintada (agua/playa) que ya quedaba oculta. El borde inferior recibió un desvanecido alfa de 46 px para fundirse en el plano de agua procedural en vez de cortar en seco.
- El panorama ya no se repite en mosaico ni se refleja: es una sola instancia colocada una vez, con desplazamiento paralaje lento (`PanoramaParallax=.022`) sin envoltura (`Mathf.Repeat`). A esa velocidad se desplaza muy poco durante todo el recorrido, que es exactamente el comportamiento esperado de un fondo genuinamente lejano.
- Su escala vertical ahora es fija (`10.8/7.24`, la proporción original de diseño) en vez de recalcularse desde el alto del archivo, para que el recorte no reescale ni "haga zoom" sobre el arte.
- Su borde inferior se posiciona exactamente en y=.35, el borde superior del plano de agua procedural (`BuildOpenOcean`), eliminando el corte de color visible.
- Los acantilados de distancia media pasaron de 3 copias idénticas a 5 instancias con variación determinística de altura, ancho y tono por índice, para que la repetición periódica no se lea como copias idénticas.

Esta limpieza mantiene el estilo vectorial/procedural actual; no introduce todavía pixel art. Las fases siguientes (mar, barcos, nadador, montañas/nubes, texturas) reemplazan estos elementos por sprites de pixel art generados por código, según lo acordado con el usuario.

## Fase 2: mar animado en pixel art con marea sincronizada (18 de septiembre)

El mar procedural de color plano (`BuildOpenOcean`, cresta/espuma en cintas vectoriales) se sustituyó por un mar de pixel art de verdad, generado por código y sincronizado a la música.

- Sprites generados con `Tools/gen_costa_sea_sprites.py` (Python/PIL): `SeaWaveBig.png` (olas grandes, con cresta clara), `SeaWaveSmall.png` (olas chicas), `SeaFoam.png` (espuma dibujada, en parches irregulares) y `SeaSparkle.png` (reflejo/destello). Todos dibujados a mano por código en una grilla de píxel pequeña (sin antialiasing), luego escalados x8 con vecino más cercano, y diseñados para repetirse horizontalmente sin costura (el patrón se calcula en función de `x mod ancho_del_tile`).
- Importados como Sprite con filtro Point, sin compresión y con wrap Repeat (`Assets/SuyuRun/Editor/CostaSeaTextureImport.cs`, un `AssetPostprocessor` que aplica esto automáticamente a todo lo que caiga en `Art/Resources/Sea/`, así que regenerar los PNG mantiene el import correcto).
- Capas en `RunnerWater.cs`: cuerpo de agua profunda (plano, color liso), banda de "olas grandes", línea de horizonte suave, banda de "olas chicas", banda de "espuma" y 16 destellos de reflejo — cada banda anima su propio scroll de UV (no reutiliza el mesh vectorial existente); las olas grandes y chicas y la espuma son capas independientes, no una sola textura.
- Marea: `Assets/costa_music_map.json`, subido por el usuario, se deja intacto en su ubicación original; `CostaMusicMapSetup.cs` (menú `SuyuRun > Audio > Sincronizar mapa musical de Costa`) copia una réplica byte a byte a `Assets/SuyuRun/Data/Resources/` para poder cargarla en tiempo real, y escribe `Artifacts/Audio/costa-music-map-validation.txt` con una verificación legible (secciones, muestras de energía/graves, comparación de duración). El mapa es un análisis automático de baja confianza sobre una transcripción MIDI (no el mp3 real); su propia nota lo advierte. `CostaMusicMap.cs` interpola la energía y los graves muestreados cada 0.25 s y sostiene el último valor más allá de la duración registrada.
- `RunnerWater.cs` calcula el segundo de canción actual con `AudioSettings.dspTime - started` mientras se corre el nivel (mismo reloj que ya usaba la música), suaviza el resultado con `SmoothDamp` para que la marea "suba y baje" sin saltos de 0.25 s, y lo traduce en: un balanceo vertical leve del conjunto del mar, más/menos opacidad de olas grandes y espuma, y velocidad de scroll variable. Fuera de una carrera (menú/museo) usa un vaivén sintético lento en vez de leer el mapa.
- Verificación: el reporte de prueba automática (`Artifacts/smoke-coast.txt`) ahora incluye `SeaTideMin`/`SeaTideMax`/`SeaEnergyMax`; en la corrida de verificación la marea recorrió 0.001–0.792 y la energía llegó a 0.999, confirmando que sí sigue el mapa durante una carrera real y no queda estática.
- Pendiente explícito: la sincronía no se validó de oído (el propio mapa avisa baja confianza de BPM); esto es una traducción visual de energía/graves a marea, no una coreografía rítmica verificada. Las fases 3–6 (barcos, nadador, montañas/nubes, texturas) siguen pendientes.

## Fase 3: barcos variados con remeros sincronizados al balanceo (18 de septiembre)

Los barcos vectoriales anteriores (`BuildCoastalBoat`, casco por polígono, remo fijo) se sustituyeron por barcos de pixel art con remero animado.

- Sprites generados con `Tools/gen_costa_boat_sprites.py` (Python/PIL): `BoatWood.png` (bote de madera con vela pequeña) y `BoatReed.png` (canoa de totora), más tres fotogramas del remero con el remo dibujado en cada uno (`RowerReach`, `RowerPull`, `RowerRecover`) en vez de un remo como objeto separado — la posición del remo queda resuelta por el fotograma, no por rotación en tiempo real. Importados con el mismo `AssetPostprocessor` de la fase 2, ahora renombrado `CostaPixelArtImport.cs` y ampliado para cubrir también `Art/Resources/Boats/`.
- Seis instancias (`BuildBoat` en `RunnerParallax.cs`), dos niveles de profundidad: tres barcos lejanos (escala .5–.62, orden de dibujo -23, paralaje más lento, detrás de los destellos del mar) y tres cercanos (escala .85–1.15, orden -21, paralaje más rápido, delante). Alternan casco de madera/vela y canoa de totora; cada uno tiene su propia posición inicial dentro de su ciclo de repetición, así que no aparecen en fase idéntica.
- El remero cambia de fotograma según la misma fase que ya hace balancearse (bob/tilt) al barco en `UpdateParallaxLayers` (`piece.onFloatPhase`) — no corre en un reloj aparte, así que el remo queda sincronizado al balanceo como se pidió, en vez de animarse de forma independiente. El intercambio de textura usa `MaterialPropertyBlock` sobre un material compartido por remero.
- Verificado con la prueba automática completa (177,372 s, mismo resultado de recorrido que antes, sin regresión) y captura (`Artifacts/costa-presentation.png`) mostrando dos barcos de distinta profundidad, tamaño y tipo simultáneamente, con el remero visible sobre la canoa.
- Pendiente explícito: no hay revisión visual humana de la fluidez del ciclo de remo (solo automática/capturas); fases 4–6 (nadador, montañas/nubes, texturas) siguen pendientes.

## Fase 4: nadador con ciclo de nado y espuma (18 de septiembre)

Figura de nado añadida en la orilla, homenaje estilizado a José Olaya Balandra (pescador chorrillano que llevó mensajes de los patriotas a nado entre Lima y el Callao durante la independencia; ver `CULTURAL_SOURCES.md` para la fuente institucional — Biblioteca Nacional del Perú). No existe retrato documentado de él: la silueta es una interpretación original, siguiendo el mismo criterio ya usado con el bailarín ("no es... una caricatura").

- Sprites en `Tools/gen_costa_swimmer_sprites.py`: tres fotogramas de estilo crol (`SwimmerReach`, `SwimmerPull`, `SwimmerGlide`) y un `Splash.png` de espuma, mismo import de pixel art (`CostaPixelArtImport.cs`, ampliado a `Art/Resources/Swimmer/`).
- Misma técnica que el remero de la fase 3: el fotograma del nadador se decide con la fase que ya lo hace flotar/balancearse (`ParallaxPiece.onFloatPhase`), no con un reloj aparte. La espuma es un cuarto sprite independiente cuya opacidad sube y baja alrededor de la mitad del tirón (cuando la mano líder entraría al agua) usando la misma fase.
- Una sola instancia, con repetición periódica igual que los barcos (`Layer`, periodo 140), a diferencia del bailarín que usa copias instanciadas manualmente — mismo resultado visual (aparece varias veces a lo largo del recorrido) con menos código.
- Verificado: compila sin errores, prueba automática completa sin regresión (mismo resultado de recorrido, marea aún sincronizada), y revisión aislada por captura posicionada de cámara confirmando que el cuerpo y la espuma se dibujan y cambian de fotograma correctamente.
- Pendiente explícito: no se capturó una imagen del nadador visible junto al resto de la composición en la misma captura (aparece y desaparece por el mismo sistema de repetición que los barcos); revisión visual humana del ciclo completo sigue pendiente. Fases 5–6 (montañas/nubes, texturas) siguen pendientes.

## Fase 5: montañas y nubes en pixel art (18 de septiembre)

Los acantilados de distancia media (mallas vectoriales, fase 1) y las nubes procedurales se sustituyeron por pixel art con variación controlada.

- Sprites en `Tools/gen_costa_sky_sprites.py`: cuatro siluetas de montaña (`Mountain0`–`Mountain3`, cada una con una semilla distinta que decide número y altura de picos, de mesa a pico agudo) y cuatro nubes (`Cloud0`–`Cloud3`, número y tamaño de lóbulos también por semilla). Mismo import de pixel art, ahora ampliado a `Art/Resources/Sky/`.
- "Combinaciones aleatorias controladas": seis instancias de montaña eligen una de las cuatro siluetas por índice (`i%4`) más variación de escala/tono por instancia (misma técnica de la fase 1), en vez de generación verdaderamente libre — el conjunto de piezas está acotado y cada una ya viene con dos-tres tonos de sombreado propios. Las cuatro nubes usan cada una una silueta distinta (una de cada variante), no copias idénticas.
- "Transiciones suaves": nuevo mecanismo genérico `ParallaxPiece.onWrapFade` (`RunnerParallax.cs`/`RunnerWater.cs`) que atenúa la opacidad de una pieza envuelta cerca del borde de su ciclo de repetición (`Mathf.Repeat`), antes de que salte de vuelta al otro extremo. Ese salto siempre ocurre fuera de cámara (los periodos son mucho más anchos que la ventana visible), así que antes era invisible igualmente, pero ahora además se desvanece en vez de cortar en seco si llega a rozar el borde de la vista. Se aplicó tanto a montañas como a nubes.
- Verificado: compila sin errores, prueba automática completa sin regresión (mismo resultado de recorrido, marea aún sincronizada), captura de la composición real (`Artifacts/costa-presentation.png`) muestra una montaña de pixel art en primer plano sobre el agua profunda, y una captura aislada confirmó que las nubes se dibujan correctamente.
- Pendiente explícito: no hay revisión visual humana de la variedad percibida a lo largo de todo el recorrido (solo capturas puntuales); fase 6 (texturas de plataformas y costa) sigue pendiente.

## Fase 6 (última): texturas de plataformas y costa (18 de septiembre)

La "Sunlit adobe facade" de cada estructura jugable (color plano hasta ahora) se sustituyó por una textura de pixel art repetible, distinta según el tipo de superficie.

- Sprites en `Tools/gen_costa_ground_sprites.py`: `Adobe.png` (bloques con mortero, para plataformas elevadas) y `Sand.png` (arena moteada, para el suelo costero a nivel del jugador). Ambas diseñadas para ser perfectamente repetibles en ambos ejes (grilla regular para adobe, motas independientes para arena), porque `BuildStructure` las repite tanto a lo ancho como a lo alto de cada estructura, y el segmento base de "Coastal foundation" puede llegar a medir cientos de unidades.
- Riesgo identificado y evitado: `BakeStructure` combina todas las mallas de una estructura por orden de dibujo en una sola malla con el material plano compartido (`material`), así que agregar directamente un quad con textura ahí habría perdido la textura al hornear. La solución fue construir el quad texturado **después** de llamar a `BakeStructure`, como hijo independiente de `root` — nunca pasa por el horneado y conserva su propio material.
- Todo lo demás de cada estructura (silueta de sombra, cornisa, "Readable landing lip", relieves, contrafuertes, escaleras decorativas, fachadas de arcada/santuario) se dejó exactamente igual: son geometría vectorial plana que no afecta la colisión (la colisión usa los datos de `RunnerCourse`/`RunnerTerrain`, nunca las mallas visuales), pero cambiarla sin necesidad habría sido riesgo innecesario para el último tramo del encargo.
- Dos materiales cacheados y compartidos (`adobeMaterial`, `sandMaterial`) entre las decenas de estructuras del recorrido, en vez de uno nuevo por instancia.
- Verificado: compila sin errores, prueba automática completa con el mismo resultado de recorrido que todas las fases anteriores (177,372 s, aterrizajes elevados y saltos sin cambios, marea aún sincronizada) — confirma que retexturizar no rompió el aterrizaje ni la colisión. Captura de la composición real muestra el suelo arenoso con grano visible bajo el HUD y las cajas de memoria.
- Pendiente explícito: no se confirmó visualmente una plataforma elevada con la textura de adobe en la misma sesión (se infiere del mismo mecanismo verificado con la arena, pero no se vio en pantalla); revisión visual y auditiva humana de Costa completa sigue pendiente, igual que Sierra y Selva.

## Estado al cierre de las seis fases

Costa tiene ahora una sola escena de fondo limpia, mar animado en pixel art con marea sincronizada a `costa_music_map.json`, barcos variados con remeros sincronizados al balanceo, un nadador con ciclo de nado y espuma, montañas y nubes en pixel art con variación controlada, y texturas de pixel art en plataformas y suelo costero. Todo generado por código (Python/PIL) e importado como Sprite con filtro Point, según lo pedido. Cada fase se verificó con una corrida automática completa (mismo resultado de recorrido en las seis) y capturas de pantalla; ninguna sustituye revisión humana de sensación de juego, balance o sonido. Sierra y Selva no fueron tocadas en este encargo. El resto del alcance (tres niveles completos, museo-mapa de tres regiones, personajes desbloqueables, etc.) sigue siendo trabajo aparte.
