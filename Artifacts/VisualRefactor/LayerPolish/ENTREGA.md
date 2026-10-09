# SUYURUN — lote completo de capas, 08/10/2026

63 capas únicas redibujadas por separado, usadas en 12 contextos: Costa, tres biomas y dos minicuevas de Sierra, seis zonas de Selva (incluyen las dos arenas). Catálogo exacto: CAPAS_POR_MAPA.md/JSON.

Incluye cielos, nubes, montañas, arquitectura, vegetación lejana/cercana/frontal, suelos, plataformas de cuevas, niebla, rayos y agua. Cinco hojas ambientales mantienen 40 frames: aves18, mar4, río Sierra6, fondo y superficie Selva6 cada una. Tres hojas adicionales de huaraca: carga2, giro8, lanzamiento8.

## Integración

Recursos nuevos: Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/LayerPolish y TaytaSierra/*_v3. Los cargadores de entorno/materiales y Tayta los prefieren; originales conservados. Importador Point, sin compresión/mipmaps, PPU64; superficie de río legible por CPU, transparencia original. Suelo Costa repite su friso.

Capas amplias 1280×360, lógico640×180; cuevas Sierra1280×768, lógico640×384. Tiles conservan doble nativo y tamaño lógico original. Redibujos nuevos, no simplemente ampliaciones. Densidad final conserva el contrato2× del refactor para no cambiar tamaño físico, cámara, MIDI, música, colisiones ni recorrido.

Paletas regionales existentes, máximo24colores por hoja. Máscara de apoyo original conservada en suelos/plataformas; composición y espacio libre registrados. HUD y portales comprobados por el usuario intactos. Huaraca sin ajuste de ancho por pose, escala uniforme y pies anclados.

## Verificación y límites

Compilación runtime/Editor PASS. Auditor PNG:456hojas/1570frames, cero fallos de dimensiones, paleta, alfa y ocupación. Incluye el banco anterior, no son1570poses nuevas. Auditor Unity intentado en entorno restringido y fuera de él: ambos terminan antes de ejecutar el método al iniciar los servicios de licencia. Logs unity_batch*.log; no existe PASS de carga real. Puede repetirse en Editor con SuyuRun/Refactor/Verificar lote de capas. No es una prueba en Play.

Inspección por láminas Review/*.png realizada. Falta el recorrido visual completo en Unity Play y aceptación estética del usuario; no se certifica todo el juego al100% ni ausencia de bugs. Final cinematográfico, nivel secreto y nuevas mejoras de jugabilidad Selva quedan fuera por indicación del usuario.

ZIP: banco completo existente,66hojas nuevas, fuentes, metadatos, código, herramientas y documentación. MANIFEST.json distingue nuevas del trabajo anterior. PROMPTS.json registra prompts del built-in imagegen; tres briefs iniciales se identifican como reconstruidos, no transcripción exacta.
