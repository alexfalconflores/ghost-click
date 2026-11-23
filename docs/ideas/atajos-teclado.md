# Atajos de teclado

**Estado:** Propuesta · **Área:** Usabilidad · **Prioridad:** Baja · **Creada:** 2026-11-23

## Propuesta
- `Ctrl+R` → Reiniciar.
- `Ctrl+,` → Configuración.
- (Futuro) `Ctrl+C` → Copiar resultados.

## Implementación
`Command` de Reactor con `Accelerator(VirtualKey.R, VirtualKeyModifiers.Control)`. Ver `skills/commanding.md`: el accelerator debe estar en un ámbito que cubra toda la ventana (`CommandHost` o `MenuBar`/`CommandBar` en la raíz).