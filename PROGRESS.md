# SuyuRun: estado verificable

## (f) Resultados y animación de logro propuestos

Tras confirmar la integración en Unity, se produjo `costa_resultados.png` (panel de resultados, zonas de texto en blanco para la fuente pixel real) y `costa_heroe_logro.png` (4 fotogramas, brazos aparecen con brillo dorado — la animación de fin de nivel que desbloquea Sierra). Validación de paleta/alfa/hashes de todo lo anterior en `Artifacts/ArtApproval/costa_resultados_validation.json`. Pendiente de aprobación antes de (g): HUD, inicio, selector, museo y tienda.

## Integración en Unity: capas, suelo, plataformas, héroe y un obstáculo, funcionando en Play

Pedido del usuario: acoplar todo lo aprobado al nivel de Costa, luego seguir con (f) y (g). Se armó en `Prototype_Costa.unity` (no en `Level_Costa.unity`, que sigue siendo el nivel vectorial de las fases 1-6 de Claude, ya comprometido en git) para no arriesgar ese trabajo mientras se verifica la integración nueva. Detalle técnico completo en `ART_APPROVALS.md`.

Se instaló el paquete Pixel Perfect Camera, se creó un postprocesador de importación (`PixelCostaImport.cs`) que aplica el contrato de la biblia y recorta las hojas de sprites, una capa de scroll genérica (`PixelParallaxLayer.cs`), un reproductor de fotogramas (`FrameAnimator.cs`) y un script de ensamblado (`AssembleCostaPixelLevel.cs`, menú `SuyuRun > Pixel Art > Ensamblar nivel Costa pixel art`) que arma cámara + 7 capas + suelo + plataformas + héroe + muestra de obstáculo/coleccionables.

Se encontraron y corrigieron dos errores de posicionamiento antes de confirmar la integración: la cámara mal centrada (el mar solo se veía en la mitad izquierda) y el suelo/plataformas/héroe duplicando el desplazamiento vertical del contenedor raíz (aparecían flotando en el cielo). Verificado en Play con capturas reales y una prueba de scroll (el suelo avanzó de X=10.85 a X=1.68 en 11 s reales), sin errores en consola.

Pendiente explícito: es una integración visual de demostración, no el controlador de juego real (sin input, física de salto/colisión, ni estado de animación completo del héroe conectados todavía). Cómo se reconcilia con `Level_Costa.unity` sigue sin decidirse. A continuación corresponden (f) y (g) de la sección 12 de la biblia.

## Coleccionables aprobados; efectos propuestos — arte de mundo de Costa completo (a–e)

«listo continua» aprueba coleccionables. Se produjo `costa_efectos.png` (64x80): polvo, salpicadura, brillo de monedas, destello de coleccionable y farol (2 estados). Con esto están completos los puntos (a)–(e) de la sección 12 de la biblia. Faltan (f) resultados/animación de logro y (g) HUD/inicio/selector/museo/tienda — pantallas de UI, todavía sin producir. Nada de esto está integrado al nivel jugable todavía (Pixel Perfect Camera, importación, parallax, controlador, colliders: todo pendiente aparte). Validación de paleta/alfa/hashes de todo lo anterior en `Artifacts/ArtApproval/costa_efectos_validation.json`.

## Obstáculos aprobados; coleccionables propuestos

«esta bien continua» aprueba obstáculos/enemigos. Se produjo `costa_coleccionables.png` (32x64): huaco retrato moche, textil Paracas, cerámica Nazca y moneda de oro moche, 16x16 con 2 fotogramas de brillo cada uno. Interpretaciones originales, no reproducciones documentadas; la moneda se trata como ficha lúdica. Validación de paleta/alfa/hashes de todo lo anterior en `Artifacts/ArtApproval/costa_coleccionables_validation.json`. Pendiente de aprobación antes de efectos.

## Plataformas y protagonista aprobados; obstáculos/enemigos propuestos

«esta bien, continua» aprueba plataformas y protagonista juntos. Se retoma la disciplina de un asset por entrega: se produjo únicamente `costa_obstaculos.png` (384x96) con tres tipos en filas separadas — hexágono con patas (4 fotogramas), pájaro atacante (6 fotogramas), bote con pescador (4 fotogramas, balanceo completo). Validación de paleta/alfa/hashes de todo lo anterior en `Artifacts/ArtApproval/costa_obstaculos_validation.json`. No integrado al nivel. Se espera aprobación antes de coleccionables.

