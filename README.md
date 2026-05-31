# AutoTallerManager

AutoTallerManager es un backend RESTful desarrollado sobre ASP.NET Core y Entity Framework Core con soporte para PostgreSQL. El sistema esta disenado para centralizar y automatizar los procesos clave de un taller de reparacion automotriz moderno, abarcando desde el registro de clientes hasta el cierre de ordenes de servicio, facturacion y auditoria de actividades.

## Arquitectura del Proyecto

El sistema esta implementado bajo los principios de la Arquitectura Hexagonal (Ports and Adapters), dividida en cuatro capas claramente estructuradas para garantizar el desacoplamiento y facilitar el mantenimiento a largo plazo:

1. Capa de Dominio (Domain): Contiene las entidades de negocio puras (Cliente, Vehiculo, OrdenServicio, Repuesto, Factura, Usuario, Auditoria), los enums y las reglas basicas de negocio independientes de cualquier framework.
2. Capa de Aplicacion (Application): Coordina y ejecuta los casos de uso principales. Incluye los DTOs, las interfaces de repositorio y unidad de trabajo, las configuraciones de mapeo mediante AutoMapper y los servicios de aplicacion.
3. Capa de Infraestructura (Infrastructure): Implementa el acceso y persistencia de datos contra la base de datos PostgreSQL utilizando Entity Framework Core, aplicando el patron Repository y el patron Unit of Work de manera transaccional.
4. Capa de API (Api): Expone los servicios de la aplicacion en forma de endpoints HTTP RESTful, controlando la autenticacion, autorizacion por roles, rate limiting e integrando la documentacion interactiva a traves de Swagger.

## Funciones Principales

El backend cubre las siguientes necesidades operativas del negocio:

### 1. Gestion de Clientes y Vehiculos
* Registro transaccional: Permite crear un registro de cliente junto con uno o mas vehiculos asociados en una sola operacion atomica. Si algun registro del vehiculo falla, la transaccion completa es anulada en la base de datos.
* CRUD completo de clientes y vehiculos con filtros por VIN, cliente y paginacion.
* Proteccion contra eliminacion: Impide la eliminacion de un vehiculo o cliente si posee ordenes de servicio activas en el taller.

### 2. Gestion de Inventario de Repuestos
* Control de existencias en tiempo real con endpoints protegidos para que solo los administradores puedan alterar el catalogo base.
* Restricciones de negocio a nivel de dominio para impedir que el stock de repuestos caiga por debajo de cero durante las operaciones.

### 3. Ciclo de Vida de Ordenes de Servicio
* Creacion de ordenes: Registro de ingreso de vehiculos definiendo el tipo de servicio (Mantenimiento Preventivo, Reparacion, Diagnostico) y calculo automatico de la fecha estimada de entrega.
* Seguimiento y asignacion: Asignacion de mecanicos responsables y registro de estados de la orden (Ingresada, Diagnosticando, EnReparacion, Cerrada, Cancelada).
* Carga de repuestos por orden: Permite que el mecanico registre las piezas consumidas en la orden, disminuyendo de forma automatica y sincronizada el inventario de repuestos.

### 4. Generacion Automatizada de Facturas
* Liquidacion de servicios: Al cerrarse una orden de servicio, el sistema calcula de forma matematica el total sumando el valor de los repuestos consumidos mas el costo estandar de mano de obra asociado al tipo de servicio.
* Emision de facturas inmutables asociadas a las ordenes correspondientes y almacenamiento de historial de cobro.

### 5. Control de Seguridad y Roles
* Autenticacion JWT: Emision de tokens firmados que encapsulan la identidad y el rol del usuario actual.
* Autorizacion restrictiva basada en tres perfiles especificos del personal del taller:
  * Admin: Control absoluto del sistema (CRUD de usuarios, bitacora de auditorias, informes y edicion de inventario).
  * Recepcionista: Acceso exclusivo a la creacion de clientes, asociacion de vehiculos y generacion de ordenes de servicio.
  * Mecanico: Permisos para actualizar estados de avance en las ordenes de trabajo, registrar consumo de repuestos y facturar ordenes terminadas.

### 6. Sistema Automatico de Auditoria
* Auditoria de base de datos en segundo plano: Implementacion transparente dentro del contexto de datos que intercepta las operaciones de creacion, modificacion y borrado.
* Registra que usuario (ID y correo electronico) realizo la accion, la marca de tiempo exacta, la entidad afectada y el detalle pormenorizado del cambio (indicando campos alterados, valores originales y nuevos valores).

### 7. Control de Trafico (Rate Limiting)
* Middleware configurado para limitar el numero maximo de peticiones concurrentes por minuto en endpoints sensibles para evitar ataques de denegacion de servicio y abusos a la API, retornando HTTP 429 cuando es excedido.
