# SuyuRun - Encargo maestro de implementación para GPT-6 Astra

## Prioridad vigente — refactor y escala global, 03/10/2026

El usuario amplió el encargo a todos los escenarios/obstáculos, sin cambiar
recorrido ni música. Leer ART_BIBLE.md, ART_APPROVALS.md y
Artifacts/VisualRefactor/GLOBAL_SCALE_AND_DETAIL.md. Héroe con escala común
0.85, carrera/disparo corriendo/agachado revisados; colisiones proporcionales.
Seis capas detalladas con contrato de resolución independiente, ya conectadas.
Doce pruebas de combate completas a 30/60/120 FPS; cuatro portales, cargadores
y clearance verificados. Smoke Unity sin excepciones, NO recorrido completo.
Resta redibujar otras capas, plataformas/obstáculos/UI y validar visualmente
todo el recorrido. No afirmar que TODO el arte fue aumentado ni que no hay bugs.
No crear otro ejecutable. Fuentes y anteriores conservados.

## Prioridad vigente — Selva V2, 02/10/2026

El usuario cambió el encargo de paquete de arte a pulido + integración Unity. Ver `Docs/SELVA_V2_ARTE_E_INTEGRACION.md`. Escena `Level_Selva`, recursos `SelvaV2`, caimán físico con hitboxes por frame, agua reactiva y lianas pendulares. Costa/Sierra cerrados; no reabrir su gameplay. Paleta propuesta 28 colores dentro de la nueva dirección 24–32, revisión visual pendiente.

Verificaciones y capturas reales en `Artifacts/SelvaV2`; GIF de estudio separado de las pruebas Play. No compilar otro ejecutable. Falta validación humana de dificultad y de notas MIDI dudosas; no asumir que la extracción automática del MIDI equivale a validación de oído.

## Prioridad nueva — cuevas de Sierra

Última regla confirmada: `Docs/SIERRA_CIERRE_JUGABILIDAD_26_SEPTIEMBRE.md`.
Salida3s después de derrotar a todos; prórroga5s al minuto si un objetivo completo.

Revisión más reciente: `Docs/SIERRA_CUEVAS_COMBATE_26_SEPTIEMBRE.md`.
Munición5/recarga4s, hasta15esbirros, cinematográficas y combos específicos.

Leer `Docs/SIERRA_CUEVAS_25_SEPTIEMBRE.md` antes de continuar. El usuario autorizó
expresamente producir el lote completo de arte de ambas cuevas; paleta/estilo sin
cambios. Cada una dura su MP3 de60,0555s, fuera de la ingeniería MIDI del runner.
Controles laterales A/D, W/ESP doble salto, S bajar, clic/E pistola. No autorun
ni huaraca dentro. Congelar el segundo de Sierra y reanudarlo al salir, sin deriva.
No tomar los reportes antiguos como prueba de las cuevas nuevas.

## Dirección vigente — especificaciones adicionales de Sierra

Entrega funcional para prueba: `Docs/SIERRA_LISTO_PARA_PROBAR.md`.
Estado por sección: `Docs/SIERRA_CHECKLIST_ESPECIFICACIONES.md`; halcón y roca aprobados.
Portal/cámara, museo, tienda, ajustes y cierre de nivel integrados. No confundir pruebas automáticas
con aceptación manual del usuario o validación auditiva de la correspondencia MIDI/MP3.

Leer completo `Docs/SIERRA_ESPECIFICACIONES_ADICIONALES.md`, copia del documento nuevo del usuario.
El fondo actual de Sierra queda congelado. Reutilizar los sistemas existentes de RunnerPrototype
(monedas, respawn, HUD, pausa, resultados, progreso); no ampliar la integración aislada como arquitectura definitiva.
Conservar Costa y los avances de Sierra. La migración debe mantener el comportamiento de Costa.
Confirmado por el usuario: la recompensa es el traje protector YA EXISTENTE para continuar a Selva;
las menciones a traje de guerrilla y regreso a Costa en el documento adjunto son erróneas.
Autorizado conectar el teleporte final de Costa con Sierra. Costa permanece cerrado en todo lo demás:
no cambiar su arte, recorrido, obstáculos, música ni balance. Conservar su secuencia de portal existente.
La conexión debe formar parte del flujo compartido; no presentar la escena aislada de integración como
cumplimiento definitivo de la arquitectura solicitada.