## Protagonista de Costa propuesto (sin aprobación de plataformas todavía)

Aclaración del usuario sobre los brazos: nunca hay brazos en Costa (corre todo el tiempo, nunca se le ven); son la habilidad que se gana al terminar el nivel, con animación de logro (sección 12.f de la biblia), y en Sierra ya con brazos usa la huaraca con click para atacar objetivos aéreos. Eso es trabajo de Sierra, no de esta entrega.

Se generó `costa_heroe.png`: hoja de 26 fotogramas (idle 4, run 8, jump 3, land 2, slide 3, ráfaga 4, daño 2), 48x48 nativo, sin brazos en ninguna. La ráfaga se resolvió como un aro de destellos alrededor de la cabeza (no hay brazo para sostener nada), coherente con el "aro de energía" del diseño original de Suyu. Dos errores de la primera generación (fondo negro en la vista previa, colores fuera de paleta en los fotogramas de daño) se detectaron y corrigieron antes de mostrarla. Validación de paleta estricta (alfa binario, cada píxel opaco pertenece a los 24 colores) y hashes de capas/plataformas intactos en `Artifacts/ArtApproval/costa_heroe_validation.json`.

Nota de proceso: se generaron plataformas Y protagonista en el mismo turno sin esperar aprobación de plataformas primero, rompiendo la disciplina de "un asset por mensaje" que pide la biblia. Se avisa explícitamente al usuario y se retoma el ritmo de a uno a partir de aquí.

## Plataformas (bloque, rampa, pilar) propuestas

Pedido del usuario: seguir la biblia hasta tener todo listo para el nivel Costa. Se saltó el plano frontal opcional (decorativo, factor 1.3) y se pasó directo a lo jugable, siguiendo el orden de la sección 11 de la biblia. Se produjo `costa_plataformas.png` (128x64): bloque de roca/adobe escalonado, rampa y pilar, arte únicamente — la colisión se define aparte en Unity. Validación de paleta/alfa/hashes de las 7 capas en `Artifacts/ArtApproval/costa_plataformas_validation.json`, todas intactas. Vista de contexto (no animada, porque las plataformas son objetos colocados, no una capa de parallax) en `costa_plataformas_preview_x4.png`. Pendiente de aprobación. Después corresponde el protagonista, con la ambigüedad de los brazos por resolver antes.

## Capa 7 aprobada: las 7 capas obligatorias de parallax de Costa están completas

«si esta bien aprobado» aprueba el suelo jugable. Con esto Costa tiene las 7 capas de parallax que pide la biblia (cielo, nubes/aves, Paracas/Candelabro, mar, huacas, algarrobos/rocas/lobos marinos, suelo), todas aprobadas y con validación de paleta/alfa/costuras/hashes intacta capa por capa. Ninguna está todavía integrada al nivel jugable (Pixel Perfect Camera, ParallaxLayer, importación) — sigue pendiente. Falta decidir con el usuario: plano frontal opcional (factor 1.3) o pasar directo a plataformas/rampas/pilares, protagonista, etc.

## Capa 6 aprobada; capa 7 (suelo jugable) propuesta

«apruebo continua» aprueba capa 6. Se produjo únicamente capa 7: `costa_capa7_suelo.png`, tile único de 32x32 (no panel de 640x180 como las capas anteriores) — curva de adobe, friso original inspirado en Paracas y arena con grano, factor 1.0, opaco, costuras izquierda=derecha y arriba=abajo. Para la vista de revisión se repitió el tile 20 veces formando una franja de 640px, sin cambiar cómo se dibujan las capas 1–6. Validación de tamaño, paleta, alfa, costuras y hashes de capas 1–6 en `Artifacts/ArtApproval/costa_capa7_validation.json`, todas intactas. No integrado al nivel. Se espera aprobación de capa 7.

## Retomado por Claude Code tras el relevo; capa 5 aprobada, capa 6 propuesta

El usuario indicó que Astra (la otra sesión de IA) se quedó sin tokens en torno a la capa 4/5, y pidió continuar según `ART_BIBLE.md` desde `Handoff_Claude/`. Se leyó `Handoff_Claude/README.md`, `ART_BIBLE.md`, `ART_APPROVALS.md` y el estado real del proyecto activo (no el snapshot de respaldo) antes de tocar nada. Capa 5 (huacas) seguía pendiente de aprobación tal como dejó Astra; se mostró al usuario para revisión («continua segun la biblia» la aprobó) antes de producir nada nuevo, respetando el flujo de un asset por entrega.

