# Correcciones y especificaciones adicionales — Nivel Sierra

> Aclaración posterior confirmada por el usuario (25/09/2026): se conserva el traje protector
> existente como recompensa para continuar a SELVA. Las referencias inferiores a traje de
> guerrilla y regreso a Costa no aplican. Se autoriza conectar el teleporte final de Costa
> con Sierra; el resto de Costa permanece cerrado y sin modificaciones.

## 0. Aprovechar la infraestructura y el código que ya existe

Antes de realizar cualquier modificación, es importante tener en cuenta que gran parte de la infraestructura del juego ya está implementada a nivel de código.

No se debe desarrollar nuevamente desde cero todo lo que ya funciona.

Actualmente ya existen sistemas, abstracciones y lógica para diferentes elementos del juego, como:

* Sistema de personajes.
* Movimiento.
* Saltos.
* Obstáculos.
* Colisiones.
* Interacciones.
* Sistema de escape.
* Sistema de atrapamiento.
* Sistema de monedas.
* Respawn.
* Puntos de control.
* Elementos que el protagonista puede recoger.
* Elementos que puede utilizar posteriormente.
* Lógica de enemigos.
* Animaciones.
* Transiciones.
* Estructura de los niveles.
* Interfaz.
* Desbloqueo de niveles.
* Diferentes abstracciones utilizadas por los niveles existentes.

Todo esto debe reutilizarse a nivel de código.

La idea no es crear sistemas paralelos o duplicar funcionalidades que ya existen.

Lo que se debe hacer es tomar la infraestructura existente y:

**Código existente → reutilizar → adaptar → mejorar → integrar con Sierra.**

No quiero que se elimine la arquitectura actual ni que se vuelva a implementar desde cero una funcionalidad que ya existe.

Si alguna funcionalidad necesita mejoras, se modifica sobre la implementación existente.

También deben mantenerse las abstracciones actuales siempre que sean compatibles con el nuevo diseño.

---

# 0.1. Adaptación visual de las abstracciones al nivel Sierra

Aunque se reutilice la infraestructura de código, visualmente todos los elementos deben adaptarse al nivel Sierra.

Esto incluye:

* Colores.
* Sprites.
* Texturas.
* Elementos del escenario.
* Obstáculos.
* Plataformas.
* Monedas.
* Objetos recogibles.
* Elementos que se puedan atrapar.
* Efectos.
* Partículas.
* Animaciones.
* Indicadores.
* Interfaces.
* Elementos decorativos.

La lógica puede ser la misma, pero la representación visual debe corresponder al nivel Sierra.

Por ejemplo, si existe una abstracción de plataforma en Costa, se puede reutilizar su funcionamiento, pero la plataforma debe tener el diseño, colores y apariencia correspondientes al entorno de Sierra.

Lo mismo debe ocurrir con las monedas, objetos recogibles, obstáculos, enemigos y demás elementos.

**No se debe copiar únicamente la apariencia de Costa.**

Se debe reutilizar principalmente la estructura y funcionalidad a nivel de código y después adaptar visualmente cada elemento al contexto de Sierra.

---

# 1. Transición del primer nivel y entrada al portal

Como mencioné anteriormente, al acercarnos al final del primer nivel, la cámara debe acercarse especialmente al punto donde se realizará la transición.

La cámara debe quedar fija durante ese momento para que la escena tenga mayor control cinematográfico.

El personaje debe entrar hacia ese punto y la cámara debe acompañar la acción de manera profesional.

No quiero simplemente que el personaje corra y que aparezca una pantalla negra.

Debe existir una animación específica relacionada con lo que está sucediendo:

* El protagonista debe tener una animación de entrada.
* Deben mostrarse las expresiones del protagonista.
* La expresión debe corresponder a la situación que está viviendo.
* El personaje debe demostrar mediante su animación que está entrando al portal o zona de transición.
* La cámara debe acompañar la acción y después alejarse de manera progresiva.
* La transición debe sentirse como una escena y no como un cambio de pantalla.

Después, la pantalla debe oscurecerse progresivamente y luego retroceder/revelar la siguiente escena.

En la siguiente escena debe aparecer primero Francisco, el protagonista, saliendo de la puerta o portal.

