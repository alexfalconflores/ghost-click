# Histograma de intervalos

**Estado:** Propuesta · **Área:** Resultados · **Prioridad:** Media · **Creada:** 2026-11-23

## Problema
La lista de intervalos es difícil de leer y no ayuda a elegir el umbral.

## Propuesta
Gráfico de barras con la distribución de intervalos entre clics:
- Los clics fantasma aparecen como un grupo separado a la izquierda (típicamente < 30 ms).
- Línea vertical en el umbral actual.
- Opción "Ajustar umbral a mis datos": propone un valor en el hueco entre ambos grupos.

## Implementación
Reactor incluye charting (port de D3): ver `skills/charts.md` del repo de Reactor.