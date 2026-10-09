# Refactor visual — estado verificable, 03/10/2026

## Estado vigente consolidado — 04/10/2026

Ver `CIERRE_REFACTOR_VISUAL.md`. Acabado final de 106 hojas adicionales, 152 materiales verificados realmente en Unity, 371 hojas / 1359 frames sin fallos de contratos PNG. Resolución nativa de agua/lianas y atlas largos corregida; relojes de poses y propiedad compartida de aves corregidos. Producción de este lote consolidada, no certificación de juego al 100% ni recorrido cronológico completo.

Los párrafos siguientes son históricos: sus pendientes de producción de capas/materiales no describen el último lote. Se conservan como registro, no como checklist vigente.

## Actualización — 04/10/2026

### Revisión de fluidez posterior

46 hojas de materiales verificadas en Unity; 265 hojas / 1020 frames en contratos PNG, sin fallos. Reloj de carrera Costa corregido, estados locales Sierra/cuevas, reentrada de salto Selva y movimiento en cuadrícula nativa. Compilación y pruebas 30/60/120 FPS PASS. Detalles y pendientes en FLUIDEZ_04_OCT.md.

El nuevo tour diagnóstico todavía registra Costa Dead con su piloto. No declarar Costa superado ni diagnóstico resuelto. El muestreo de biomas no sustituye el recorrido completo. Pendiente redibujado de capas ambientales restantes, efectos secundarios y UI, además de aceptación visual de movimientos.

### Revisión posterior verificada

42 hojas adicionales de materiales integradas; auditor real Unity PASS para carga, dimensiones lógicas, tamaño en mundo, Point y repetición del suelo de Costa. PNG: 261 hojas / 1000 frames, cero fallos; incluye alternativas. Originales conservados.

El tour del Editor actualizado muestreó Sierra (tres actos/dos cuevas) y Selva (seis zonas), sin excepciones. Costa murió ante el ave inicial en el piloto diagnóstico; se aisló el input externo y se llevan las decisiones del piloto a cada subpaso. Esa corrección compila, pero falta repetirla en el Editor actualizado.

HUD Selva: miniaturas de recogida en lugar de láminas de museo reducidas; el museo conserva sus imágenes detalladas. Bloqueados en silueta. No certificada todavía esta revisión visual.

Pendiente: recorrido cronológico completo, finales y sincronía auditiva. El refactor NO se declara al 100%. Las notas inferiores describen revisiones anteriores.

- Trece capas independientes de entorno conectadas: se añaden camino y valle de Sierra, arcos de sus dos cuevas, río, cueva y noche de Selva a las seis anteriores.
- Tronco de Selva conectado: dibujo de corteza/anillos/musgo a 128x32, conservando el tamaño lógico 64x16 y alfa original. Fuente, prompt y PNG anteriores conservados.
- Auditor completo de texturas runtime: 218 hojas y 932 frames, sin fallos de contrato. No equivale a poses únicas o revisión visual.
- Las dos nuevas capas de esta entrega conservan exactamente el alfa original, dimensiones lógicas 640x180, PPU 64 y máximo 24 colores; originales intactos.
- HUD común: pie compacto, casillas separadas del mensaje, paneles 9-slice, puntero y GUI con la misma transformación de pantalla. Geometría verificada en ocho resoluciones.
- Orientación de jefes, esbirros y aves corregida; giro sin desplazar el centro visual. Túnel de Sierra y cuatro portales de cuevas centrados, con ocho pruebas contra entrada mantenida.
- Nueva prueba diagnóstica de Unity Play solicitada tras estas correcciones. Su informe es play_visual_tour.txt; no equivale a recorrido cronológico completo ni prueba de toda la música.
- Esa prueba terminó sin excepciones, pero el Editor todavía estaba usando la versión anterior del código (HUD antiguo y auditor de seis capas). No atribuir ese resultado a los cambios nuevos. Se necesita refrescar el Editor y repetir.
- Pendiente global: completar detalle de plataformas/obstáculos y capas restantes, revisar cada transición y recorrido en Play. No declarar refactor al 100%.

Las cifras y pruebas del 03/10 que siguen son históricas.

## Producido y conectado

- Banco: 205 hojas, 922 frames; seis capas independientes de entorno adicionales.
  210 hojas/capas desplegadas como recursos. Este conteo incluye hojas de staging y selecciones repetidas,
  no significa que todas estén activas en gameplay.
- Nuevo Tayta: selección del banco en Costa, Sierra y Selva, con fallback
  a estados originales que todavía no tienen sustituto.
- Sierra: carga, giro y lanzamiento de huaraca; defensa, recarga, escalada,
  baile y transformación al traje protector, además de estados básicos.
