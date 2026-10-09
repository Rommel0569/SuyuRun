# HUD tallado y poses aéreas — 08/10/2026

## Revisión posterior: definición y teclas enmarcadas

Seis variantes *_v2.png a 1248x80, reexportadas desde la fuente generada original (no ampliación de los anteriores PNG de 624x40, ni dibujo nuevo). Conservadas las variantes anteriores. RunnerHud prefiere la revisión v2. Teclas ahora son casillas con relieve dibujadas por el renderer nativo, etiquetas centradas y ajuste del conjunto completo al ancho disponible; aplicado a Costa, Sierra, Selva y ambas familias de minicuevas, incluidas acciones de puños. Ajustados centros de MON/PTS dentro de sus ventanas; títulos Sierra/Selva sin invadir la fila de vida/munición. Progreso superior intacto. Compilación y geometría en ocho resoluciones PASS. Validación visual en Unity Play todavía pendiente.

Pedido vigente: franjas de las tres referencias del usuario, aplicadas a niveles y minicuevas; escenarios sin modificaciones. Barra de progreso y porcentaje siempre arriba.

## Implementado

- Seis PNG versionados: cabecera/pie Costa, Sierra y Selva. Recursos `HudCarved`, 624x40, paleta regional de máximo 24 colores, transparencia binaria, filtro Point, sin mipmaps. Fuentes generadas conservadas.
- Marco tallado, adornos regionales, piedra, sol y cuatro casillas doradas. Etiquetas, monedas, puntos y controles son texto/estado del juego, no imágenes con valores ficticios.
- Renderer compartido RunnerHud; variantes según escena también durante minicuevas. Cabecera alta de combate conserva los frisos sin estirar sus bordes verticales. Iconos del museo dentro de las casillas; originales conservados.
- Costa: nuevas poses con la coreografía de Selva y vestimenta Costa sin brazos visibles. Impulso, piernas abiertas, recogimiento y extensión; tres hojas versionadas jump/double_jump/fall, mismo PPU, escala común y ancla de pies. No copia de la textura del traje gorila.
- Progreso superior con porcentaje en zona 0–4, separado de cabecera 4–24; no tapado por el marco.

## Verificación

Compilación C# runtime/editor PASS. Geometría HUD en ocho resoluciones PASS. Relojes de carrera/vuelo 30/60/120 FPS PASS. Exportadores validan número de paneles/poses, transparencia y paleta. NO equivalen a aprobación visual en Play. Unity no está ejecutándose; intento anterior de auditoría no arrancó por su entorno de licencia. Pendiente revisión real del nuevo lote en Unity; no afirmar juego al 100% ni recorrido completo.

## Producción

Habilidad utilizada: imagegen, herramienta integrada (no API externa). Fuentes intactas en generated_images y copia en estas carpetas de Artifacts. Exportadores solo segmentan, normalizan paleta y registran escala; no redibujan.

Prompt HUD: seis paneles pixel art transparentes, header/footer por región; madera/terracota tallada, piedra desgastada, frisos andinos, sol dorado, olas/montañas/hojas; interiores vacíos, sin texto/valores/personajes. Referencias: tres capturas del pedido.

Prompt Costa: cinco celdas transparentes en fila; identidad de la hoja Costa, coreografía de la hoja Selva; agachado, impulso, apertura de piernas, recogimiento, extensión de caída. Poncho sin brazos visibles, rostro a derecha, proporciones constantes. Referencias: hojas runtime jump de ambas regiones.

Previews: `hud_preview.png` y `../CostaAir/costa_air_preview.png`. Son vistas de assets, NO capturas del juego.