> DIRECCIÓN VIGENTE: leer completo `ART_BIBLE.md` antes de cualquier asset y consultar `ART_APPROVALS.md`. La biblia pixel art del usuario sustituye las decisiones anteriores incompatibles. Un asset/capa por entrega; esperar aprobación antes del siguiente. El mandato de autonomía siguiente no autoriza saltarse esas aprobaciones.

## Mandato de ejecución

Construye el videojuego descrito en este documento dentro de este proyecto Unity. No te limites a proponer un plan, explicar cómo hacerlo o crear únicamente una demo técnica: implementa, integra, prueba y deja una build jugable. Trabaja de forma autónoma en todas las decisiones rutinarias y reversibles. Pregunta solo cuando falte una decisión del usuario que cambie materialmente el producto o cuando exista un bloqueo externo real.

Conserva el trabajo existente, realiza cambios pequeños y verificables, y mantén el proyecto compilando. Después de cada sistema importante, comprueba la consola de Unity, ejecuta las pruebas pertinentes y prueba el flujo en Play Mode. No declares una función terminada si solo existe el script pero no está conectada a escenas, prefabs, datos, controles y UI.

## Fuente de verdad y alcance

La conversación actual y este archivo tienen prioridad sobre los PDF académicos anteriores. Los PDF son contexto y no convierten automáticamente ideas adicionales en requisitos.

El producto obligatorio es un runner rítmico 2D para Windows inspirado en la legibilidad y ritmo de Geometry Dash, pero con identidad, arte, código y contenido propios. Debe contener tres niveles completos: Costa, Sierra y Selva. No se aceptan dos niveles demostrativos ni un cuarto nivel inventado.

Configuración técnica existente:

- Unity 6.3 LTS, versión exacta `6000.3.17f1`.
- Plantilla Universal 2D con URP 2D Renderer.
- Input System de Unity.
- Repositorio Git interior con rama `main`.
- Plataforma objetivo inicial: Windows 64-bit.
- El paquete Unity AI Assistant fue retirado porque la cuenta no tiene suscripción. No volver a instalarlo.

## Visión del juego

`SuyuRun` es un runner rítmico peruano en el que el personaje avanza automáticamente por tres regiones. El jugador salta, esquiva y activa una ráfaga ofensiva temporal mientras sigue patrones asociados a la música. Recolecta monedas, objetos culturales y representaciones de personajes históricos. El museo central funciona también como mapa de niveles y revela información cultural progresivamente.

El objetivo de diseño es entretener y despertar curiosidad por la historia y el patrimonio cultural peruano. El contenido educativo debe integrarse sin interrumpir constantemente la carrera.

Core loop:

1. Elegir región y personaje en el museo-mapa.
2. Correr automáticamente y reconocer patrones visuales y musicales.
3. Saltar, esquivar y usar la ráfaga rítmica.
4. Recoger monedas y coleccionables culturales.
5. Alcanzar checkpoints y completar el nivel.
6. Recibir puntuación, monedas, desbloqueos y fichas culturales.
7. Volver al museo, consultar lo descubierto y elegir otra región.

## Protagonista original: Suyu

Diseña un protagonista original, legible en tamaño pequeño y sin copiar a Io ni otros personajes protegidos.

Concepto visual:

- Nombre provisional: `Suyu`.
- Núcleo geométrico flotante, entre cubo redondeado y piedra luminosa facetada.
- Silueta compacta en reposo, con un centro de luz expresivo que funciona como rostro abstracto.
- Paleta base: turquesa profundo, dorado cálido, crema y un acento magenta moderado.
- Detalles geométricos inspirados de forma respetuosa en tejidos y organización modular andina, sin copiar una pieza arqueológica ni usar símbolos sagrados como simple decoración.
- Apariencia tecnológica-fantástica y amable, apta para público desde 13 años.

