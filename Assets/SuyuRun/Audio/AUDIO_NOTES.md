# El Alcatraz — integración de Costa

## Procedencia

`El Alcatraz.mp3` fue proporcionado directamente por el usuario desde Downloads. No se descargó ni se extrajo del enlace de YouTube. Se conserva el archivo completo dentro del proyecto. Duración decodificada: 177,3976 segundos; 44.100 Hz, estéreo. Antes de distribuir públicamente la build deben documentarse intérprete, autoría y permiso/licencia de esta grabación; el título del archivo no demuestra esos datos.

Referencia de intención del usuario: https://youtu.be/XhrfyjTE26c?list=RDnzsF-6jXrB8. La herramienta web no pudo cargar ese video, así que no se afirma haberlo escuchado o visto completo.

## Sincronización implementada

- `RunnerSoundtrack` permite asignar un AudioClip, un punto de inicio, BPM, desfase del primer pulso y marcas de beat para cambios de tempo.
- Análisis automático reproducible: `Tools/analyze_rhythm.py`, usando muestras decodificadas por Unity y numpy. Resultado en `Artifacts/Audio/alcatraz-analysis.json`.
- Estimación: 128,15 BPM; primer marcador a 0,25542 s; 379 marcas. Son estimaciones de ataques sonoros: no equivalen a una transcripción musical validada ni aseguran que cada marca corresponda al pulso correcto. Revisión auditiva pendiente.
- El recorrido CostaAlcatraz utiliza la canción completa y tres secciones ordenadas, derivadas del diseño vertical. Los límites de terreno y eventos se llevan a la misma escala temporal de las marcas. La autoría del beatmap y el balance todavía requieren revisión; mapear un obstáculo a un beat no garantiza que el momento de pulsar salto caiga en ese beat.
- La reproducción se programa con el reloj DSP de Unity. Reanudar o reaparecer vuelve al tiempo correspondiente, sin repetir la canción en bucle. No se destruye el AudioClip importado al salir de la escena.
- El bailarín calcula su pose desde la posición musical del nivel, no desde un temporizador independiente: se congela con la pausa/muerte y se reposiciona al reiniciar.

## Bailarín y representación cultural

Bailarín afroperuano adulto, pequeño dentro del paisaje, con ropa clara y faja cálida. Ilustración articulada original construida con mallas; no se recorta ni reutiliza a las personas de la foto de referencia. La roca pertenece a una capa de fondo sin collider. La danza usa pasos alternados, flexión de rodillas, brazos y balanceo al pulso. Es una animación estilizada provisional, NO una reconstrucción coreográfica validada del Alcatraz ni de toda la escena de pareja mostrada en la fotografía.

Referencias culturales para continuar la revisión:

- SENAJU, festival de danza y música afroperuana en Barranco, con presentación de El Alcatraz: https://juventud.gob.pe/2024/04/dia-mundial-del-arte-decenas-de-jovenes-y-vecinos-de-barranco-asistieron-al-primer-festival-de-danza-y-musica-afroperuana/
- Pachamama Peruvian Arts, enseñanza de danza y movimiento de caderas en Alcatraz: https://www.pachamamaperuvianarts.org/danza/

## Configuración

Selecciones adicionales recibidas del usuario e importadas sin extraer audio de sitios externos:

- `Audio/sierra.mp3`: 177,55 segundos; asignada a Sierra en `SelectedMusic.asset`.
- `Audio/selva.mp3`: 180,53 segundos; asignada a Selva en el mismo registro.
- `Audio/Resources/Inicio.mp3`: copia de `inicio (2).mp3`, 295,29 segundos; reproducción en bucle suave al volumen 0,28 en título/museo, detenida al iniciar Costa.

No se han analizado ni sincronizado todavía los pulsos de Sierra y Selva. Los archivos aportados no demuestran por sí solos permiso de distribución; conservarlos para desarrollo y completar créditos/licencias antes de publicar. La aclaración posterior del usuario autoriza pruebas en Play, sin generar un ejecutable nuevo en este avance.

La escena carga primero `Data/Resources/CostaAlcatraz.asset`, después CostaVertical si aún no existe. La pista está en `Data/Resources/AlcatrazSoundtrack.asset`. Estos datos se pueden editar desde Inspector; no volver a ejecutar la integración inicial para sobrescribir el diseño ya editado.

No se ha ejecutado el EXE para esta integración, siguiendo la indicación del usuario. Pendientes: escuchar y ajustar pulsos/acento, revisar la danza y el recorrido completo en Play, calibrar latencia y documentar créditos.