La salida también debe tener animación.

Por ejemplo:

* El protagonista sale caminando.
* Puede tener una expresión de sorpresa o confusión.
* Puede decir o expresar algo relacionado con dónde se encuentra.
* Después de esa pequeña interacción, debe continuar corriendo.
* A partir de ahí continúa el recorrido, con energía y nuevos obstáculos.

La transición debe sentirse como una continuidad narrativa.

Actualmente lo que sucede es simplemente:

**pantalla negra → aparece el personaje → continúa corriendo → se detiene.**

Eso debe cambiar.

Tiene que existir una escena donde realmente se vea que el protagonista va a entrar al portal, que ocurre la transición y que posteriormente sale por el otro lado.

También puede incorporarse una animación relacionada con la música y con los diferentes elementos que están ocurriendo durante la transición, para que todo esté sincronizado.

La transición debe estar sincronizada entre:

* Animación.
* Cámara.
* Música.
* Efectos.
* Portal.
* Entrada del personaje.
* Salida del personaje.
* Inicio del nuevo recorrido.

---

# 2. Nivel Sierra — Jugabilidad general

En el nivel Sierra se debe utilizar como referencia un juego conocido del género Running, similar al concepto del juego que estamos creando.

También tenemos como referencia el nivel Costa que ya hemos realizado.

El nivel Costa tiene muchos más obstáculos, aunque actualmente todavía puede mejorarse.

Por eso, el objetivo para el nivel Sierra es que sea mucho más interactivo y que también esté relacionado con el ritmo.

Sin embargo, independientemente del ritmo, deben existir suficientes obstáculos para que el nivel sea realmente jugable.

No quiero que el ritmo sea la única mecánica.

Debe existir una combinación entre:

* Ritmo.
* Obstáculos.
* Enemigos.
* Saltos.
* Ataques.
* Movimiento.
* Reacciones.
* Interacciones con el escenario.
* Elementos recogibles.
* Monedas.
* Sistema de escape.
* Sistema de atrapamiento.

Todos estos sistemas deben aprovechar la infraestructura de código que ya existe.

---

# 3. Obstáculos, objetos recogibles y lanzamiento de piedras

En el nivel Sierra deben incorporarse más obstáculos y elementos interactivos.

Por ejemplo:

* Piedras que el protagonista pueda recoger.
* Lanzamiento de piedras.
* Obstáculos físicos.
* Diferentes elementos del escenario.
* Obstáculos relacionados con el terreno.
* Diferentes situaciones que obliguen al jugador a reaccionar.

El sistema debe permitir que el protagonista recoja las piedras y posteriormente pueda utilizarlas para atacar cuando corresponda.

Si ya existe en el código una abstracción para recoger, atrapar o utilizar objetos, debe reutilizarse.

No se debe crear nuevamente un sistema de recogida diferente solamente para Sierra.

Se debe reutilizar el sistema existente y adaptar:

* El objeto.
* El sprite.
* El color.
* La animación.
* La interacción.
* El comportamiento específico de Sierra.

---

# 4. Pájaros y sistema de ataques

También deben existir más aves.

Puede existir una cantidad aproximada de tres aves que interactúen con el protagonista.

Sin embargo, no deben atacar todas al mismo tiempo.

No tendría sentido que las tres aves ataquen simultáneamente.

Debe existir una secuencia de ataque.

Por ejemplo:

1. El ave 1 comienza el ataque.
2. El protagonista empieza a reaccionar o atacar al ave 1.
3. Mientras ocurre esto, el ave 2 empieza a entrar en escena.
4. Cuando el ave 2 comienza su ataque, el ave 3 empieza a incorporarse.
5. Posteriormente puede volver a entrar otra ave o cambiar la secuencia.

Debe existir una especie de intervalo o brecha breve entre ataques para que exista una ventana jugable.

La dificultad debe ser media.

No debe ser:

* Demasiado fácil.
* Demasiado difícil.
* Tres enemigos atacando exactamente al mismo tiempo.
* Ataques completamente predecibles.

Debe sentirse como un nivel diseñado para jugar y reaccionar.

---

# 5. Movimiento aleatorio de las aves

El movimiento de las aves debe ser mucho más variado.

