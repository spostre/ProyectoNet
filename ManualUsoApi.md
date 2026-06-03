# 🛠️ Manual Completo de la API — AutoTallerManager

API REST para la gestión integral de un taller mecánico.  
**Base URL:** `http://localhost:5003`

---

## 📋 PREPARACIÓN

El sistema utiliza **MySQL** y crea la base de datos automáticamente al iniciar.

1. Verifica que tu servidor MySQL esté activo en el puerto `3306`.
2. Ajusta las credenciales si es necesario en `Api/appsettings.json`:
   ```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=AutoTallerManager;User=root;Password=;"
   ```
3. Inicia la API desde la raíz del proyecto:
   ```bash
   dotnet run --project Api
   ```

---

## 🧭 CÓMO USAR INSOMNIA — GUÍA RÁPIDA

Antes de empezar, entiende la diferencia entre los tipos de petición y cómo configurarlas:

### ¿Qué significa GET, POST, PUT, DELETE?

| Método | ¿Para qué sirve? | ¿Lleva body (JSON)? |
|---|---|---|
| `GET` | **Consultar / Leer** datos | ❌ No |
| `POST` | **Crear** un nuevo registro | ✅ Sí |
| `PUT` | **Editar** un registro existente | ✅ Sí |
| `DELETE` | **Eliminar** un registro | ❌ No |

---

### Cómo configurar una petición en Insomnia paso a paso

#### Para una petición `GET` (consultar):
1. Abre Insomnia y haz clic en **New Request**.
2. En el selector de método (a la izquierda de la URL), deja **GET**.
3. Escribe la URL en el campo grande, por ejemplo: `http://localhost:5003/api/Clientes`
4. Ve a la pestaña **Auth → Bearer Token** y pega tu token en el campo TOKEN.
5. Haz clic en **Send**.
6. La respuesta aparecerá en el panel derecho. Si ves un JSON con datos, ¡funcionó!

#### Para una petición `POST` (crear):
1. Crea una nueva petición y cambia el método a **POST**.
2. Escribe la URL, por ejemplo: `http://localhost:5003/api/Clientes/registrar-con-vehiculo`
3. Ve a **Auth → Bearer Token** y pega tu token.
4. Ve a la pestaña **Body** y selecciona **JSON** en el menú desplegable.
5. Escribe o pega el JSON con los datos que quieres crear (ver cada sección abajo).
6. Haz clic en **Send**.
7. Si ves `201 Created` o `200 OK` en el panel derecho, el registro fue creado.

#### Para una petición `PUT` (editar un registro existente):
> [!IMPORTANT]
> El `PUT` **reemplaza completamente** los datos del registro. Debes enviar **todos los campos**, incluso los que no cambiaron. Si omites un campo, quedará vacío en la base de datos.

1. Crea una nueva petición y cambia el método a **PUT**.
2. Escribe la URL **incluyendo el ID** del registro que quieres editar.
   - Ejemplo para editar el cliente con Id 2: `http://localhost:5003/api/Clientes/2`
   - *(El número al final es el ID. Cámbialo según el registro que quieres editar.)*
3. Ve a **Auth → Bearer Token** y pega tu token.
4. Ve a la pestaña **Body → JSON** y escribe el JSON con los **nuevos valores** que quieres guardar.
5. Haz clic en **Send**.
6. Si ves `200 OK` y el JSON de respuesta tiene los datos actualizados, ¡la edición fue exitosa!

#### Para una petición `DELETE` (eliminar):
1. Crea una nueva petición y cambia el método a **DELETE**.
2. Escribe la URL con el ID del registro a eliminar: `http://localhost:5003/api/Clientes/2`
3. Ve a **Auth → Bearer Token** y pega tu token.
4. **No necesitas Body.**
5. Haz clic en **Send**.
6. Si ves `204 No Content`, el registro fue eliminado correctamente.

---

### Cómo autenticarte (obtener y usar el token)

> [!IMPORTANT]
> Casi todos los endpoints requieren un token JWT. Sin él, recibirás `401 Unauthorized`.

