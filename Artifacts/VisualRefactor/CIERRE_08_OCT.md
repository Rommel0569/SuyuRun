# Refactor solicitado — 08/10/2026

## Implementación guardada

- Progreso porcentual superior; Sierra 224 → 200 px/s; doble salto libre regional.
- Ocho hojas de obstáculos con contraste y brillo cíclico, fuentes originales preservadas; acantilados Sierra sobre eventos existentes.
- Relojes de animación locales, carrera y disparo con escala corporal fija; HUD regional con vida y balas más visibles.
- Cámara de combate ampliada; margen de dibujo extendido para no retirar plataformas en los extremos.
- Tutorial de controles en AYUDA del menú compartido. Es informativo, no un nivel tutorial interactivo.
- Sonido de disparo Kenney Digital Audio conectado; licencia CC0 y fuente en Artifacts/Audio.
- Minicuevas: canción en bucle hasta finalizar el combate. Eliminadas muertes automáticas por duración de canción y volumen Sierra que llegaba a cero al minuto; mensajes de tiempo corregidos.
- Selva: sin octágono visual; conserva traje y protección de vida.
- Revivir acepta monedas de partida/cueva y banco. Selva y cuevas Sierra preservan enemigos derrotados, posición y combate, restauran vida y munición y otorgan protección temporal; reintentar sigue siendo independiente.
- Pachacútec y monolito usan animaciones de caminar al perseguir y reposo al recuperarse, no guardia deslizándose.
- Portales: fondo detrás del héroe, jamba/dintel delante; paso lateral y recorte en umbral, sin encoger/desvanecer Tayta. Extendido a túneles Sierra y portal de llegada.

## Evidencia

- Compilación runtime y Editor: PASS (08/10).
- Modelo de cuatro cuevas a 30/60/120 FPS: 12 recorridos completados.
- Ocho centrados de salidas con movimiento/salto/disparo sostenido: PASS.
- Cuatro combates siguen disponibles después de 65 s: PASS.
- Contratos PNG: 379 hojas / 1407 frames, cero errores; no equivale a 1407 poses únicas ni aprobación visual.
- Render real Unity de puerta Costa: cinco fases PASS, escala/alfa constantes; inspeccionadas fases 50/100. Encontrado y corregido MaterialPropertyBlock nulo.
- Pruebas geométricas HUD y relojes de animación: PASS. No sustituyen capturas de todos los mapas.

## Límites explícitos

La auditoría conjunta posterior de Unity no arrancó: completion_unity.log termina con código 1 antes de cargar el proyecto y registra fallo de validación del cliente de licencia. No presentar esa ejecución como PASS. El bloqueo temporal de la sesión de QA cerrada se conservó en Temp/UnityLockfile.qa08.backup.

Pendiente ejecutar la auditoría conjunta y el recorrido visual cronológico del código final: orientación de todos los esbirros, portales de cada mapa, márgenes de cámara y dificultad de acantilados. No se certifica ausencia de bugs, acabado artístico total ni 100% jugable. No se creó un ejecutable.

Para auditoría en Editor: SuyuRun.Editor.PixelViewportAudit.RunCompletion. El test de cartera restaura el saldo original con finally. Después usar RefactorPlayAudit.Start para el tour diagnóstico, no confundirlo con una partida completa humana.