Capa 6 producida como propuesta: `costa_capa6_algarrobos.png`, PNG transparente 640x180, factor 0.75, dos algarrobos secos (tronco nudoso, ramas desnudas), dos grupos de rocas y dos lobos marinos en silueta, sin dithering, paleta intacta. `Tools/pixel_costa_preview.py` se extendió para incluir también la capa 5 en la composición de revisión (antes solo mostraba capas 1–4 más la nueva), conservando sin cambios cómo se dibujan las capas anteriores. Validación de dimensiones, paleta, alfa, bordes y hashes de capas 1–5 en `Artifacts/ArtApproval/costa_capa6_validation.json`, todas intactas. No integrada al nivel. Se espera aprobación de capa 6 antes de continuar con la capa 7 (suelo jugable).

Nota de continuidad para el propio Claude Code en conversaciones futuras: el rediseño de Costa en 6 fases descrito más abajo (fondo limpio, mar, barcos, nadador, montañas/nubes, texturas) fue un enfoque previo, ya integrado directamente en el juego (`RunnerCoastalArt.cs`, `RunnerWater.cs`, etc.) y con sus propios commits de git. La dirección `ART_BIBLE.md` de Astra es un sistema de arte distinto y más ambicioso (capas PNG independientes, protagonista y HUD pixel art completos) que todavía NO está integrado al nivel jugable. Ambos trabajos coexisten sin conflicto por ahora porque este vive en una carpeta separada (`Assets/SuyuRun/Art/PixelCosta/`) sin tocar el código del juego; la decisión de cómo reconciliar ambos (cuál reemplaza a cuál) queda pendiente para cuando llegue el momento de integrar la biblia al nivel.

## Capa 4 aprobada; capa 5 y relevo para Claude

«Esta excelente» aprueba el mar. Capa 5 producida como propuesta: arquitectura de adobe pixel art en PNG transparente 640x180, factor 0.50, frisos originales, terrazas y escaleras decorativas. Validación e inspección visual de previsualización realizadas; capas aprobadas intactas. No integrada al nivel. Se espera aprobación de capa 5. Por pedido del usuario, `Handoff_Claude/README.md` conserva el punto exacto de continuidad, fuentes, rutas y límites de verificación; `CLAUDE.md` dirige a esa carpeta y a la biblia completa.

## Capa 3 aprobada; propuesta de capa 4

El usuario aprobó Paracas/Candelabro. Se creó únicamente capa 4: `costa_capa4_mar.png`, cuatro frames 640x180 dentro de un único PNG 640x720, paleta aprobada, mar oscuro, olas suaves, garúa y caballito de totora lejano. Parallax propuesto 0.30. GIF de revisión y comprobaciones de frames, transparencia, continuidad lateral y hashes en Artifacts/ArtApproval. Capas 1–3 y paleta intactas. Capa 4 pendiente de aprobación; no se ha conectado al nivel ni creado capa 5.

## Capa 2 aprobada; propuesta de capa 3

Confirmación del usuario: «continua me gusta». Se produjo únicamente la siguiente capa: `costa_capa3_paracas.png`, 640x180 transparente con siluetas de Paracas y grabado interpretativo del Candelabro. Paleta intacta, cinco colores usados, factor 0.15. Vista animada de revisión y validación de bordes, alfa y hashes en Artifacts/ArtApproval. Capas 1/2 sin cambios. Capa 3 pendiente de aprobación; mar y conexión de la composición nueva al nivel siguen pendientes.

## Cielo aprobado; entrega de capa 2

El usuario aprobó capa 1 y paleta. Se guardó ese estado sin modificar píxeles del cielo. Capa 2 propuesta: PNG transparente 640x180 de nubes altas y sprites auxiliares de aves con seis frames y tres distancias. GIF de revisión sobre el cielo aprobado, con vuelo continuo, aleteo y planeo. Componente Unity aislado `PixelCostaSkyLayer` preparado, pero no integrado ni validado en Play aún: se espera aprobación. La validación de imágenes verifica paleta, bordes, alfa y conservación del cielo. No se produjo capa 3.

## Nueva dirección: biblia pixel art y aprobación por asset

