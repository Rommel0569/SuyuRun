# SUYURUN — lote consolidado de resolución y fluidez

04/10/2026. Entrega conjunta autorizada por el usuario; no se generó un ejecutable.

## Integrado

- Tayta y sus variantes Costa/Sierra/Selva, enemigos y cuatro jefes: banco versionado, escala corporal común y anclas registradas. Costa conserva ausencia de brazos; Selva conserva traje protector tipo gorila.
- Carrera humana de doce poses ligada a distancia. Corregido el reloj de Costa que saltaba poses; disparo corriendo comparte fase de piernas, sin encoger el sprite.
- Estados de salto, caída, daño y disparo con edad local; reentrada y rebobinado de checkpoint reinician la animación. Agachado mantiene todos sus frames.
- Trece capas ambientales de detalle anteriores, más 152 hojas de materiales registradas en Unity: plataformas, suelo, obstáculos, túneles, portales, capas restantes, decoración y efectos. 106 de esas hojas corresponden al acabado final de contornos/clusters nativos. No es un redibujado completo de cada fondo ni una simple ampliación presentada como dibujo nuevo.
- Fondos independientes y composición original conservados. Resolución nativa doble, Point, sin mipmaps ni compresión. Atlas largos de agua importados sin reducción automática de tamaño.
- Río y liana de Selva a resolución nativa doble: máscara de inmersión conserva transparencia original; curvatura y agarre conservan física y trayectoria. El aumento de detalle no cambia hitboxes.
- HUD/menu común con bordes nativos, barras y casillas dentro del canvas. Recursos adicionales de UI preparados; las pantallas comunes siguen usando su interfaz compartida, no cada PNG regional preparado.
- Aves decorativas con sprites detallados compartidos Costa/Sierra; el cierre de una escena no destruye el recurso que otra utiliza.
- Movimiento visual a media unidad lógica para aprovechar la cuadrícula nativa. Originales, MIDI, duración musical, recorrido y progreso conservados.
- Referencia de render adaptativa en las tres cámaras: 640x360 a tamaño suficiente, 320x180/32 PPU en una vista acoplada menor, 160x90/16 PPU en vistas todavía menores. El cociente resolución/PPU conserva exactamente el encuadre y también el zoom de cinemáticas. No oculta el aviso: evita su causa. Vistas inferiores a 160x90 no son un tamaño de prueba soportado.

## Comprobado

- Compilación de C# runtime y Editor con referencias Unity: PASS, sin construir Player.
- Contratos PNG: 371 hojas / 1359 frames, cero fallos. Incluye variantes y respaldos, no poses únicas ni recursos todos activos.
- Registro real Unity: 152 hojas de materiales; tamaño lógico y mundial, recursos, frames y Filter Point: PASS.
- Reloj de animación: doce poses sin saltos a 30/60/120 FPS; reentrada y rebobinado: PASS.
- Doce simulaciones compiladas de combate de las cuatro cuevas: salida alcanzada, sin muerte del piloto del modelo.
- Ocho pruebas de centrado de salidas de cuevas con movimiento/salto/disparo mantenidos: PASS.
- Geometría HUD en ocho resoluciones: PASS. Pantallas no 16:9 preservan proporción con bandas, no estiran el pixel art.
- Referencia adaptativa verificada en el Editor real: doce combinaciones de seis tamaños y zoom normal/cinemático, incluido 549x309: PASS. La referencia nunca supera el destino probado y el tamaño visible del mundo se conserva.
- Tour diagnóstico real de Unity: muestreo de tres actos/dos cuevas de Sierra y seis zonas de Selva. Consultar `play_visual_tour.txt` para resultado final y errores, si los hubiera.

## Límites de esta entrega

El piloto diagnóstico de Costa muere ante las aves iniciales. No equivale a un bug confirmado del juego ni a un nivel superado por esta prueba. No se alteró el mapa aprobado para hacer pasar al piloto.

No se certifica un recorrido cronológico completo Costa → Sierra → Selva, sincronía auditiva de todas las transiciones ni ausencia absoluta de bugs. Las pruebas automáticas no sustituyen evaluación humana de dificultad, calidad artística o todas las poses de mano/arma.

Algunos microefectos procedurales y recursos alternativos originales permanecen: no se afirma que cada objeto visible del juego sea un asset nuevo. El nivel secreto no pertenece a este lote.

## Archivos

- Recursos activos/versionados: `Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank`.
- Registro de acabado final: `remaining_visual_finish.json`.
- Contratos: `runtime_texture_contracts.json`, `material_runtime_audit.txt`.
- Pruebas: `animation_clock_audit.txt`, `physics_models.txt`, `play_visual_tour.txt`, `CompileCheck/result.txt`.
- Biblioteca consolidada: `Pack_Arte_Completo_Base_y_Refactor.zip`; incluye originales, refactor, integración y herramientas.

Esta ficha sustituye los pendientes históricos de producción de capas/materiales en `ESTADO_ACTUAL.md`, pero no borra las limitaciones de validación anteriores.
