# Clics combinados (varios botones a la vez)

Estado: ✅ Implementada (2026-11-23)
**Área:** Detección
**Creada:** 2026-11-23


## Problema
WinUI solo lanza `PointerPressed` con el **primer** botón que se pulsa. Si el usuario mantiene pulsado el botón izquierdo y hace clic con el derecho (o al revés), WinUI lo trata como un movimiento del puntero (`PointerMoved`) y ese segundo clic **no se cuenta**.

En el uso normal no se nota, pero:
- Un clic fantasma que ocurra mientras el otro botón está pulsado se pierde.
- Impide probar los dos botones a la vez.

## Propuesta
Escuchar también `OnPointerMoved` y detectar el cambio de estado de cada botón con `PointerPoint.Properties.PointerUpdateKind`:
- `LeftButtonPressed` / `RightButtonPressed` → registrar el clic como hoy.
- Ignorar los movimientos sin cambio de botón (`PointerUpdateKind.Other`).

Mantener la misma función pura `OnClick(state, button, timestamp, thresholdMs)`; solo cambia de dónde llegan los eventos.

## Consideraciones
- `PointerMoved` se dispara muchísimo: filtrar pronto para no sobrecargar el reducer.
- Llamar a `CapturePointer` al pulsar para no perder eventos si el puntero sale de la zona de prueba con un botón pulsado.
- Probar con mouse de 5 botones: decidir si los laterales (X1/X2) se cuentan o se siguen ignorando.


## Decisiones
- **Botones laterales y rueda: sí.** Se detectan los 5 botones: izquierdo, central, derecho, atrás (X1) y adelante (X2).
- **Indicador visual: sí.** Una tarjeta por botón con un punto que se enciende mientras está pulsado, más sus contadores.

## Implementación
- `OnPointerPressed` + `OnPointerMoved` + `OnPointerReleased` comparten un manejador; `PointerUpdateKind` indica qué botón cambió.
- `CapturePointer` al pulsar; `OnPointerCaptureLost` limpia el estado de pulsado.
- `e.Handled = true` para que "atrás" no navegue y el clic derecho no abra menús.
- Reducer: `OnButtonDown` (cuenta y mide) y `OnButtonUp` (solo actualiza el indicador).