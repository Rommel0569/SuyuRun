# Selva: entrega artística, no integración

Dirección del usuario: completar el lote de arte manteniendo EXACTAMENTE el
lenguaje pixel de Costa/Sierra. Cueva y ruinas pasan a ser mininiveles laterales.
Lianas con agarre, péndulo, salto de transferencia y caimanes debajo.

Costa y Sierra cerrados por prueba/aprobación explícita. No se tocan.

Paquete: `Artifacts/SelvaArtPack_v1/SUYURUN_Selva_Arte_v1.zip`.
Galería: `Artifacts/SelvaArtPack_v1/Package/index.html`.
Brief/inventario/limitaciones: README y manifiesto dentro del paquete.

Seis ambientes: bosque, río, cueva, ruinas, noche y templo final fantástico.
Originales nativos RGBA,24colores de la propuesta existente; sprites/frames con
medidas explícitas, fondos independientes y repetibles, ejemplos animados.
PNG alojados dentro del paquete (fuera de Assets activo), no se sobreescriben
archivos de Costa/Sierra. Scripts de generación en Tools/build_selva_artpack.py
y Tools/preview_selva_artpack.py; ZIP con package_selva_artpack.py.

No usar la primera imagen imagegen texturizada como guía; se descartó al comparar
con Costa/SierraV3. Se reutiliza su construcción nativa de color y silueta.
Paleta y arte nuevos pendientes de aprobación. El traje conserva el diseño
que ya se recibe en Sierra, no se sustituye por un personaje nuevo.

Próximo paso tras aprobación: importar/slicing/clips en Unity y recién después
construir escenas, física de lianas, encuentros y mininiveles. Investigar fichas
de museo y escoger música de cueva/ruinas antes de presentar contenido histórico.
