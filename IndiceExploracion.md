# Índice de Exploración del Código y Archivos - AutoTallerManager

Este archivo sirve como un mapa interactivo del proyecto. Utiliza los siguientes enlaces para abrir y explorar directamente los archivos de código fuente de cada una de las capas de la aplicación.

---

## 📂 1. MAPA DE ARCHIVOS POR CAPAS

### 🏢 Capa de API y Contratos (`Api/`)
Esta capa maneja la interacción con el cliente (endpoints REST), la autenticación de usuarios, los objetos de transferencia de datos (DTOs), el mapeo AutoMapper y los servicios transaccionales.

* 🌐 **Configuración y Arranque**:
  * [Program.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Program.cs): Punto de inicio de la aplicación. Configura MySQL, JWT, inyección de dependencias y auto-migraciones.
  * [appsettings.json](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/appsettings.json): Archivo de configuración que contiene la cadena de conexión a MySQL.

* 🎮 **Controladores (Endpoints)**:
  * [ClientesController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/ClientesController.cs): Puntos de entrada para el registro de clientes y sus vehículos.
  * [VehiculosController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/VehiculosController.cs): CRUD de vehículos y consulta por VIN/Cliente.
  * [OrdenesServicioController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/OrdenesServicioController.cs): Apertura de órdenes, reporte de trabajo de mecánicos, aprobación y facturación.
  * [RepuestosController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/RepuestosController.cs): Inventario de repuestos con límite de tasa de peticiones.
  * [FacturasController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/FacturasController.cs): Histórico de cobros generados.
  * [MarcasController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/MarcasController.cs): CRUD de marcas (ej: Toyota, Honda).
  * [ModelosController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/ModelosController.cs): CRUD de modelos enlazados a marcas.
  * [ProveedoresController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/ProveedoresController.cs): Gestión de proveedores de repuestos.
  * [CatalogoServiciosController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/CatalogoServiciosController.cs): Lista de servicios con tarifas fijas de mano de obra.
  * [AuditoriasController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/AuditoriasController.cs): Endpoint administrativo para leer la bitácora de cambios.
  * [UsuariosController.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/Auth/UsuariosController.cs): Registro y Login de usuarios (Admin/Mecánico/Recep) con firma JWT.

* 📦 **DTOs (Objetos de Entrada y Salida)**:
  * Ubicados en la carpeta: [Api/DTOs/](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/).
  * Ejemplos destacados:
    * [ClienteDto.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/ClienteDto.cs): Respuesta estándar de cliente con su lista de vehículos enlazada.
    * [CreateClienteDto.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/CreateClienteDto.cs): Payload para crear clientes de forma simultánea con vehículos.

* ⚙️ **Mappings (AutoMapper)**:
  * [AutoMapperProfile.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Mappings/AutoMapperProfile.cs): Configura las reglas de transformación entre entidades y DTOs.

* 🧠 **Servicios de Lógica y Transacciones**:
  * [ClienteService.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/ClienteService.cs): Orquesta la creación transaccional atómica del cliente y sus autos.
  * [OrdenServicioService.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/OrdenServicioService.cs): Controla las operaciones complejas de órdenes (cálculo de precios, stock e inventario, aprobación y facturación).

---

### 💻 Capa de Infraestructura (`Infrastructure/`)
Esta capa maneja la conexión directa a la base de datos MySQL, la persistencia, la bitácora de auditoría y los repositorios.

* 🗄️ **Base de Datos**:
  * [AutoTallerDbContext.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/Data/AutoTallerDbContext.cs): DbContext principal. Mapea relaciones, define la inserción de datos semilla, e intercepta guardados para registrar auditorías de forma automática.

* 🧱 **Patrón Repositorio y Unit of Work**:
  * [GenericRepository.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/Repositories/GenericRepository.cs): Implementación genérica de operaciones CRUD (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Remove`, `Update`).
  * [UnitOfWork.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/UnitOfWork/UnitOfWork.cs): Administra el ciclo de vida de los repositorios y confirma las transacciones pendientes mediante `CommitAsync()`.

---

### 🏛️ Capa de Aplicación - Abstracciones (`Application/`)
Esta capa aloja las abstracciones básicas del patrón repositorio para desacoplar el acceso a datos.

* 📋 **Interfaces de Persistencia**:
  * [IGenericRepository.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Application/Interfaces/IGenericRepository.cs): Firma de métodos comunes de lectura/escritura de datos.
  * [IUnitOfWork.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Application/Interfaces/IUnitOfWork.cs): Firma que expone todos los repositorios disponibles en el taller.

---

### 👑 Capa de Dominio - Lógica Pura (`Domain/`)
Contiene las entidades puras del negocio y la lógica de encapsulamiento interna. No tiene dependencias de ninguna otra capa.

* 🚗 **Entidades Core**:
  * [Cliente.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Cliente.cs) | [Vehiculo.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Vehiculo.cs) | [Marca.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Marca.cs) | [Modelo.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Modelo.cs)
  * [OrdenServicio.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/OrdenServicio.cs): Maneja estados (`EstadoOrden`) y transiciones lógicas.
  * [DetalleOrden.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/DetalleOrden.cs): Registro de repuestos usados en una reparación.
  * [Repuesto.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Repuesto.cs): Lógica de control de inventario (`AumentarStock`, `ReducirStock`).
  * [Factura.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Factura.cs): Representación final del cobro generado.
  * [Proveedor.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Proveedor.cs) | [CatalogoServicio.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/CatalogoServicio.cs)
  * [Usuario.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Usuario.cs) | [Auditoria.cs](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Auditoria.cs)

---

## 📕 2. GUÍAS Y MANUALES ADICIONALES

Para complementar la exploración del código, revisa estos manuales integrados en el repositorio:

1. 📖 **[Guía de Estudio y Manual de Arquitectura](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/GuiaEstudioProyecto.md)**:
   * Contiene la chuleta de examen con ejemplos de cómo añadir campos, endpoints o búsquedas.
   * Explica detalladamente el funcionamiento del guardado con auditoría automática.
2. 🚀 **[Manual de Uso de la API y Endpoints](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/ManualUsoApi.md)**:
   * Guía completa de uso de Insomnia, simulación de roles (`Admin`, `Mecanico`, `Recepcionista`), datos semilla cargados y flujo de peticiones.
