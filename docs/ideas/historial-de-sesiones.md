# Historial de sesiones

**Estado:** Propuesta
**Área:** Datos / Diagnóstico
**Creada:** 2026-11-23

## Problema
Hoy cada prueba se pierde al pulsar Reiniciar. El usuario no puede saber si su mouse está empeorando con el tiempo, que es justo lo que necesita para decidir si cambiarlo.

## Propuesta
Guardar un resumen por sesión y mostrar una comparación entre sesiones.
Ejemplo: "La semana pasada: 3 clics fantasma de 200. Hoy: 15 de 200".

Datos por sesión:
- Fecha y hora
- Clics totales
- Clics fantasma
- Botón probado (izquierdo / derecho)
- Intervalo mínimo registrado
- Umbral usado (ms)

## Decisión técnica
1. **Fase 1 — JSON** en `ApplicationData.LocalFolder` (o `UsePersisted`).
   - Pocos registros al mes; no justifica una base de datos.
   - `ClickState` ya es un `record` inmutable → se serializa casi directo.
   - Usar `System.Text.Json` con source generator (compatible con AOT/trimming).
2. **Fase 2 — SQLite** (`Microsoft.Data.Sqlite`) solo si:
   - Se guarda cada clic individual (miles de filas por sesión), o
   - Se identifican varios mouses por nombre/modelo, o
   - Se necesitan consultas (filtrar por fechas, comparar dispositivos).

Mantener el acceso a datos detrás de una interfaz (`ISessionStore`) para poder pasar de JSON a SQLite sin tocar la UI.

## Preguntas abiertas
- ¿Guardar automáticamente al reiniciar, o con un botón "Guardar sesión"?
- ¿Cuántas sesiones conservar? (¿últimas 50? ¿90 días?)
- ¿Página nueva "Historial" o sección dentro de Main?
- ¿Exportar a CSV?

## Relacionado
- Umbral configurable (hoy fijo en 80 ms)
- Distinguir clic izquierdo / derecho