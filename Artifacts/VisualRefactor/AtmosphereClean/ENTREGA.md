# Cielos y queñua con menor ruido

Pedido del usuario: aliviar el aspecto borroso/ruidoso del cielo, aumentar calidad y resolución, revisar cielos de todos los niveles y árbol de Sierra, quitar montañas inferiores y centrar controles.

## Integrado en recursos y código

- Ocho fondos independientes: Costa, Sierra compartido, Selva bosque/río/noche/cueva/ruinas/templo. Redibujados con imagegen; menos tramado repetitivo y contornos más finos. Cueva/ruinas mantienen paredes, no se convierten en cielos.
- 2560×720 por fondo, lógico640×180, PPU128. Doble densidad respecto a la revisión1280×360; tamaño en mundo sin cambios. La exportación usa registro nearest-neighbor de las fuentes generadas: la resolución del archivo no implica que cada píxel adicional sea un detalle nuevo.
- Queñua Sierra: cuatro poses nuevas, celda256×288, lógico64×72, PPU128. Follaje orgánico, corteza y base alineada; máscara nueva, no recortada dentro de los polígonos antiguos.
- Pie Sierra limpio: interior sin montañas, ichu ni estrellas. Borde y cuatro casillas EXACTOS del anterior. Selección footer_clean; original conservado.
- Controles con zona común centrada178×9 en Costa/Sierra/Selva y mininiveles, en vez de los130px de Sierra que hacían diminuto el texto.
- Carga prioritaria AtmosphereClean en RefactorMaterialArt, importación Point/uncompressed/PPU128. Acepta densidades2y4 conservando contratos de geometría. Originales LayerPolish/Materials/UnifiedHud sin sobrescribir.

## Alcance y verificación

Inspección de los PNG realizada y compilación runtime/Editor PASS. VALIDATION.json comprueba dimensiones, paleta≤24, alfa y relación tamaño/PPU, más conservación exacta del marco inferior fuera del área solicitada. No equivale a Unity Play ni aceptación visual. No se generan ejecutables, ni se cambian recorrido, música, enemigos, colisiones o portales.

Fuentes y prompts en esta carpeta. Se conserva algo de textura fina de las generaciones: no se anuncia ausencia absoluta de ruido en todo el juego. Las demás capas no se sustituyen indiscriminadamente.
