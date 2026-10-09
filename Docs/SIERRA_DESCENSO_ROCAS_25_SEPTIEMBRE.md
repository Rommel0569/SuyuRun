# Sierra — descenso letal, agachado y rocas

Solicitud: halcón aprobado; ave veloz que obliga a descender, obstáculo posterior para agacharse,
agachado como Costa, rocas que rueden y exijan saltar con aves apuntables durante el salto.

## Implementado

- Siete aves de cruce con aviso previo0.65s, velocidad mundial800px/s hacia la izquierda.
  Su contacto mata aunque exista gracia de daño. Son peligros de evasión, no blancos de ráfaga.
- Las siete secuencias terminan en una viga situada1.8s después del impulso inicial.
  Mantener S/↓ al aterrizar permite pasar por debajo; el escenario no genera obstáculos azarosos.
- Agachado: proporción vertical0.48/1.06, ancho1.12 y pies anclados, copiando la lógica visual de Costa.
  La pose se anima en ciclo mientras se mantiene la tecla y solo se muestra en suelo.
- Tres rocas de32px ruedan hacia el jugador a55px/s durante el encuentro.
  Se conserva su instante de cruce del mapa. No son destruibles: obligan a saltar.
- Cada roca incorpora un ave que acompaña la aproximación y puede fijarse con clic para la huaraca.
  No se limita el clic al suelo: se puede apuntar durante el salto/doble salto.
- Roca revisada en8fotogramas: grietas giratorias y luz superior fija, avance de animación
  proporcional al desplazamiento/circunferencia. Es el único bitmap modificado en esta entrega.
  Backup: `Artifacts/SierraV3/BeforeRollingRock`. Aprobación visual pendiente.

## Evidencia

- `Tools/test_sierra_mobility.py`:7pruebas PASS, incluida trayectoria con/sin descenso en las7secuencias
  y comprobación de los márgenes transparentes usados para anclar los pies al agacharse.
- Unity Play completo a las16:14:31 del25/09/2026:177.55s, errores0,
  Pads7 / Orbs7 / Dives7 / Escapes1 / PaidRevives1 / GuideTrue.
- El pase completo es autoplay diagnóstico, no aprobación manual de dificultad ni de oído.
- Tras ese pase se ajustó solo la cadencia visual del agachado/giro y se añadió una prueba corta
  `test-controls`, con captura `Artifacts/SierraV3/unity-slide.png` y reporte `controls-verification.txt`.
- Prueba corta final25/09/2026 20:43:56: Running, SlideCaptured=True, Pads1/Orbs1/Dives1,
  errores0. Captura inspeccionada: silueta agachada y pies visibles apoyados, sin elevar el sprite por su margen transparente.
- Costa y el fondo de Sierra no se modificaron. Sin ejecutable nuevo.

El cierre completo de Sierra sigue en `SIERRA_CHECKLIST_ESPECIFICACIONES.md`.
