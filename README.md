# Inventario de Equipos — Proyecto BD2

Aplicación web ASP.NET MVC para la gestión de inventario de equipos de una empresa, usando **MongoDB** como base de datos. Permite administrar Áreas, Empleados y Equipos (con historial de mantenimiento), además de generar reportes mediante agregaciones.

---

## Tecnologías

- **Backend:** ASP.NET Core MVC (.NET 8) — Visual Studio 2022 / 2026
- **Base de datos:** MongoDB Community Server (7.0 u 8.0) + MongoDB.Driver (NuGet, v3.9.0)
- **Frontend:** Razor Views (`.cshtml`) + CSS
- **Herramientas:** MongoDB Compass (opcional, recomendado) / mongosh

---

## Estructura del proyecto

```
/
├── InventarioEmpresaWeb.sln
├── README.md
├── .gitignore
│
├── doc/
│   └── InventarioMongoDB_Exposicion.pdf   ← presentación del proyecto
│
├── InventarioEmpresaWeb/
│   ├── InventarioEmpresaWeb.csproj
│   ├── Program.cs
│   ├── appsettings.json              ← connection string de MongoDB
│   │
│   ├── Data/
│   │   └── DBConnection.cs           ← conexión a MongoDB inyectada por DI (singleton)
│   │
│   ├── Models/
│   │   ├── Area.cs
│   │   ├── Empleado.cs
│   │   ├── Equipo.cs
│   │   └── MantenimientoRegistro.cs
│   │
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── AreaController.cs
│   │   ├── EmpleadoController.cs
│   │   ├── EquipoController.cs
│   │   └── ReporteController.cs
│   │
│   ├── Views/
│   │   ├── _ViewImports.cshtml
│   │   ├── _ViewStart.cshtml
│   │   ├── Home/
│   │   │   ├── Index.cshtml
│   │   │   └── Error.cshtml
│   │   ├── Area/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── Delete.cshtml
│   │   ├── Empleado/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── Delete.cshtml
│   │   ├── Equipo/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   ├── Details.cshtml        ← historial de mantenimiento + agregar mantenimiento
│   │   │   └── Delete.cshtml
│   │   ├── Reporte/
│   │   │   └── Index.cshtml          ← 3 reportes de agregación
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   │
│   └── wwwroot/
│       └── css/
│           └── site.css
│
└── scripts/
    ├── 01_setup_database.js          ← colecciones + validación + índices
    ├── 02_seed_data.js               ← datos de prueba (Áreas, Empleados, Equipos)
    ├── 03_security_setup.js          ← usuarios y roles (admin, app, consulta)
    └── 04_aggregations_reference.js  ← consultas de los 3 reportes
```

---

## Módulos funcionales

| Módulo | Funcionalidad |
|---|---|
| **Áreas** | Listado, crear, editar, eliminar |
| **Empleados** | Listado, crear, editar, eliminar, con desplegable de área (referencia por `area_id`, resuelta manualmente en el controller ya que MongoDB no hace JOIN) |
| **Equipos** | CRUD completo, filtro dinámico por área/estado (`Builders<T>.Filter.And`), especificaciones técnicas embebidas (`Dictionary<string,string>`), vista "Ver" con historial de mantenimiento embebido (`$push`) y opción de agregar nuevo mantenimiento |
| **Reportes** | 3 reportes generados con el framework de agregación de MongoDB: áreas con su personal y equipos (`$match` → `$lookup` → `$project`), equipos agrupados por estado (`$group` → `$sort`), costo total de mantenimiento por equipo (`$unwind` → `$group`) |

---

## Arquitectura

- **Conexión Singleton:** `DBConnection` se registra una sola vez en `Program.cs` y se inyecta por constructor en cada controller.
- **Validación de esquema:** `$jsonSchema` exige campos obligatorios y tipos correctos antes de guardar (definido en `01_setup_database.js`).
- **Índices:** índice único en `codigo_inventario` evita equipos duplicados; otros índices aceleran las búsquedas.
- **Modelado referenciado** (Área ↔ Empleado/Equipo) y **documento embebido** (`historial_mantenimiento` dentro de cada Equipo).

---

## Seguridad

El proyecto implementa control de acceso de MongoDB (`--auth`) con 3 usuarios y roles diferenciados:

| Usuario | Rol | Base | Uso |
|---|---|---|---|
| `admin_inventario` | `root` | `admin` | Administración total |
| `app_inventario` | `readWrite` | `inventario_empresa` | Usado por la aplicación (connection string en `appsettings.json`) |
| `consulta_inventario` | `read` | `inventario_empresa` | Solo lectura, para pruebas de restricción de permisos |

---

## Cómo levantar el proyecto desde cero

### Requisitos previos

1. **MongoDB Community Server** (7.0 u 8.0) + **mongosh**
2. **MongoDB Compass** (opcional, recomendado)
3. **Visual Studio 2022 Community** con la carga de trabajo **"Desarrollo web y ASP.NET"** (ASP.NET and web development)
4. **.NET 8 SDK** (Visual Studio 2022 ya lo trae normalmente)

### Paso 1 — Levantar MongoDB y cargar los datos

Instala MongoDB, verifica que corre, y ejecuta desde la terminal (sin autenticación activada todavía):

```bash
mongosh --file scripts/01_setup_database.js
mongosh --file scripts/02_seed_data.js
```

Esto crea la base `inventario_empresa` con las colecciones `areas`, `empleados`, `equipos` (validación + índices) y datos de prueba.

### Paso 2 — Abrir el proyecto en Visual Studio

