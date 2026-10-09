# Contrato de importación del banco

Este paquete incluye assets de staging; no constituye el refactor completo.
PNG horizontal, dimensiones y conteo en JSON; sin recorte automático por frame.

Core Costa/Sierra usa celdas 192x128, con pie lógico en (96,124). El JSON incluye
renderOffsetFrom96Cell=(-48,-31): conservar origen corporal al usar celdas más
anchas. Este offset debe aplicarse en el consumidor al activar el nuevo banco;
el renderer actual NO lo aplica automáticamente. No conectar sin ese ajuste.
Point, None, Clamp, sin mipmaps. PPU64 para personajes nuevos. Pivote (0,1),
conservando la convención del renderer actual. No cambiar colisiones.

Bosses: MotionV2 contiene ataques separados, ocho poses por acción.
durationsMs es una propuesta de actuación: no sustituir el reloj lógico de
preparación/ataque/recuperación del boss. El frame de impacto debe ajustarse
al evento de daño real; jamás derivar el hitbox de la silueta del sprite.

Unity: SuyuRun > Refactor > Importar banco a staging. Copia PNG/JSON a
Assets/SuyuRun/Art/RefactorStaging. NO lo conecta al juego. Importa textura
Single; el renderer crea los subframes mediante Sprite.Create, como TaytaArt.
Catálogo unity_catalog.json con dimensiones, duración, bounds y hashes.

Antes de activar: comprobar tamaño corporal efectivo entre estados, origen de
pies, arma/mano, anclajes de lianas, orientación, eventos de impacto y caída.
640x360 sigue siendo una propuesta de render, no un cambio ya aplicado.
Originales, escenas y sprites actuales permanecen intactos. Sin nuevo ejecutable.
