# Sierra — auditoría contra las especificaciones adicionales

Este registro distingue implementación de aprobación. No reemplaza las especificaciones.
Los avances anteriores se conservan; el fondo actual no fue regenerado.

Últimas acotaciones del usuario y evidencia: `SIERRA_DESCENSO_ROCAS_25_SEPTIEMBRE.md`.
Halcón y roca rodante aprobados. Cierre funcional: `SIERRA_LISTO_PARA_PROBAR.md`.

| Sección | Estado y evidencia |
|---|---|
|0 /23 Reutilización|Compartidos RunnerPickup, RunnerProgress, RunnerHud, RunnerModal y RunnerMotion. Controladores de terreno/combate siguen adaptados por región; no se declara migración total de todos los controladores.|
|0.1 Identidad Sierra|Sprites y paleta Sierra en obstáculos, protagonista y paneles; fondo intacto.|
|1 Portal cinematográfico|Costa conserva brazos/sorpresa/absorción; cámara de acercamiento1x/2x, cierre pixelado y carga real de Sierra. Llegada con revelado, salida caminando, reacción/frase y cámara general al comenzar. Zoom entero para no suavizar píxeles.|
|2 Jugabilidad|60 encuentros (32 del mapa,7 cruces,7 vigas posteriores,3 aves de roca,8 de oleadas,3 atacantes de puentes), más21 puntos de movilidad.|
|3 Piedras|Recogibles, inventario limitado, selección de ave, carga/giro/lanzamiento.|
|4 Secuencias de aves|Oleadas de3 y5 con entrada cada1.6s y cola de ataque mínimo1.5s. Corredores sin obstáculos de suelo incompatibles.|
|5 Vuelo|Entradas laterales, cambios de altura, curvas y persecución. Variación por partida; semilla estable en diagnóstico.|
|6 Respawn|Retorno seguro después de mitad; coste5; retroceso visual; recogidas y bajas preservadas. Prueba diagnóstica fuerza una compra real con monedas del recorrido, sin persistir guardado.|
|7 Puentes|Tres puentes de768px divididos en tramos, terreno entre ellos, integridad y colapso físico.|
|8 Saltos|Siete ejercicios de base→carga aérea→descenso; ESP y S/↓; aterrizajes reservados.|
|9 Interacción|Guardián fantástico del puente dialoga/gesticula; Francisco reacciona; música y avance se reanudan desde el mismo instante. Arte existente reutilizado.|
|10 Ruinas|Escalones existentes, rocas, troncos, barro, pinchos simples/dobles, vigas y obstáculos elevados. No se agregó aceleración opcional porque alteraría el mapa fijo.|
|11 Más aves|Oleada de5 en ruinas más cruces y aves individuales.|
|12 Fondo|Conservado, sin regeneración.|
|13 Animación base|Cuerpo y animaciones base de Costa remapeados, brazos/huaraca Sierra.|
|14 Frases/recompensa|Llegada, guardián, puente, checkpoint, trampa, portal; traje protector existente para Selva.|
|15 Acabado de personajes|Halcón6frames y roca8frames aprobados. Se conservan los demás recursos existentes, huaraca y reacciones integradas; agachado con apoyo visible corregido y animación cíclica.|
|16 Referencias|Opcionales; este avance reutiliza recursos originales existentes.|
|17 Monedas|Datos de recogida y reglas de gasto/guardado compartidas; respawn pagado.|
|18 HUD|RunnerHud compartido, sprites Sierra, monedas/puntos/casillas.|
|19 Pausa|RunnerModal compartido; tema9-slice, botones pixelados, pausa en interludios y pestañas de museo/ajustes/tienda.|
|20 Fin|Francisco se desplaza al centro apoyado en el suelo real, cámara cercana, transformación/efecto protector/sorpresa; resultado espera3s y vuelve al plano general.|
|21 Victoria|Mismo RunnerModal en ambos niveles; monedas/puntos/banco, botones contextuales.|
|22 Desbloqueo|Selva y traje se guardan; selector refleja progreso. Selva sigue en desarrollo, no se carga una escena inexistente.|
|24 Consistencia|Recursos y fuente pixel existentes, sin reemplazar fondo. Aprobación visual final pendiente.|
|25 Entrega total|Cierre funcional preparado para prueba del usuario. La aprobación manual de dificultad/estética y alineación de oído no se sustituye por autoplay. Selva sigue fuera de esta entrega.|

