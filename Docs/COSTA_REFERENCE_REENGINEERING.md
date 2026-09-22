# Costa: integración del preview aprobado (19 septiembre 2026)

## Petición y alcance

El usuario rechazó la composición anterior y pidió que Level_Costa se vea como `Artifacts/ArtApproval/costa_capa7_suelo_preview_x4.png`, animada y jugable. No regenerar PNG aprobados. Conservar el trabajo de Claude y el recorrido MIDI, salvo correcciones justificadas de jugabilidad.

## Implementado

- `RunnerReferenceCoast.cs`: constructor activo llamado desde RunnerPrototype; deja el constructor viejo disponible, sin borrarlo. Ventana central 320x180, PPU32, cámara Pixel Perfect URP, suelo en fila148. Capas nativas, sin cambios de escala/alfa ni filas duplicadas. Dos copias por capa; cielo estático.
- Mar: cuatro frames a 4 FPS. Nubes y bandadas: componente existente adaptado a origen superior izquierdo. Bailarín existente: seis poses, fase musical y pies sobre techo de huaca (fila131), comparte desplazamiento .50 con la huaca. Se repite con la misma capa, no en plataformas arbitrarias.
- Suelo: un único friso superior; ya no se repite verticalmente. El cuerpo inferior de plataformas se rellena con arena. Conservadas colisiones, huecos, plataformas, orbes, pads, combate y museo.
- HUD de juego reducido, sin los paneles gigantes sobre el cielo. Tipografía pixel y UI definitiva siguen pendientes: no afirmar que toda la interfaz fue terminada.
- Pasos de física de hasta 1/120s contra el reloj DSP; evita avanzar horizontalmente todo el tiempo de audio mientras se limita solo la física vertical a .05s.
- `Editor/ReferenceCoastCheck.cs`: prueba en Play de Level_Costa, sin EXE ni salida del editor. Menú SuyuRun/QA/Probar Costa de referencia (sin EXE). Guarda imágenes y estado en Artifacts/ReferenceCoast. No modifica guardados de progreso (usa AutomatedSmokeTest).

## Evidencia y estado de pruebas

Primera revisión visual en Play: encuadre restaurado y bailarín visible sobre huaca. Las primeras pruebas automáticas murieron en 26–33 s, antes del ajuste de física por subpasos. No equivalen a completar el nivel. Consultar `Artifacts/ReferenceCoast/result.txt` y su fecha para el último ensayo; `state.txt` es el progreso más reciente. No confundir resultados antiguos con el código actual. Falta terminar validación de todo el recorrido y revisar capturas al inicio/20/60/120/175 s.

`Tools/audit_reference_coast.py`: ejecutado. Cobertura de tiles comprobada cada segundo hasta177s para los cuatro factores; ninguna ventana sin cobertura. Hashes de PNG de Resources coinciden con los originales aprobados. Informe en Artifacts/ReferenceCoast/audit.json. No es sustituto de probar Play.

## MIDI: resultado de inspección, sin inventar sincronización

Assets/El Alcatraz.midi: una pista, canal0, 12266 note-on, ningún canal de batería, ticks_per_beat43, tempo declarado500000 (120 BPM), última nota174.8837s. El filtrado de Claude sí usa onsets MIDI para los obstáculos. Pero `AlcatrazSoundtrack.asset` todavía contiene la grilla anterior de pulsos estimados MP3 (128.15 BPM); ApplyMidiRhythm convierte segundos a esa grilla y de vuelta. NO afirmar que beatTimes procede ya del MIDI ni que la sincronía está validada de oído. Costa dura177.36755s. No se modificaron ni el MIDI ni CostaAlcatraz.asset ni AlcatrazSoundtrack.asset en esta reingeniería.

El extractor anterior `analyze_costa_midi.py` solo considera pista0 y aplica tempo a ticks absolutos; para tempo variable/múltiples pistas debe sustituirse por acumulación delta con mapa global. El auditor nuevo sí recorre todas las pistas y acumula correctamente, pero no reemplaza todavía el timing jugable sin validación de oído.

## Relevo

Trabajar en SuyuRun ORIGINAL. La instantánea Handoff_Claude/Proyecto_20260918_194303 es histórica y no contiene los cambios de Claude ni esta reingeniería. Leer ART_BIBLE.md completo antes de cualquier asset y ART_APPROVALS.md (tiene algunas casillas históricas contradictorias; leer entradas fechadas/aprobaciones). No volver al fondo anterior ni sobreescribir este proyecto con la instantánea antigua.