1. Haz `POST` al endpoint de login (sin token, es público).
2. En la respuesta verás un campo `"token"` con un texto largo. **Cópialo completo.**
3. En cada petición protegida, ve a la pestaña **Auth**, selecciona **Bearer Token** y pega el token en el campo **TOKEN**.
4. El token dura **60 minutos**. Pasado ese tiempo, debes hacer login de nuevo.

---

## 📊 DATOS SEMILLA (Pre-cargados automáticamente al iniciar)

| Entidad | Id | Datos |
|---|---|---|
| Usuario | 1 | `admin@taller.com` / `admin123` — Rol: `Admin` |
| Usuario | 2 | `mecanico@taller.com` / `mecanico123` — Rol: `Mecanico` |
| Cliente | 1 | Juan Perez — `juan@perez.com` |
| Vehículo | 1 | Toyota Corolla 2020 — VIN: `VIN1234567890COROLLA` — ClienteId: 1 |
| Repuesto | 1 | Filtro de Aceite — `FILT-ACEITE` — $15.50 — Stock: 50 |
| Repuesto | 2 | Pastillas de freno — `PAST-FREN-DEL` — $45.00 — Stock: 20 |
| Repuesto | 3 | Bujía de platino — `BUJIA-PLAT` — $8.20 — Stock: 100 |

---
---

## 👤 USUARIOS — `/api/Usuarios`

---

### `POST /api/Usuarios/login` — Iniciar Sesión
**🔓 Sin token** — este endpoint es público.

**Configura en Insomnia:**
- Método: `POST`
- URL: `http://localhost:5003/api/Usuarios/login`
- Auth: *ninguna*
- Body (JSON):
```json
{
  "correo": "admin@taller.com",
  "password": "admin123"
}
```

**Respuesta esperada `200 OK`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "rol": "Admin",
  "correo": "admin@taller.com"
}
```
➡️ **Copia el valor de `"token"` y úsalo en el campo Bearer Token de las demás peticiones.**

*Otras credenciales disponibles: `mecanico@taller.com` / `mecanico123`*

---

### `GET /api/Usuarios` — Listar todos los usuarios
**🔐 Rol requerido:** `Admin`

- Método: `GET`
- URL: `http://localhost:5003/api/Usuarios?pageNumber=1&pageSize=10`
- Auth: Bearer Token

---

### `GET /api/Usuarios/{id}` — Ver un usuario específico
**🔐 Rol requerido:** `Admin`

- Método: `GET`
- URL: `http://localhost:5003/api/Usuarios/1` *(cambia el `1` por el ID que buscas)*
- Auth: Bearer Token

---

### `POST /api/Usuarios` — Crear nuevo usuario
**🔐 Rol requerido:** `Admin`

- Método: `POST`
- URL: `http://localhost:5003/api/Usuarios`
- Auth: Bearer Token
- Body (JSON):
```json
{
  "correo": "recepcionista@taller.com",
  "password": "recepcionista123",
  "rol": "Recepcionista"
}
```
*Roles válidos: `Admin`, `Mecanico`, `Recepcionista`*

---

### `PUT /api/Usuarios/{id}` — Editar usuario existente
**🔐 Rol requerido:** `Admin`

> Úsalo cuando quieras cambiar el correo, contraseña o rol de un usuario ya creado.

- Método: `PUT`
- URL: `http://localhost:5003/api/Usuarios/3` *(reemplaza `3` con el ID del usuario a editar)*
- Auth: Bearer Token
- Body (JSON) — **incluye todos los campos aunque no todos cambien:**
```json
{
  "correo": "recepcionista@taller.com",
  "password": "nuevaPassword456",
  "rol": "Recepcionista"
}
```

---

### `DELETE /api/Usuarios/{id}` — Eliminar usuario
**🔐 Rol requerido:** `Admin`

- Método: `DELETE`
- URL: `http://localhost:5003/api/Usuarios/3` *(reemplaza `3` con el ID del usuario a eliminar)*
- Auth: Bearer Token
- Sin body.
- Respuesta: `204 No Content`

