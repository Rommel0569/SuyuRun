# Lote de materiales jugables — 4 de octubre

El usuario ratifica entregar el lote completo sin pausas. Se conservan todos los originales.

## Integración en código

42 hojas con acabado nativo de materiales: Sierra (escaleras, rampas, pilares, plataformas/rocas de cueva, piedra, pinchos, tronco y cuatro suelos); Selva (piedra, rampa, puente, bote, victoria regia, cinco obstáculos y dieciséis tiles); Costa (bloque, rampa, pilar y suelo).

Materiales con juntas, biseles, vetas, nudos o nervaduras según el tipo. Se conserva el alfa original por frame y se añaden marcas nativas a resolución doble. No se sustituye la animación existente: el contador y el reloj de frames se mantienen. No cambia el perfil de aterrizaje, colisión ni mapa MIDI.

Ruta de PNG y JSON: Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Materials. Constructor editable: Tools/refine_playable_materials.py. Registry: RefactorMaterialArt.cs. Sierra y Selva llaman al registro desde sus dibujadores; Costa desde NamedSprite y SpriteMaterial. Los tamaños de las piezas costeras se calculan con PPU real para no duplicarse.

## Verificación hasta este punto

- Runtime y Editor compilados con referencias Unity.
- Contratos de 261 hojas / 1000 frames desplegados PASS. Incluye hojas alternativas; no son 1000 poses únicas activas.
- Trece pruebas de Sierra PASS; cuatro cuevas simuladas a 30/60/120 FPS PASS; ocho pruebas de centrado y ocho geometrías de HUD PASS.
- Auditor real Unity de las 42 hojas PASS: carga, frames, tamaño original y Point; suelo Costa Wrap Repeat PASS.
- Tour de Sierra/Selva completado sin excepciones registradas. No es recorrido cronológico. Costa murió en el piloto; se corrigió aislamiento y evaluación por subpaso del piloto, pendiente repetir en Editor actualizado.

No se certifica aún recorrido completo Costa–Sierra–Selva, sincronía auditiva de todas las transiciones ni calidad uniforme de cada frame. No se genera un ejecutable nuevo.
