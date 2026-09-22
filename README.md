# SuyuRun

Runner rítmico 2D para Unity 6.3 LTS (`6000.3.17f1`). Estado actual: Costa con recorrido vertical, fondo ilustrado y El Alcatraz aportada por el usuario.

## Ejecutar en Unity

1. Abrir este directorio (`SuyuRun`, el que contiene `Assets`, `Packages` y `ProjectSettings`) desde Unity Hub.
2. Esperar a que termine la importación y compilación.
3. Menú `SuyuRun > Abrir Costa` (o `Abrir prototipo original` para la versión sencilla de cerros).
4. Pulsar Play y después el botón Jugar o Enter.

Las escenas actuales son `Assets/SuyuRun/Scenes/Level_Costa.unity` y `Prototype_Legacy.unity`. Los menús anteriores configuran la escena correspondiente para Play. El prototipo original conserva su paisaje sencillo y recorrido plano de 64 segundos, usando los controles compartidos. Las escenas anteriores y de recuperación se conservan sin modificar.

## Controles

| Acción | Teclado / ratón | Mando |
| --- | --- | --- |
| Empezar | Enter o botón Jugar | Botón Jugar con ratón |
| Saltar | Espacio / clic izquierdo | Botón sur |
| Segundo impulso | Recoger un rombo en el aire y volver a pulsar salto | Volver a pulsar botón sur |
| Deslizar | Mantener S / flecha abajo | Mantener botón este |
| Ráfaga | E / clic derecho | Gatillo derecho |
| Pausa | Escape | No asignado todavía |
| Reiniciar tras morir | R / botón Jugar | Botón Jugar con ratón |

Los peligros naranjas se saltan o se esquivan por debajo según su altura. Los objetivos rosas también pueden destruirse con la ráfaga. La energía se recupera con tiempo, monedas y acciones a ritmo. Costa usa ahora la grabación completa de El Alcatraz proporcionada por el usuario: aproximadamente 2:57. El análisis estima 128,15 BPM; las marcas necesitan revisión auditiva, no se consideran sincronización final verificada.

La ruta contiene 21 plataformas elevadas y nueve huecos con impulso en tres secciones. Los rombos turquesa/violeta cargan un solo segundo salto: pulsar de nuevo mientras estás en el aire, no mantener el botón. La carga se consume al utilizarla o al aterrizar. Sigue los bordes claros para identificar las superficies reales; los relieves escalonados de las fachadas son decoración. Saltar contra una pared de la estructura o caer en un hueco causa muerte.

Hay picos individuales y dobles, también encima de algunas bases. La base violeta del suelo impulsa automáticamente al pisarla. Las fachadas tienen variantes de terraza, nichos y relieves escalonados. Los últimos cambios se encuentran en el proyecto Unity; no asumir que el EXE anterior los contiene. La aclaración actual autoriza pruebas en Play; no generar otro ejecutable externo durante este avance.

Datos actuales: `Assets/SuyuRun/Data/Resources/CostaAlcatraz.asset` (posiciones en unidades del mundo, elevación relativa al suelo), con `AlcatrazSoundtrack.asset` para pista y pulsos. Se conservan CostaVertical (64 s) y CostaPrototype (plano). Play utiliza CostaAlcatraz. Los checkpoints solo se aceptan en suelo seguro.

La reaparición desde un checkpoint cuesta cinco monedas del intento en este prototipo; es una versión provisional anterior al inventario de reintentos del juego final. Las monedas recogidas no reaparecen al revivir. Las monedas del intento se guardan al completar el recorrido.

## Compilar

Menú `SuyuRun > Build Windows prototipo`. Salida: `Builds/Prototype/SuyuRun.exe`. Distribuir toda la carpeta, no solo el EXE.

El código de Editor también puede compilar con `-batchmode -quit -executeMethod SuyuRun.Editor.PrototypeSetup.Build`. No abrir el mismo proyecto en dos procesos Unity simultáneamente.

## Arte y audio

El prototipo construye mallas 2D originales para Suyu, sus piernas, núcleo, emisor, monedas, peligros, arquitectura y figuras costeras. El fondo es una ilustración original generada mediante ImageGen en `Assets/SuyuRun/Art/Resources/CoastPanorama.png`; la dirección visual está en `Assets/SuyuRun/Art/ART_DIRECTION.md`. Las animaciones son procedurales; no utiliza sprites descargados ni modelos de Dota/Geometry Dash. Costa usa El Alcatraz aportada por el usuario y el inicio usa Inicio.mp3. Sierra y Selva están importadas y registradas, pero sus niveles y mapas rítmicos siguen pendientes. Los efectos y la música del prototipo original son sintetizados. Créditos y licencias de las grabaciones deben completarse antes de distribuirlas.

## Costa: presentación y colección

Parallax por capas con cielo, mar abierto de horizonte limpio, promontorios pequeños, orilla y botes. Oleaje sutil, reflejos móviles y balanceo de embarcaciones; muelles, cestas, vasijas, totora y conchas originales, distribuidos en viñetas espaciadas. El bailarín se repite por la orilla, las bandadas de cinco aves alternan aleteo y planeo y las nubes derivan lentamente. La decoración queda detrás de las superficies de juego; las aves oscuras de combate sí son obstáculos. Los almacenes del primer pase se retiraron de la composición para evitar saturación.

Las cajas destruidas dan tres monedas y liberan una memoria cultural; los gallinazos de combate dan dos monedas y algunos descubren una memoria. El museo inicial de Costa tiene cuatro fichas (huaco mochica, manto Paracas, Nicomedes Santa Cruz y María Reiche), con adelantos de lo no recogido y fuentes consultables. Se abre desde el inicio o al completar la ruta. Es la primera colección, no el museo-mapa de tres regiones terminado.

## Evidencias

El bailarín pequeño del fondo usa el mismo reloj que la música y se detiene al pausar. Es una animación estilizada en desarrollo. La procedencia del audio, sus limitaciones y el proceso de sincronización están en `Assets/SuyuRun/Audio/AUDIO_NOTES.md`; documentar créditos y permiso antes de publicar la grabación con el juego.

`Artifacts` guarda capturas y resultados de las pruebas. El comando de prueba automática aplica decisiones de salto/esquive/ráfaga al controlador real. Es una comprobación del recorrido; no sustituye las pruebas humanas de dificultad, sonido o sensación de control. Durante las pruebas automáticas no se guarda moneda ni récord en el perfil.

## Pendientes

Consultar `ART_APPROVALS.md`. El museo, investigación cultural, tres niveles finales, personajes alternativos y banda sonora final todavía no se consideran entregados.
