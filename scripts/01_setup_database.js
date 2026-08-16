// ============================================================
// 01_setup_database.js
// Crea la base de datos, las 3 colecciones con validación
// ($jsonSchema) y los índices principales.
//
// Ejecutar con:
//   mongosh --file 01_setup_database.js
// o, dentro de una sesión de mongosh ya abierta:
//   load("01_setup_database.js")
// ============================================================

db = db.getSiblingDB("inventario_empresa");

// --- Colección areas ---
db.createCollection("areas", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["nombre"],
      properties: {
        nombre: { bsonType: "string", description: "obligatorio, texto" },
        responsable: { bsonType: "string" }
      }
    }
  }
});

// --- Colección empleados ---
db.createCollection("empleados", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["nombre"],
      properties: {
        nombre: { bsonType: "string", description: "obligatorio, texto" },
        dni: { bsonType: "string" },
        cargo: { bsonType: "string" },
        area_id: { bsonType: ["objectId", "null"] },
        fecha_ingreso: { bsonType: ["date", "null"] }
      }
    }
  }
});

// --- Colección equipos ---
db.createCollection("equipos", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["codigo_inventario", "tipo", "estado"],
      properties: {
        codigo_inventario: { bsonType: "string", description: "obligatorio, debe ser único" },
        tipo: { bsonType: "string", description: "obligatorio" },
        marca: { bsonType: "string" },
        modelo: { bsonType: "string" },
        numero_serie: { bsonType: "string" },
        estado: {
          enum: ["activo", "en mantenimiento", "dado de baja"],
          description: "obligatorio, solo puede ser uno de los 3 valores permitidos"
        },
        area_id: { bsonType: ["objectId", "null"] },
        asignado_a: { bsonType: ["objectId", "null"] },
        especificaciones: { bsonType: "object" },
        historial_mantenimiento: { bsonType: "array" }
      }
    }
  }
});

// --- Índices ---
db.equipos.createIndex({ codigo_inventario: 1 }, { unique: true });
db.equipos.createIndex({ area_id: 1 });
db.equipos.createIndex({ estado: 1 });
db.empleados.createIndex({ area_id: 1 });

print("Base de datos, colecciones, validación e índices creados correctamente.");
