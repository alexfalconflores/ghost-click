# Velocidad de doble clic de Windows

**Estado:** Propuesta · **Área:** Configuración · **Prioridad:** Baja · **Creada:** 2026-11-23

## Propuesta
Mostrar en Configuración, junto al umbral, el valor de Windows como contexto:
"Windows considera doble clic hasta 500 ms. Tu umbral de clic fantasma es 80 ms."

## Implementación
Win32 `GetDoubleClickTime()` (user32) vía P/Invoke o CsWin32.