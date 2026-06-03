# Manual Completo de la API - AutoTallerManager

API REST para la gestion integral de un taller mecanico.
**Base URL:** `http://localhost:5003`

---

## 1. PREPARACION E INICIO

El sistema utiliza **MySQL** y crea la base de datos automaticamente al iniciar.

1. Verifica que tu servidor MySQL este activo en el puerto `3306`.
2. Ajusta las credenciales si es necesario en `Api/appsettings.json`:
   ```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=AutoTallerManager;User=root;Password=;"
   ```
3. Inicia la API desde la raiz del proyecto:
   ```bash
   dotnet run --project Api
   ```

---

## 2. CONCEPTOS BASICOS: COMO FUNCIONA TODO

El taller mecanico funciona a traves de un flujo definido de 5 pasos.
**Importante:** Esta API es un backend, lo que significa que **no tiene pantallas ni botones fisicos**. Todo lo que los empleados del taller hacen en la vida real (registrar un auto, descontar repuestos, facturar) se hace enviando peticiones web a esta API. Para las pruebas, usaremos la herramienta **Insomnia** para simular que somos los diferentes empleados del taller enviando esas peticiones.

### Paso 1: El taller se prepara (Admin)
Antes de que llegue cualquier cliente, el Admin configura el sistema enviando peticiones a la API. Crea las cuentas del Recepcionista y del Mecanico para que puedan iniciar sesion. Tambien llena el inventario de repuestos: agrega cada pieza (filtros, bujias, pastillas de freno) con su codigo, precio unitario y stock.

### Paso 2: Llega un cliente nuevo (Recepcionista)
El Recepcionista, usando Insomnia, envia un `POST` al sistema para registrar al cliente y su vehiculo (marca, modelo, VIN). El sistema vincula el vehiculo al cliente. 

### Paso 3: El vehiculo entra al taller (Recepcionista)
El Recepcionista abre una Orden de Servicio enviando otro `POST`. Indica que vehiculo ingreso, el tipo de servicio (Mantenimiento, Reparacion, Diagnostico) y a que mecanico se le asigna. El sistema crea la orden con estado "Ingresada" (es como el ticket de entrada).

### Paso 4: El mecanico trabaja (Mecanico)
Aqui es donde el mecanico interactua con la API. Cuando termina el trabajo fisico con el auto, abre Insomnia y envia un reporte mediante una peticion `PUT`.
En esa peticion indica los repuestos que utilizo (ej: 2 filtros, 4 bujias) y cambia el estado a "Terminada".
El sistema recibe esto y automaticamente descuenta esos repuestos del stock del taller.

### Paso 5: Se genera la factura (Mecanico o Admin)
Finalmente, el mecanico llama al endpoint de facturar (un `POST`). En ese instante, el sistema calcula matematicamente:
`(Precio de cada repuesto x cantidad usada) + Costo fijo de mano de obra = TOTAL`.
La factura se guarda, la orden queda "Cerrada" y ya no puede modificarse. La API no procesa el pago monetario, solo genera el documento de cobro.

---

## 3. ROLES DEL SISTEMA Y PERMISOS

El sistema tiene **3 tipos de usuario** que inician sesion y reciben un token. El `Cliente` (dueno del auto) **no es un usuario de la API** - es solo un registro gestionado por el personal.

### Admin (Superusuario)
Puede hacer absolutamente todo: gestionar usuarios, inventario completo, ordenes, facturas, y tiene acceso exclusivo a la **Bitacora de Auditorias** (el historial inborrable de quien hizo cada operacion).

### Recepcionista (Atencion al cliente)
Registra clientes, vehiculos y crea las nuevas Ordenes de Servicio cuando ingresan los autos. No puede gestionar repuestos ni facturar.

### Mecanico (Tecnico del taller)
Solo puede consultar datos (clientes, repuestos, ordenes) y su funcion principal es **registrar su trabajo**. Puede actualizar el estado de una orden, reportar repuestos usados y finalmente generar la factura. No puede crear clientes ni usuarios.

---