Transformación de movimiento:

- En `Idle`, el núcleo flota y pulsa suavemente; no muestra piernas permanentes.
- Al empezar a correr, se eleva ligeramente y despliega dos piernas cortas de energía o segmentos geométricos. La animación debe comunicar con claridad contacto con el suelo, impulso y velocidad en vista lateral 2D.
- En salto, retrae parcialmente las piernas, comprime el núcleo al despegar y se estira siguiendo el arco.
- Al caer, orienta las piernas hacia el suelo y muestra anticipación de aterrizaje.
- En esquive, reduce su altura de manera evidente: se aplana, desliza o convierte temporalmente sus piernas en una estela baja. El collider debe acompañar la silueta.
- Durante la ráfaga, abre paneles laterales o un aro de energía y dispara pulsos luminosos al ritmo. No usar un arma de fuego realista.
- Al recibir daño, la luz parpadea y el cuerpo se fragmenta brevemente sin gore.
- Al morir, se dispersa en fragmentos luminosos y se recompone al revivir.

Assets y animaciones mínimos del protagonista:

- Hoja de modelo con frente, perfil y paleta.
- Sprites con fondo transparente y pivotes consistentes.
- Idle flotante.
- Transición de núcleo flotante a corredor.
- Carrera con piernas, mínimo 8 fotogramas o animación esquelética equivalente bien pulida.
- Anticipación de salto, ascenso, ápice, caída y aterrizaje.
- Esquive/deslizamiento y recuperación.
- Inicio, bucle y final de ráfaga.
- Daño, muerte, reaparición y victoria.
- Estela de velocidad, polvo por región, destello de salto, pulsos de ataque y partículas de reaparición.
- Avatar para HUD, retrato de selección y silueta de desbloqueo.

Usa ImageGen cuando sea apropiado para producir raster art original. Mantén un archivo con los prompts, semillas o decisiones visuales necesarias para reproducir el estilo. Revisa visualmente cada asset, elimina fondos residuales, corrige bordes y configura los importadores de Unity. No entregues imágenes sin integrar.

## Mecánicas obligatorias

### Carrera

- Desplazamiento horizontal automático a velocidad configurable.
- Cámara lateral suave con anticipación hacia delante.
- Física estable a 60 FPS y comportamiento independiente del frame rate.
- Incrementos de velocidad solo cuando el diseño del nivel los comunique.

### Salto

- Input con `Space`, clic o botón de mando equivalente.
- Coyote time y pequeño input buffer para evitar controles injustos.
- Altura de salto consistente y ajustable.
- Feedback visual y sonoro al saltar y aterrizar.

### Próximo avance prioritario: doble salto y plataformas

Petición del usuario tras probar el prototipo: el recorrido actual se siente demasiado plano y seco. Antes de ampliar sistemas secundarios, enriquecer la jugabilidad con puntos de doble salto y bases donde aterrizar y continuar corriendo.

Dirección confirmada por el usuario: guiarse por Geometry Dash para conseguir una experiencia parecida en fluidez, ritmo, lectura de obstáculos y encadenamiento de plataformas, sin convertir SuyuRun en una copia. Priorizar patrones cortos que se aprenden y luego se combinan, impulsos aéreos señalizados, cambios de altura y aterrizajes satisfactorios. Conservar como identidad central a Suyu con sus piernas y transformaciones, las regiones Costa/Sierra/Selva, la música de inspiración peruana, el esquive, la ráfaga temporal y los coleccionables conectados al museo cultural. No copiar niveles, personajes, música ni recursos de Geometry Dash, ni incorporar automáticamente todas sus modalidades de juego.

Referencia visual añadida el 18 de septiembre: https://youtu.be/v9JsLfmJuqg y captura facilitada por el usuario. El estilo ilustrado del fondo es un requisito explícito: colores vivos, nubes grandes, formas limpias y planos con profundidad, adaptados a cada región. También son obligatorias bases arquitectónicas elaboradas donde aterrizar: cornisas, relieves, escalones, volumen y soportes; no simples cajas. Consultar `Assets/SuyuRun/Art/ART_DIRECTION.md` para decisiones, límites y procedencia del primer fondo integrado.

