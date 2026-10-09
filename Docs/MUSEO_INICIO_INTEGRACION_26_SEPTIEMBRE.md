# Inicio Costa–Sierra y museo — 26/09/2026

## Alcance autorizado

Lote completo de inicio y piezas de museo autorizado por el usuario. Las imágenes
van en la vitrina de su propia ficha. Se conservan los PNG anteriores y las piezas
del recorrido. Selva permanece pendiente; no se crea un ejecutable.

## Git: integración selectiva, sin publicar

Se consultó origin/main hasta `8103d4b`. El cambio de código posterior a
`165ac80` está en `RunnerCoastalCombat.cs`: identificadores y registro de cambios
Seek/Wander de las aves (`3ad00b3`). Integrado conservando el trabajo local.
La demo compilada de `8103d4b` no se incorpora al árbol local ni se ejecuta.

No se ha hecho merge de historiales, commit ni push: `main` local contiene
`21003e7` (documentación explícitamente solo local/no pushear), además de muchos
archivos Sierra aún no versionados. Para publicar, preparar una rama desde el
remoto con los cambios de juego revisados, sin incluir esa documentación privada,
y volver a consultar el remoto antes del push. No usar force-push.

## Implementación

### Cambio posterior del usuario: INICIO_FINAL e interfaz única

La propuesta inicial de dos panoramas queda sustituida: `INICIO_FINAL.jpeg` es
la composición exacta. `prepare_inicio_final.py` conserva todos los píxeles fuera
de las máscaras de los paisajes. El marco incluye logo, marcos, rótulos, botones
y tapices originales. Los escenarios se dibujan debajo con capas reales. Selva
conserva su espacio vacío hasta disponer del nivel; no se muestra la selva ajena
del JPEG como si perteneciera al juego. Original JPEG intacto.

`RunnerSharedGameMenu.cs` es el renderer único de pausa, muerte, resultados,
museo, ajustes y tienda. `SharedMenuBindings.cs` conecta acciones y datos de los
dos motores de juego sin duplicar ventanas. Incluye pausa/muerte de cuevas.
El museo permite cambiar Costa/Sierra sin cargar ni iniciar un nivel. Las piezas
van dentro de la misma vitrina con ficha y navegación en ambos contextos.
Los métodos antiguos permanecen como referencia, pero la ruta activa es compartida.

La interfaz del JPEG se conserva en su resolución original por orden explícita
del usuario; es una excepción documentada al contrato de paleta/resolución del
arte jugable, que no se ha modificado.

- Inicio: panoramas Costa/Sierra reutilizando capas independientes ya existentes,
  parallax pixelado y animación de frames; se conserva logo y botones de Costa.
- Selector: acceso directo a Sierra cuando está desbloqueada; progreso persistente
  reconoce tanto la clave de desbloqueo como CostaComplete. Selva no simula acceso
  a un nivel que todavía no existe.
- Museo: siete PNG nuevos 64×64, paleta de su región, alfa binario, Point y sin
  compresión. Costa: imagen en vitrina y miniaturas. Sierra: imagen en cada ficha.
  No recogidas en silueta. No cambia los coleccionables del recorrido.
- Audio: copia de `Artifacts/Audio/MUSEO.mp3` en Resources/Museum, reproducción en
  ambos museos, ajuste global de volumen y entrada/salida de 1.2s. Reloj MIDI intacto.
- Conexión existente: final de Costa guarda desbloqueo y botón de continuación
  reproduce transición hacia Level_Sierra_Integration. No se salta resultados.

## Catálogo y referencias consultadas

Son reinterpretaciones pixel art documentadas, no facsímiles ni fotografías
arqueológicas. Los motivos textiles y del kero son simplificados, no una copia
exacta de una pieza inventariada. Los retratos son interpretaciones originales.

| ID de ficha/PNG | Memoria | Referencia |
|---|---|---|
| huaco_moche | Huaco retrato mochica | https://www.museolarco.org/exposicion/exposicion-permanente/obras-maestras/vasos-retrato-mochica/?origin=177 |
| textil_paracas | Manto Paracas | https://www.museolarco.org/exposicion/exposicion-permanente/obras-maestras/manto-paracas/ |
| nicomedes | Nicomedes Santa Cruz | https://memoriaperu.bnp.gob.pe/micrositio/nicomedes-santacruz |
| maria_reiche | María Reiche | https://www.miraflores.gob.pe/municipalidad-de-miraflores-rindio-homenaje-a-maria-reiche-a-25-anos-de-su-fallecimiento/ |
| cabeza_clava | Cabeza clava | https://www.gob.pe/institucion/cultura/noticias/126679-la-milenaria-cultura-chavin-lista-para-ser-vista-de-manera-virtual-desde-tu-casa |
| kero | Kero ceremonial | https://www.metmuseum.org/art/collection/search/316842 |
| quipu | Quipu | https://library.harvard.edu/about/news/2023-04-12/long-w-2-there-was-quipu-accounting-systems-incan-and-andean-peoples |

Referencia de Costa: Museo Larco – Lima, Perú. El atlas original generado se
conserva en `Artifacts/MuseumMenu/museum_source_atlas.png`; script reproducible
`Tools/prepare_museum_art.py`, validación `Artifacts/MuseumMenu/art_validation.json`.
La aprobación del lote autoriza integración, no implica aprobación visual final.

## Verificación

Capturas y prueba Play en `Artifacts/MuseumMenu/`. Consultar el resultado real
`play-verification.txt`; no confundir una captura diagnóstica con una partida manual.

Verificado 26/09 a las18:13:44: INICIO_FINAL, selector, siete texturas64px Point,
audio de museo reproduciéndose con fade, museo/pausa/ajustes/tienda compartidos,
0errores. Capturas `play-1.png`, `pause-costa.png`, `pause-sierra.png`,
`settings-costa.png`, `settings-sierra.png`, `shop-sierra.png`.
Prueba independiente de portal a las18:14:20: final real de Costa→cortina→carga
de Sierra→llegada→carrera; rejugar reinicia Camino, 0errores.
Esto verifica integración y render, no sustituye la aprobación visual del usuario.
