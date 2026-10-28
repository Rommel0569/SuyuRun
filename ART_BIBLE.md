# BIBLIA DE ARTE Y PRODUCCIÓN: SUYURUN

Dirección del usuario recibida el 18 de septiembre de 2026. Sustituye decisiones anteriores incompatibles. Se normalizan únicamente las marcas de escape y el formato de listas de la conversación.

## Enmienda vigente — refactor visual, 03/10/2026

27/10/2026, dirección posterior: el nuevo Tayta debe conservar todos los movimientos del anterior; si no resulta equivalente, el usuario prefiere mantener el anterior. Se reactiva el banco anterior en gameplay y final y se conserva Uniform como respaldo desactivado. No reiniciar otro refactor amplio sin una nueva indicación del usuario.

24/10/2026, pedido posterior vigente: unificar la anatomía humana de Tayta en todos los trajes/estados y aplicar las mejoras al juego. Banco común de personajes para niveles, mininiveles y final, conservando los anteriores. Costa mantiene brazos ocultos hasta su logro, Sierra brazos visibles, Selva traje protector completo sobre anatomía humana. Sustituye para los actores la separación previa de recursos entre gameplay y epílogo; no modifica la independencia de los fondos ni autoriza cambiar recorridos, colisiones o música. Lote autorizado por el usuario sin pausas individuales; aceptación visual de las nuevas hojas pendiente.

Final22/10/2026: usuario autoriza rehacer en lote TODO el arte visual del epílogo, sin reutilizar ciclos ni fondos de gameplay. Banco FinalMovieV3 exclusivo, capas independientes, identidad de Tayta y paletas conservadas, música elegida y créditos originales. No confundir autorización de producción con aprobación visual final. Conservar las versiones anteriores y verificar reproducción en Unity Play, sin generar ejecutable.

Ratificación posterior 08/10/2026: usuario probó y aprobó la calidad del lote AtmosphereClean y pide extender ese acabado a todas las capas de fondo usadas por biomas y mininiveles. Aplicar limpieza de tramado repetitivo, detalle nativo y exportación de mayor densidad conservando paleta, geometría y originales; no atribuir aprobación de capas nuevas a esa referencia aprobada.

Ratificación 08/10/2026: usuario pide «TODO ... COMPLETO EN PAQUETE» y autoriza aumentar calidad/detalle nativo de todas las capas, niveles y mininiveles sin pausas individuales. Conservar el HUD y portales que comprobó conformes, originales, paletas, tamaños lógicos, colisiones y recorrido. Corregir deformación de Tayta al lanzar huaraca. El acabado cinematográfico final y nuevas mejoras de jugabilidad Selva quedan para después por su indicación. No confundir autorización del lote con aprobación visual de cada resultado.

Ratificación 04/10/2026: «Dámelo completo ... tipo profesional, complétalo ... al 100%». El usuario solicita completar el lote sin otra pausa de aprobación individual. Conservar los originales, las paletas, las colisiones y el flujo; verificar en Unity antes de anunciar finalización.

Pedido más reciente: ampliar el refactor a escenarios, obstáculos y todo el
arte del juego principal, aumentando detalle nativo sin aspecto suave ni
cambiar el flujo. La resolución del PNG no determina su tamaño en mundo:
conservar encuadre, posiciones, música y conexiones. El usuario pide reducir
al protagonista: escala común 0.85 y colisiones proporcionales, también en
cuevas; agacharse debe tener una silueta realmente baja. Mantener originales,
paletas regionales y hojas nuevas versionadas. El aumento de resolución no
autoriza declarar todo terminado ni presentar un reescalado como dibujo nuevo.

Pedido posterior vigente: «arte como si ya tuvieras que incorporarlo al juego»
y «pack completo de todo», ratificado con «LO APRUEBO». Lote autorizado sin
pausas por asset. Preparar PNG nativos y contrato de importación verificable;
no considerar una preview como integración ni un banco parcial como completo.

Actualización posterior: el usuario solicita el banco completo en lote
(«dame el banco completo de todos, confío en ti»). Autoriza avanzar sin
pausar por aprobación de cada hoja, manteniendo la dirección visual aprobada.
Esta autorización sustituye el paso de aprobación individual para este banco.

El usuario autoriza aumentar la resolución de los sprites para conservar el
detalle de la referencia de Tayta. Primera propuesta: personajes 96x96, mismo
tamaño en mundo y mismas colisiones. Sustituye 48x48 y cuadrícula única donde
sea necesario para estos sprites; mantiene pixel art y colores regionales.
Traje protector de Selva: tipo gorila fantástico, rostro humano visible.
Refactor de personajes, jefes, animaciones y efectos de habilidades, uno por
uno con aprobación. Escenarios admiten pulido visual sin cambiar el flujo.
No se consideran aprobadas hojas nuevas por aprobar la dirección general.