Se guardó íntegra `ART_BIBLE.md` y se enlazó desde `AGENTS.md` y el brief. Esta dirección sustituye la ilustración anterior. Se conserva el trabajo jugable existente sin reemplazarlo antes de aprobación. Primera propuesta: `Assets/SuyuRun/Art/PixelCosta/costa_capa1_cielo.png`, cielo independiente 640x180 creado con Pillow, paleta candidata de 24 colores (10 usados), sol bajo y nubes con dithering. Solo se produjo la capa 1; queda pendiente de aprobación. Ver `ART_APPROVALS.md` para checklist, referencias y ambigüedades. El MIDI fue copiado a SourceMedia pero no se ha analizado ni se ha cambiado el timing. Las pruebas históricas de abajo NO certifican cumplimiento de la nueva biblia.

## Rediseño de Costa por fases: fase 1, diagnóstico y limpieza del fondo (18 de septiembre)

Petición del usuario: leer el proyecto y el nivel Costa antes de tocar nada, y trabajar el rediseño visual de Costa por fases (fondo limpio, mar animado en pixel art sincronizado a la música, barcos con remeros, nadador José Olaya, montañas/nubes en pixel art, texturas de plataformas y costa), haciendo commit y avisando al terminar cada fase.

Fase 1 completada: diagnóstico y limpieza del fondo actual, sin introducir todavía pixel art (llega en las fases siguientes). Detalle completo del diagnóstico y la corrección en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- Causa de las "capas duplicadas/repeticiones": `CoastPanorama.png` es una ilustración de perspectiva única (una bahía, una ruina, rocas asimétricas), no una tira pensada para repetirse; el código la mostraba en 4 copias en mosaico con espejo horizontal alternado, duplicando visiblemente la bahía y la ruina.
- Causa de los "bordes": un plano de agua procedural independiente se dibujaba sobre el mar ya pintado en la mitad inferior de la misma imagen, con un borde recto que no coincidía en color con la ilustración.
- Corregido: el panorama se recortó por código (Python/PIL) a solo cielo/nubes/acantilados distantes con desvanecido alfa en el borde inferior, se muestra una sola vez (sin mosaico ni espejo) con paralaje lento sin envoltura, escala fija, y su borde inferior coincide exactamente con el borde superior del plano de agua procedural. Los tres acantilados de distancia media, antes copias idénticas, pasaron a 5 instancias con variación de altura/ancho/tono por índice.
- Verificado en el editor: compilación sin errores ni advertencias (consola limpia) y revisión visual en Play del menú/inicio de Costa; falta revisar visualmente el resto del recorrido y la transición completa en distintas tasas de frames.
- El usuario subió `Assets/costa_music_map.json` tras preguntarle; pidió expresamente no modificar su contenido y que se genere algo verificable para confirmar la sincronización.

## Fase 2 completada: mar animado en pixel art con marea sincronizada (18 de septiembre)

Detalle técnico completo en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- El mar procedural de color plano se reemplazó por un mar de pixel art generado por código (`Tools/gen_costa_sea_sprites.py`): olas grandes, olas chicas, espuma dibujada y reflejos, cada uno su propia capa con scroll de UV independiente, importados como Sprite con filtro Point (`CostaSeaTextureImport.cs`, aplica el import automáticamente).
- `Assets/costa_music_map.json` (subido por el usuario) queda intacto en su lugar; `CostaMusicMapSetup.cs` (menú `SuyuRun > Audio > Sincronizar mapa musical de Costa`) copia una réplica exacta a Resources para cargarla en tiempo real y escribe `Artifacts/Audio/costa-music-map-validation.txt` como verificación legible. El mapa es un análisis automático de baja confianza (MIDI, no el mp3; lo dice su propia nota); la duración difiere ~2,47 s de la grabación real, documentado y manejado (se sostiene el último valor).
- La marea usa `AudioSettings.dspTime` (mismo reloj que la música) durante la carrera, suavizada para subir/bajar sin saltos.
- Verificación: prueba automática completa (177,372 s, mismo resultado de recorrido que antes: 43 saltos, 300 monedas, 4/4 museo — sin regresión de jugabilidad) con nuevo dato `SeaTideMin=0.001 SeaTideMax=0.792 SeaEnergyMax=0.999` en `Artifacts/smoke-coast.txt`, confirmando que la marea sí se movió siguiendo el mapa durante una corrida real. Revisión visual en Play confirmada por captura (`Artifacts/costa-presentation.png`).
- Pendiente explícito: sin revisión auditiva de la sincronía (el propio mapa musical tiene baja confianza de BPM); es una traducción de energía/graves a marea, no una coreografía verificada a oído. Fases 3–6 (barcos con remeros, nadador José Olaya, montañas/nubes en pixel art, texturas de plataformas y costa) siguen pendientes.

