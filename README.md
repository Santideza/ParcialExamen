# ParcialExamen

Aplicación ASP.NET Core para gestión de solicitudes de crédito.

## Características

- Gestión de clientes con ingresos mensuales
- Solicitudes de crédito con validaciones
- Estados de solicitud: Pendiente, Aprobado, Rechazado
- Rol de Analista para revisar solicitudes
- Base de datos SQLite con EF Core

## Requisitos

- .NET 10.0
- SQLite

## Instalación

1. Clonar el repositorio
2. Ejecutar `dotnet restore`
3. Ejecutar `dotnet ef database update`
4. Ejecutar `dotnet run`

## Validaciones

- Ingresos mensuales > 0
- Monto solicitado > 0
- Un cliente solo puede tener una solicitud en estado Pendiente
- No se puede aprobar una solicitud si el monto es mayor a 5 veces los ingresos mensuales

## Datos Iniciales

- 2 clientes (cliente1@example.com, cliente2@example.com)
- 1 usuario con rol Analista (analista@example.com)
- 2 solicitudes (1 pendiente, 1 aprobada)

Contraseña inicial: Password123!
