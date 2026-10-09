# Fluidez de presentación — 04/10/2026

## Cambios integrados

- Costa: carrera vinculada al avance lógico/longitud de zancada. Se elimina el reloj distance*2 que seleccionaba aproximadamente 336 frames por segundo a velocidad normal, saltando las poses de una hoja de 12 frames.
- Costa: las cuatro poses completas de agachado se conservan, sin reducir el ciclo a tres muestras.
- Sierra: reloj local para poses no repetidas; correr/disparar comparten fase de zancada. Caída tiene su estado propio.
- Cuevas Sierra: reloj de pose local, daño y caída separados; disparo corriendo mantiene la fase de piernas.
- Selva: se registra la entrada a todos los estados, incluso run/burst, para que un segundo salto no conserve el reloj del primero.
- Sierra/Selva: movimientos de sprites detallados alineados a medio píxel lógico (un píxel nativo) en lugar de saltar dos píxeles nativos. Sin blur ni interpolación de sprites. Colisiones y recorridos sin cambios.
- FrameAnimator: selección aritmética de frames ante un frame largo, sin bucles de recuperación ni acceso a un renderer destruido.
- Cuatro hojas adicionales de túneles/portales: juntas y biseles nativos, oscuridad de la abertura y fuego preservados; tamaño y celdas originales conservados.

## Verificación

Compilación runtime/Editor PASS. Pruebas de zancada a 30/60/120 FPS: 12 poses visitadas, ninguna omitida a velocidad de Costa. Reentrada de estado y rebobinado de checkpoint PASS.

Auditor Unity: 46 hojas de materiales PASS, tamaño en mundo, carga y Point. PNG: 265 hojas / 1020 frames, cero fallos de contrato; incluye alternativas, no poses únicas activas.

Tour diagnóstico actualizado solicitado; consultar play_visual_tour.txt. No equivale a recorrido cronológico, validación auditiva ni garantía de fluidez artística de cada hoja.

## Falta del alcance global

Redibujado nativo de las capas ambientales restantes, efectos secundarios y UI; revisión visual de todos los ciclos y transiciones en el recorrido completo. Este avance NO certifica todo el juego al 100%.
