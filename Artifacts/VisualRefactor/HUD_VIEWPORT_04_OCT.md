# Encadre y HUD — 04/10/2026

Cambios integrados en código:
- Cámara Costa/Sierra/Selva: StretchFill en lugar de Windowbox.
- HUD y puntero Selva comparten la misma escala y origen del encuadre.
- Casillas del patrimonio abajo, separadas de mensajes; iconos reales del museo.
- Contadores centrados verticalmente; eliminado el segundo marco superpuesto de Sierra.

Validación: compilación runtime y Editor satisfactoria, sin generar Player.
Pendiente: inspección visual en Unity Play y ampliar el dibujo nativo de fondos
restantes. No se declara terminado el refactor global.

En pantallas 16:9 el encuadre cubre toda la vista, también en resoluciones no
múltiplos de 640x360. En otros aspectos se conserva la proporción con bandas:
no se recorta gameplay ni se deforman personajes. Una adaptación panorámica
sin bandas requiere ampliar el contenido de las capas y validar cada recorrido,
no estirar la imagen.