## Dificultad y seguridad

- Pinchos simples/dobles, rocas rodantes no destruibles, troncos, vigas, barro con escape y guardianes.
- Barro: tres pulsaciones de ESPACIO en2.2s; escenario/música pausados; si falla pierde una vida y recupera control.
- Separación mínima actual del mapa:2.28s. La mayor intensidad se consigue combinando mecánicas, no superponiendo daño inevitable.
- Zonas de movilidad reservadas desde2s antes hasta3s después; las oleadas reservan su propio corredor.
- Física comparte el paso semiimplícito de Costa y consume DSP en subpasos de1/120s; evita perder simulación en frames lentos.
- No se ha validado de oído la discrepancia de duración MIDI/MP3. Los nuevos eventos de grilla están marcados, no fingidos como notas detectadas.

## Revisión visual individual

`Tools/refine_sierra_halcon.py`: solo halcón en vuelo,6frames32x32, paleta24, alfa binaria.
Anterior conservado en `Artifacts/SierraV3/BeforeFalconRefinement`; vista `Artifacts/SierraV3/halcon_revision.gif`.
Halcón y roca rodante8frames aprobados por el usuario. No se generaron más PNG en el cierre funcional.

## Verificación del 25/09/2026

Unity Play completó177.55s a las15:54:20 (hora local), cero errores registrados:
7bases,7orbes,7descensos,1escape de barro,1reintento pagado y diálogo del guardián.
Es autoplay diagnóstico: NO certifica dificultad manual, puntería con ratón ni sincronización de oído.
Resultado: `Artifacts/SierraV3/unity-play-result.txt`.
Segunda pasada completada a las16:02:32:177.55s, cero errores y los mismos contadores.
Captura `Artifacts/SierraV3/unity-results.png` inspeccionada: título, traje, desbloqueo,
monedas132, puntos4010, memorias3/3 y botones legibles, sin solaparse.
El banco2319 es el cálculo diagnóstico; esta prueba no guarda esas ganancias.
Esas capturas son históricas: el cierre posterior reemplaza los botones Sierra por geometría pixelada.
Las siete pruebas actuales de `Tools/test_sierra_mobility.py` pasan. Hashes protegidos de Costa:0 cambios.

## Cierre funcional posterior

- Museo Sierra permanente con3fichas, siluetas/anticipos, fuentes y acceso desde pausa/resultados o museo general.
- Tienda comparte banco y mejoras; vida extra, ráfaga e imán aplicados a Sierra.
- Ajustes de música, compensación del juicio y tap-tempo, sin estirar ni fingir corregir el MIDI.
- Rachas/puntos por ritmo y sonidos originales sintetizados para las acciones.
- Unity25/09/2026 20:58:03: Complete177.55, errors0, Pads7/Orbs7/Dives7, Escape1, PaidRevive1, GuideTrue.
- Interfaces25/09/2026 20:54:17: PASS, errors0, pausa congela el recorrido y reanudar lo continúa;
  museo/ajustes/tienda capturados e inspeccionados sin solapamientos.
- Verificación posterior del portal y replay en `Artifacts/SierraV3/portal-verification.txt`.
  PASS25/09/2026 21:02:45, errors0: enlace real y reinicio a Camino. Una ejecución previa
  invalidada por recarga de scripts en Play fue detenida y repetida en limpio.
