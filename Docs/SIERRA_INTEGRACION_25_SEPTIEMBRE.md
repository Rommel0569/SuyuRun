# Sierra — integración funcional del 25 de septiembre

## Cambios implementados

- Se registra Sierra en las escenas disponibles, sin construir un ejecutable ni borrar otras escenas registradas.
- El final de Costa conserva su animación. Su resultado permite CONTINUAR A SIERRA y guarda el desbloqueo.
- El selector existente permite volver a entrar a Sierra después del desbloqueo.
- `RunnerProgress` extrae las reglas de gasto/reintento y guardado del banco existente: mismas claves, coste5.
- Costa y Sierra usan ese código compartido para gastos de reintento y el cierre de nivel.
- `RunnerPickup` extrae los campos del Item existente de Costa; Sierra reutiliza esa representación para piedras/monedas.
- Sierra ofrece monedas y piedras durante el recorrido, munición limitada y punto de retorno seguro después de la mitad.
- El reintento mantiene recogibles consumidos y enemigos derrotados para evitar duplicar recompensas.
- Puentes de tres tramos,768px, con terreno entre ellos; daño/collapse afecta la superficie física.
- Aves con variación de fase entre partidas, ataques separados al menos1.5s y reloj DSP.
- Final de Sierra guarda banco, récord, traje protector EXISTENTE y desbloqueo de Selva. Selva todavía no es una escena terminada.
- Controles funcionales de pausa, reintento, retorno al menú y resultado; frases breves contextuales.

## No confundir implementación parcial con entrega final

La integración arquitectónica es parcial: el controlador de Sierra aún tiene física y pantallas de pausa/resultado propias.
El HUD superior ahora comparte `RunnerHud` con Costa: mismas posiciones y lógica de casillas, monedas y puntos,
con sprites de cada región. Falta extraer/reutilizar íntegramente pausa y resultado; no se declara cumplido ese requisito.
No se generaron nuevos assets. Fondo conservado. Pendientes: enemigos refinados, cinemáticas de cámara/entrada/salida,
NPC e interacción narrativa, impulsores/escape/atrapamiento, variedad adicional de obstáculos, revisión manual de
dificultad y calibración MIDI de oído. La prueba automática no sustituye estas revisiones.

La conexión de Costa se ofrece desde el resultado del portal, no como carga automática que elimine la pantalla de victoria.

Prueba Unity Play: Complete177.55, errors=0, 25/09/2026 06:22:24-05:00. Autoplay diagnóstico,
sin guardar progreso del usuario; no equivale a balance manual ni aprobación musical. Esta prueba precede
la extracción del HUD compartido, que se comprueba aparte.

Verificación compartida: PASS para gasto válido/insuficiente/negativo, recogida única, guardado diagnóstico
aislado y registro de escena. HUD inspeccionado en Play; se ajustó su fuente/padding para evitar recortar
los contadores al pasar de las coordenadas Costa1280 a Sierra320. Pendiente repetir captura de ese ajuste.
Las últimas pruebas cortas quedaron en0–0.38s sin avanzar; sus capturas son antiguas y NO validan
el ajuste. Se añadió `Application.runInBackground=true`, igual que Costa, para evitar detenciones
por pérdida de foco. Este último cambio requiere nueva prueba. Se solicitó detener el autoplay.