GUÁRDALO EN TU MEMORIA. Antes de generar, modificar o corregir CUALQUIER asset, relee este documento completo. Si te pido un cambio, modifica SOLO lo pedido y respeta reglas, paleta y estilo ya aprobados. Al terminar cada asset, resume en 1 línea qué hiciste y espera mi confirmación. Si algo choca con estas reglas, avísame en una línea antes de generar.

## 1. PROYECTO

SUYURUN (una sola palabra; reemplaza "SUYU / RUN" en todo el arte y la UI). Videojuego 2D runner rítmico en Unity (curso Desarrollo de Software para Juegos, UNSA). El personaje corre solo; el jugador salta, se desliza, dispara ráfagas y esquiva al compás de música peruana, estilo Geometry Dash. Recolecta monedas y piezas de patrimonio cultural para un museo virtual permanente. Tres niveles (COSTA, SIERRA, SELVA) más un FINAL; cada nivel da un poder nuevo al protagonista.

## 2. ESTILO GLOBAL (OBLIGATORIO)

- Usa la imagen adjunta SOLO como referencia de estilo: pixel art 16-bit, atardecer dramático, paleta cálida limitada, nubes con dithering, capas de siluetas oscuras, luz cálida, faroles brillantes.
- TODO en pixel art: fondos, personajes, enemigos, objetos, efectos y UI. Nada ilustrado suave ni fotográfico.
- Una sola cuadrícula de píxel (1 px de arte = 4x4 de pantalla). Resolución base 320x180.
- Paleta de 24 colores POR NIVEL, compartida por fondo, sprites y UI del nivel.
- Sin degradados suaves, anti-aliasing, blur, sombreado realista, 3D ni texto dentro del arte.
- Contorno oscuro de 1 px en personajes, enemigos y plataformas. Dithering solo en cielo y niebla.
- Vista lateral 2D plana.
- Sprites con fondo transparente, frames alineados en cuadrícula, sin espacio sobrante.
- Personajes originales: nada con derechos de autor (nada de Tarzán ni similares).
- El protagonista mantiene el mismo diseño en los tres niveles y solo suma su poder nuevo. Sprites de 48x48 por frame.

## 3. PARALLAX REAL POR CAPAS (NO NEGOCIABLE)

PROHIBIDO usar una sola imagen recortada o dividida en franjas. Cada capa es una imagen INDEPENDIENTE con su propio contenido, 640 px de ancho (2x pantalla) y tile horizontal sin costuras (izquierda = derecha, excepto el cielo). PNG con transparencia. Una capa por mensaje, sin mezclar capas.

## 4. UI (PIXEL ART)

Fuente pixel (Press Start 2P / m5x7). Paneles 9-slice estilo adobe/terracota, sin fondos translúcidos ni esquinas redondeadas suaves. Cada nivel usa su paleta.

HUD: barra de vida, barra de RÁFAGA/escudo, MONEDAS, PUNTOS, nombre del nivel, pista de controles (ESPACIO saltar, S/↓ deslizar, E ráfaga, ESC pausa) y 4 casillas de piezas de patrimonio abajo con marco de adobe.

PANEL DE INICIO (320x180): logo "SUYURUN" grande en letras pixel con textura de piedra/oro; fondo que une COSTA, SIERRA y SELVA en tres bandas con parallax suave; protagonista corriendo al centro; botones JUGAR, NIVELES (Sierra y Selva con candado), MUSEO, AJUSTES, SALIR; cursor pixel.

SELECTOR DE NIVEL: tres tarjetas (Costa, Sierra, Selva), cada una con su paleta y las piezas coleccionadas (ej. 2/4).

## 5. PROTAGONISTA Y PODERES

- COSTA: personaje base, SIN brazos visibles. Animaciones (48x48): idle 4 frames (respiración y parpadeo), run 8 (brazos/piernas/ropa que se mueven, se nota vivo), jump 3 (impulso, aire, caída), land 2 (squash), slide 3, ráfaga 4, daño 2. Polvo de arena en pies al correr y aterrizar. Al final del nivel gana el poder de la Sierra.
- SIERRA: obtiene BRAZOS musculosos con breve brillo dorado. Con click apunta y lanza piedras con HUARACA. Frames extra: girar la honda (3) y lanzar (2). Piedra 8x8 con 2 frames de giro y efecto de impacto de 4 frames. Al final gana el traje de la Selva.
- SELVA: obtiene TRAJE PROTECTOR amazónico (placas de fibra y madera con patrones kené shipibo turquesa y rojo, hombreras de plumas), escudo proporcional a su vida. Frames: idle, run, jump, swing en liana (una mano agarrada), daño. Efecto de escudo que se rompe por partes (4 frames).