## 4. DATOS SEMILLA (Precargados al iniciar)

| Entidad | Id | Datos |
|---|---|---|
| Usuario | 1 | `admin@taller.com` / `admin123` - Rol: Admin |
| Usuario | 2 | `mecanico@taller.com` / `mecanico123` - Rol: Mecanico |
| Cliente | 1 | Juan Perez - `juan@perez.com` |
| Vehiculo | 1 | Toyota Corolla 2020 - VIN: `VIN1234567890COROLLA` - ClienteId: 1 |
| Repuesto | 1 | Filtro de Aceite - `FILT-ACEITE` - $15.50 - Stock: 50 |
| Repuesto | 2 | Pastillas de freno - `PAST-FREN-DEL` - $45.00 - Stock: 20 |
| Repuesto | 3 | Bujia de platino - `BUJIA-PLAT` - $8.20 - Stock: 100 |

> No existe un usuario Recepcionista en los datos semilla. Debes crearlo con un POST desde la cuenta Admin.

---

## 5. GUIA RAPIDA DE INSOMNIA Y AUTENTICACION

| Metodo | Para que sirve? | Lleva body (JSON)? |
|---|---|---|
| `GET` | **Consultar / Leer** datos | No |
| `POST` | **Crear** un nuevo registro o accion | Si |
| `PUT` | **Editar** un registro existente | Si |
| `DELETE` | **Eliminar** un registro | No |

### Obtener y usar el Token
> [!IMPORTANT]
> Casi todos los endpoints requieren token. Sin el recibiras `401 Unauthorized`.

1. Haz `POST http://localhost:5003/api/Usuarios/login` con el correo y contrasena de tu rol.
2. Copia el `"token"` completo de la respuesta.
3. En cada peticion protegida ve a **Auth > Bearer Token** y pega el token en el campo **TOKEN**. El token dura 60 minutos.

### Regla para el PUT (Editar)
> [!IMPORTANT]
> El `PUT` **reemplaza completamente** el registro. Debes enviar **todos los campos** en el Body JSON, incluso los que no cambiaste, o de lo contrario quedaran vacios en la base de datos.

---
---

## 6. REFERENCIA DE ENDPOINTS

A continuacion, la lista completa de peticiones disponibles.

### USUARIOS - `/api/Usuarios`

* `POST /api/Usuarios/login` (Publico) - Inicia sesion y obtiene el token.
* `GET /api/Usuarios` (Solo Admin) - Lista todos los usuarios.
* `POST /api/Usuarios` (Solo Admin) - Crea un nuevo usuario.
* `PUT /api/Usuarios/{id}` (Solo Admin) - Edita correo, contrasena o rol de un usuario.
* `DELETE /api/Usuarios/{id}` (Solo Admin) - Elimina un usuario.

### CLIENTES - `/api/Clientes`

* `POST /api/Clientes/registrar-con-vehiculo` (Admin/Recep) - Crea un cliente nuevo. Opcionalmente puede enviar array de `vehiculos` en el mismo JSON para registrarlos de una vez.
* `POST /api/Clientes/{id}/vehiculos` (Admin/Recep) - Anade uno o mas vehiculos a un cliente ya existente.
* `GET /api/Clientes` (Todos) - Lista clientes paginados.
* `GET /api/Clientes/{id}` (Todos) - Ver detalle de un cliente.
* `PUT /api/Clientes/{id}` (Admin/Recep) - Edita nombre/telefono/correo.
* `DELETE /api/Clientes/{id}` (Solo Admin) - Elimina un cliente.

### VEHICULOS - `/api/Vehiculos`

* `GET /api/Vehiculos` (Todos) - Lista todos o filtra (ej: `?clienteId=2` o `?vin=HONDA`).
* `GET /api/Vehiculos/{id}` (Todos) - Detalle de un vehiculo especifico.
* `POST /api/Vehiculos?clienteId={id}` (Admin/Recep) - Registrar un auto suelto a un cliente.
* `PUT /api/Vehiculos/{id}` (Admin/Recep) - Edita datos (marca, modelo, kilometraje).
* `DELETE /api/Vehiculos/{id}` (Solo Admin) - Elimina un vehiculo.