No quiero que todas tengan la misma dirección de movimiento.

Los movimientos deben ser aleatorios y pueden producirse desde diferentes direcciones.

Un ave puede:

* Volar de izquierda a derecha.
* Volar de derecha a izquierda.
* Subir.
* Bajar.
* Entrar desde arriba.
* Entrar desde abajo.
* Realizar movimientos en curva.
* Hacer movimientos diagonales.
* Cambiar de dirección.
* Acercarse desde diferentes posiciones.

Debe existir una combinación de estos patrones.

El movimiento debe ser suficientemente aleatorio para que el jugador no pueda memorizar exactamente dónde aparecerá el ave.

También puede acercarse y generar una situación de peligro.

Pero el ave no necesariamente debe hacer daño directamente.

Si el protagonista choca con un obstáculo o falla una interacción, debe producirse la lógica correspondiente de pérdida y respawn.

La idea es que el jugador tenga que reaccionar constantemente y no simplemente esperar a que el pájaro desaparezca.

---

# 6. Dificultad y puntos de respawn

Deben existir puntos de respawn durante el nivel.

La idea es que el respawn aparezca aproximadamente a la mitad del nivel.

Por ejemplo:

* Si el jugador todavía no ha llegado a la mitad, vuelve al inicio correspondiente.
* Si ya pasó la mitad del nivel, se activa un punto de respawn en esa zona.
* De esta manera, cuando pierda, no tendrá que repetir todo el nivel.

También se puede mantener una lógica similar a la del nivel Costa, donde existe una animación de retroceso cuando el jugador pierde.

Esta lógica debe incorporarse también al nivel Sierra.

El respawn puede estar relacionado con las monedas.

Es decir, cuando el jugador pierda y quiera utilizar un respawn adicional, puede existir la posibilidad de comprarlo utilizando las monedas que haya conseguido durante el nivel.

Debe reutilizarse la lógica de respawn existente y adaptarse a Sierra, no desarrollar un sistema independiente.

---

# 7. Segundo nivel — Puentes

En el segundo nivel, específicamente en la parte de los puentes, actualmente el puente es demasiado pequeño.

Debe tener mucha más jugabilidad.

No quiero que el puente ocupe todo el nivel.

Debe ocupar solamente una parte para mantener variedad y consistencia en el recorrido.

La idea es que exista un recorrido compuesto principalmente por puentes, pero que también existan otros escenarios entre ellos.

Por ejemplo:

* Puede comenzar con una zona diferente.
* Después aparecer un puente.
* Posteriormente puede aparecer otra zona.
* Luego otro puente.
* Finalmente puede existir una transición que permita avanzar hacia el tercer nivel.

De esta manera, el puente es una parte importante del nivel, pero no ocupa absolutamente todo.

---

# 8. Interacción y salto en los puentes

En los puentes debe existir una interacción más interesante.

Puede utilizarse:

* Salto simple.
* Doble salto.
* Plataformas especiales.
* Plataformas que impulsen al protagonista.
* Obstáculos que puedan empujar al protagonista.

La idea es justificar la utilización del doble salto.

Una posibilidad es incorporar plataformas especiales de salto.

Estas plataformas pueden impulsar al protagonista para que pueda superar determinadas zonas sin tocar directamente el suelo o determinadas partes del puente.

Lo importante es que exista una razón jugable para utilizar el doble salto.

No quiero que simplemente se agregue el doble salto porque sí.

Debe estar justificado por el diseño de los obstáculos y las plataformas.

---

# 9. Interacción entre personajes en el segundo nivel

También debe existir una interacción entre personajes.

Debe existir una animación específica cuando el protagonista tenga una reacción.

Por ejemplo:

* Un personaje puede reaccionar.
* El protagonista puede responder.
* Puede existir una animación específica de interacción.
* El segundo personaje puede realizar otra animación.
* Después continúa el recorrido.

La idea es que este nivel también tenga elementos narrativos y no sea únicamente correr y saltar.

---

# 10. Tercer nivel — Mayor personalización

En el tercer nivel hay que realizar un trabajo mucho más personalizado.

Actualmente las alturas de las escaleras o las distancias entre las partes inferiores y superiores están mal proporcionadas.

