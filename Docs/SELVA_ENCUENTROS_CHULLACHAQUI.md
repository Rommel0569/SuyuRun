# Selva — encuentros y recorrido, 02/10/2026

## Dirección confirmada

Bosque → río con lianas → cueva/Guardián de Raíces → selva nocturna → mininivel/Chullachaqui → templo final.
El usuario autorizó generar e integrar el lote completo. Esto NO equivale a aprobación visual de los nuevos sprites.
Costa y Sierra no se modificaron. El escarabajo ya no aparece en el recorrido; se conservan sus archivos antiguos.

## Implementado

- Canción Selva completa: bosque 0–45 s, río 45–90 s, nocturna 90–135 s, templo 135–180.5322 s. Los mininiveles no consumen fragmentos de esa canción.
- 88 eventos fijos sobre la grilla MIDI: 16 saltos/pinchos, 20 techos para deslizarse, 10 raíces destructibles, 11 orbes de doble salto, 11 estacas, 10 trampas y 10 enemigos aéreos. Además, plataformas elevadas, lianas, bancos/victorias regias y caimanes físicos en el río.
- El río reserva obstáculos para las orillas y detiene la música en el límite de su sección si el balanceo manual tarda más, sin saltarse la siguiente sección musical.
- Transición de salida: cámara fija y cierre de 0.8 s; entrada: apertura 0.9 s y cruce de volumen de 1.25 s. El DSP sigue siendo el reloj del juego.
- Ambas canciones propias duran 60.05551 s (confirmado por AudioClip.length en Unity). Música de cueva: CuevaGuardian.mp3; combate: Chullachaqui.mp3.
- Actualización solicitada al probar: al derrotar al jefe y a todos los esbirros, acercarte al portal inicia una salida de 3 s sin esperar al final de la canción. Las músicas siguen siendo de un minuto; hay 5 s de gracia si quedan enemigos al terminar ese tiempo.
- Guardián de Raíces: brotes desde el suelo y trampas de inmovilización de 1.1 s; aviso de 0.85 s antes del área dañina. Los saltos siguen funcionando durante el amarre; no hay bloqueo total de los controles.
- Chullachaqui: presentación de 6 s, sello automático de armas, puñetazo, patada y uppercut; patada aérea; guardia corta con recuperación. Invoca ecos reconocibles de los dos jefes de Sierra y maldiciones aéreas.
- Los esbirros aparecen antes del jefe, persiguen entre plataformas y reaparecen en distintas alturas; presupuesto inicial de 15 apariciones y refuerzos mágicos limitados a tres vivos en arena.
- Pistola de exploración: 5 balas, recarga 4 s. Bajo maldición no dispara ni recarga. El reintento en mininivel conserva el recorrido anterior y reinicia ese encuentro.
- 30 spritesheets nuevos; colección total V2: 201 PNG / 777 frames. Paleta existente de 28 colores, píxel nativo, transparencia binaria excepto agua/rayos ya existentes, originales preservados.

## Controles de combate

A/D movimiento, W/ESPACIO doble salto, S bajar de plataformas. Antes de la maldición: click/E pistola, R recarga. Después: click/E puñetazo (patada si estás en el aire), F patada, Q uppercut, SHIFT guardia corta. ESC pausa.

## Verificación y límites

- Compilación de código de juego con el compilador de Unity: correcta.
- Simulación de física/daño real, cinco vidas y sin inmunidad: ambos encuentros completables. Las pruebas actuales comprueban salida en 3 s incluso con victoria temprana, maldición, daño a puños y movimiento mientras disparas/recargas.
- Validación de los 201 PNG: 28 colores usados, sin errores de paleta, alfa ni costuras declaradas.
- Auditoría dentro del editor: confirmó la importación y duración de ambos AudioClip. El menú `SUYURUN/Selva/Auditar encuentros (sin cambiar escena)` permite repetir la prueba sin cerrar Unity ni cambiar tu escena.
- Prueba visual preparada en `SUYURUN/Selva/Verificar nuevos encuentros en Play`: requiere escenas guardadas, restaura la escena anterior y no cierra Unity. Esta última prueba NO se ejecutó en esta entrega; no se deja una solicitud automática pendiente.
- No se generó un ejecutable del juego. El ensamblado model-test.dll es una herramienta de diagnóstico, no un build de SUYURUN.
- Pendiente: prueba humana del recorrido completo y de la dificultad, revisión visual en Unity Play de los nuevos ataques, de las transiciones y de los hitboxes. El autotest no demuestra acabado artístico perfecto ni sincronización perceptiva de oído.
- Las notas MIDI dudosas siguen en Artifacts/SelvaV2/midi_review.json; no se ha declarado validación de oído.

## Referencia cultural

El Chullachaqui es un guardián del bosque en relatos amazónicos. La maldición y los ecos son ficción original del juego, no una atribución histórica universal.
Fuente primaria: [Biblioteca Nacional del Perú, Narraciones de la Amazonía](https://repositoriodigital.bnp.gob.pe/bnp/recursos/2/html/narraciones-de-la-amazonia/).

## Archivos

Runtime: SelvaStageDirector.cs, SelvaStagePresentation.cs, SelvaMiniWorld.cs y SelvaMiniMagic.cs.
Assets: Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2.
Audio: Assets/SuyuRun/Audio/Resources/SelvaAudio.
Generación reproducible: Tools/build_selva_encounters.py; conserva assets previos.
Diagnóstico: Tools/SelvaModelAudit.cs y Editor/SelvaEncounterAudit.cs.
