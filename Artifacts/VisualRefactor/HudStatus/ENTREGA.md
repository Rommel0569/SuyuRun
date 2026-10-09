# HUD común y disparo aéreo Sierra — 08/10/2026

Pedido vigente: reemplazar SUYURUN/nivel por vida y arma dentro del marco, en los tres niveles y las cuatro minicuevas. Se elimina el bloque oscuro externo y se conservan monedas, puntos y progreso.

## Integración

Seis PNG regionales en Assets/SuyuRun/Art/RefactorRuntime/Resources/HudStatus: costa/sierra/selva × sling/gun. Cada módulo mide 568x64, sin compresión, Point, sin mipmaps, alfa binario y paleta regional <=24. Diseño nuevo de corazón, huaraca trenzada y pistola, marco tallado; números, vida, carga y recarga se dibujan dinámicamente sobre los espacios reservados. Renderer único RunnerHud.StatusModule; caché de texturas y estilo, sin generación ni lectura de píxeles durante juego. Aproximadamente 852 KiB de texels RGBA para las seis imágenes sin mipmaps; importe real sujeto al Editor.

Controlador común usado por Costa, Sierra, Selva y minicuevas. Recarga muestra segundos y después balas; maldición muestra X. Sierra muestra piedras y barra de energía, Costa porcentaje de energía. Costa conserva vida de un golpe: no se inventan nuevas vidas ni se cambia su mecánica.

Mensajes centrados con caja calculada según longitud, altura y ajuste; no panel de ancho fijo innecesario.

## Arte / prompts

Skill imagegen, herramienta integrada. Fuente original conservada aquí en generated_source.png. Prompt: dos tiras de HUD pixel art transparente, marco adobe/bronce con bordes de oro y compartimentos de vida/arma; corazón rojo detallado, canal vacío para barra, primera huaraca de cordones trenzados y bolsa, segunda pistola; mismos márgenes, sin letras/números ni escena. Exportación mecánica con Tools/export_status_hud.py, sin sustituir diseño por reescalado del arte anterior. Previews regionales en esta carpeta.

Disparo aéreo Sierra: edición de SierraAir/generated_source.png para cambiar brazos y añadir pistola agarrada, manteniendo cinco poses de despegue, subida, ápice, descenso y llegada, vestuario, rostro y piernas. Fuente nueva y preview en Artifacts/VisualRefactor/SierraAirGun. Runtime jump_shoot / double_jump_shoot / fall_shoot, tamaño 192x128, mismo PPU/pie124 y paleta Sierra. Selección en SierraStageDirector y SierraCaves usa el reloj aéreo existente; no reinicia salto al apretar E. Originales conservados.

## Verificación y límites

Compilación runtime/editor PASS y ocho resoluciones de geometría HUD PASS. Falta aceptación visual de la integración actual en Unity Play; las previews no son capturas del juego. No se generó ejecutable.
