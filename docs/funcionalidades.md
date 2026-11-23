# Ghost Click — Funcionalidades

Estado actual de la app. Última actualización: 2026-11-23.

## Qué hace
Detecta si el mouse registra clics que el usuario no hizo ("clics fantasma"): dos pulsaciones del **mismo botón** separadas por menos tiempo que el umbral configurado. Es el síntoma típico de un interruptor desgastado.

## Prueba de clics (`MainPage`)

### Zona de prueba
- **Tarjeta estilo WinUI**: fondo de tarjeta, borde fino y esquinas de 8 px, con un icono de mouse y el texto "Haz clic aquí" en el centro.
- **Respuesta al pulsar**: mientras hay algún botón pulsado, el fondo cambia al tono secundario de tarjeta, como un control nativo.
- **Clic fantasma detectado**: el borde (2 px) y el icono pasan al color de error del sistema, el icono cambia a advertencia y el texto a "Clic fantasma detectado". Se queda así hasta reiniciar.
- Todos los colores son tokens del sistema: se adaptan a claro, oscuro y alto contraste.

### Detección
- **5 botones**: izquierdo, central (rueda), derecho, atrás y adelante. Cada uno tiene sus propios contadores; alternar botones rápido **no** cuenta como fallo.
- **Clics combinados**: se registran también los botones pulsados mientras otro sigue pulsado.
- **El primer clic de cada botón no genera intervalo.**
- **Medición precisa** con `PointerPoint.Timestamp` (microsegundos del propio evento).
- Dentro de la zona de prueba, "atrás" no navega y el clic derecho no abra menús.

### Tarjetas por botón
- Una por botón, del mismo ancho, con un punto que se enciende mientras está pulsado y sus contadores de clics y clics fantasma.

### Historial de intervalos
- **Panel a la derecha de la zona de prueba**, oculto por defecto.
- Se abre y cierra con el botón **Historial**, junto a **Reiniciar**.
- **Animación**: el ancho crece de 0 a 260 px en 250 ms (desaceleración) y empuja la zona de prueba; al cerrar, se encoge y la devuelve. Sin animación si el usuario las desactivó en Windows.
- Cada fila muestra el botón, el intervalo en ms y, si fue clic fantasma, la marca y el tiempo en color de error.
- Conserva los últimos 500 intervalos.
- Cerrado, queda fuera del lector de pantalla y del orden de Tab.

### Cabecera
- Umbral actual a la izquierda; **Historial** y **Reiniciar** a la derecha.
- **Reiniciar** borra contadores, historial, indicador y estado de clic fantasma.

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

## Persistencia
| Dato | Dónde |
|---|---|
| Tema | `UsePreferredTheme` (persistido por Reactor) |
| Umbral | `LocalSettings` → `GhostClickThresholdMs` |
| Última versión vista | `LocalSettings` → `LastLaunchedVersion` |

Los resultados de las pruebas **no** se guardan. Ver [ideas/historial-de-sesiones.md](ideas/historial-de-sesiones.md).

## Publicación
- App independiente en Microsoft Store (no es una actualización de Double Click Test, que sigue publicada en UWP).
- Requiere Windows 11 (`MinVersion 10.0.22000.0`).
- Comentarios: [issues del repositorio](https://github.com/alexfalconflores/ghost-click/issues).