# Sierra — dos cuevas de exploración y respuesta de controles

## Encargo vigente

El usuario autorizó el lote artístico completo de las dos cuevas el 25/09/2026,
excepción expresa a la aprobación individual por asset para este encargo. Mantener
la paleta de24colores, vista lateral y píxel nativo. No implica aprobación visual final.
Costa sigue cerrado. Los fondos exteriores de Sierra no se regeneran.

## Flujo

Camino → cámara detenida / entrada al túnel → Galería del Monolito → salida al Valle
→ puentes → entrada al túnel → Cripta del Guardián → salida a Ruinas → portal de Selva.

Las cuevas son estados explorables dentro de la escena Sierra, no escenas separadas
ni ejecutables nuevos. El mundo de exploración mide1536x384píxeles, con cámara móvil
horizontal y vertical, plataformas superiores, ruta inferior y arena de jefe.
La cripta tiene otro trazado: cámara hundida central, subida sólida hacia la tumba
y balcones altos adicionales; no es únicamente un cambio de color de la galería.

En cada frontera se conserva el segundo exacto del runner. Su música y simulación
se pausan; la canción de la cueva se programa con DSP. Al salir se reanuda la canción
de Sierra en ese mismo segundo, sin recalcular ni estirar el MIDI.

Cada MP3 seleccionado dura60,0555s según AudioClip de Unity. No se repite en bucle.
Derrotar al jefe y alcanzar la salida antes del final; si se llega temprano, se espera
el cierre musical. Al agotarse el tiempo sin completar ambos objetivos, se reintenta
la cueva, no todo Sierra. Pausar detiene música, contador y simulación.

## Controles de las cuevas

- A/D o flechas laterales: caminar libremente.
- W, ESPACIO o flecha arriba: salto y segundo salto.
- S o flecha abajo: bajar de una plataforma atravesable. No atravesar roca sólida.
- Mantener clic izquierdo: pistola apuntada al cursor; E también dispara.
- ESC: pausa. R: recargar (reintentar cuando se pierde). Retroceso: reintento de cueva.
- Sin huaraca ni movimiento automático durante exploración.

Pistola con cargador de5balas, recarga automática o manual de4s, cadencia0,15s.
Un impacto por esbirro; hasta15apariciones, máximo5vivos simultáneos, en pisos
superiores e inferiores. Persiguen, saltan y bajan hacia el jugador. Cada cueva
comienza con al menos5vidas para dejar margen durante la recarga.
Retícula y sonidos de disparo, salto, daño y derrota conectados. Antorchas y oro
siguen animados mientras se espera el final musical en la salida.
Los tesoros opcionales dan5monedas y recuperan1vida. Los enemigos otorgan monedas;
se transfieren al runner solo al completar la cueva para no duplicar con reintentos.

## Encuentros y arte

- Galería: custodios pétreos y guardián monolítico original; despertar, aviso,
  embestida comprometida, recuperación y escombros al derrotarlo.
- Cripta: centinelas óseos y guerrero fantástico de inspiración inca; tumba,
  oro animado, despertar, embestida y proyectiles de réplica.
- Jefes invulnerables mientras despiertan; después se pueden dañar con pistola.
- Dos fondos independientes y dos capas arquitectónicas con parallax; antorchas,
  suelo, roca, plataformas, pistola y balas.28PNG nuevos, sin reemplazar arte anterior.
  Esqueletos con6frames de ensamblaje de huesos, invertidos al caer derrotados.
- Fuente/grafismo compartidos con Sierra; cámara320x180, sprites Point/None/PPU32.
- Generador reproducible: `Tools/build_sierra_caves.py`. No ejecutar generadores
  generales de Sierra que puedan sobrescribir revisiones anteriores.

Ampliación de combate: ejecutar también `Tools/build_sierra_cave_combat.py` después
del generador base. Añade guardia, directo, uppercut, golpe al suelo, maza, ataque
solar, embestida, impacto y proyectil solar. Conserva los sprites anteriores.
Las cinemáticas duran6s dentro del minuto musical, detienen el combate y enfocan
al jefe. La segunda presenta a «Pachacútec, el Inca caído», explícitamente ficticio.
Se distinguen el dato histórico y la fantasía; la cripta no representa su tumba real.
Consulta `SIERRA_CUEVAS_COMBATE_26_SEPTIEMBRE.md` para la revisión y pruebas nuevas.

Los guardianes y la cripta son ficción: NO son el Lanzón real ni una tumba real de
Pachacútec. Chavín y la tradición inca no se presentan como una sola cultura.
Referencias consultadas para la dirección, sin copiar imágenes:

- Ministerio de Cultura, piedra y galerías de Chavín:
  https://www.gob.pe/institucion/cultura/noticias/126679-la-milenaria-cultura-chavin-lista-para-ser-vista-de-manera-virtual-desde-tu-casa
- Ministerio de Cultura, historia de Machupicchu:
  https://www.machupicchu.gob.pe/historia/

## Correcciones exteriores

- ESPACIO capturado antes de los pasos DSP, conservado0,18s y consumido una vez.
- Huaraca: liberación0,18s, recuperación0,38s, proyectil guiado más rápido y
  área apuntable ampliada. Mantiene carga/giro/lanzamiento; no cambia velocidad224px/s.
- Llama: reciclaje después de salir completamente del borde, no cuando su origen llega a0.
- Eliminado el oclusor negro instantáneo. Cámara fija al ingresar y cortina pixelada
  progresiva, sin transparencia suave ni cambios al fondo aprobado.
- Escaleras de Ruinas: las entradas señalizadas con SALTA exigen despegar; tocar
  su frente causa choque y rebote. Los peldaños pequeños de continuación no
  penalizan de nuevo. Se reservan los corredores MIDI de obstáculos y movilidad
  para no combinar instrucciones incompatibles. El mapa musical permanece intacto.

## Verificación

- `Artifacts/SierraV3/input-verification.txt`: búfer y tiempos de huaraca.
- `Artifacts/SierraCaves/model-verification.txt`: controles a30/60/144FPS,
  proyectiles, recompensas, activación de jefe, arena, salida y duración de audio.
  Incluye pasos DSP de duración variable: se corrigió un borde que desplazaba al
  personaje al extremo opuesto del suelo al resolver una falsa colisión lateral.
- `Tools/test_sierra_caves.py`:3pruebas de paleta, dimensiones/frames,4capas
  independientes con bordes repetibles, copias exactas y distintas de ambos MP3.
- Costa:51archivos protegidos comparados porSHA256, ninguno modificado.
- `Tools/test_sierra_mobility.py`:7regresiones del recorrido original.
- `Artifacts/SierraV3/unity-play-result.txt`: revisar FECHA y resultado; una prueba
  antigua sin cuevas no valida esta ampliación.
- `Artifacts/SierraCaves/play-*.png`: capturas reales de Play cuando estén generadas.
- `Artifacts/SierraCaves/cueva*_arte_x4.png`: composición artística, NO captura Unity.

Prueba integrada26/09 a07:18:50: ambas cuevas completas,0errores, retorno al runner.
Falta aceptación manual de dificultad, composición y controles. No declarar100%aprobado.
