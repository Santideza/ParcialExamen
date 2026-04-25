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
- Identity (autenticación y roles)
- Redis (cache y sesiones)
- Razor Views
- GitHub (control de versiones)
- Render.com (deploy)

## Instalación

1. Clonar el repositorio
2. Configurar base de datos
3. Ejecutar el proyecto

##Roles sistema

1.Cliente (Usuario normal)
- Registra solicitudes
- Consulta sus solicitudes
2.Analista
- Accede al panel /Analista
- Aprueba o rechaza solicitudes


## Validaciones

- Ingresos mensuales > 0
- Monto solicitado > 0
- Un cliente solo puede tener una solicitud en estado Pendiente
- No se puede aprobar una solicitud si el monto es mayor a 5 veces los ingresos mensuales

## Datos Iniciales

- Usuario cliente:
  Email: cliente1@example.com
  Password: Password123!

- Otro cliente:
  Email: cliente2@example.com
  Password: Password123!

Analista:
- Email: analista@example.com
  Password: Password123!
  Rol: Analista

##Pruebas realizadas

- Registro de usuario 
- Login 
- Creación de solicitud 
- Validaciones de negocio 
- Panel de analista 
- Cache Redis 
- Sesión Redis 

##Flujo de trabajo con Git

- feature/bootstrap-dominio
- feature/catalogo-solicitudes
- feature/solicitudes
- feature/sesion-redis
- feature/panel-analista
- deploy/render