## Fase 3 completada: barcos variados en pixel art con remeros sincronizados (18 de septiembre)

Detalle técnico completo en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- Barcos vectoriales reemplazados por pixel art generado por código (`Tools/gen_costa_boat_sprites.py`): casco de madera con vela y canoa de totora, con un remero de tres fotogramas (alcance/tirón/recuperación) que lleva el remo dibujado en cada fotograma.
- Seis barcos con posición, tamaño y profundidad distintos (tres lejanos orden -23, tres cercanos orden -21), cada uno con su propia fase de balanceo. El fotograma del remero se decide con la misma fase que balancea el barco (no un reloj aparte), así el remo queda sincronizado al balanceo tal como se pidió.
- Verificado con prueba automática completa: mismo resultado de recorrido que antes (177,372 s, 4/4 museo, `SeaTideMin=0.001 SeaTideMax=0.801` — marea sigue sincronizada), sin errores en consola. Captura `Artifacts/costa-presentation.png` muestra dos barcos de distinta profundidad y tipo a la vez, con el remero visible.
- Pendiente explícito: sin revisión visual humana de la fluidez del ciclo de remo, solo automática/capturas. Fases 4–6 (nadador José Olaya, montañas/nubes en pixel art, texturas de plataformas y costa) siguen pendientes.

## Fase 4 completada: nadador con ciclo de nado y espuma (18 de septiembre)

Detalle técnico completo en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- Añadida una figura de nado en pixel art (`Tools/gen_costa_swimmer_sprites.py`): homenaje estilizado a José Olaya Balandra, con fuente institucional citada en `CULTURAL_SOURCES.md` (Biblioteca Nacional del Perú). No hay retrato documentado de él; la silueta es una interpretación original, no una reconstrucción.
- Ciclo de nado de tres fotogramas y espuma independiente, sincronizados a la misma fase que ya usan los barcos para balancearse (mismo mecanismo de la fase 3, sin reloj aparte).
- Verificado: prueba automática completa sin regresión (mismo resultado de recorrido, marea aún sincronizada `SeaTideMin=0.001 SeaTideMax=0.802`), sin errores en consola. Revisión aislada por captura de cámara confirma que el cuerpo y la espuma se dibujan y animan correctamente.
- Pendiente explícito: no hay una captura con el nadador visible junto al resto de la composición a la vez (aparece y desaparece periódicamente, igual que los barcos); falta revisión visual humana. Fases 5–6 (montañas/nubes en pixel art, texturas de plataformas y costa) siguen pendientes.

## Fase 5 completada: montañas y nubes en pixel art (18 de septiembre)

Detalle técnico completo en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- Acantilados vectoriales y nubes procedurales reemplazados por pixel art generado por código (`Tools/gen_costa_sky_sprites.py`): 4 siluetas de montaña y 4 de nube, cada una con una semilla distinta.
- Combinaciones aleatorias controladas: seis montañas eligen entre las cuatro siluetas por índice más variación de escala/tono; las cuatro nubes usan cada una una silueta distinta.
- Transiciones suaves: nuevo mecanismo `onWrapFade` que atenúa la opacidad de montañas y nubes cerca del borde de su ciclo de repetición, en vez de cortar en seco.
- Verificado: prueba automática completa sin regresión (mismo resultado de recorrido, marea aún sincronizada), sin errores en consola. Captura de la composición real muestra una montaña de pixel art integrada sobre el agua; captura aislada confirmó las nubes.
- Pendiente explícito: sin revisión visual humana de la variedad percibida a lo largo de todo el recorrido, solo capturas puntuales. Fase 6 (texturas de plataformas y costa) sigue pendiente — es la última fase del encargo.

## Fase 6 completada (última fase): texturas de plataformas y costa (18 de septiembre)

Detalle técnico completo en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Resumen:

