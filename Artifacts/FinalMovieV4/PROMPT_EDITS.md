# Prompts de corrección y procedencia

Herramienta usada: generación/edición de imágenes integrada, no CLI/API externa. Los prompts de producción están en PLAN_COMPLETE.json; Sources/ contiene las fuentes finales copiadas al proyecto. Las nueve capas y los efectos heredados pertenecen al banco cinematográfico V3, no al gameplay.

Corrección de transparencia (Costa, Sierra y Selva): extraer el fondo coloreado, preservar exactamente24poses, anatomía, cuerdas y ropa; alfa transparente auténtico, sin mate ni halo. Exportación mecánica posterior con cuantización a la paleta existente y alfa binario.

Corrección Sierra mate: cambiar solamente el acabado a textil seco, terracota/ocre apagado, piel cálida mate y botas mates. Mantener24poses, cuadrícula4x6, silueta, proporciones, manos y cuerdas. Quitar puntos de brillo especular sin adelgazar/engordar al actor ni cambiar su identidad. Transparencia real, sin escena, halo, texto o rejilla. La exportación omite cuatro tonos pálidos existentes para reforzar ese acabado; no introduce otra paleta.

Fuentes recuperadas de la generación ya realizada, antes de continuar:

- epilogue_sky: exec-23bdf2cd-ab3f-42ab-96cc-dade9d901439.png
- epilogue_mid: exec-36b0f8cd-f9e9-4ca5-bc22-451670f74d60.png
- epilogue_ground: exec-bc861b8c-1891-43d4-ae8d-5a6582450102.png
- epilogue_portal_back: exec-0228a5d0-729c-490d-a903-4e7df348ae23.png
- epilogue_portal_front: exec-1d2c2247-9611-474f-aaad-388354b675c0.png
- epilogue_hero: exec-c524df5a-329e-4bb0-a44e-d6d06edcf847.png
- sierra_hero mate: exec-fa92b233-6d94-4445-9292-3daccc32a255.png

La referencia gemini_generated_video_6aa99b98.mp4 se estudió mediante muestras guardadas en ReferenceReview/. No se usó su personaje ni su metraje como asset final.

## Identidad y acabado finales,24/10/2026

Costa: reconstruir24poses4x6 usando Sierra y Selva como referencias estrictas de rostro humano y anatomía. El atlas Costa previo aporta solo ropa naranja/ocre y orden de acciones, nunca la cabeza gigante o cara enmascarada. Mantener rostro humano visible, ojos oscuros, nariz/boca/mejillas, proporciones similares a Tayta de Sierra/Selva, piernas humanas, poncho compacto y ambos brazos ocultos. Ocho poses de carrera, cuatro de preparación/impulso, cuatro de vuelo, cuatro de contacto/recuperación y cuatro de deslizamiento. Pixel art16/32bit, materiales mates, fondo transparente, sin rejilla, letras, blur o brillo metálico. Fuente final: exec-dbd57ce3-e9fd-48fa-be92-5175e31115c3.png.

Sierra: preservar exactamente24poses4x6, dimensiones, rostro, manos y cuerdas. Cambiar solo color/material: piel humana neutra terracota/tan/marrón como Selva, nunca dorada; chullo/poncho rojo ladrillo mate, bordado ocre discreto, pantalón azul y botas marrones mates. Sin reflejos blancos, halo amarillo, sudor plástico o bloom. Fondo transparente, sin texto/rejilla. Fuente final: exec-7b94f09f-28a9-4232-8a5c-c1f87688955e.png.

Respaldo anterior: BeforeIdentityFix/. El plano inicial pasa de zoom2.05a1.08→1.0; escala de acción de cuerpo completo68unidades lógicas por celda, sin escaladoXindependiente. Primeros planos y plano general de victoria siguen teniendo su propio encuadre deliberado.