## 6. NIVEL COSTA

Ambientación: desierto costero del Pacífico al atardecer. Elementos únicos: huacas de adobe escalonadas (tipo Huaca Pucllana), muros de Chan Chan con frisos de olas y peces, acantilados desérticos con geoglifo del colibrí de Nazca, península de Paracas y el Candelabro a lo lejos, mar turquesa oscuro con garúa en el horizonte, pelícanos, guanayes, lobos marinos, caballitos de totora, algarrobos secos. Suelo: arena con borde de adobe y patrones textiles Paracas.

Paleta: naranjas y ocres de atardecer, arena cálida, rojo terracota, acentos turquesa del mar, morado en sombras.

Capas (de atrás hacia adelante, con velocidad de parallax):

1. cielo atardecer + sol bajo + nubes dithering (0.0)
2. nubes altas lentas y gaviotas/pelícanos lejanos (0.05)
3. siluetas lejanas Paracas/Candelabro (0.15)
4. mar con olas pixel art en 4 frames + garúa + caballito de totora lejano (0.30)
5. huacas escalonadas y muros de Chan Chan (0.50)
6. plano medio: algarrobos secos, rocas, lobos marinos (0.75)
7. suelo jugable: arena con borde de adobe y friso Paracas, tile 32x32 (1.0)
8. opcional, frontal: plantas/rocas oscuras en silueta (1.3)

Plataformas y obstáculos: bloque de roca/adobe escalonado, rampas, pilares, con luz, sombra, textura y contorno de 1 px.

Enemigos/obstáculos vivos (4 a 6 frames): hexágono con patas, pájaros, bote con pescador.

Coleccionables (16x16, brillo de 2 frames): huaco retrato, textil Paracas, cerámica Nazca, moneda de oro Moche.

Efectos: polvo, salpicadura de agua, brillo de monedas, destello de coleccionable, faroles/antorchas brillantes de 2 frames.

## 7. NIVEL SIERRA (para después)

Sierra al amanecer dorado: andenes tipo Moray, muros incas, nevados con nubes dithering, ichu amarillo, puente Q'eswachaka, llamas y vicuñas, lagunas azul intenso, kantuta. Paleta: azul cielo profundo, dorado de ichu, verde oliva, gris piedra, rojo textil, blanco nieve. Capas: cielo, nevados en silueta, cerros con andenes y laguna, muros/ichu/llamas, suelo de piedra inca tile 32x32. Enemigos (vida 1, derribables con piedras): cóndor en picada 64x48 (4 frames + 1 daño/caída), halcón 32x32 (4), ave de puna (2 de ataque). Coleccionables: cabezas clavas, keros ceremoniales, quipus.

## 8. NIVEL SELVA (para después)

Río Amazonas al atardecer verde-dorado con neblina; saltos entre plataformas sobre el río. Ceibas con lianas, palmeras de aguaje, victoria regia, guacamayos, delfín rosado, monos, luciérnagas. Paleta: verdes profundos, verde lima, marrón río, naranja, rojo guacamayo y turquesa. Capas: cielo con neblina, selva lejana, árboles con lianas (punto de agarre claro), río con reflejos, primer plano de hojas. Plataformas: tronco 64x16, piedra 48x16, victoria regia 48x12 (2 frames). Liana 16x96 con nudo brillante, 2 frames; click en el nudo para balancearse a la siguiente. Enemigos: CAIMANES 64x32, saltador 6 frames (emerge, salta, muerde, cae), estático 4 de mordida + 2 idle, salpicadura de 4 frames. Coleccionables: cerámica Shipibo, instrumentos de viento.

## 9. FINAL (para después)

Plaza festiva al atardecer 320x180 que une las tres regiones (costa izquierda, andenes centro, selva derecha). Tres personajes bailando, 6 frames en loop, 48x48: protagonista afroperuano de la costa (camisa blanca, pañuelo, festejo/zapateo), campesino andino (chullo, poncho rojo, ojotas, huayno), guardián amazónico original (pintura kené, taparrabo de fibras, corona de plumas, saltos). Pétalos, notas musicales y vitrina del museo con los objetos coleccionados.

## 10. MUSEO Y PROGRESO