---
---

## 🧑 CLIENTES — `/api/Clientes`

---

### `POST /api/Clientes/registrar-con-vehiculo` — Crear cliente + vehículos
**🔐 Rol requerido:** `Admin`, `Recepcionista`

Crea un nuevo cliente y opcionalmente uno o más vehículos en una sola operación.

- Método: `POST`
- URL: `http://localhost:5003/api/Clientes/registrar-con-vehiculo`
- Auth: Bearer Token
- Body (JSON):
```json
{
  "nombre": "Maria Lopez",
  "telefono": "555-8888",
  "correo": "maria@lopez.com",
  "vehiculos": [
    {
      "marca": "Honda",
      "modelo": "Civic",
      "anio": 2021,
      "vin": "VINHONDA2021CIVIC001",
      "kilometraje": 25000
    }
  ]
}
```
> Si no quieres añadir vehículos ahora, envía `"vehiculos": []`.

---

### `POST /api/Clientes/{id}/vehiculos` — Añadir vehículos a cliente existente
**🔐 Rol requerido:** `Admin`, `Recepcionista`

> Útil cuando ya tienes un cliente registrado y quiere ingresar un auto adicional al taller.

- Método: `POST`
- URL: `http://localhost:5003/api/Clientes/2/vehiculos` *(reemplaza `2` con el ID del cliente)*
- Auth: Bearer Token
- Body (JSON) — **es un array, aunque solo agregues uno:**
```json
[
  {
    "marca": "Toyota",
    "modelo": "Yaris",
    "anio": 2022,
    "vin": "VINYARIS2022001",
    "kilometraje": 10000
  }
]
```

**Respuesta `200 OK`:**
```json
{
  "message": "1 vehículo(s) agregado(s) al cliente Id 2.",
  "vehiculos": [
    { "id": 3, "marca": "Toyota", "modelo": "Yaris", "vin": "VINYARIS2022001" }
  ]
}
```

---

### `GET /api/Clientes` — Listar todos los clientes
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Clientes?pageNumber=1&pageSize=10`
- Auth: Bearer Token

---

### `GET /api/Clientes/{id}` — Ver un cliente específico
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Clientes/2` *(cambia el `2` por el ID que buscas)*
- Auth: Bearer Token

---

### `PUT /api/Clientes/{id}` — Editar datos de un cliente
**🔐 Rol requerido:** `Admin`, `Recepcionista`

> Úsalo para corregir el nombre, teléfono o correo de un cliente ya registrado.

- Método: `PUT`
- URL: `http://localhost:5003/api/Clientes/2` *(reemplaza `2` con el ID del cliente a editar)*
- Auth: Bearer Token
- Body (JSON) — **escribe los nuevos datos. Todos los campos son obligatorios:**
```json
{
  "nombre": "Maria Lopez Actualizada",
  "telefono": "555-9999",
  "correo": "maria.nueva@lopez.com",
  "vehiculos": []
}
```
*El campo `"vehiculos": []` es requerido por el DTO pero no modifica los vehículos existentes.*

---

### `DELETE /api/Clientes/{id}` — Eliminar cliente
**🔐 Rol requerido:** `Admin`

> ⚠️ Fallará si el cliente tiene vehículos con órdenes de servicio activas (no cerradas).

- Método: `DELETE`
- URL: `http://localhost:5003/api/Clientes/2`
- Auth: Bearer Token
- Sin body.
- Respuesta: `204 No Content`

---
---

## 🚗 VEHÍCULOS — `/api/Vehiculos`

---

### `GET /api/Vehiculos` — Listar vehículos (con filtros opcionales)
**🔐 Rol requerido:** Cualquier usuario autenticado

Puedes filtrar por cliente o buscar por VIN parcial.

- Método: `GET`
- URLs de ejemplo:
```
http://localhost:5003/api/Vehiculos
http://localhost:5003/api/Vehiculos?clienteId=2
http://localhost:5003/api/Vehiculos?vin=HONDA
http://localhost:5003/api/Vehiculos?clienteId=2&pageNumber=1&pageSize=5
```

