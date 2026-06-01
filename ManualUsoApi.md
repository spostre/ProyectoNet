# 🛠️ Guía Rápida de Pruebas: Flujo Completo del Taller (Insomnia)

Esta guía te muestra cómo probar **AutoTallerManager** siguiendo el **orden lógico del negocio** (flujo secuencial del taller).

---

## 📋 PREPARACIÓN (Paso Único)

El sistema ahora utiliza **MySQL** como motor de base de datos y está configurado para **crear y migrar la base de datos de forma 100% automática** al iniciar la API. No necesitas ejecutar comandos de Entity Framework manualmente.

### Pasos para iniciar:

1. **Asegura tu MySQL local**: Verifica que tu servidor MySQL esté activo (usualmente en el puerto `3306`).
2. **Configura tus credenciales**: Abre el archivo [appsettings.json](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/appsettings.json) y ajusta los campos de usuario (`User`) y contraseña (`Password` o `Pwd`) si tu servidor MySQL local usa credenciales personalizadas (por defecto viene configurado para `root` sin contraseña).
3. **Inicia la API**: Abre la terminal en la raíz del proyecto y ejecuta:
   ```bash
   dotnet run --project Api
   ```
   *Al iniciar, la API detectará si la base de datos `AutoTallerManager` no existe en tu MySQL, la creará de forma transparente, generará todas las tablas y sembrará los datos semilla de prueba. Quedará escuchando en `http://localhost:5003`.*

> [!IMPORTANT]
> **Cómo usar la seguridad en Insomnia:**
> En cada paso, tras hacer login, copia el token y pégalo en la pestaña **Auth -> Bearer Token** de tu petición actual en Insomnia.

---

## 🚶‍♂️ FLUJO SECUENCIAL DEL TALLER

### PASO 1: Iniciar Sesión (Autenticación)
*   **¿Por qué?:** Necesitas identificarte para obtener un Token JWT y que el sistema sepa tu rol y registre tus acciones.
*   **Rol Autorizado:** Cualquier empleado registrado.
*   **Método:** `POST`
*   **URL:** `http://localhost:5003/api/Usuarios/login`
*   **Cuerpo (JSON):**
```json
{
  "correo": "admin@taller.com",
  "password": "admin123"
}
```
*   **Respuesta (200 OK):** Te devolverá un `"token"`. Cópialo y úsalo como **Bearer Token** en los siguientes pasos.
*   *(Nota: También puedes loguearte como Recepcionista usando `"recepcionista@taller.com"` / `"recepcionista123"` o como Mecánico con `"mecanico@taller.com"` / `"mecanico123"`).*

---

### PASO 2: Registrar al Cliente y sus Vehículos
*   **¿Por qué?:** No puedes reparar un auto ni agendar una orden si no conocemos al dueño ni los datos del vehículo. Este paso crea ambos de forma segura en una sola transacción.
*   **Rol Autorizado:** Admin, Recepcionista
*   **Método:** `POST`
*   **URL:** `http://localhost:5003/api/Clientes/registrar-con-vehiculo`
*   **Cuerpo (JSON):**
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
      "vin": "VINHONDA987654321A",
      "kilometraje": 25000
    }
  ]
}
```
*   **Resultado:** Se creará María López con `Id: 2` y su Honda Civic con `Id: 2` (el ID `1` lo tienen los datos semilla de Juan Pérez).

---

### PASO 3: Crear la Orden de Servicio
*   **¿Por qué?:** El cliente deja el auto en el taller. Registramos qué servicio necesita, asignamos al mecánico encargado e iniciamos la orden en estado `Ingresada`.
*   **Rol Autorizado:** Admin, Recepcionista
*   **Método:** `POST`
*   **URL:** `http://localhost:5003/api/OrdenesServicio`
*   **Cuerpo (JSON):**
```json
{
  "vehiculoId": 2,
  "tipoServicio": "MantenimientoPreventivo",
  "mecanicoId": 2
}
```
*   *(Nota: Tipos de servicio válidos: `MantenimientoPreventivo` [$150], `Reparacion` [$200], `Diagnostico` [$80]).*
*   **Resultado:** Se creará la Orden con `Id: 1` y estado `"Ingresada"`.

---

### PASO 4: Registrar el Trabajo e Insumos del Mecánico
*   **¿Por qué?:** El mecánico realiza la reparación. Cambia el estado de la orden a `EnReparacion` y registra las piezas del inventario que consumió. El sistema **descontará automáticamente** estas piezas del stock.
*   **Rol Autorizado:** Admin, Mecanico
*   **Método:** `PUT`
*   **URL:** `http://localhost:5003/api/OrdenesServicio/1/trabajo?nuevoEstado=EnReparacion`
*   **Cuerpo (JSON):**
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
*   **Resultado:** La orden pasa a estar en proceso y se restan 2 filtros de aceite y 4 bujías del inventario.

---

### PASO 5: Generar Factura y Cerrar la Orden
*   **¿Por qué?:** El carro está listo. El sistema calcula la suma de los repuestos usados más el costo de la mano de obra del servicio, genera la factura electrónica inmutable y cierra la orden de servicio.
*   **Rol Autorizado:** Admin, Mecanico
*   **Método:** `POST`
*   **URL:** `http://localhost:5003/api/OrdenesServicio/1/facturar`
*   **Cuerpo (JSON):** *(Dejar vacío)*
*   **Resultado (200 OK):**
```json
{
  "id": 1,
  "ordenServicioId": 1,
  "resumenServicios": "Servicio de MantenimientoPreventivo para vehículo 2",
  "totalRepuestos": 63.80,
  "totalManoObra": 150.00,
  "total": 213.80,
  "fechaGeneracion": "2026-05-31T22:00:00Z"
}
```

---

## 🔍 CONTROL Y AUDITORÍA (Solo Administradores)

Una vez completado el flujo, puedes consultar los registros de control interno del taller:

### PASO A: Consultar la Bitácora de Auditoría
*   **¿Por qué?:** Saber con exactitud qué usuario del taller realizó qué operación (creaciones, modificaciones, eliminaciones) en segundo plano.
*   **Rol Autorizado:** Admin
*   **Método:** `GET`
*   **URL:** `http://localhost:5003/api/Auditorias?pageNumber=1&pageSize=10`
*   **Resultado:** Verás una lista detallada con los campos modificados, fecha y correo del responsable de cada paso anterior.

### PASO B: Consultar el Historial de Facturas
*   **¿Por qué?:** Llevar la contabilidad del taller y filtrar facturas por cliente o rango de fechas.
*   **Rol Autorizado:** Todos los autenticados
*   **Método:** `GET`
*   **URL:** `http://localhost:5003/api/Facturas?clienteId=2&pageNumber=1&pageSize=10`
