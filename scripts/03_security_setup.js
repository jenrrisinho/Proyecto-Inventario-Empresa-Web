// ============================================================
// 03_security_setup.js
// Crea los usuarios y roles de MongoDB.
//
// IMPORTANTE: ejecuta este script ANTES de activar --auth,
// mientras todavía puedes conectarte sin autenticación.
// Después de crear los usuarios, activa la autenticación
// (ver GUIA_DESPLIEGUE.md, paso "Activar seguridad") y
// vuelve a conectarte usando admin_inventario.
//
//   mongosh --file 03_security_setup.js
// ============================================================

// 1) Usuario administrador a nivel de servidor
db = db.getSiblingDB("admin");
db.createUser({
  user: "admin_inventario",
  pwd: "CAMBIA_ESTA_CLAVE_ADMIN",
  roles: [ { role: "root", db: "admin" } ]
});

// 2) Usuario de la aplicación con permisos de lectura y escritura
//    Este es el usuario que debe usar la aplicación C# en el día a día
//    (es el que aparece precargado en la pantalla de login de la app).
db = db.getSiblingDB("inventario_empresa");
db.createUser({
  user: "app_inventario",
  pwd: "CAMBIA_ESTA_CLAVE_APP",
  roles: [ { role: "readWrite", db: "inventario_empresa" } ]
});

// 3) Usuario de solo lectura
//    Útil para demostrar en la sustentación que la app rechaza
//    intentos de insertar/editar cuando el usuario no tiene permisos.
db.createUser({
  user: "consulta_inventario",
  pwd: "CAMBIA_ESTA_CLAVE_CONSULTA",
  roles: [ { role: "read", db: "inventario_empresa" } ]
});

print("Usuarios creados correctamente:");
print(" - admin_inventario   (root, base: admin)");
print(" - app_inventario     (readWrite, base: inventario_empresa)");
print(" - consulta_inventario (read, base: inventario_empresa)");
print("");
print("Ahora activa --auth en MongoDB y vuelve a conectarte con admin_inventario.");