---

### `GET /api/Vehiculos/{id}` — Ver un vehículo específico
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Vehiculos/1`
- Auth: Bearer Token

---

### `POST /api/Vehiculos?clienteId={id}` — Registrar un vehículo a un cliente
**🔐 Rol requerido:** `Admin`, `Recepcionista`

> Alternativa para agregar un solo vehículo a un cliente existente. El `clienteId` va en la URL como parámetro de query, **no en el body**.

- Método: `POST`
- URL: `http://localhost:5003/api/Vehiculos?clienteId=2`
- Auth: Bearer Token
- Body (JSON):
```json
{
  "marca": "Chevrolet",
  "modelo": "Spark",
  "anio": 2023,
  "vin": "VINSPARK2023001",
  "kilometraje": 5000
}
```

---

### `PUT /api/Vehiculos/{id}` — Editar datos de un vehículo
**🔐 Rol requerido:** `Admin`, `Recepcionista`

> Úsalo para actualizar el kilometraje, corregir el VIN u otros datos del vehículo.

- Método: `PUT`
- URL: `http://localhost:5003/api/Vehiculos/2` *(reemplaza `2` con el ID del vehículo a editar)*
- Auth: Bearer Token
- Body (JSON) — **incluye todos los campos:**
```json
{
  "marca": "Honda",
  "modelo": "Civic",
  "anio": 2021,
  "vin": "VINHONDA2021CIVIC001",
  "kilometraje": 30000
}
```

---

### `DELETE /api/Vehiculos/{id}` — Eliminar vehículo
**🔐 Rol requerido:** `Admin`

> ⚠️ Fallará si el vehículo tiene órdenes de servicio activas.

- Método: `DELETE`
- URL: `http://localhost:5003/api/Vehiculos/2`
- Auth: Bearer Token
- Sin body.
- Respuesta: `204 No Content`

---
---

## 🔧 REPUESTOS — `/api/Repuestos`

> **Rate Limit:** Máximo **30 peticiones por minuto.**

---

### `GET /api/Repuestos` — Ver inventario completo
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Repuestos?pageNumber=1&pageSize=10`
- Auth: Bearer Token

---

### `GET /api/Repuestos/{id}` — Ver un repuesto específico
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Repuestos/1`
- Auth: Bearer Token

---

### `POST /api/Repuestos` — Crear nuevo repuesto
**🔐 Rol requerido:** `Admin`

- Método: `POST`
- URL: `http://localhost:5003/api/Repuestos`
- Auth: Bearer Token
- Body (JSON):
```json
{
  "codigo": "CORREA-DIST-01",
  "descripcion": "Correa de distribución reforzada",
  "cantidadStock": 15,
  "precioUnitario": 75.00
}
```

---

### `PUT /api/Repuestos/{id}` — Editar repuesto existente
**🔐 Rol requerido:** `Admin`

> Úsalo para cambiar el precio, actualizar el stock manualmente o corregir la descripción.

- Método: `PUT`
- URL: `http://localhost:5003/api/Repuestos/4` *(reemplaza `4` con el ID del repuesto a editar)*
- Auth: Bearer Token
- Body (JSON) — **todos los campos son obligatorios:**
```json
{
  "codigo": "CORREA-DIST-01",
  "descripcion": "Correa de distribución (descripción actualizada)",
  "cantidadStock": 20,
  "precioUnitario": 80.00
}
```

---

### `DELETE /api/Repuestos/{id}` — Eliminar repuesto
**🔐 Rol requerido:** `Admin`

- Método: `DELETE`
- URL: `http://localhost:5003/api/Repuestos/4`
- Auth: Bearer Token
- Sin body.
- Respuesta: `204 No Content`

---
---

## 📋 ÓRDENES DE SERVICIO — `/api/OrdenesServicio`

> **Rate Limit:** Máximo **60 peticiones por minuto.**

