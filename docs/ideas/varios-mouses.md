# Identificar varios mouses

**Estado:** Propuesta · **Área:** Alcance · **Prioridad:** Baja · **Creada:** 2026-11-23

## Propuesta
Distinguir qué dispositivo hizo cada clic (p. ej. touchpad del portátil vs. mouse externo) y mostrar resultados por dispositivo.

## Implementación
Raw Input de Win32 (`RegisterRawInputDevices`, `WM_INPUT`): da el handle del dispositivo; nombre vía `GetRawInputDeviceInfo`.

## Relación
Combina con el [historial de sesiones](historial-de-sesiones.md): evolución por dispositivo. Aquí sí podría justificarse SQLite.