- Color plano de las fachadas jugables reemplazado por dos texturas de pixel art repetibles: adobe (plataformas elevadas) y arena (suelo costero), generadas por código (`Tools/gen_costa_ground_sprites.py`).
- El resto de cada estructura (cornisa, borde de aterrizaje, relieves, contrafuertes) se dejó sin cambios — es geometría que no afecta la colisión (la colisión usa datos de `RunnerCourse`, no las mallas visuales), y no había necesidad de tocarla en la última fase.
- Verificado: prueba automática completa con el mismo resultado de recorrido que todas las fases anteriores (177,372 s, mismos saltos y aterrizajes elevados, marea aún sincronizada) — confirma que no se rompió el aterrizaje ni la colisión. Captura de la composición real muestra el suelo arenoso con textura visible.
- Pendiente explícito: no se confirmó visualmente una plataforma elevada con la textura de adobe en la misma sesión (mismo mecanismo que la arena, pero no capturado en pantalla).

## Cierre de las seis fases del rediseño de Costa

Con esta fase se completó el encargo por fases solicitado por el usuario: fondo limpio (fase 1), mar animado en pixel art con marea sincronizada a `costa_music_map.json` (fase 2), barcos variados con remeros sincronizados al balanceo (fase 3), nadador José Olaya con ciclo de nado y espuma (fase 4), montañas y nubes en pixel art con variación controlada (fase 5) y texturas de plataformas/costa (fase 6). Todo el arte nuevo se generó por código (Python/PIL) y se importó como Sprite con filtro Point, según lo pedido; no se dejaron placeholders. Cada fase se verificó con una corrida automática completa del recorrido (mismo resultado en las seis) más capturas de pantalla — eso es evidencia de que no se rompió el recorrido ni la sincronización, no un reemplazo de revisión humana de sensación de juego, balance o sonido, que sigue pendiente. Sierra y Selva no se tocaron en este encargo; el resto del alcance de `ASTRA_BUILD_BRIEF.md` (tres niveles completos, museo-mapa, personajes desbloqueables, etc.) sigue siendo trabajo aparte y no se considera parte de este rediseño de Costa.

## Presentación de Costa: figuras, agua, museo y escenas separadas

### Revisión de composición solicitada después de ver el primer resultado

El primer pase se percibía recargado. Se eliminó la repetición de almacenes/redes delante del mar, se redujeron promontorios, vegetación y frecuencia de utilería. Se añadió un plano continuo de mar con gradiente y horizonte, dejando abierta la vista. Las figuras costeras quedan como viñetas espaciadas, no como una pared de adornos.

Se repite el bailarín cada 18 unidades de su capa, con cuatro instancias articuladas que comparten pulso. Las aves decorativas ahora vuelan en grupos de cinco con aleteos y planeo, desplazándose independientemente del jugador. Cuatro nubes adicionales derivan lentamente. Estos elementos acompañan el agua y no tienen colisión. La solicitud de «100 %» se refiere aquí al acabado visual y la animación, no a cambiar a GDevelop ni a declarar terminados los tres niveles.

Corregida la creación de `MaterialPropertyBlock`: se inicializa al construir el agua en Awake, no en el inicializador del MonoBehaviour. Tras una primera prueba interrumpida, el prototipo original completó una nueva ejecución de 64,007 s, con 7 saltos, 4 esquives y 3 ráfagas (`Artifacts/smoke-legacy.txt`). La escena conserva el paisaje sencillo de cerros y el recorrido plano.

