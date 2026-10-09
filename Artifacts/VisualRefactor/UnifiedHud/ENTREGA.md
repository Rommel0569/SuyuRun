# HUD unificado — 08/10/2026

Se regeneraron tres kits regionales mediante imagegen, nueve strips de producción: cabecera con pistola, cabecera con huaraca y pie de cada región. Exportados a Resources/UnifiedHud, 1248×80 cabeceras y 1248×76 pies. Originales preservados en cada carpeta regional y recursos anteriores retenidos como fallback.

Dirección: pieza tallada única terracota/oro/tinta, ondas Costa, montañas Sierra, hojas Selva; corazón y canal de salud integrados; arma y espacio de estado; moneda, sol y puntuación; pie despejado con cuatro marcos de museo. Sin textos horneados, blur ni ilustración del juego. Fuente referenciada: HudCarved/costa_header_v2 y costa_footer_v2. Tres bandas independientes por kit, fondo transparente.

Exportación mecánica: alpha binario, recorte de bandas, nearest neighbour, cuantización a máximo 24 colores de la paleta regional. Validaciones detalladas en cada validation.json.

RunnerHud dibuja una sola cabecera seleccionando arma y región. Vida, carga/munición y cifras ocupan los huecos del dibujo. Las teclas se centran en el área libre regional, sin tapar las montañas de Sierra. Iconos de museo centrados dentro de sus marcos. Progreso y porcentaje conservados arriba. Todas las rutas Costa/Sierra/Selva y minicuevas emplean el mismo componente. No se modificaron escenarios ni flujo.

Compilación runtime/editor PASS. check_hud_layout.ps1 PASS: tres regiones por ocho resoluciones, controles dentro del pie, iconos dentro de marcos y sin superposición de mensajes. Inspección de los nueve PNG efectuada. Estos resultados no constituyen prueba visual Unity Play ni aprobación del usuario.
