# Selva: cierre musical y museo — 02/10/2026

- Río: el final del tramo musical (90 s de la canción global) inicia la salida con fundido, aunque el jugador haya tardado balanceándose. Ya no se pausa la música manteniendo el recorrido activo.
- Todos los biomas de carrera concluyen con la sección de canción asignada. Mininiveles conservan sus pistas independientes y salida temprana de 3 s si se completan objetivos.
- Portal final: reposicionado tres segundos de recorrido antes del final; el héroe se detiene en su centro y no lo atraviesa. La zona próxima al cierre queda libre de obstáculos.
- Celebración: nuevo sprite `selva_heroe_baile`, 8 frames de 48x48. Conserva rostro/traje, piernas alternadas y brazos que acompañan el ritmo. Se reproduce antes de resultados, sin refactorizar el protagonista.
- Música final: descenso gradual de volumen durante los últimos 1.4 s y resultados después de la celebración. No se crea una segunda canción ni se modifica el MP3.
- Museo de Selva: cuatro sprites nuevos de 64x64, sufijo `_v3`, transparencia binaria, paleta existente de 28 colores, sin antialiasing. MuseumArt prioriza estas versiones únicamente para Selva. Costa y Sierra intactos; originales de Selva conservados.
- Autorización de lote recibida. Integrados para prueba; no se declara aprobación artística automática ni igualdad de calidad demostrada sin revisión del usuario.

## Referencias y límites culturales

- Cerámica: [Conibo jar, Smithsonian/NMAI](https://americanindian.si.edu/exhibitions/infinityofnations/amazon/181949.html). La pieza referida fue recogida en Amazonas, Brasil: se usa para estudiar forma y superficie, no para afirmar que es un hallazgo arqueológico peruano.
- Textil: [Woman's Wrapper, Shipibo, The Metropolitan Museum of Art](https://www.metmuseum.org/art/collection/search/829475). Algodón y patrones pintados; el patrón del sprite es una interpretación, no una reproducción del tejido inventariado.
- Flauta: [registro de flauta de Loreto, NMAI](https://americanindian.si.edu/collections-search/object/NMAI_218689). La ficha se consultó por su procedencia; no se ha certificado una reconstrucción exacta del instrumento. Mantiene identificación de objeto original del viajero.
- Sonaja: [referencia visual de sonaja contemporánea de Pucallpa](https://hanaqpacha.patternbyetsy.com/listing/1747624101/shipibo-gourd-rattle-from-the-peruvian). Se estudian calabaza, mango y cordón; no se incorporan afirmaciones comerciales sobre propiedades curativas. La pieza sigue siendo original del juego.

Generación nativa autorizada por ART_BIBLE §13: `Tools/build_selva_museum_finish.py`. Preview: `Artifacts/SelvaV2/museo_selva_v3_x4.png`. Los sprites nuevos cumplen paleta/alfa; falta revisión visual humana y comprobación de oído de los fundidos.