- Nuevos esqueletos, esbirros pétreos y brotes, guardián con huaraca, cóndor, halcón, ave de puna
  y ciclo del caimán. Fuentes originales conservadas en Actors.
- Presentación de jefes, esbirros y aves conectada mediante RefactorActorArt.
  Los enemigos mantienen hitboxes originales. El héroe y las cuevas tienen
  ahora una reducción común de escala de 15%, también en sus colisiones.
- Conectados los efectos de raíces, atadura, maldición e impactos de arma
  y piedra, además de rotura del escudo; otros efectos mantienen los originales.
- Costa: extras de salto, caída, aterrizaje, defensa, recarga, baile y recompensa;
  nuevos hexágono, ave, pescador y bailarín de 12 frames, sobre el ciclo original de dos beats.
- MotionV2: ataques de los cuatro jefes seleccionados por acción y fase;
  la recuperación de Selva vuelve a reposo, no repite una carga.
- Sierra: derrota de jefes y esbirros usa las nuevas poses, conservando
  los escombros originales como fallback.
- Registro al pie y canvas lógico constante: se mantiene el mismo recorrido.
  Render 640x360/64 PPU conserva el encuadre de 320x180/32 PPU y permite
  detalle doble en personajes; fondos y GUI mantienen su diseño actualmente.
- Revisión actual: 12 poses de carrera humana por región, 12 poses de disparo
  corriendo en Sierra/Selva y cuatro poses de agachado profundo por región.
- Carrera de cuevas vinculada al desplazamiento; mano de liana registrada
  con el número real de frames; paredes y techos usan la nueva escala corporal.
- Contrato y pendientes ampliados: GLOBAL_SCALE_AND_DETAIL.md.
- Arreglado acceso a frames de salto de Costa fuera de rango; eliminado
  squash adicional en las nuevas poses de agacharse.

## Comprobaciones

- Validador PNG: geometría del atlas, alfa binario, máximo 24 colores,
  márgenes transparentes y número de frames por hoja.
- Unity: compilación e importación satisfactorias.
- `runtime_audit.txt`: 303 comprobaciones de carga/registro satisfactorias
  en la última ejecución; aún no incluye la nueva comprobación de siluetas de agachado.
- `play_smoke.txt`: resultado de prueba corta en Editor Play; no certifica
  recorrido completo, sincronización de golpes ni acabado visual.
- Muestreados Costa, Sierra y Selva durante ocho segundos cada uno, sin
  errores registrados. Selva incluye cambios diagnósticos a río y jefes.
- Captura adicional de agarre real en Play: `play_Selva_Liana.png`.
  Se ve el protector sujeto a la liana; no es una certificación de todas las poses.
- Costa terminó en muerte con el control automático de la prueba corta.
  No se considera nivel superado por este ensayo; Sierra/Selva quedaron Running.
- No se generó ningún ejecutable nuevo.

## Pendiente real — no marcar como finalizado

- Revisión visual completa en Play de pies, tamaños, mira, huaraca y agarres
  de liana. Las anclas de mano de Selva siguen siendo estimadas.
- El material generado adicional de Sierra requiere comparar el modelo y
  el arma con los frames básicos: no asumir uniformidad milimétrica.
- No todas las hojas del banco se activan: hay poses alternativas de revisión,
  efectos alternativos y un export temprano del caimán conservado como respaldo.
- Costa: los extras/recompensa corregidos sustituyen la segmentación rechazada
  que unía figuras con partículas; los originales se conservan.
- Refactor de detalle de TODOS los escenarios/obstáculos pedido posteriormente:
  obligatorio y parcialmente realizado. Seis capas independientes activas;
  revisar composición y costuras y completar otros fondos/plataformas/obstáculos/UI.
- human_gait_audit.txt verifica carrera, PPU común, cuerpos reducidos,
  siluetas reales de agachado y clearance compartido: PASS en Unity.
- global_engineering_audit.txt: seis capas y cuatro salidas, cargadores,
  recarga y prórroga: PASS en Unity.
- physics_models.txt: las cuatro cuevas superadas a 30, 60 y 120 FPS,
  doce pruebas del modelo compilado. No certifica dificultad humana ni imagen.
- Último play_smoke.txt sin errores de ejecución; conserva la limitación
  de ocho segundos por escena y muerte del piloto automático de Costa.
- Capas de Paracas, huacas, nevados, ruinas andinas, bosque y árboles del
  templo Selva activadas; el resto del redibujado global sigue pendiente.

El banco y el juego NO deben anunciarse como refactor terminado al 100%.