Ampliación confirmada con una segunda captura: incluir picos anclados al suelo y a las plataformas que obliguen a saltar, individualmente y en grupos alcanzables. Los colliders deben respetar la silueta de los picos para evitar muertes por esquinas vacías. Diferenciar estructuras con nichos, cornisas y fachadas escalonadas; no amontonar ornamentos, monedas y peligros ni confundir el fondo con elementos jugables. Tratar cada estructura como un conjunto de capas: el suelo del fondo no debe dibujarse por encima de su fachada. Dar aire entre retos, anticipación suficiente y trayectorias de monedas sin duplicados. Priorizar composición y claridad sobre cantidad de objetos.

Preferencia operativa actual del usuario: continuar desarrollando sin ejecutar pruebas del EXE durante este avance. Se permiten comprobaciones de compilación; documentar qué cambios no tienen todavía validación jugable y no reutilizar resultados antiguos como si probaran la versión nueva.

- Implementar puntos de impulso aéreo con diseño original. Interpretación inicial ajustable: al atravesar un punto señalado, se habilita un segundo salto al pulsar de nuevo el control de salto; no es un doble salto ilimitado disponible en cualquier lugar.
- Comunicar la disponibilidad y el consumo del impulso con una silueta reconocible, animación y sonido. Mantener pulsado el botón no debe consumir automáticamente el segundo salto.
- Añadir plataformas elevadas, escalones y huecos con aterrizajes reales, no bases meramente decorativas. El personaje debe poder correr sobre ellas, saltar hacia otra y caer al perder el apoyo.
- Encadenar secuencias de salto, impulso aéreo, aterrizaje y esquive, alternadas con descansos. Introducir primero cada mecánica en una situación sencilla y luego combinarlas con la música.
- Usar arcos de monedas y señales visuales para sugerir trayectorias. Ajustar alturas, distancias y anticipación a la velocidad y al alcance real del salto; evitar saltos ciegos o imposibles.
- Crear arte y efectos propios para puntos de impulso y plataformas; tomar Geometry Dash como referencia de fluidez, no copiar sus assets ni patrones de nivel.
- Incorporar posiciones, tamaños y puntos de impulso a los datos editables del recorrido. Restaurar correctamente el apoyo y el estado del impulso al reiniciar o reaparecer.

Criterio de aceptación del siguiente hito: una sección de Costa jugable con un encadenamiento completo de salto, segundo impulso y aterrizaje en una plataforma elevada, seguida de continuación de la carrera. Verificar colisiones desde arriba y laterales, caída por huecos, ausencia de saltos infinitos, reaparición segura y funcionamiento a diferentes tasas de frames. Actualizar la prueba automática para que valide estas mecánicas, además de probarlas manualmente. No marcar esta petición como implementada por documentarla.

### Esquive

- Input con flecha abajo, `S` o botón de mando.
- Reduce temporalmente la altura del collider.
- Sirve para atravesar obstáculos altos o proyectiles claramente anunciados.
- Tiene inicio y final legibles; no debe quedarse bloqueado.

### Ráfaga rítmica

- Poder ofensivo temporal, no ataque permanente.
- Se obtiene al llenar un medidor con acciones correctas, coleccionables especiales o secciones diseñadas del nivel.
- Al activarse, Suyu dispara pulsos de energía en una ráfaga rápida durante pocos segundos.
- Los pulsos destruyen obstáculos o enemigos marcados como destructibles; no eliminan peligros estructurales.
- Mantener acciones cercanas al pulso puede conservar o potenciar la ráfaga.
- Debe tener anticipación, indicador de duración, feedback de impactos y final claro.
- Balancear para que sea una recompensa y no permita ignorar todo el nivel.

### Ritmo

