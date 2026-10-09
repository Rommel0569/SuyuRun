# Inca — locomoción nueva integrada, 08/10/2026

Lote autorizado. No se reemplazan ni borran los originales.

## Entrega

- charge_run: 12 poses dibujadas, ciclo de 24 FPS, zancada, rodilla, empuje y capa.
- retreat: 8 poses dibujadas, ciclo de 12 FPS, pasos hacia atrás manteniendo guardia.
- Hojas Unity: Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/inca/MotionV3/*.png y JSON. Celdas 192x128, alpha binario, paleta Sierra <=24 colores, pie Y124, extensión registrada 102. Contenido íntegro dentro de márgenes; no ajuste de tamaño independiente por pose.
- charge_run.gif / retreat.gif y previews en esta carpeta; fuentes generadas conservadas.
- RefactorActorArt selecciona estados nuevos; SierraCaves diferencia carga y retirada y usa e.timer. Alias de fallback a guardia evitan pedir un sprite legacy inexistente.
- Importador RefactorRuntimeImport aplica Point, sin compresión, sin mipmaps, máximo 16384. Colisiones/mecánica de charge originales sin cambios.

## Generación

Skill imagegen, herramienta integrada, referencias de identidad: RefactorBank/inca/andar.png. Fuentes originales en .codex/generated_images, copiadas también aquí. Exportación mecánica: Tools/export_inca_locomotion.py. No dibujo procedimental ni ampliación vendida como arte nuevo.

Prompt de carga: atlas Unity pixel art, mismo Inca de corona/armadura roja y oro/capa azul/maza solar; doce poses secuenciales en 4x3, nativo mirando derecha, carrera pesada con inclinación de torso, rodillas alternadas, despegue y apoyo, maza firme, capa con inercia; proporciones/cámara constantes; fondo transparente; sin texto, suavizado ni recortes.

Prompt de retirada: mismo personaje; ocho poses en 4x2, rostro/torso mirando derecha pero desplazamiento hacia izquierda; guardia, pie trasero alcanza, traslado de peso, empuje delantero, paso de recuperación, compresión y vuelta a guardia; articulación de tobillo/rodilla/cadera y capa; misma escala, transparencia y pixel art.

## Comprobaciones

Veinte frames únicos; márgenes seguros, paleta y alpha PASS. Auditor global: 387 hojas/1445 frames sin fallos de contrato. Compilación runtime y Editor PASS. Simulaciones de cuatro minicuevas a 30/60/120 FPS PASS. No equivale a aceptación artística ni Play completo. Falta revisar visualmente timing, orientación y transición de estas dos acciones dentro de Unity.
