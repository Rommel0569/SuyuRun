# Paquete completo de biblioteca: base + refactor

Pack_Arte_Completo_Base_y_Refactor.zip tiene dos secciones explícitas:

- BaseJuego: copia íntegra de Assets/SuyuRun/Art actual, incluyendo metadata
  de Unity, animaciones y controles que ya existen. NO es arte nuevo ni un
  refactor completo de mayor calidad. No sobrescribir el proyecto con esta copia.
- Refactor: banco nuevo de 91 hojas / 418 frames, catálogo de importación,
  prompts e importador de staging. PNG/JSON/GIF por secuencia.

Este es un paquete completo de la biblioteca disponible, NO una afirmación de
que todos los personajes ya tienen terminado el nuevo refactor visual.
Faltan los estados extra/recompensas Costa/Sierra, esbirros, aves, caimanes,
efectos adicionales, registro corporal y vinculación de animaciones al gameplay.

BaseJuego conserva el arte de estos personajes pendientes para no omitirlos
del paquete ni reemplazarlos por ampliaciones artificiales. Versiones originales
siguen en el proyecto. No integrado ni probado en Unity Play en este avance.

La validación automática comprueba celdas, alpha, paleta <=24 colores,
márgenes, duración y hashes. NO verifica anatomía, sombreros reconstruidos,
manos/armas, tamaño percibido, balanceo, colisiones o eventos de daño.

ImportVisualRefactorBank.cs compiló con las referencias del Editor6000.3.17f1.
No se ha ejecutado el menú. Al ejecutarlo crea solo RefactorStaging, sin
modificar escenas, Resources activos, progresión, audio o física.
