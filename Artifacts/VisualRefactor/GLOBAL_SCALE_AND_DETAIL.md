# Escala y detalle — alcance ampliado

## Ya implementado en esta revisión

- Carrera: 12 poses por región, contacto, impulso y vuelo; ciclo vinculado al desplazamiento.
- Disparo corriendo: 12 poses nuevas en Sierra y Selva; Costa conserva piernas corriendo sin brazos visibles.
- Agacharse: cuatro poses nuevas por región, silueta máxima 44 píxeles nativos.
- Escala común del héroe 0.85; se aplica también a salto, disparo y daño.
- Cuerpos, paredes y techos de ambas familias de cuevas ajustados a la escala.
- Obstáculos para agacharse de Costa/Sierra mantienen necesidad real de agacharse.
- Agarre de liana usa el número real de poses y alinea mano, pies y cuerpo físico.
- Seis capas independientes de detalle nativo 1280x360 integradas: Paracas,
  huacas, nevados, ruinas de Sierra, bosque y árboles del templo de Selva.
  Registro lógico 640x180. Las tres estructuras costeras se registran por
  separado para conservar los techos y alinear al bailarín.
- Muros y aterrizajes de Costa ajustados a la misma reducción de escala.
- No se conserva en caché un fallo transitorio de carga durante importación.
- Disparos de Selva parten de la altura ajustada; disparo detenido en cuevas
  avanza su animación sin depender de que el jugador camine.

## Verificado

- Compilación de C# runtime y Editor con las referencias de Unity, sin Player nuevo.
- Auditor de Unity: seis capas, dimensión física, filtro Point, cuatro portales
  (salida 3s y extremo derecho), cargadores de cinco balas y recarga 4s.
- 12 simulaciones de combate completas: cuatro cuevas a 30/60/120 FPS,
  todas superadas sin muerte, con jefe y esbirros derrotados y portal usado.
- Auditor de Tayta: PPU común, 12 poses de carrera, cuatro poses bajas por
  región, altura visible de agachado compatible con 19 píxeles de paso.
- Último smoke Unity Play: ocho segundos por escena, sin errores registrados.
  Costa murió con el piloto automático breve; no constituye nivel superado.

## Contrato

320x180 sigue siendo el espacio lógico. Mayor resolución del dibujo no debe
duplicar dimensiones físicas. Héroe: PPU 64/0.85; cuerpo de pie 28.9 píxeles
lógicos. Cuerpo de cuevas 15.3x28.9. Pies son el ancla estable.
Sprites de escenario propuestos 1280x360 equivalen a capas lógicas 640x180,
con PPU 64 y separación independiente de parallax. No cambiar X/beat del mapa.

## Pendiente: no anunciar juego completo

- Redibujado de todos los fondos, plataformas, obstáculos, portales y UI con detalle coherente.
- Las seis capas están activas; falta aprobación visual completa de repetición,
  siluetas y composición en cada tramo. Coincidencia de columna de borde no
  certifica una costura visual perfecta durante todo el desplazamiento.
- Prueba visual completa Costa → Sierra → Selva de nuevos tamaños y ciclos.
- Revisar alcance de proyectiles, anclas de armas y espacios jugables en cada bioma.
- Comparar las nuevas poses con la identidad aprobada; no sustituir silenciosamente originales.

Los informes automáticos validan recursos y geometría; no certifican acabado
visual, dificultad ni ausencia absoluta de bugs. No se ha generado un ejecutable.