- Añadidos parallax de avance y capas independientes: panorama, acantilados separados, orilla, embarcaciones, adobe, vegetación y bailarín más cercano.
- Figuras originales de malla: embarcaciones de pesca y totora, redes, muelles con tablones y amarres, cestas tejidas, vasijas, atados de totora, piedras, conchas y pelícanos lejanos. Paleta común y decoración detrás del gameplay. No se cambió de Unity a GDevelop.
- Agua con espuma que se desplaza, varía suavemente de tamaño y transparencia; reflejos lentos y leve balanceo de botes. El movimiento se congela en pausa y no usa pulsaciones agresivas al beat.
- Gallinazos de combate vinculados a zonas de ráfaga: dos monedas por ave; cajas con tres monedas y memoria liberada. Los pelícanos lejanos son decorativos, sin colisión.
- Museo inicial de cuatro memorias con ilustraciones originales, adelantos bloqueados, guardado y fuentes: huaco mochica, manto Paracas, Nicomedes Santa Cruz y María Reiche. Ver `CULTURAL_SOURCES.md`. Sigue pendiente el museo-mapa completo de tres regiones y la tienda.
- Inicio con canción del usuario y accesos a Costa, museo y prototipo original. Escenas nuevas `Level_Costa` y `Prototype_Legacy`; no se borró la escena anterior.
- Las músicas de Sierra (177,55 s), Selva (180,53 s) e Inicio (295,29 s) están importadas. Registro `Data/Resources/SelectedMusic.asset`; evidencia `Artifacts/Audio/selected-music.txt`. Sierra/Selva aún requieren niveles y beatmaps.
- Aclaración vigente: se permite probar en Unity Play, sin crear otro EXE. Las restricciones anteriores reflejan el estado de avances pasados.
- Costa completó de nuevo el recorrido con la composición final de este avance: 177,372 s, 43 saltos, 9 impulsos aéreos, 28 aterrizajes elevados, 9 orbes, 3 impulsores de suelo, 9 esquives y 9 ráfagas; 300 monedas, 18 aves derrotadas y 4/4 memorias. Evidencia `Artifacts/smoke-coast.txt`. Prueba con conductor automático e inspección visual en Game ampliado; no equivale a prueba humana de dificultad. La captura `Artifacts/costa-presentation.png` muestra la composición despejada. La compilación y la ejecución posterior a corregir la inicialización del agua no produjeron nuevas excepciones en el tramo revisado del log.
- Pasaron además las validaciones de recorrido, colisiones de terreno, checkpoints, existencia de ambas escenas, cuatro grabaciones importadas y cuatro fichas culturales (`Artifacts/course-validation.txt`).
- Revisión de interfaz en Unity: título renderizado, entrada al museo y selección de fichas de huaco mochica y María Reiche con adelantos legibles. Las pruebas automáticas no escribieron los descubrimientos al perfil: la colección manual permanece 0/4, como corresponde.
- No se generó ni ejecutó otro EXE. Sigue pendiente revisión auditiva, balance manual y validación completa de reaparición, guardado y distintas tasas de frames.

## Música y bailarín: El Alcatraz

- MP3 aportado por el usuario importado en `Assets/SuyuRun/Audio/El Alcatraz.mp3`. No se extrajo audio de YouTube. Duración 177,3976 s.
- Nuevo recorrido predeterminado `CostaAlcatraz.asset`: 177,368 s, tres secciones ordenadas basadas en el recorrido vertical, 21 plataformas, 9 huecos con 9 orbes y 3 impulsores de suelo. Las variantes de fachada cambian entre secciones; la composición todavía reutiliza patrones del prototipo.
- `AlcatrazSoundtrack.asset` enlaza el clip con BPM estimado 128,15 y 379 marcas de pulso obtenidas automáticamente. Inicio de reproducción programado por DSP; pausa y checkpoints reubican el audio. No se repite esta canción en bucle.
- Bailarín afroperuano pequeño añadido sobre una roca del fondo, ilustrado con mallas originales y articulaciones de brazos y piernas. Su pose sigue el mismo reloj musical. Es una representación estilizada provisional, no una coreografía del Alcatraz validada.
- Compilación e integración de datos realizadas; evidencia en `Artifacts/Audio/integration.txt`. El análisis se puede reproducir con `Tools/analyze_rhythm.py` y las muestras exportadas por Unity. No se ejecutó el EXE.
- Pendiente: revisión auditiva de los pulsos y tiempos de acción, latencia, revisión visual del bailarín en movimiento y balance del recorrido de 2:57. Los resultados anteriores de 64 s NO validan la ruta nueva. Créditos y licencia de la grabación deben documentarse antes de distribución pública.
- Procedencia, configuración y límites: `Assets/SuyuRun/Audio/AUDIO_NOTES.md`. Se conservan CostaVertical y CostaPrototype como versiones previas.

## Avance del 18 de septiembre: Costa vertical e ilustrada

- Integrado un fondo original de Costa generado con ImageGen: cielo luminoso, nubes, Pacífico, dunas, acantilados y arquitectura estilizada de adobe. Archivo y prompt: `Assets/SuyuRun/Art/ART_DIRECTION.md`.
- Ruta editable nueva `CostaVertical.asset`, conservando el recorrido plano anterior: siete bases elevadas, tres huecos y tres puntos de segundo impulso aéreo.
- Implementadas colisiones de aterrizaje, paredes, caída al vacío, margen de salto al abandonar un borde y selección de checkpoints seguros.
- Tres variantes de estructura: terraza, fachada de nichos y fachada escalonada. Cornisas alineadas con el apoyo físico, sombras, relieves y contrafuertes. Son diseños fantásticos originales, no reconstrucciones patrimoniales.
- Añadido un impulsor de suelo violeta y trayectorias de monedas que orientan los saltos.
- Añadidos picos individuales y dobles, incluyendo peligros sobre plataformas. La prueba de contacto reduce el ancho del pico hacia la punta para evitar colisión con esquinas vacías del rectángulo envolvente.
- Corregida la superposición de suelo y fachadas mediante grupos de orden de dibujo; reducido el amontonamiento de monedas y espaciada la decoración. Las monedas cercanas a picos se elevan para sugerir el salto.
- Mallas arquitectónicas combinadas por profundidad para reducir llamadas de dibujo. Mallas, materiales y clips transitorios liberados al destruir el controlador.

