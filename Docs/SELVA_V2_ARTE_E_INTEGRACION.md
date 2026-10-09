# Selva V2 — dirección del 2 de octubre de 2026

La petición actual autoriza pulir el paquete e integrarlo en Unity. Sustituye el límite anterior de 24 colores por 24–32 y permite transparencia **solo** en la superficie del río y rayos discretos. La revisión usa 28 colores; su aceptación visual final corresponde al usuario. No se declara aprobada por el mero hecho de generarla.

## Arte entregado

- 171 PNG, 597 celdas nativas. Cada PNG tiene JSON de dimensiones, rectángulos de frame, FPS y loop. Caimanes incluyen cajas de cuerpo y mandíbula por frame.
- Seis ambientes: bosque, río, cueva, ruinas, noche, templo. Cielo, lejanía, estructura, cercanía y frontal se dibujan independientemente a 640×180; niebla y rayos son capas adicionales.
- Copas orgánicas en clusters limpios, troncos con corteza, raíces y musgo. Sin blur, antialias o rotaciones de sprites a resolución de pantalla.
- Río: seis frames para fondo y seis para superficie transparente. Burbujas, ondas, espuma, splash de salida y splash de entrada separados.
- Caimán: acecho 4, anticipación 3, emergencia 4, snap 3, caída 4, inmersión 6, nado 4. 64×64 por celda. El frame 1 de snap concentra la mordida; la inmersión oscurece progresivamente el cuerpo.
- Héroe: diseño y traje protector conservados, 48×48; correr, salto, doble salto, aterrizar, deslizar, daño, disparo, recarga, agarrar, balancear, soltar y victoria. Balanceo usa banco de poses de 16 ángulos: NO reproducir como loop indiferente al péndulo.
- Liana: hoja de 17 poses clave más curva rasterizada en píxel nativo en runtime. El punto de mano es (24,2) del frame del héroe; el largo físico es 82 px. Inercia y flexión comparten el punto de agarre, sin desalinear mano/cuerda.
- Props, obstáculos, plataformas, fauna, piezas del museo, efectos, dos guardianes originales y esbirros incluidos. V1, Costa y Sierra no se reemplazan.

## Integración

Escena `Assets/SuyuRun/Scenes/Level_Selva.unity`, preparada mediante `SuyuRun > Selva > Preparar escena` y abierta mediante `SuyuRun > Selva > Abrir nivel`.

`SelvaStageDirector` ejecuta el recorrido y comparte las ventanas de pausa, resultados, museo, ajustes y tienda existentes. Sierra ofrece SIGUIENTE hacia Selva; el selector respeta el desbloqueo existente. La ventana Selva del inicio usa capas reales, con posición fija y fauna animada.

Importador: Point, sin compresión/mipmaps, PPU 32 y rectángulos exactos del JSON como sprites múltiples. `SelvaV2Animation/Hero.controller` contiene los estados y clips del héroe. El Animator se evalúa a tiempo DSP con velocidad automática cero; selección de keys exacta y sin interpolación. El ZIP es un overlay para este proyecto SUYURUN, no un proyecto independiente ni un ejecutable.

El río tiene emboscadas físicas, aviso inofensivo, mordida con cajas por frame, arco gravitatorio, inmersión sin desaparición instantánea y reubicación bajo agua. Las gotas de cascada y cruces de superficie producen ondas. La superficie se dibuja delante del cuerpo sumergido. Las lianas usan péndulo, impulso, liberación tangencial y doble salto.

Cueva y ruinas reutilizan **sin modificar** el modelo de combate/plataformas de Sierra con arte y guardianes originales de Selva. A/D mover, W/ESP doble salto, S bajar de plataformas, click/E disparar, R recargar; cargador de 5 y recarga de 4 segundos. La salida exige derrotar guardián y esbirros y permanecer tres segundos en el portal.

Controles runner: ESP salto/doble salto; S/↓ deslizar; click en nudo cercano para agarrarse; en balanceo ESP suelta, click en siguiente nudo prepara el próximo agarre; E dispara; ESC pausa. El agua profunda hace perder vida y devuelve a la orilla del módulo, no bloquea el nivel.

## Audio y MIDI — límites explícitos

El MIDI real `Assets/selva.midi` se convierte con mido y mapa de tempo. `Data/Resources/SelvaIntegration/selva_midi_exact.json` conserva notas y tiempos; `selva_course.json` conserva coordenadas por beat. La instancia del soundtrack carga esa grilla exacta sin reemplazar el asset original.

La ruta terrestre corre a 160 px/s. Saltos NO se bloquean a la espera de un beat; hay buffer de 180 ms y coyote de 100 ms. Las zonas de lianas y exploración admiten tiempo variable: no se presentan como timing musical rígido. Al cambiar de zona se retoma el fragmento asignado con crossfade de 1,25 s. Los mininiveles usan por ahora la canción Selva existente; no se inventan dos canciones nuevas ni un MIDI para ellos.

`Artifacts/SelvaV2/midi_review.json` contiene notas de cuantización dudosa que requieren revisión de oído. No son usadas como obstáculos individuales. El MIDI suministrado tiene tempo declarado 120 BPM; no se presupone que todas las notas transcritas sean musicalmente correctas.

## Museo

Cerámica y textil con kené son interpretaciones, no réplicas arqueológicas. Flauta y sonaja son objetos originales del juego: se evita atribuir su diseño a un pueblo sin evidencia. Referencia cultural oficial: https://bdpi.cultura.gob.pe/pueblos/shipibo-konibo y https://www.gob.pe/institucion/cultura/informes-publicaciones/1379-kene-arte-ciencia-y-tradicion-en-diseno.

El guardado de piezas es inmediato e independiente de ganar. Las pruebas diagnósticas NO escriben progreso, banco ni museo.

## Verificación

Validación de arte: `Artifacts/SelvaV2/art-validation.json` (171 PNG, 597 celdas, 28 colores; sin fallos). Prueba de importación, física y escenas en Play: `Artifacts/SelvaV2/unity-verification.txt`. Incluye ciclo caimán completo, aviso inofensivo, caja de mordida, longitud/flexión de liana, doble salto inmediato, liberación, seis zonas y ambos mininiveles completables con cinco vidas y munición limitada. Las capturas `unity-*.png` son del runtime; `selva_emboscada_balanceo.gif` es estudio de animación generado, NO prueba de gameplay.

No se genera EXE. Una prueba automática de integración no sustituye una pasada humana de dificultad, oído y aprobación visual.
