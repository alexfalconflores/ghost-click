# Ghost Click — Funcionalidades

Estado actual de la app. Última actualización: 2026-11-23.

## Qué hace
Detecta si el mouse registra clics que el usuario no hizo ("clics fantasma"): dos pulsaciones del **mismo botón** separadas por menos tiempo que el umbral configurado. Es el síntoma típico de un interruptor desgastado.

## Prueba de clics (`MainPage`)
- **Zona de prueba**: naranja mientras todo va bien; se vuelve roja al detectar el primer clic fantasma y se queda así hasta reiniciar.
- **5 botones detectados**: izquierdo, central (rueda), derecho, atrás y adelante. Cada uno tiene sus propios contadores; alternar botones rápido **no** cuenta como fallo.
- **Clics combinados**: se registran también los botones pulsados mientras otro sigue pulsado.
- **Indicador en vivo**: una tarjeta por botón con un punto que se enciende mientras está pulsado.
- **El primer clic de cada botón no genera intervalo.**
- **Medición precisa** con `PointerPoint.Timestamp` (microsegundos del propio evento).
- **Historial de intervalos** con el nombre del botón y una marca si fue clic fantasma.
- **Umbral visible** en la cabecera.
- **Reiniciar** borra contadores, historial, indicador y color.
- Dentro de la zona de prueba, "atrás" no navega y el clic derecho no abre menús.

## Configuración (`SettingsPage`)
- **Tema**: sistema, claro u oscuro; se guarda entre sesiones.
- **Umbral de clic fantasma**: 20–500 ms (predeterminado 80 ms). Se guarda en `LocalSettings`.
- **Acerca de**: versión, arquitectura del proceso, enlaces a redes, comentarios, valorar en la Store y otras apps del desarrollador.
- **Bibliotecas de código abierto** usadas.

## Diálogos de inicio
- **Bienvenida** en la primera ejecución, con instrucciones y el umbral actual.
- **Novedades** tras cada actualización, con el número de versión.
- Se muestran una sola vez por ejecución; la última versión vista se guarda en `LocalSettings`.

## Idiomas
- Inglés (predeterminado) y español; se elige según el idioma de Windows.
- Textos en `Strings/<idioma>/` separados por área: `Common`, `Main`, `Settings`, `Onboarding`.
- Las claves se declaran en `Localization/Loc.cs` (mantener sincronizado con los `.resw`).

## Interfaz
- WinUI 3 con Microsoft.UI.Reactor (arquitectura MVU: estado inmutable + funciones de actualización puras).
- Fondo Mica; barra de título personalizada con botón de Configuración animado.
- Colores de estado del sistema (`Theme.SystemCaution` / `Theme.SystemCritical`): se adaptan a claro, oscuro y alto contraste.
- Etiquetas de accesibilidad en la zona de prueba, el historial y los controles de configuración.

## Limitaciones conocidas
- **Clics combinados**: si un botón está pulsado, el clic del otro no se cuenta. Ver [ideas/clics-combinados.md](ideas/clics-combinados.md).
- **Requiere empaquetado MSIX**: `Package.Current` y `ApplicationData.Current` no funcionan sin identidad de paquete.
- **Usuarios que vienen de la versión UWP** (Double Click Test) verán la bienvenida en lugar de las novedades la primera vez.

## Persistencia
| Dato | Dónde |
|---|---|
| Tema | `UsePreferredTheme` (persistido por Reactor) |
| Umbral | `LocalSettings` → `GhostClickThresholdMs` |
| Última versión vista | `LocalSettings` → `LastLaunchedVersion` |

Los resultados de las pruebas **no** se guardan. Ver [ideas/historial-de-sesiones.md](ideas/historial-de-sesiones.md).