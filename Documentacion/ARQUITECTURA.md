Guía de Arquitectura y Funcionamiento Interno: LocalLink
Este documento describe la estructura técnica y el flujo de datos del proyecto LocalLink, desarrollado bajo el framework .NET MAUI. Está diseñado para servir como punto de entrada a nuevos desarrolladores o evaluadores técnicos.

1. Patrón de Diseño: MVVM
El proyecto implementa el patrón Model-View-ViewModel (MVVM), el cual permite una separación clara entre la interfaz de usuario y la lógica de negocio.

Models (Modelos): Definen la estructura de los datos (ej. Comercio, Usuario, Servicio). Son clases puras de C#.

Views (Vistas): Archivos XAML que definen la jerarquía visual. No contienen lógica de negocio, solo referencia a los elementos visuales.

ViewModels: Actúan como intermediarios. Contienen la lógica de presentación y se comunican con las Vistas mediante Data Binding (enlace de datos) y Commands.

2. Ciclo de Vida y Flujo de Datos
Cuando un usuario interactúa con la aplicación, el flujo sigue este orden lógico:

Entrada de Usuario: El usuario interactúa con un componente en la View (ej. presiona un botón de "Buscar").

Ejecución de Comando: La View dispara un ICommand alojado en el ViewModel.

Procesamiento: El ViewModel ejecuta la lógica (filtrado, validación o llamada a un servicio externo).

Notificación de Cambio: Al actualizarse las propiedades del ViewModel, este dispara el evento INotifyPropertyChanged.

Actualización Visual: La View se actualiza automáticamente gracias al enlace de datos, mostrando la nueva información al usuario.

3. Estructura de Archivos Críticos
Para entender el funcionamiento, el desarrollador debe enfocarse en:

MauiProgram.cs: El punto de entrada donde se configuran los servicios, las fuentes y se registran las dependencias (Dependency Injection).

AppShell.xaml: Define la jerarquía de navegación global de la aplicación (TabBars, Flyouts y rutas).

Services/: (Si aplica) Capa encargada de la persistencia de datos o consumo de APIs externas.

4. Inyección de Dependencias (DI)
El proyecto utiliza el contenedor de dependencias nativo de .NET. Esto significa que las clases no crean sus propias dependencias, sino que las reciben en el constructor, lo que facilita:

La realización de pruebas unitarias.

El desacoplamiento del código.

La gestión eficiente de la memoria.

5. Gestión de Recursos Multiplataforma
Dado que el desarrollo es en MAUI, el sistema gestiona los recursos de forma inteligente:

Imágenes y Fuentes: Se almacenan en la carpeta /Resources. MAUI se encarga de redimensionar y adaptar estos recursos para cada sistema operativo (Android, iOS, Windows) durante la compilación.

Handlers nativos: El código escrito en C# se traduce a controles nativos de cada plataforma, asegurando que el rendimiento sea óptimo y no una simple "vista web".

6. Consideraciones para Contribuidores
Si desea modificar o extender la funcionalidad:

Tipado: Se debe mantener el uso estricto de tipos de C# para evitar errores en tiempo de ejecución.

Binding: Priorice el uso de CommunityToolkit.Mvvm (si está instalado) para reducir el código repetitivo (boilerplate) en los ViewModels.

Navegación: Utilice Shell.Current.GoToAsync() para el manejo de rutas, asegurando que el paso de parámetros entre páginas sea consistente.
