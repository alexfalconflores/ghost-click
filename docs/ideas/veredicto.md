# Veredicto claro

**Estado:** Propuesta · **Área:** Resultados · **Prioridad:** Alta · **Creada:** 2026-11-23

## Problema
El usuario ve contadores y tiene que interpretarlos. La pregunta real es "¿mi mouse está mal?" y la app no la responde.

## Propuesta
Mostrar un resultado por botón y uno general, basado en el **porcentaje** de clics fantasma (y sueltas fantasma, cuando exista):

| Resultado | Criterio (orientativo) |
|---|---|
| ✅ Está bien | 0 % |
| ⚠️ Revisa el botón X | > 0 % y < 5 % |
| ❌ Conviene cambiarlo | ≥ 5 % |

- Solo mostrar veredicto con un **mínimo de clics** (p. ej. 50 por botón); antes, "Sigue haciendo clic…".
- Usar `InfoBar` con `Severity` (Success / Warning / Error) → colores del sistema.

## Preguntas abiertas
- ¿Los porcentajes son razonables? Validar con mouses reales.
- ¿Recomendar limpieza / cambio de interruptor además de "cambiar el mouse"?