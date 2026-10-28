# Tayta: anatomía común para juego y final

Estado posterior27/10/2026: este banco se conserva como respaldo DESACTIVADO. Se reactivó el personaje anterior por la preferencia del usuario de conservar todos sus movimientos. Ver ACTIVE_STATUS.md. La integración y pruebas descritas abajo son el historial del24/10, no el estado activo actual.

Pedido del24/10/2026: unificar las proporciones del personaje y llevar las mejoras visuales al juego. Se conserva chullo, rostro humano y vestuario regional; Costa oculta los brazos, Sierra los muestra y Selva mantiene el traje protector completo. Se usan proporciones humanas más esbeltas y pies/cabeza menores que en los bancos anteriores. La guía visual es de unas6.5cabezas, no un certificado de anatomía realista exacta en cada dibujo.

## Integración

75hojas de estado y366celdas físicas para los tres niveles y mininiveles: carrera16poses, idle, salto/doble salto/caída, contacto, deslizamiento, disparo, recarga, daño, derrota, guardia y celebración. Sierra incorpora carga/giro/lanzamiento de huaraca y disparo aéreo; Selva suma agarre/balanceo/suelta/reagarre/escalada, puños/patada/uppercut/esquiva. Algunas variantes comparten poses; no son366dibujos únicos.

Siete montajes de actores para el final salen del mismo banco. El pedido actual de uniformidad sustituye para los personajes la separación antigua entre arte de gameplay y epílogo; los fondos cinematográficos y el montaje siguen independientes. Los recursos anteriores se conservan, la carga prioriza RefactorBank/TaytaUniform. No se reemplazan escenarios, música, guardado ni recorridos.

Todos los estados comparten PPU150.588antes de aplicar HeroScale0.85y el mismo origen de pies en una celda384×320. El cuerpo neutro mide160píxeles nativos:28.9píxeles lógicos visibles en juego. El mayor tamaño del PNG aporta detalle sin aumentar la altura del personaje ni las colisiones. No se aplican estiramientosX/Ypara simular aterrizaje. El final calibra el encuadre de esta misma anatomía, conservando el aspecto de los píxeles.

Las filas fuente que se dibujaron a una escala menor se calibran mediante su pose humana neutra de recuperación. Se escala el dibujo completo de forma uniforme en ambos ejes, nunca un brazo, cabeza o pierna por separado; frameUniformScales deja constancia del registro. La pose doblada no se fuerza a ocupar la altura de una pose de pie. El deslizamiento Costa mantiene un límite visual inferior a19píxeles lógicos para respetar el techo existente.

La zancada visual pasa a40píxeles lógicos por ciclo; cambia la cadencia de dibujo, no la velocidad del runner. El polvo se emite por contactos y aterrizajes, con partículas reutilizadas y desplazamiento relativo al suelo. La presentación del minijuego Proyectiles también usa Tayta uniforme. La animación aérea armada de Selva selecciona poses con pistola en mano.

## Producción y límites

Generación mediante imagegen integrado, con una guía anatómica común y seis hojas fuente principales. PLAN.json guarda los prompts y correcciones; SOURCES.json identifica las fuentes finales. Separación por componentes, registro rígido, exportación de estados y cuantización son operaciones mecánicas; no se pintaron sprites con Python. Paletas regionales anteriores de hasta24colores, alfa binario, Point y sin compresión. Sierra evita los cuatro tonos más pálidos para conservar el acabado mate.

Dos poses de una edición de Sierra se tocaron; se separó su distribución espacial usando la hoja de referencia. Una pose de daño que quedó incompleta se rechaza y se reemplaza por una pose completa de recuperación del propio banco. La fila aérea armada de Selva se toma de su fuente generada anterior a la limpieza de lianas, porque esa corrección retiró accidentalmente sus pistolas; esa fila no contiene lianas. No se presentan los intentos descartados como resultados aprobados.

La verificación exacta de Unity y los límites del recorrido se registran en VERIFICATION.json. VALIDATION.json comprueba PNG y metadatos; UNITY_RESOURCE_AUDIT.txt comprueba carga, PPU y origen. La aceptación estética del usuario sigue pendiente. No se genera ejecutable ni se sube al repositorio en este avance.