- Usar `AudioSettings.dspTime` o un reloj de audio equivalente como referencia temporal.
- Los niveles se diseñan manualmente mediante datos, no por generación procedural.
- Separar la lógica de beatmap de los GameObjects para poder editar tiempos, tipos de evento y posiciones.
- Clasificar acciones de ritmo como `Perfecto`, `Bien` o `Fuera de ritmo` cuando corresponda, sin impedir que un jugador casual avance.
- Incluir offset de sincronización configurable si las pruebas revelan latencia perceptible.

### Monedas, vidas y checkpoints

- Las monedas se recogen durante los niveles y persisten localmente.
- El jugador puede comprar `Reintentos` en el museo.
- Al morir, si acepta usar un reintento, reaparece en el último checkpoint válido conservando el estado coherente del nivel.
- Sin reintentos, reinicia el nivel.
- Evitar duplicación de monedas al reaparecer: cada moneda tiene identidad y estado por intento.
- Los reintentos solo utilizan moneda interna del juego; no hay pagos reales.

### Personajes desbloqueables

- Incluir al menos tres apariencias/personajes jugables con la misma base mecánica para no romper el balance.
- Se desbloquean alcanzando una puntuación objetivo o, como alternativa, comprándolos con monedas acumuladas.
- Mostrar requisitos claramente en selección.
- Cada variante necesita retrato, sprite/animaciones completos y una descripción breve; no basta un cambio de color sin intención.

## Tres niveles terminados

Cada nivel debe durar aproximadamente entre 2 y 3 minutos, tener inicio, progresión, clímax y meta, y poder completarse de principio a fin. Diseñar primero un greybox jugable, validar el ritmo y luego sustituirlo por arte final.

### Costa

- Selección actual del usuario: El Alcatraz (MP3 entregado), integrado con respeto a su contexto afroperuano. La sugerencia inicial de marinera queda reemplazada para este nivel. Documentar los derechos de la grabación antes de distribuirla.
- Paleta cálida, mar, desierto costero y arquitectura/entornos pertinentes.
- Coleccionables posibles: huaco retrato, textil Paracas, cerámica Nazca y personajes investigados relacionados con la región.
- Obstáculos con lectura limpia: muros bajos, arcos altos, plataformas, aves o peligros estilizados.

### Sierra

- Identidad sonora inspirada en huayno y carnaval andino.
- Montañas, terrazas, caminos pétreos y cambios de altura.
- Coleccionables posibles: quipu, kero, cabeza clava y personajes investigados relacionados con la región.
- Patrones que combinen saltos, desniveles y esquives sin saltos de dificultad injustos.

### Selva

- Identidad sonora inspirada en tradiciones amazónicas y percusión orgánica, con investigación y atribución respetuosas.
- Vegetación estratificada, ríos, neblina, luz bioluminiscente moderada y alta claridad del camino.
- Coleccionables posibles: cerámica Shipibo-Konibo, instrumentos, textiles u objetos documentados, junto con personajes investigados relacionados con la región.
- Patrones más fluidos, lianas, plataformas y secciones finales que combinen todas las mecánicas.

No presentar a Costa, Sierra o Selva como culturas homogéneas. Las fichas deben nombrar cultura, periodo, lugar y fuente concreta cuando sea aplicable.

## Museo y mapa de niveles

El menú principal jugable será un museo-mapa con transición visual entre Costa, Sierra y Selva.

- Tres alas o espacios conectados, uno por región.
- Cada ala permite iniciar su nivel y consultar piezas/personajes.
- Las entradas obtenidas muestran ilustración, nombre y ficha completa.
- Las entradas no obtenidas no permanecen totalmente ocultas: muestran silueta, título parcial o categoría, una breve “probadita” informativa y una pista para volver a jugar.
- Diferenciar visualmente objetos culturales y personajes históricos.
- Mostrar progreso regional y total.
- Permitir comprar reintentos y seleccionar personajes.
- Al acabar un nivel, mostrar primero resultados y luego las fichas nuevas obtenidas.

Contenido mínimo: cuatro entradas por región, incluyendo al menos dos objetos y un personaje, para un mínimo total de doce fichas. Investigar y citar fuentes institucionales o académicas confiables. Guardar las fuentes en los datos y mostrarlas de forma discreta en la ficha. No inventar hechos ni atribuciones.

