# Orientación y entrada central — 4 de octubre

Solicitud: corregir orientación, no rediseñar escenarios, ataques ni recorridos.

## Implementado

- Giro hacia Tayta del guardián del puente, enemigos de Sierra, jefes y esbirros de ambas regiones. Durante una embestida se respeta la dirección de ataque fijada.
- Aves de combate de Costa orientadas hacia Tayta; bandadas de Sierra y guacamayos decorativos de Selva orientados hacia su desplazamiento. Las bandadas de Costa ya se desplazan hacia la derecha, acorde al sprite.
- Un giro conserva el centro visual del sprite para evitar desplazamientos respecto a la colisión. Aplicado también a Tayta y al arma de respaldo en las cuevas de Sierra.
- Entrada al túnel de Sierra calculada desde el ancho real de su sprite, en lugar de una coordenada arbitraria.
- Portales de las cuatro cuevas: aproximación al centro durante los tres segundos de salida. Se bloquea la entrada de movimiento/salto/disparo durante esa salida para no atravesar el portal.
- Revisados los cálculos de centrado de los portales finales de Costa, Sierra y Selva; conservados sus recorridos.

## Verificación

- Compilación de runtime y Editor con referencias de Unity: PASS.
- Simulación de las cuatro cuevas a 30/60/120 FPS: 12 casos completados.
- Prueba específica de centrado con movimiento izquierda/derecha, salto, bajar y disparo mantenidos: 8 casos PASS.
- Estas pruebas no sustituyen una revisión visual en Unity Play. Los últimos cambios de orientación aún requieren esa comprobación; no se declara ausencia total de bugs.

No se reemplazó ningún PNG ni se generó otro ejecutable. Los dos borradores de fondo generados previamente quedan sin integrar en este ajuste de orientación.