1. Abre `InventarioEmpresaWeb.sln`.
2. Deja que Visual Studio restaure el paquete NuGet `MongoDB.Driver` (3.9.0). Si no lo hace automáticamente: clic derecho en el proyecto → **Restaurar paquetes NuGet**.
3. Revisa `appsettings.json` — por defecto apunta a `mongodb://localhost:27017` sin autenticación, para la primera prueba.
4. Compila con **Ctrl+Shift+B**.

### Paso 3 — Ejecutar la aplicación

Presiona **F5**. Se abrirá el navegador en `https://localhost:xxxx` con el menú superior: **Áreas, Empleados, Equipos, Reportes**.

Prueba:
- **Áreas**: listar, crear, editar, eliminar.
- **Empleados**: lo mismo, con el desplegable de área.
- **Equipos**: lista con filtro por área/estado, CRUD completo, y el botón **"Ver"** que lleva al detalle donde se puede **agregar mantenimiento** y ver el historial.
- **Reportes**: los 3 reportes de agregación descritos arriba.

### Paso 4 — Activar seguridad (opcional)

1. Ejecuta `scripts/03_security_setup.js` (cambia antes las contraseñas de ejemplo).
2. Activa `authorization: enabled` en `mongod.cfg` y reinicia el servicio **"MongoDB Server"**.
3. Actualiza el `connectionStrings:MongoDB` en `appsettings.json` con el usuario `app_inventario`:
   ```json
   "MongoDB": "mongodb://app_inventario:TU_CLAVE@localhost:27017/inventario_empresa?authSource=inventario_empresa"
   ```
4. **Detén y vuelve a presionar F5** (ver nota importante abajo).
5. Para demostrar el rechazo de permisos, cambia temporalmente el connection string al usuario `consulta_inventario` (solo lectura) y prueba crear un equipo — debe mostrar el error de `MongoCommandException`.

⚠️ **Importante:** `appsettings.json` solo se lee al iniciar la aplicación. Si haces cambios mientras la app está corriendo, debes **Detener** y volver a presionar **F5**, no basta con guardar el archivo.

### Resumen rápido

```bash
# 1. Instalar Visual Studio + MongoDB Community Server + Compass
# 2. Ejecutar en orden (mongosh o terminal de Compass):
#    - scripts/01_setup_database.js
#    - scripts/02_seed_data.js
#    - scripts/03_security_setup.js   (editando antes las contraseñas de ejemplo)
# 3. Activar authorization: enabled en mongod.cfg y reiniciar el servicio "MongoDB Server"
# 4. Configurar appsettings.json con el usuario app_inventario
# 5. Abrir InventarioEmpresaWeb.sln → F5
```

---

## Solución de problemas comunes

| Problema | Causa probable | Solución |
|---|---|---|
| "No se pudo conectar" / MongoDB no reinicia tras editar `mongod.cfg` | Servicio detenido, connection string mal, o error de indentación en `security:`/`authorization` | Revisa que el servicio de MongoDB esté activo; ambas líneas sin `#`, 2 espacios de indentación; revisa el log |
| NuGet no restaura `MongoDB.Driver` / no compila | Sin conexión a internet la primera vez | Verifica tu conexión |
| Error 400 al enviar un formulario (antiforgery) | Cookies bloqueadas o formulario mal generado | Asegúrate de no haber quitado los tag helpers (`asp-action`, etc.) de los `<form>` |
| `MongoCommandException: requires authentication` | Connection string sin usuario, o app no reiniciada tras editar `appsettings.json` | Revisa el connection string y haz Detener + F5 |
| Conecta con usuario pero sigue sin funcionar | Contraseña de `admin_inventario` confundida con la de `app_inventario` | Revisa usuario/contraseña usados |
| "E11000 duplicate key" al crear un equipo | Ya existe un equipo con ese `codigo_inventario` (índice único) | Usa un código distinto |
| Desplegables de Área/Empleado vacíos | No se ejecutó `02_seed_data.js` | Repite la carga de datos de prueba y verifica con Compass que `areas`/`empleados` tengan documentos |
| Cambios en historial de mantenimiento no aparecen | Se agregaron desde "Editar" en vez de "Ver" | El botón "Agregar mantenimiento" está en Equipos → Ver (Details); Editar no toca ese campo a propósito |

---

## Qué cubre el proyecto respecto al patrón MVC

- **Model**: `Area.cs`, `Empleado.cs`, `Equipo.cs`, `MantenimientoRegistro.cs` — con atributos BSON para mapear a los mismos campos de MongoDB (`nombre`, `area_id`, `historial_mantenimiento`, etc.)
- **View**: cada acción CRUD tiene su `.cshtml` propio, sin lógica de negocio — solo muestra lo que el Controller le pasa
- **Controller**: `AreaController`, `EmpleadoController`, `EquipoController`, `ReporteController` — manejan las peticiones, hablan con MongoDB a través de `DBConnection`, y deciden qué Vista mostrar
- Referencias (área ← empleados/equipos) y documento embebido (historial de mantenimiento)
- CRUD completo, validación de esquema, índices, agregación (`$lookup`, `$group`, `$unwind`), seguridad con roles

---

## Documentación adicional

La presentación del proyecto (exposición) está disponible en [`doc/InventarioMongoDB_Exposicion.pdf`](./doc/InventarioMongoDB_Exposicion.pdf).

---

## Autor(es)

Proyecto realizado para el curso de **Base de Datos II** — Facultad de Ingeniería Eléctrica y Electrónica, Universidad Nacional de Ingeniería.

- **Juan Carlos Matías Gonzales Avendaño** — Áreas y Empleados
- **Alexandro Achalma Galindo** — Equipos (CRUD y filtros)
- **Carlos Henrry Santana Palomino** — Mantenimiento y Reportes
- **Antony Jamel Lipa Benito** — Arquitectura y Seguridad