## Arte completo

Crear e integrar una dirección artística 2D coherente. Evitar copiar Geometry Dash, Dota 2, Never Alone, Muse Dash o cualquier referencia. Usarlas solo para comprender legibilidad, ritmo o tratamiento cultural.

Entregables visuales mínimos:

- Protagonista Suyu y todas sus animaciones.
- Tres personajes/apariencias jugables completos.
- Tilesets separados para Costa, Sierra y Selva.
- Fondos con parallax de al menos tres capas por región.
- Plataformas, suelo, obstáculos bajos/altos y peligros destructibles.
- Enemigos u objetivos simples necesarios para la ráfaga.
- Moneda, reintento, checkpoint, meta y coleccionables.
- Efectos visuales de movimiento, ritmo, impacto, recolección y muerte.
- Museo completo, pedestales, mapa regional y transiciones.
- UI: logo, botones, paneles, HUD, resultados, pausa, configuración, selección y fichas.
- Tipografía legible con licencia compatible y soporte de caracteres españoles.
- Iconos consistentes para controles, monedas, puntuación, combo, vidas y sonido.

Los assets finales deben tener tamaño, compresión, filtros, pivotes, sorting layers y pixels-per-unit coherentes. Usar atlas cuando mejore rendimiento. No dejar sprites de prueba, cuadrados blancos o textos de desarrollador visibles en la build final.

## Audio

### Selección del usuario para Costa: El Alcatraz

El usuario entregó `El Alcatraz.mp3` y pidió sincronizar el runner con esa grabación, además de incluir un bailarín afroperuano pequeño en la orilla o sobre una roca del paisaje, sin competir visualmente con el protagonista. No tratarlo como enemigo ni convertirlo en caricatura. Mantener una animación articulada original que siga el mismo reloj de la música, se detenga con la pausa y recupere el pulso al reaparecer. La fotografía aportada sirve como referencia contextual, no como asset para recortar.

Usar la canción completa para la evolución de Costa; validar tempo, marcas y ventanas de acción a oído antes de declarar sincronización final. Un análisis automático es un punto de partida, no prueba de sincronización perfecta ni de autenticidad coreográfica. Ver implementación, procedencia y límites en `Assets/SuyuRun/Audio/AUDIO_NOTES.md`. Documentar licencia y créditos de la grabación antes de publicar o distribuir. Esta elección del usuario prevalece sobre la sugerencia anterior de marinera para Costa; no imponer esa sugerencia como requisito.

- Una pista original o legalmente compatible por región.
- Efectos de salto, caída, esquive, moneda, pieza cultural, checkpoint, ráfaga, impacto, daño, muerte, compra, desbloqueo y navegación UI.
- Mezclador con buses de música y efectos, controles de volumen y mute.
- Registrar origen, autor y licencia de cada recurso externo.
- Si la música final no puede producirse legalmente durante el desarrollo, crear pistas originales provisionales suficientemente estructuradas para validar beatmaps y dejar documentado el reemplazo; no descargar música sin licencia.

## Escenas y arquitectura

Escenas mínimas:

- `Bootstrap`
- `MainMenu`
- `Museum`
- `Level_Costa`
- `Level_Sierra`
- `Level_Selva`
- `Results`

Organizar `Assets/SuyuRun/` con carpetas para `Art`, `Audio`, `Data`, `Prefabs`, `Scenes`, `Scripts`, `Tests` y `UI`. Separar código en ensamblados Runtime, Editor y Tests cuando sea útil.

Sistemas esperados:

- Estado global y carga de escenas.
- Input desacoplado.
- Controlador del runner y estados del personaje.
- Cámara.
- Reloj rítmico y beatmaps data-driven.
- Spawning/activación de eventos del nivel.
- Monedas y coleccionables con IDs estables.
- Checkpoints y reaparición.
- Ráfaga y objetivos destructibles.
- Puntuación, combo y resultados.
- Museo, fichas y desbloqueos.
- Tienda de reintentos y selección de personajes.
- Persistencia local versionada con recuperación ante datos dañados.
- Audio mixer y configuración.
- Pausa, reinicio y navegación completa.