---

### Referencia: Estados posibles de una orden

| Estado | Descripción |
|---|---|
| `Ingresada` | Recién creada — estado inicial automático |
| `EnReparacion` | El mecánico está trabajando |
| `Terminada` | Trabajo listo, pendiente de facturar |
| `Cerrada` | Facturada y cerrada — estado final |
| `Cancelada` | Cancelada — estado final |

### Referencia: Tipos de servicio y costo de mano de obra

| TipoServicio | Costo Mano de Obra |
|---|---|
| `MantenimientoPreventivo` | $150.00 |
| `Reparacion` | $200.00 |
| `Diagnostico` | $80.00 |

### Fórmula de facturación

```
Total Repuestos  = Σ (PrecioUnitario × Cantidad de cada repuesto usado)
Total Mano Obra  = Costo fijo según TipoServicio de la orden
──────────────────────────────────────────────────────────────────────
TOTAL FACTURA    = Total Repuestos + Total Mano Obra
```

**Ejemplo con datos semilla** (repuesto 1 ×2 unidades + repuesto 3 ×4 unidades, servicio MantenimientoPreventivo):
```
Filtro de Aceite:   $15.50 × 2 uds = $31.00
Bujía de platino:   $ 8.20 × 4 uds = $32.80
                    ─────────────────────────
                    Total Repuestos = $63.80
                    Total Mano Obra = $150.00
                    ─────────────────────────
                    TOTAL FACTURA   = $213.80
```

---

### `POST /api/OrdenesServicio` — Crear orden de servicio
**🔐 Rol requerido:** `Admin`, `Recepcionista`

El cliente deja el auto. Se registra qué servicio necesita y qué mecánico lo atenderá.

- Método: `POST`
- URL: `http://localhost:5003/api/OrdenesServicio`
- Auth: Bearer Token
- Body (JSON):
```json
{
  "vehiculoId": 2,
  "tipoServicio": "MantenimientoPreventivo",
  "mecanicoId": 2
}
```

**Respuesta `200 OK`:**
```json
{
  "id": 1,
  "vehiculoId": 2,
  "tipoServicio": "MantenimientoPreventivo",
  "estado": "Ingresada",
  "mecanicoId": 2,
  "fechaIngreso": "2026-06-02T18:00:00Z",
  "aprobadaPorCliente": false
}
```
➡️ **Anota el `"id"` que te devuelve** — lo necesitarás en los siguientes pasos.

---

### `PUT /api/OrdenesServicio/{id}/trabajo?nuevoEstado={estado}` — Registrar trabajo del mecánico
**🔐 Rol requerido:** `Admin`, `Mecanico`

El mecánico reporta qué repuestos usó y cambia el estado de la orden. El stock de esos repuestos se descuenta automáticamente.

- Método: `PUT`
- URL: `http://localhost:5003/api/OrdenesServicio/1/trabajo?nuevoEstado=EnReparacion`
  - *(reemplaza `1` con el ID de la orden)*
  - *(cambia `EnReparacion` por el estado que quieras asignar)*
- Auth: Bearer Token
- Body (JSON) — **lista de repuestos utilizados:**
```json
[
  {
    "repuestoId": 1,
    "cantidad": 2
  },
  {
    "repuestoId": 3,
    "cantidad": 4
  }
]
```

**Respuesta `200 OK`:**
```json
{
  "message": "Trabajo actualizado correctamente."
}
```

---

### `POST /api/OrdenesServicio/{id}/facturar` — Generar factura y cerrar la orden
**🔐 Rol requerido:** `Admin`, `Mecanico`

El auto está listo. El sistema calcula el total, genera la factura y cierra la orden automáticamente.

- Método: `POST`
- URL: `http://localhost:5003/api/OrdenesServicio/1/facturar` *(reemplaza `1` con el ID de la orden)*
- Auth: Bearer Token
- **Sin body** — deja el body vacío.

