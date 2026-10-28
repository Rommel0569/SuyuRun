# Correcciones sobre las fuentes generadas

Herramienta integrada imagegen; no se usó CLI/API ni se pintaron sprites con scripts.

- Sierra: edición de fondo a transparencia real; reducir recipiente exagerado a pequeño bolsillo oval de waraca de dos cuerdas; quitar la piedra gris separada de las poses de lanzamiento porque el reproductor crea el proyectil.
- Selva: quitar segmentos verdes de liana junto a los dedos sin modificar manos, poses ni grilla, para que la cuerda del reproductor sea la única.
- Exportación mecánica: registro por bandas de filas para no cortar pies, escala uniforme para mantener anatomía, aislamiento de componentes completos para no recortar alas ni insertar sus extremos en el frame de vegetación siguiente. Paleta cuantizada a24colores regionales y alfa binaria. No hay reconstrucción de partes faltantes por código.

Los prompts iniciales completos están en PLAN_COMPLETE.json y las rutas de las últimas fuentes generadas en GENERATED_SOURCES.json. Los PNG de Sources corresponden a esas revisiones exportadas.