Usar `ScriptableObject` para configuración de niveles, personajes, piezas culturales, precios y balance. Evitar singletons rígidos innecesarios, búsquedas repetidas por nombre y dependencias ocultas. Mantener valores de diseño configurables desde Inspector.

## Controles iniciales

- Saltar: `Space`, clic principal o botón sur de mando.
- Esquivar: `S`, flecha abajo o botón este.
- Ráfaga: `E`, clic secundario o gatillo.
- Pausa: `Escape` o botón Start.

Mostrar controles antes del primer nivel y permitir reasignación si el Input System lo hace viable sin comprometer el plazo.

## Persistencia

Guardar localmente:

- Monedas totales.
- Reintentos comprados.
- Personajes desbloqueados y seleccionado.
- Niveles completados y mejores puntuaciones.
- Coleccionables obtenidos.
- Fichas del museo reveladas.
- Volumen, controles y offset de audio.

Incluir versión del formato, validación básica y valores seguros si el archivo está incompleto o corrupto. No almacenar secretos ni datos personales.

## Calidad, accesibilidad y rendimiento

- Objetivo de 60 FPS en un PC de laboratorio razonable.
- Escenas sin excepciones ni errores rojos en consola.
- Evitar asignaciones por frame y búsquedas costosas en Update.
- Pooling para proyectiles, efectos y objetos repetitivos cuando corresponda.
- Colisiones y capas configuradas explícitamente.
- Resoluciones 16:9 desde 1280x720 hasta 1920x1080.
- Contraste suficiente entre personaje, peligros y fondo.
- No depender solo del color para indicar peligro o coleccionable.
- Opción para reducir destellos y sacudida de cámara.
- Volumen separado y pausa funcional.

## Pruebas obligatorias

Crear y ejecutar pruebas útiles para:

- Persistencia y migración de datos.
- Compra de reintentos sin saldo negativo ni doble descuento.
- Monedas sin duplicación tras checkpoint.
- Desbloqueo por puntuación o compra.
- Clasificación de ventanas rítmicas.
- Uso y expiración de la ráfaga.
- Restauración correcta del collider después del esquive.

Además, probar manualmente:

- Los tres niveles de inicio a fin.
- Muerte con y sin reintentos.
- Pausa/reanudación sin romper audio ni beatmap.
- Museo antes y después de recolectar piezas.
- Reinicio de aplicación y recuperación del progreso.
- Navegación completa con teclado y ratón.
- Build Windows limpia.

## Orden de implementación

1. Auditar el proyecto y dejar configuración/Git limpios.
2. Crear estructura de carpetas, escenas base y datos.
3. Construir un vertical slice gris de Costa con carrera, salto, esquive, cámara y muerte.
4. Integrar reloj musical, beatmap y feedback de precisión.
5. Añadir ráfaga, monedas, checkpoints, reintentos y persistencia.
6. Construir museo-mapa, fichas, selección y resultados.
7. Completar los greyboxes de Sierra y Selva.
8. Crear la biblia visual y producir todos los assets originales.
9. Sustituir placeholders e integrar audio y contenido cultural verificado.
10. Balancear los tres niveles, ejecutar pruebas, corregir errores y generar build.

No esperes a tener arte final para validar gameplay. No conserves placeholders en la entrega final.

## Definition of Done

### Solicitudes de presentación de Costa (18 de septiembre)