### REPUESTOS - `/api/Repuestos`
*(Rate Limit: Maximo 30 peticiones por minuto)*

* `GET /api/Repuestos` (Todos) - Ver el inventario.
* `GET /api/Repuestos/{id}` (Todos) - Ver un repuesto en detalle.
* `POST /api/Repuestos` (Solo Admin) - Registrar nueva pieza en el inventario.
* `PUT /api/Repuestos/{id}` (Solo Admin) - Editar repuesto (precio o stock manual).
* `DELETE /api/Repuestos/{id}` (Solo Admin) - Eliminar repuesto.

### ORDENES DE SERVICIO - `/api/OrdenesServicio`
*(Rate Limit: Maximo 60 peticiones por minuto)*

* `GET /api/OrdenesServicio` (Todos) - Listar historial de ordenes.
* `POST /api/OrdenesServicio` (Admin/Recep) - Abre una orden indicando `vehiculoId`, `tipoServicio` y `mecanicoId`. Entra en estado "Ingresada".
* `PUT /api/OrdenesServicio/{id}/trabajo?nuevoEstado=Terminada` (Admin/Mecanico) - El **Mecanico envia esta peticion** para reportar los repuestos que uso. El sistema descuenta el stock. En el body (JSON) envia la lista exacta de repuestos, por ejemplo:
  ```json
  [
    { "repuestoId": 1, "cantidad": 2 },
    { "repuestoId": 3, "cantidad": 4 }
  ]
  ```
* `POST /api/OrdenesServicio/{id}/facturar` (Admin/Mecanico) - Genera la factura matematicamente y cierra la orden.

### FACTURAS - `/api/Facturas`
*(Las facturas son inmutables)*

* `GET /api/Facturas` (Todos) - Ver historial. Filtros utiles: `?clienteId=2` o `?ordenServicioId=1`.
* `GET /api/Facturas/{id}` (Todos) - Ver desglose de la factura.
* `GET /api/Facturas/orden/{ordenId}` (Todos) - Buscar factura usando el ID de la orden.

### AUDITORIAS - `/api/Auditorias`

* `GET /api/Auditorias` (Solo Admin) - Ver la bitacora de quien inserto/modifico/elimino cada cosa en la base de datos, con la fecha y valores nuevos.

---
---

## 7. FLUJO PASO A PASO EN INSOMNIA

Sigue esta secuencia exacta para probar un ciclo de vida completo de inicio a fin:

| Paso | Quien (Rol) | Metodo | Endpoint en Insomnia | Que ocurre logicamente |
|---|---|---|---|---|
| **1** | Admin | `POST` | `/api/Usuarios/login` | Obtiene su token inicial |
| **2** | Admin | `POST` | `/api/Usuarios` | Crea el usuario Recepcionista |
| **3** | Recep. | `POST` | `/api/Usuarios/login` | Inicia sesion y obtiene su propio token |
| **4** | Recep. | `POST` | `/api/Clientes/registrar-con-vehiculo` | Registra a Maria y su Honda Civic |
| **5** | Recep. | `POST` | `/api/OrdenesServicio` | Crea orden de Mantenimiento para el Civic |
| **6** | Mecanico | `POST` | `/api/Usuarios/login` | Inicia sesion y obtiene su token |
| **7** | Mecanico | `PUT` | `/api/OrdenesServicio/1/trabajo?nuevoEstado=Terminada` | Reporta que uso 2 filtros de aceite y la orden termina |
| **8** | Mecanico | `POST` | `/api/OrdenesServicio/1/facturar` | La API suma los costos, genera Factura #1 y cierra la orden |
| **9** | Recep. | `GET` | `/api/Facturas?clienteId=2` | Consulta cuanto le debe cobrar a Maria |
| **10** | Admin | `GET` | `/api/Auditorias` | Revisa el historial de quien hizo los pasos 2 al 8 |
