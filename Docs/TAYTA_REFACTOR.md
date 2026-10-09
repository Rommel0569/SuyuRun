# Tayta — reemplazo visual regional

## Ajustes de alineación — 02/10/2026

- Sierra: compensación de un píxel entre el antiguo canvas feet-47 y Tayta de 48px. Agacharse usa el pivote superior izquierdo real, escala actual y anclaje al suelo, sin el padding del personaje anterior. Mirar a la izquierda en cuevas compensa 48px para mantener el centro físico. Se evita dibujar una segunda pistola durante ráfaga.
- Costa: distancia final fijada al portal, aterrizaje cinematográfico y reset de pose antes de revelar brazos. El encogimiento recalcula el offset del sprite y converge al vórtice; se mantiene la pose con brazos al pasar a resultados.
- Sierra final: portal centrado en el destino de la caminata de victoria, con ancho/altura leídos del sprite y suelo de esa coordenada.
- Cuevas de Sierra: portal dentro del ancho del campo, centro compartido con AtPortal, detección solo en su piso y fijación al centro después de los 3 segundos. Selva: portal de 64px centrado sobre la coordenada usada por AtPortal y misma fijación al salir.
- Lianas: metadatos de conexión por pose calculados de los píxeles superiores del agarre; el render se ancla a Grip sin alterar la trayectoria física.
- Pruebas: 48 hojas verificadas con baseline al píxel48; compilación runtime sin errores; PortalAlignmentAudit PASS para ambas cuevas de Sierra y ambos mininiveles de Selva, con salida de3s en el centro/suelo del portal y rechazo del piso incorrecto en Sierra. Diagnóstico .NET, no build de jugador ni ejecutable del juego. La revisión visual interactiva de todos los recorridos en Unity Play sigue pendiente.

Encargo y lote de generación/integración autorizados por el usuario. Identidad basada en su referencia: chullo, poncho, rostro expresivo, pantalón y calzado. La aclaración posterior conserva Costa sin brazos visibles y su revelación al final; Sierra lleva brazos y huaraca; Selva lleva protección de fibras/madera, con rostro y chullo reconocibles.

## Entrega

- 48 spritesheets, 323 celdas de 48x48 y sus JSON en Assets/SuyuRun/Art/Tayta/Resources/Tayta. Paletas regionales existentes, alfa binario, Point, sin compresión ni mipmaps.
- Costa: idle, carrera, salto, aterrizaje, doble salto/caída, slide, ráfaga, daño, revelación de brazos. La cinemática termina con brazos visibles.
- Sierra: idle, carrera, salto, slide, carga/giro/lanzamiento de huaraca, pistola, daño, derrota, baile y traje de recompensa. Trepar usa una secuencia de poses elevada derivada del salto, no una nueva simulación.
- Selva: idle, carrera, salto/doble salto/caída/aterrizaje, slide, pistola, recarga, guardia, puño, patada, uppercut, daño, derrota, baile/victoria, agarre/reagarre/suelta/trepa y balanceo de 16 poses.
- Salto, caída, aterrizaje y transiciones de liana comparten poses seleccionadas de su secuencia; no se anuncian como animaciones independientes dibujadas desde cero.
- Fuentes imagegen retenidas en Artifacts/Tayta, con manifiesto, validación, previews x4 y GIF de carrera. Herramienta integrada imagegen, normalización nativa Pillow autorizada por la biblia; generador reproducible Tools/prepare_tayta.py.
- TaytaArt es solo presentación. Conectado en RunnerPrototype, RunnerCoastalEnding, SierraStageDirector (incluye sus cuevas) y SelvaStagePresentation. Sprites originales y Animator anterior conservados como respaldo. No se cambian velocidades, hitboxes, acciones, balance ni música.
- Museo V4 aprobado e integrado. No se crea nivel secreto ni ejecutable.

## Verificación y límites

Validación PNG: 48 hojas / 323 frames, dimensiones, alfa y máximo de colores sin fallos. Compilación de scripts runtime y del importador Editor sin errores. La prueba de los tres niveles en Unity Play y la aceptación visual final de las animaciones siguen pendientes; no se declara todo el juego certificado ni 100% probado.

Las imágenes generadas no traen una cuadrícula vertical perfectamente uniforme; el importador usa bandas alfa y separaciones por proyección para evitar arrastrar sombreros y partes de frames vecinos. Hay derivación de poses y variaciones pequeñas de proporción propias de las fuentes que deben revisarse al tamaño real en Play.