- Conservar una escena ejecutable independiente del primer prototipo de cerros, además de la Costa ilustrada actual. Implementadas: `Prototype_Legacy` y `Level_Costa`.
- Usar las cuatro grabaciones aportadas: El Alcatraz en Costa, sierra.mp3 y selva.mp3 en sus respectivas regiones e inicio (2).mp3 en el menú. Importadas; no confundir importación con sincronización final de Sierra/Selva.
- Parallax de avance por capas, con bailarín afroperuano en plano intermedio. Añadir figuras originales coherentes: embarcaciones, totora, muelles, redes, vasijas, cestas, conchas, arquitectura y aves. Mantener contraste de peligros y no cubrir aterrizajes.
- Agua con movimiento natural y discreto: espuma y reflejos lentos, balanceo leve de embarcaciones; no hacer que todo el mar palpite agresivamente con cada beat.
- Ráfaga con blancos claros: cajas y aves de combate; recompensas de monedas y memorias culturales. Los pelícanos lejanos son solo decorativos.
- Objetos y personajes de museo contextualizados por región, con adelantos de fichas no recogidas. Primera colección de Costa: Moche, Paracas, Nicomedes Santa Cruz y María Reiche. No presentar decoración fantástica como reconstrucción arqueológica.
- El usuario aclaró que permite probar el juego; durante este avance usar Unity Play, sin generar otro ejecutable externo. Esta aclaración prevalece sobre la restricción anterior de pruebas. La build de entrega final sigue siendo un hito separado.
- Exigir revisión visual, jugable y auditiva antes de declarar Costa o el juego completo al 100 %. La cantidad de adornos no sustituye composición, balance ni pruebas.
- Aclaración de acabado: «100 %» en esta petición significa presentación visual cuidada y animaciones completas, comparable con un juego profesional hecho en GDevelop; NO cambiar de motor. Priorizar mar visible y espacio libre. Repetir al bailarín a lo largo de la orilla, bandadas con vuelo propio y nubes en deriva lenta. Eliminar saturación de decoraciones antes de añadir más.

El proyecto solo se considera completo cuando:

- Los tres niveles están terminados y se pueden completar.
- Todas las mecánicas obligatorias están integradas y explicadas al jugador.
- El museo funciona como mapa, colección, tienda y selector.
- Hay al menos doce fichas culturales documentadas.
- Suyu y los personajes alternativos tienen arte y animaciones completos.
- No quedan cuadrados provisionales, textos de depuración ni enlaces rotos.
- El progreso persiste y los reintentos funcionan desde checkpoints.
- No hay errores de compilación ni excepciones en consola.
- Las pruebas relevantes pasan.
- Existe una build Windows ejecutable verificada.
- `README.md` explica controles, ejecución, estructura, créditos, licencias y fuentes culturales.
- Se entrega un breve registro de lo implementado, pruebas realizadas, limitaciones reales y rutas de los artefactos.

## Cronograma objetivo y obligación de entrega completa

Usar la siguiente estimación realista como referencia de ejecución:

- Prototipo jugable básico: 4-8 horas.
- Un nivel completo como modelo: 1-3 días.
- Los tres niveles, museo, monedas, reintentos, personajes, ataque y esquive: 5-10 días.
- Pulido, sincronización musical, balance, contenido histórico y corrección de bugs: 1-2 semanas adicionales.

Estas duraciones sirven para organizar hitos y comunicar avance; no autorizan a recortar el alcance ni a declarar el proyecto terminado antes de cumplir la Definition of Done.

La entrega final debe quedar al 100 %. Esto incluye código, tres niveles completos, gameplay, museo, fichas culturales verificadas, personajes, animaciones, arte final, UI, efectos visuales, efectos de sonido, música integrada y sincronizada, mezcla de audio, guardado, balance, pruebas, créditos, licencias y build Windows ejecutable. No entregar una demo incompleta, un greybox, assets provisionales, música pendiente ni sistemas desconectados. Si un recurso externo no puede utilizarse legalmente, crear o integrar una alternativa original y compatible antes de considerar finalizado el proyecto.

## Forma de trabajo y comunicación

Empieza inspeccionando el estado real del proyecto. Luego ejecuta el trabajo; no devuelvas solo un plan. Comparte actualizaciones breves al completar hitos o encontrar un bloqueo real. Toma decisiones razonables sobre nombres, organización y valores provisionales, dejando los parámetros ajustables.

Si falta un recurso externo, avanza con una alternativa original y legalmente segura. Si necesitas que el usuario elija entre opciones que cambian materialmente el producto, prepara primero una versión concreta y revisable de las alternativas. Mantén una lista de pendientes verificable y no declares el juego terminado hasta cumplir la Definition of Done.
