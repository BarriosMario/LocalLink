 # LocalLink: Marketplace Comunitario
**Proyecto de Desarrollo en .NET MAUI - Equipo 1**

Este repositorio contiene el desarrollo de LocalLink, una aplicación multiplataforma diseñada para conectar a la comunidad mediante un mercado digital eficiente y moderno.

## Integrantes del Equipo
**Alvarez Cruz Ivette Vianney** 
**Amaro Gonzalez Angel Fernando** 
**Ayim Viana Karol Ivanna** 
**Barrios Aquino Mario Antonio**
**Bojorquez Balderas Freddy Adrian** 

## Arquitectura del Software
Para este proyecto hemos seleccionado la arquitectura (Aun no se) considerando las caracteristicas de la aplicacion.


Pilares de nuestra arquitectura:
**Desacoplamiento Total:** Eliminación del *Code-Behind* mediante el uso de comandos y generadores de código del Community Toolkit.
**Reactividad:** Implementación de la interfaz `INotifyPropertyChanged` para asegurar que la UI responda en tiempo real a los cambios en los datos.
**Inyección de Dependencias:** Gestión centralizada de servicios y ViewModels en `MauiProgram.cs`.

## Estructura del Proyecto
Siguiendo la anatomía estándar de un proyecto MAUI robusto, organizamos nuestro código de la siguiente manera:

 **`/Models`**: Entidades de datos (Producto, Usuario, Categoría).
 **`/Views`**: Definición de la jerarquía de la app en XAML (AppShell, MainPage).
 **`/ViewModels`**: Lógica de vinculación y procesamiento de comandos.
 **`/Services`**: Lógica de persistencia de datos (SQLite/JSON) e integración de hardware.

## Hoja de Ruta (Roadmap 2026)
Nuestra progresión de desarrollo se divide en tres puntos clave:

1.  ** 1er Avance: Cimientos y UI (4 de Marzo):** Navegación definida en `AppShell.xaml`, vistas maquetadas con `Grid` y `StackLayout`, y estilos dinámicos aplicados.
2.  ** 2do Avance: Lógica y Datos (20 de Mayo):** Implementación de persistencia (SQLite/API), uso de **MAUI Essentials** (Cámara/GPS) y eliminación total de lógica en la vista.
3.  ** Entrega Final: Despliegue (5 de Junio):** Publicación de versión Web, generación de `.aab` para Android y soporte completo para modo oscuro.

## Requerimientos de Infraestructura
**GitHub:** Repositorio central con documentación técnica.
**Backend:** Servicios de base de datos en Azure o Firebase.
**Distribución:** Preparación de paquetes para Google Play Console.