Las escaleras deben tener una distribución más adecuada.

Los obstáculos para saltar deben tener aproximadamente una proporción relacionada con el cuerpo del protagonista.

No quiero obstáculos excesivamente altos o espacios donde ni siquiera sea evidente que existe que saltar.

Debe existir mucha más variedad.

Por ejemplo:

* Charcos.
* Enemigos.
* Obstáculos de piedras.
* Diferentes partes del escenario.
* Elementos que obliguen a reaccionar.
* Obstáculos más rápidos.
* Obstáculos que aparezcan de manera inesperada.

También puede existir una sección donde el protagonista tenga que correr más rápido.

Esto permitiría que el ritmo del escenario cambie y que el jugador tenga que reaccionar de una manera diferente.

---

# 11. Más pájaros en el tercer nivel

En este nivel pueden aparecer muchos más pájaros.

Por ejemplo:

* Cinco pájaros.
* Siete pájaros.
* Dos pájaros simultáneamente.
* Diferentes combinaciones.

Pero nuevamente debe existir una distribución jugable.

El objetivo no es simplemente llenar la pantalla de pájaros.

El jugador debe poder reaccionar y utilizar la pistola o las herramientas disponibles durante el nivel.

Todo lo que se haya diseñado para interactuar con los enemigos debe poder utilizarse correctamente.

---

# 12. No modificar el fondo

No se debe tocar el fondo actual.

El fondo debe mantenerse como está.

La modificación debe centrarse en:

* Personajes.
* Obstáculos.
* Animaciones.
* Plataformas.
* Enemigos.
* Interacciones.
* Jugabilidad.

Pero el fondo y la ambientación general deben conservarse.

---

# 13. Animaciones tomando como referencia el nivel Costa

Se debe copiar la animación de los personajes que existe en el nivel Costa.

Actualmente el personaje simplemente parece bajar un poco la cabeza, pero eso no es suficiente.

Debe tener la misma calidad de animación que existe en Costa.

En este punto sí quiero que se tome específicamente como referencia el nivel Costa, porque ya existe una base de animaciones.

El nivel Sierra debe utilizar esa base, pero mejorándola.

Por lo tanto:

**Nivel Costa = referencia/base de animaciones.**

**Nivel Sierra = misma base + mejoras + nuevas animaciones.**

No se trata de copiar visualmente todo el nivel Costa, sino de utilizarlo como referencia para mantener consistencia entre los niveles.

---

# 14. Diálogos y frases

El nivel Sierra también necesita más frases.

Actualmente hay algunas frases y ejemplos que aparecen al final de Costa.

Quiero que el nivel Sierra tenga más frases durante su recorrido y que estas estén relacionadas con lo que está ocurriendo.

También pueden incorporarse frases específicas al momento de:

* Entrar al nivel.
* Encontrar determinados obstáculos.
* Llegar a una parte importante.
* Interactuar con personajes.
* Entrar al portal.
* Terminar el nivel.

Al final del nivel Sierra debe existir una recompensa relacionada con un traje de guerrilla.

Este traje permitirá posteriormente entrar o continuar hacia el nivel Costa.

Debe existir una conexión narrativa entre ambos niveles.

Sin embargo, visualmente debe mantenerse parecido y consistente con lo que ya tenemos.

---

# 15. Personajes

Todos los personajes deben ser mucho más profesionales.

Esto incluye:

* Diseño.
* Animaciones.
* Expresiones.
* Movimiento.
* Reacciones.
* Interacciones.
* Animaciones específicas para cada situación.

Cada personaje debe sentirse integrado al nivel.

No deben parecer simplemente sprites colocados en el escenario.

Las animaciones deben estar diseñadas específicamente para estos niveles.

---

# 16. Referencias para elementos del escenario

También se pueden buscar referencias visuales de Internet para determinados elementos que se puedan incorporar.

Por ejemplo:

* Piedras.
* Plataformas.
* Obstáculos.
* Elementos naturales.
* Objetos del escenario.

Pero todas estas referencias deben adaptarse al estilo de **pixel art** que estamos utilizando.

No quiero elementos que rompan el estilo visual del juego.

Todo debe verse como parte del mismo universo gráfico.

---