Cada pieza recogida se registra al instante en un códice y se guarda de forma permanente, aunque se pierda el nivel. Pantalla MUSEO pixel art: una vitrina por pieza con ficha breve (nombre, cultura de origen, función histórica); las no recogidas salen en silueta. Las monedas se gastan en mejoras (más vida, más ráfaga, imán de monedas) en una tienda pixel art.

## 11. FLUJO DE TRABAJO Y NOMBRES

1. Un asset o capa por mensaje y espera mi confirmación.
2. Orden: Costa (capas 1 a 7, suelo, plataformas, protagonista, obstáculos, coleccionables, efectos, HUD, panel de inicio), luego Sierra, Selva, Final.
3. Cuando apruebe un asset, guarda su paleta y estilo como referencia fija para el resto del nivel.
4. Archivos: nivel_capa_descripcion.png (ej. costa_capa2_nubes.png, sierra_enemigo_condor.png).

## 12. LO QUE DEBE ACABARSE HOY: NIVEL COSTA COMPLETO

Sierra y Selva quedan para después. Hoy se entrega, con acabado alto y todo en pixel art coherente:

a) Las 7 capas de parallax (más la frontal opcional), cada una en su propio PNG de 640 px con tile sin costuras; el mar con 4 frames animados.

b) Plataformas, rampas, pilares y suelo con luz, sombra, textura y contorno.

c) Protagonista 48x48 con todas sus animaciones (idle, run, jump, land, slide, ráfaga, daño) y polvo de arena.

d) Enemigos y obstáculos vivos con 4 a 6 frames.

e) Coleccionables, monedas y efectos.

f) Pantalla de resultados pixel art (puntos, monedas, piezas obtenidas, REINTENTAR / SIGUIENTE) y animación de fin de nivel: brillo dorado, aparecen los BRAZOS, pose de victoria de 4 frames.

g) HUD, panel de inicio SUYURUN, selector de nivel, pantalla MUSEO y tienda, todo en pixel art.

Empieza YA con COSTA, capa 1 (cielo). Confirma que guardaste esta biblia.

## 13. PARALLAX Y RENDER EN UNITY

Una capa = un PNG (nunca recortes una imagen en franjas). Script ParallaxLayer con factor por capa (0, 0.05, 0.15, 0.30, 0.50, 0.75, 1.0, 1.3), dos copias que se reciclan para repetición infinita. Sprites en Filter Mode Point, Compression None, mismo PPU, Pixel Perfect Camera 320x180. Animator con estados idle, run, jump, land, slide, burst, hit, sin interpolación. Reemplaza paneles translúcidos por sprites 9-slice y fuente pixel. Si no hay PNG de Astra en /Assets, genéralos con Python (Pillow) píxel por píxel con la paleta fija de 24 colores.

## 14. RITMO TIPO GEOMETRY DASH (mapa fijo, NO análisis en vivo)

1. Extrae el .mid con Python + mido (NO uses costa_music_map.json para el timing). Toma note-on de batería/bajo y conviértelos a segundos con el mapa de tempo del MIDI (set_tempo, ticks_per_beat, cambios de tempo). Exporta beatTimes[] y notas (tiempo, pista, pitch) a JSON.
2. Cárgalo en RunnerSoundtrack.beatTimes y usa BeatToSeconds / SecondsToBeat.
3. Diseña el nivel contra esa grilla fija, como CostaAlcatraz.asset: saltos, aterrizajes, huecos, obstáculos y monedas puestos a mano en números de beat. X = velocidad * tiempo del beat. El salto despega en un beat y aterriza en el siguiente.
4. Reloj único AudioSettings.dspTime (nunca Time.time). Calibración de latencia en Ajustes y modo tap-tempo.
5. Complejidad por secciones (intro fácil, desarrollo, clímax denso). Graves = obstáculos de suelo, agudas = aéreos/monedas, acordes = combinación. Rachas de aciertos en beat dan bonus de PUNTOS.
6. Marca las notas dudosas (adornos fuera de tiempo, mala cuantización) y no las fijes sin mi validación de oído.
7. costa_music_map.json solo para efectos ambientales (marea, pulso de luz).

## 15. SISTEMAS DE COSTA

Museo con guardado permanente (PlayerPrefs o JSON), tienda de mejoras, fin de nivel que otorga el poder de la Sierra (desbloquea Sierra, que empieza con candado), escena MainMenu "SUYURUN" con JUGAR, NIVELES, MUSEO, AJUSTES, SALIR.

## ENTREGA

Checklist de qué quedó y qué falta, y lista de notas MIDI dudosas para validar de oído.
