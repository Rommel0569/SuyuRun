# Sierra — últimos retoques visuales solicitados

- Cuevas: 12 antorchas por mapa, colocadas en huecos de pared con un margen de
  4 píxeles respecto de toda plataforma y suelo sólido. Se conservan sprite,
  animación de llama, paleta y orden de profundidad.
- Ruinas: cada punto válido «SALTA» marca el escalón real con una franja luminosa
  animada que sigue el perfil del suelo y una flecha ascendente. No se añaden
  obstáculos ni colisiones ocultas; se respetan los corredores rítmicos reservados.
- Rectángulos cafés: identificado el sprite de tronco del encuentro `log`.
  Se sustituye su representación por un sello pétreo biselado azul claro, con
  incrustación escalonada y levitación local de ±2 píxeles. No cambia su anclaje
  horizontal ni su envolvente de colisión. El PNG anterior se conserva.

Implementación gráfica nativa de código, sin sobrescribir assets aprobados.
No se modifica Costa ni su conexión con Sierra: el usuario explicará después
qué falta en esa conexión. No se genera ejecutable.

Verificación: `SierraCaveChecks` valida las 24 posiciones de antorchas y el modelo
de ambas cuevas; `test_sierra_mobility.py` pasa sus 7 pruebas. Las capturas
`Artifacts/SierraV3/finishing-0.png` a `finishing-3.png` son vistas reales de Unity
Play con el reloj detenido por una herramienta exclusiva del Editor para inspección
visual; NO son una nueva corrida completa ni validación manual de dificultad.
El último recorrido completo de juego sigue registrado en `unity-play-result.txt`.
