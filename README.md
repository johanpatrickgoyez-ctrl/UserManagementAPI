# UserManagementAPI

Una API REST profesional construida con **.NET 10** para la gestión de usuarios, implementando altos estándares de seguridad, autenticación mediante **JWT** y contenedores con **Docker**.

## Tecnologías Utilizadas

* **Framework:** .NET 10 (ASP.NET Core)
* **Base de Datos:** SQL Server mediante Entity Framework Core (Code First)
* **Seguridad:** Autenticación JWT (JSON Web Tokens) y Hashing SHA512
* **Mapeo:** AutoMapper (para transferencia limpia de DTOs)
* **Contenedores:** Docker & Docker Compose
* **Documentación:** Swagger / OpenAPI

## Características de Seguridad

El proyecto no guarda contraseñas en texto plano. Implementa un sistema de seguridad robusto:
1. **Hashing:** Uso de `HMACSHA512` para transformar contraseñas.
2. **Salting:** Generación de un `PasswordSalt` único por cada usuario para prevenir ataques de diccionario.
3. **Roles:** Sistema de autorización basado en roles (`Admin` y `User`).



## Requisitos Previos

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* [SQL Server Express](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) o LocalDB

## Instalación y Configuración

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/johanpatrickgoyez-ctrl/UserManagementAPI.git](https://github.com/johanpatrickgoyez-ctrl/UserManagementAPI.git)
   cd UserManagementAPI