# 17. Sistema de monedas

También se debe copiar el sistema de monedas del nivel Costa/Nivel 1.

El sistema debe mantenerse consistente a nivel de código.

Las monedas deben poder recogerse durante el recorrido.

Estas monedas posteriormente pueden utilizarse para determinadas funciones, incluyendo la posibilidad de comprar un respawn cuando el jugador haya perdido, según la lógica que ya existe.

No se debe crear un sistema nuevo de monedas.

Se debe reutilizar el sistema actual y adaptarlo visualmente al nivel Sierra.

---

# 18. Interfaz superior — Monedas y elementos del HUD

La interfaz que aparece durante el juego también debe reutilizarse.

Específicamente, la franja superior donde actualmente aparecen las monedas y demás indicadores debe mantenerse estructuralmente igual a la de Costa.

No se debe crear otro HUD completamente diferente.

Debe reutilizarse:

* La estructura.
* La posición de los elementos.
* La lógica.
* Los indicadores.
* El sistema de monedas.
* Las referencias de código.
* Las animaciones existentes del HUD cuando corresponda.

Pero visualmente debe adaptarse al nivel Sierra.

Es decir:

**HUD de Costa → misma estructura y código → colores y estética de Sierra.**

Los colores, fondos, bordes, iconos y demás elementos visuales deben combinar con la ambientación de Sierra.

La interfaz debe sentirse perteneciente al nivel Sierra, pero debe mantener la consistencia con el resto del juego.

No quiero que parezca una interfaz nueva completamente independiente.

---

# 19. Pantalla de ESC / pausa

Lo mismo debe aplicarse a la pantalla que aparece al presionar **ESC**.

No se debe crear una pantalla de pausa completamente diferente desde cero.

Debe reutilizarse la interfaz de pausa existente en Costa a nivel de estructura y código.

Posteriormente debe adaptarse visualmente a Sierra mediante:

* Colores.
* Fondos.
* Bordes.
* Iconos.
* Tipografía si corresponde.
* Elementos decorativos.
* Estética general.

Debe conservarse la misma funcionalidad que ya existe.

La única diferencia principal debe ser la identidad visual correspondiente a Sierra.

---

# 20. Final del nivel Sierra — Pantalla de victoria

Al finalizar el nivel Sierra debe existir una escena claramente diferenciada para indicar que el jugador ha completado el nivel.

No quiero que simplemente termine el recorrido y continúe directamente al siguiente nivel.

Debe existir una pantalla o escena de victoria integrada visualmente con el nivel.

La transición debe utilizar una iluminación o efecto visual que permita indicar que el jugador ha ganado.

Actualmente, al final del nivel Sierra, la idea es que el protagonista quede en la zona central.

A partir de ese punto debe producirse una transición hacia la pantalla posterior.

La secuencia puede ser:

1. El protagonista llega al punto final.
2. Se detiene en la zona central.
3. Se activa una animación de finalización.
4. Se incorpora iluminación o un efecto visual alrededor del protagonista.
5. La escena comienza a cambiar progresivamente.
6. Aparece la interfaz de finalización del nivel.
7. Se muestra que el nivel Sierra ha sido completado.
8. Se muestra el siguiente nivel como bloqueado o disponible según corresponda.
9. Se muestra la recompensa obtenida.
10. Se muestran las monedas obtenidas.
11. Se muestra el progreso del jugador.

---

# 21. Interfaz final de victoria

La pantalla final de victoria también debe reutilizar la interfaz que ya existe en Costa.

No quiero crear una interfaz completamente diferente.

Debe tomarse como referencia la pantalla final de Costa y reutilizar su estructura, lógica y componentes a nivel de código.

Después se debe adaptar visualmente al nivel Sierra.

Es decir:

**Interfaz final de Costa → misma estructura/código → adaptación visual a Sierra.**

Los elementos visuales deben utilizar los colores y estética correspondientes a Sierra.

Debe existir coherencia entre:

* HUD durante el nivel.
* Pantalla de ESC.
* Pantalla de victoria.
* Pantalla de desbloqueo.
* Menús relacionados con el nivel.

Todo debe parecer parte del mismo nivel.

La interfaz final no debe sentirse como una pantalla externa que aparece de repente.

