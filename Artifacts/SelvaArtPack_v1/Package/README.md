# SUYURUN — paquete de arte SELVA v1

Entrega de ARTE PARA REVISIÓN, 27/09/2026. No es el nivel jugable.
Costa y Sierra quedan cerrados por aprobación y prueba del usuario. No se han
modificado sus assets, scripts ni escenas. No se creó un ejecutable.

## Dirección y aprobación

- El usuario solicita explícitamente el paquete completo: excepción al flujo de
  un asset por entrega. La aprobación de la paleta y del resultado visual sigue pendiente.
- Referencia principal: Costa aprobado y Sierra ProductionV3, no el Sierra V2.
- 320×180, vistas x4 nearest, contornos nativos, masas de color limpias. Los PNG
  de producción NO llevan escalado de presentación. 24 colores compartidos,
  sin colores añadidos ni alfa parcial. Dithering solo en nubes y niebla.
- Se mantiene la paleta propuesta `selva_palette.json`; no se declara aprobada.
- El protagonista conserva el rostro y el traje protector que recibe en Sierra,
  según la confirmación posterior del usuario; esa continuidad prevalece sobre
  rediseñarlo con el traje de fibras de la primera versión de la biblia.
- El ensayo imagegen de árboles se descartó por exceso de textura. Los PNG de
  este paquete se dibujaron con Pillow nativo, autorizado en la sección13.

## Ambientes

1. Bosque de entrada: ceibas, aguajes, raíces, flores, fauna, suelo de tierra.
2. Río: plataformas flotantes, lianas, cascada, bote/remador, caimanes.
3. Cueva — MININIVEL lateral de exploración, no carrera rítmica: fondos de roca,
   plataformas verticales, antorchas, gotas, murciélagos y guardianes de raíces.
4. Ruinas — MININIVEL lateral de exploración: pilares y dinteles cubiertos de
   vegetación, plataformas, serpientes y centinela escarabajo de bronce.
5. Bosque nocturno: siluetas, luna, luciérnagas, casa ribereña y puentes.
6. Templo final fantástico: estructura escalonada, puerta y portal animado.

Cada ambiente tiene seis capas independientes de640×180; cielo, lejanía,
estructuras, agua animada, cercanía y frontal. No se ha dividido una ilustración
completa en franjas. El suelo jugable es modular en32×32, NO parte del fondo.
Agua:4frames, cada uno640×180, apilados verticalmente en su hoja. La niebla es
otra capa independiente. Solo el cielo está exento de repetición horizontal.

## Animaciones y módulos

- Héroe48×48: idle, run, jump, double_jump, land, slide, burst, reload, hit,
  grab, swing, release, regrab, fall, climb, victory, death.
- Liana16×96 con nudo, más ciclo de balanceo128×112 y posiciones de agarre
  por frame. Agarrar → balancear → soltar → saltar → agarrar siguiente liana.
- Caimán64×32: idle, nadar, salto, mordida, caminar, daño y derrota.
- Fauna: guacamayo, mono, delfín rosado estilizado, luciérnaga, helecho al viento.
- Enemigos: murciélago, serpiente, brote animado y escarabajo.
- Jefes96×96: guardián de raíces y centinela escarabajo; despertar, idle,
  andar, anticipación, golpe, carga, daño, derrota.
- Suelos, paredes, bordes, rampa, tronco64×16, piedra48×16, victoria48×12,
  puente, bote; pinchos, estaca, raíz, techo y trampa.
- Efectos: salpicadura, polvo, hojas, impacto, astillas, escudo activo/rotura,
  destello, portal, veneno, disparo, rastro de salto, gotas.
- Accesorios: antorcha, flor, hongo, cascada, entradas de cueva/ruinas,
  cristal de salto/descenso, roca musgosa, raíz colgante, rama de anclaje,
  casa ribereña y bote con remador.
- Coleccionables16×16: cerámica, flauta, textil, sonaja y moneda.
  Cuatro iconos de museo64×64 están dibujados a esa resolución, con detalles
  propios; NO reconstrucciones de piezas históricas concretas.
- UI: panel, botón, casilla y barra9-slice. Son componentes de color opcionales:
  NO reemplazan la composición ni el sistema compartido de ventanas del juego.
  Sin texto horneado; usar la fuente pixel existente.

Las criaturas guardianas, arquitectura del templo y ornamentación son ficción
original. No se atribuyen a un pueblo amazónico ni a un yacimiento real.
Las fichas culturales de museo requieren investigación/validación antes de
integrar el nivel. No se presentan los motivos ornamentales como kené auténtico.

## Archivos y Unity

`Assets/SuyuRun/Art/PixelSelva/ProductionV1/` contiene los PNG y un JSON por hoja.
`manifest.json` enumera todos los assets y frames. `palette.json` conserva la propuesta.
`Tools/` contiene los generadores editables; `SourceReferences/`, la referencia
del traje existente. `Previews/` contiene montajes y GIF, NO capturas del juego.

Tras aprobación, copiar la carpeta `Assets` del paquete al proyecto. El importador
incluido solo afecta a `PixelSelva/ProductionV1`, aplica Point/None, PPU32,
sin mipmaps y slicing exacto. Después usar el menú:
`SUYURUN > Selva propuesta > Importar arte y crear clips`.
Ese importador se entrega preparado pero NO se ha ejecutado ni compilado aquí:
el encargo actual es arte, sin cambiar el proyecto activo.

JSON: `rectsUnity` usa origen inferior izquierdo; `handPerFrame`, `gripPerFrame`
y anclas usan origen superior izquierdo del frame. Convertir Y al integrar.
Los campos fps son una propuesta de reproducción, NO tiempos MIDI.
Para personajes/sprites usar pivot(.5,0); para capas se recomienda asignar el
renderer con esquina inferior izquierda coherente al cambiar de pivot.

## Contrato del balanceo para la siguiente etapa

1. La rama fija el ancla; el nudo indica el punto interactivo.
2. Al agarrar, colocar la mano en `gripPerFrame`; nunca el centro del héroe.
3. Balanceo pendiente de física; estos8frames muestran las poses, no implementan
   el péndulo. Seleccionar pose por ángulo, no forzar la física al fps del GIF.
4. Al soltar, conservar la velocidad tangencial y entrar en release/jump/fall.
5. Siguiente agarre solo con distancia y ventana válida. El caimán anticipa antes
   de saltar; ventana de mordida y collider se definirán en gameplay.
6. Evitar rotaciones suaves de sprites; rasterizar/seleccionar poses y snap a píxel.

## Límites de esta entrega

Incluye el lote visual y previews. Pendientes: tu aprobación, importación y prueba
en Unity, física de lianas, IA, colisiones, escenas, balance, cámaras, transiciones,
timing MIDI y música propia de mininiveles. No altera la música ya importada ni
afirma que las cuevas/ruinas tengan canciones nuevas. La plaza FINAL de las tres
regiones es otro nivel y no se incluye en el paquete Selva.
