# Prueba de la rueda (scroll que salta)

**Estado:** Propuesta · **Área:** Detección · **Prioridad:** Media · **Creada:** 2026-11-23

## Problema
Avería común: al girar la rueda hacia abajo, de vez en cuando la página sube un paso (o al revés). El encoder está sucio o desgastado.

## Propuesta
- Escuchar `OnPointerWheelChanged` en la zona de prueba.
- Detectar **cambios de dirección aislados**: un paso en sentido contrario rodeado de pasos en el mismo sentido, en poco tiempo.
- Mostrar contadores: pasos arriba, pasos abajo, saltos detectados.
- Indicador visual de la dirección del último paso.

## Consideraciones
- Los mouses con rueda libre/de alta resolución envían deltas distintos a 120: normalizar.
- `e.Handled = true` para que la zona no haga scroll de la página.