# Sueltas fantasma (rebote al soltar)

**Estado:** Propuesta · **Área:** Detección · **Prioridad:** Alta · **Creada:** 2026-11-23

## Problema
Un interruptor desgastado también **suelta el botón solo** mientras el usuario lo mantiene pulsado. Síntoma típico: al arrastrar una ventana o un archivo, se "cae" a mitad de camino. Hoy la app no lo detecta.

## Propuesta
Detectar una secuencia *soltar → pulsar* del mismo botón con muy poco tiempo entre ambos (p. ej. < 50 ms): el usuario no levantó el dedo, el interruptor rebotó.

- Modo "Mantén pulsado": el usuario mantiene cada botón unos segundos (o arrastra un elemento) y la app cuenta las interrupciones.
- También detectarlo en la prueba normal y contarlo aparte de los clics fantasma.

## Implementación
- Guardar `LastReleaseTimestamp` en `ButtonStats`.
- En `OnButtonDown`: si `timestamp - LastReleaseTimestamp` < umbral de suelta → contar como suelta fantasma (y **no** como clic fantasma, para no contar dos veces el mismo fallo).
- En `OnButtonUp`: recibir el `timestamp` (hoy no lo recibe).

## Preguntas abiertas
- ¿Umbral propio configurable o derivado del de clic fantasma?
- ¿Modo dedicado con instrucciones, o solo contarlo en la prueba normal?