Documentación Técnica: LocalLink
1. Introducción
LocalLink es una plataforma diseñada para centralizar y visibilizar la oferta de comercios y servicios locales. El proyecto tiene como objetivo fortalecer la economía de proximidad mediante una interfaz intuitiva, desarrollada bajo un enfoque multiplataforma para garantizar el acceso desde diversos dispositivos.

2. Arquitectura Técnica
El desarrollo emplea el framework .NET MAUI (Multi-platform App UI), permitiendo la creación de una aplicación coherente que puede ejecutarse de forma nativa en sistemas operativos móviles y de escritorio desde un único repositorio de código.

Stack Tecnológico
Framework Principal: .NET MAUI.
Lenguaje: C# (con XAML para la definición de interfaces).
Gestión de Datos: Integración con servicios de backend mediante protocolos estándar de comunicación.
Diseño y Estilo: Utilización de recursos de estilo integrados en MAUI para garantizar consistencia visual multiplataforma.

3. Estructura del Proyecto
La organización del repositorio sigue las convenciones de las soluciones de .NET:
/Platforms: Contiene el código específico de cada sistema operativo (Android, iOS, Windows, macOS).
/Resources: Gestión de activos (imágenes, fuentes, iconos) centralizados para toda la aplicación.
/Views y /ViewModels: Implementación del patrón de diseño MVVM (Model-View-ViewModel) para la separación de la lógica de negocio y la interfaz.
LocalLink.csproj: Archivo de configuración del proyecto que define las dependencias y las plataformas de destino.

4. Guía de Instalación y Despliegue
Para la configuración del entorno de desarrollo de .NET MAUI, es necesario contar con Visual Studio 2022 (con la carga de trabajo de desarrollo móvil con .NET) o el SDK de .NET 8/9+ junto con la extensión de MAUI para VS Code.

Procedimiento de ejecución:
Clonar el repositorio:
Bash
git clone https://github.com/BarriosMario/LocalLink.git
cd LocalLink

Restaurar dependencias:
Bash
dotnet restore

Ejecución del proyecto:
Para compilar y ejecutar en la plataforma predeterminada:
Bash
dotnet build
dotnet run

5. Características del Sistema
Multiplataforma Nativa: Ejecución nativa en dispositivos móviles y de escritorio a partir de una única base de código.
Patrón MVVM: Organización del código que facilita el mantenimiento, las pruebas unitarias y la escalabilidad de las funcionalidades.
Interfaz Adaptativa: Diseño flexible que permite ajustar la presentación de los servicios locales según el tamaño y tipo de pantalla del dispositivo.

6. Consideraciones de Entorno
El desarrollo en entornos Linux (como sistemas basados en Fedora) requiere el uso del SDK de .NET y herramientas de línea de comandos. Se recomienda verificar que las variables de entorno de .NET estén configuradas correctamente para permitir la compilación cruzada y el despliegue de la aplicación en las plataformas objetivo.
