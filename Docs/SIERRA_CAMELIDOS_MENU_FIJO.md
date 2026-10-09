# Camélidos pastando / menú fijo

- Un atlas adicional, 384×64, 24frames nativos32×32. Llama clara de cuello largo
  y alpaca marrón de cuello más corto y lana. Bajan el cuello, mastican y vuelven
  a levantarlo; las patas permanecen ancladas. Ciclos desfasados entre animales.
- Fuente editable: Tools/draw_sierra_grazers.py, reutiliza helpers/paleta SierraV3.
  No se regeneraron paisajes ni se sobrescribieron llamas anteriores.
- Menú y selector: offset horizontal constante en cada capa. Agua/vegetación y
  camélidos conservan sus frames; no se desplaza lateralmente el panorama.
- Sierra: Camino y Ruinas, sin introducir animales en los puentes ni las cuevas.
  Decorativos, no enemigos ni obstáculos nuevos. Surface fija los pies al terreno.
  Anclajes absolutos cada640px; el descarte considera32px completos del sprite.
  Sin temporizador de desaparición y sin wrap de un animal dentro de pantalla.
- QA: Artifacts/SierraGrazers/play-check.txt, capturas inicio-1/inicio-2/Camino/Ruinas.
  La prueba comprueba frames/importación y bordes, no sustituye aprobación visual.
- Selva sigue pendiente: esta entrega no genera sus assets ni modifica su plan.