**Respuesta `200 OK`:**
```json
{
  "id": 1,
  "ordenServicioId": 1,
  "resumenServicios": "Servicio de MantenimientoPreventivo para vehículo 2",
  "totalRepuestos": 63.80,
  "totalManoObra": 150.00,
  "total": 213.80,
  "fechaGeneracion": "2026-06-02T18:30:00Z"
}
```

---

### `GET /api/OrdenesServicio` — Listar todas las órdenes
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/OrdenesServicio?pageNumber=1&pageSize=10`
- Auth: Bearer Token

---
---

## 🧾 FACTURAS — `/api/Facturas`

Las facturas son **inmutables** — una vez generadas no se pueden modificar ni eliminar. Solo se pueden consultar.

---

### `GET /api/Facturas` — Listar facturas (con filtros opcionales)
**🔐 Rol requerido:** Cualquier usuario autenticado

Filtra por cliente, por orden de servicio específica, o por rango de fechas.

- Método: `GET`
- URLs de ejemplo:
```
http://localhost:5003/api/Facturas
http://localhost:5003/api/Facturas?clienteId=2
http://localhost:5003/api/Facturas?ordenServicioId=1
http://localhost:5003/api/Facturas?fechaInicio=2026-06-01&fechaFin=2026-06-30
http://localhost:5003/api/Facturas?clienteId=2&pageNumber=1&pageSize=10
```

---

### `GET /api/Facturas/{id}` — Ver una factura por su ID
**🔐 Rol requerido:** Cualquier usuario autenticado

- Método: `GET`
- URL: `http://localhost:5003/api/Facturas/1`
- Auth: Bearer Token

---

### `GET /api/Facturas/orden/{ordenId}` — Ver la factura de una orden de servicio
**🔐 Rol requerido:** Cualquier usuario autenticado

> Útil si tienes el ID de la orden y quieres ver directamente su factura sin buscar el ID de la factura.

- Método: `GET`
- URL: `http://localhost:5003/api/Facturas/orden/1` *(reemplaza `1` con el ID de la orden)*
- Auth: Bearer Token

---
---

## 🔍 AUDITORÍAS — `/api/Auditorias`

El sistema registra automáticamente **cada escritura** (crear, editar, eliminar) en todas las entidades, guardando quién hizo el cambio, cuándo y qué campos cambiaron.

---

### `GET /api/Auditorias` — Ver bitácora completa
**🔐 Rol requerido:** `Admin` — exclusivo

Los registros vienen del más reciente al más antiguo.

- Método: `GET`
- URL: `http://localhost:5003/api/Auditorias?pageNumber=1&pageSize=20`
- Auth: Bearer Token (solo funciona con token de Admin)

**Respuesta `200 OK` (ejemplo):**
```json
[
  {
    "id": 5,
    "entidadAfectada": "Vehiculo",
    "accion": "Added",
    "usuarioId": 1,
    "usuarioCorreo": "admin@taller.com",
    "fechaAccion": "2026-06-02T18:30:00Z",
    "detalles": "Entidad Vehiculo con estado Added. Nuevos valores: Marca: Honda, Modelo: Civic, ..."
  }
]
```

---
---

## 🚀 FLUJO SECUENCIAL COMPLETO DEL TALLER

Sigue estos pasos en orden para probar el ciclo de vida completo de un servicio en el taller:

| Paso | Método | URL | Quién lo hace |
|---|---|---|---|
| 1 | `POST` | `/api/Usuarios/login` | Cualquiera — obtiene el token |
| 2 | `POST` | `/api/Clientes/registrar-con-vehiculo` | Recepcionista / Admin |
| 3 | `POST` | `/api/OrdenesServicio` | Recepcionista / Admin |
| 4 | `PUT` | `/api/OrdenesServicio/1/trabajo?nuevoEstado=EnReparacion` | Mecánico / Admin |
| 5 | `POST` | `/api/OrdenesServicio/1/facturar` | Mecánico / Admin |
| 6 | `GET` | `/api/Facturas?clienteId=2` | Cualquier autenticado |
| 7 | `GET` | `/api/Auditorias` | Solo Admin |
