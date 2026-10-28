# SUYURUN — final con banco visual exclusivo

Pedido22/10/2026: no reutilizar el arte ni los ciclos del gameplay para la película. Se mantienen la identidad de Tayta, las paletas regionales, la canción elegida y los créditos solicitados. No se altera el juego principal ni los guardados.

## Banco nuevo

18hojas generadas con la herramienta integrada: nueve capas independientes (cielo, distancia y suelo por región), tres hojas de acción16poses cada una, primer plano12poses, efectos16celdas, recuerdos12celdas y tres hojas ambientales8/8/16frames. Total129celdas incluyendo las9capas estáticas. Fuentes generadas y prompts en Sources y PLAN_COMPLETE.json. Exportación256×256 por pose; capas2560×720. Paletas originales24colores, filtroPoint, sin mipmaps/compresión. Registro mecánico nearest-neighbor; el tamaño exportado no equivale a detalle original en cada píxel.

Las correcciones generadas incluyen quitar fondo opaco en Sierra, sustituir bolsa excesiva por bolsillo plano de waraca y quitar la liana dibujada junto a la mano para usar una sola cuerda animada. La exportación detecta los márgenes reales entre filas y usa una escala común, para no cortar botas ni pegar fragmentos del frame anterior. Apoyo y agarre tienen coordenadas por pose.

## Montaje

0–3s expresión/respiración/mirada;3–7Costa carrera/salto/aterrizaje/deslizamiento;7–11Sierra carga/giro/lanzamiento/recuperación;11–15Selva balanceo/suelta/caída/aterrizaje/disparo;15–17.5recuerdos realmente recogidos;17.5–20despedida/título;20–30créditos. Parallax, aves, oleaje, vegetación, mariposa y luciérnagas con animación propia. Música continua en loop y fundidos, reloj DSP único. Fondo detenido durante preparación de la huaraca, trayectoria continua desde el último agarre, movimiento de cámara compartido por todos los planos.

FinalMovieArt carga SOLO FinalMovieV3: sin fallback a gameplay, MuseumArt, TaytaArt, AtmosphereClean o bancos anteriores. Fuente pixel compartida y canción anterior se conservan por coherencia y petición expresa, no son arte visual nuevo. Los dibujos de patrimonio son interpretaciones estilizadas, no réplicas históricas documentadas.

## Prueba y límites

Auditoría de contratos: Tools/audit_final_bank.py. Compilación: Tools/compile_refactor_check.ps1. Captura automatizada real en Unity Play: FinalMoviePlayAudit.Run, con resultados e imágenes en PlayAudit; revisar la fecha del resultado de cada reproducción. La auditoría no sustituye aceptación visual del usuario ni garantiza ausencia absoluta de bugs. No se anuncia realismo fotográfico ni animación dibujada a24fps: el estilo sigue siendo pixel art con poses clave y ciclos del banco nuevo.

Previsualizar: entrar en Play y elegir SUYURUN > Final > Previsualizar pelicula (Play). El final natural continúa conectado al cierre de Selva. Versiones anteriores respaldadas, sin ejecutable nuevo y sin push automático de este avance.

Resultado de la revisión final:18hojas/129celdas verificadas, compilación runtime/Editor PASS y reproducción aislada completa de30s en Unity6000.3.17f1, cero errores de ejecución,17capturas1280×720. No equivale a haber recorrido nuevamente los tres niveles principales. Prueba y límites exactos en PLAY_VERIFICATION.json.