Debe estar integrada visualmente con Sierra.

---

# 22. Pantalla posterior y desbloqueo del siguiente nivel

Después de completar Sierra, debe aparecer la pantalla posterior correspondiente.

En esta pantalla debe poder verse claramente:

* Que el nivel Sierra fue completado.
* Las monedas obtenidas.
* La recompensa.
* El traje de guerrilla obtenido, si corresponde.
* El progreso.
* El siguiente nivel.
* El estado bloqueado/desbloqueado del siguiente nivel.

El siguiente nivel puede aparecer bloqueado inicialmente y cambiar su estado después de completar Sierra.

La interfaz debe conservar la lógica de desbloqueo que ya exista en el código.

No se debe crear un sistema nuevo de desbloqueo si ya existe uno.

Simplemente debe integrarse correctamente con Sierra.

---

# 23. Integración completa con la infraestructura existente

Todo el nivel Sierra debe construirse sobre la infraestructura que ya existe.

La implementación debe respetar la arquitectura actual del proyecto.

Se debe revisar primero cómo están implementados los niveles existentes y posteriormente reutilizar:

* Clases.
* Componentes.
* Sistemas.
* Abstracciones.
* Controladores.
* Animaciones.
* Sistemas de monedas.
* Sistema de respawn.
* Sistema de colisiones.
* Sistema de enemigos.
* Sistema de objetos.
* Sistema de interacción.
* Sistema de transiciones.
* Sistema de desbloqueo.
* HUD.
* Pantalla de ESC.
* Pantalla final.
* Interfaces existentes.

Después de reutilizarlos, se deben configurar específicamente para Sierra.

No quiero una implementación aislada.

Quiero que Sierra forme parte del mismo sistema del juego.

---

# 24. Regla visual general

Todo lo que ya funciona en Costa debe servir como referencia y base para Sierra.

Sin embargo, Sierra debe tener su propia identidad visual.

Por lo tanto:

**Costa = base funcional y visual.**

**Sierra = reutilización de la base + adaptación + mejoras + identidad propia.**

Esto aplica a:

* Personajes.
* Animaciones.
* HUD.
* Monedas.
* Pantalla de ESC.
* Pantalla final.
* Transiciones.
* Obstáculos.
* Efectos.
* Interacciones.

No se debe modificar el fondo actual de Sierra para solucionar estos puntos.

El fondo debe mantenerse y los nuevos elementos deben integrarse alrededor de él.

---

# 25. Objetivo general

El objetivo final es que el nivel Sierra se sienta como un nivel completo, profesional y perteneciente al mismo juego.

Debe existir:

* Más obstáculos.
* Más enemigos.
* Más aves.
* Más interacciones.
* Más animaciones.
* Más diálogos.
* Más situaciones de juego.
* Más variedad de movimientos.
* Más variedad de escenarios.
* Mejor utilización de las herramientas del protagonista.
* Sistema de respawn.
* Sistema de monedas.
* HUD adaptado.
* Pantalla de ESC adaptada.
* Pantalla final adaptada.
* Transiciones cinematográficas.
* Mejor interacción entre personajes.
* Mayor variedad de saltos.
* Mayor dificultad progresiva.
* Movimiento aleatorio de enemigos.
* Sincronización con el ritmo.
* Una transición profesional hacia los siguientes niveles.
* Pantalla de victoria.
* Sistema de desbloqueo.
* Recompensas.
* Interfaz final integrada al nivel.

Pero todo esto debe hacerse **sin perder la identidad visual actual**, especialmente el fondo y el estilo de pixel art.

La referencia principal para las animaciones debe ser el nivel Costa, mientras que la jugabilidad debe tomar ideas de juegos conocidos del género Running y adaptarlas al sistema que estamos creando.

El nivel Sierra debe sentirse como una evolución del nivel Costa: mantener la base que ya funciona, pero agregar mucha más interacción, variedad, dificultad, animaciones y elementos narrativos.

**La regla principal de implementación es: reutilizar la infraestructura y el código existente, no rehacerlo desde cero. Adaptar todo lo que ya funciona al contexto, estética, obstáculos, personajes, animaciones, interfaces y narrativa del nivel Sierra.**
