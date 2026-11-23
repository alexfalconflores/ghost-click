# Detección en segundo plano

**Estado:** Propuesta · **Área:** Alcance · **Prioridad:** Baja · **Creada:** 2026-11-23

## Propuesta
La app vive en la bandeja del sistema y avisa (notificación) si detecta clics fantasma mientras el usuario usa el PC con normalidad.

## Implementación
- Hook global de ratón Win32 `WH_MOUSE_LL`.
- Icono de bandeja: Reactor tiene integración con la shell (ver docs de Windows / shell integration).

## Consideraciones
- Salto técnico grande; revisar privacidad (solo tiempos, nunca posiciones ni ventanas).
- Requisitos de la Store para hooks globales.
- Arranque con Windows (opcional, desactivado por defecto).