### Evidencia y límites de este avance

La ruta vertical inicial completó una prueba dentro del editor: 64 segundos, 13 saltos, 3 impulsos aéreos, 7 aterrizajes elevados, 3 esquives y 3 ráfagas (`Artifacts/smoke-vertical.txt`). También pasaron las comprobaciones de terreno y checkpoints (`Artifacts/course-validation.txt`). Estos resultados son ANTERIORES a la incorporación del impulsor de suelo, nuevos picos y variantes finales de fachada; no validan por sí solos esos últimos cambios.

Por petición expresa del usuario no se ejecuta ni se prueba el EXE en este avance. Tras la petición, se continúa implementando y comprobando compilación. La build en `Builds/Prototype` puede ser anterior al código más reciente: usar Play en Unity para ver todos los cambios. Pendientes de revisión jugable: nuevos grupos de picos, impulsor de suelo, composición final, reaparición y diferentes tasas de frames. Sierra y Selva todavía no tienen sus fondos ni niveles completos.

## Primer prototipo

- Implementado: recorrido Costa de 64 segundos, 120 BPM, movimiento automático, salto con buffer, deslizamiento, ráfaga temporal, obstáculos, monedas, checkpoints y reinicio.
- Arte original construido con mallas geométricas: Suyu facetado, piernas animadas, núcleo, emisor, dunas, mar, moneda y obstáculos.
- Música y efectos sintetizados originales. La pista es un boceto de prototipo, no la banda sonora regional final.
- Escena creada por `SuyuRun > Preparar prototipo`; abrir con `SuyuRun > Abrir prototipo` y pulsar Play.
- Compilación y dos builds Windows completadas correctamente. Ejecutable: `Builds/Prototype/SuyuRun.exe` (requiere el resto de su carpeta).
- Prueba automática del ejecutable completada: 64 segundos, 7 saltos, 4 esquives, 3 ráfagas, 49 monedas y 2225 puntos. Evidencia: `Artifacts/Standalone/smoke-result.txt`. Esta prueba no sustituye la revisión manual de sensación de juego y audio.
- Validación de datos aprobada: recorrido válido, velocidad inválida y beats duplicados rechazados. Evidencia: `Artifacts/course-validation.txt`.
- Recorrido extraído a `Assets/SuyuRun/Data/Resources/CostaPrototype.asset`: velocidad, BPM, duración, checkpoints y obstáculos editables. La última build incluye esta extracción; la prueba independiente anterior corresponde a la build previa.

## Reparación tras el apagado

- El editor estaba abriendo una escena recuperada o SampleScene, con cámara vacía, en lugar del prototipo. Se conserva la recuperación y se configura Prototype_Costa como escena de inicio de Play y de la build.
- Corregida la regeneración del clip musical si desaparece durante una recarga del editor.
- Prueba del editor completada a los 64 segundos. Captura: `Artifacts/prototype-play.png`.

## Alcance que sigue pendiente

### Siguiente hito prioritario solicitado por el usuario

Implementada la base de doble impulso, plataformas y huecos en Costa. Continuar refinando la composición y dificultad de esta ruta antes de extender el lenguaje visual a Sierra y Selva. Ver requisitos en `ASTRA_BUILD_BRIEF.md`, sección «Próximo avance prioritario: doble salto y plataformas». No confundir la implementación actual con la validación completa de todos los criterios.

Los tres niveles finales, museo, fichas culturales verificadas, personajes alternativos, economía final, assets finales, audio regional final, controles accesibles y pruebas completas continúan pendientes. Este hito no equivale al juego final al 100%.

## Decisiones provisionales que requieren revisión durante el desarrollo

La compra directa de reaparición por cinco monedas es una simplificación del prototipo; sustituir por inventario de reintentos. La colisión del prototipo es geométrica y propia. El curso está escrito a mano en datos editables. El cronograma es una estimación, no evidencia de terminación ni una garantía de tiempo.
