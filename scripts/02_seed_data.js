// ============================================================
// 02_seed_data.js
// Inserta datos de prueba con referencias reales (ObjectId).
// Ejecutar DESPUÉS de 01_setup_database.js
//
//   mongosh --file 02_seed_data.js
// ============================================================

db = db.getSiblingDB("inventario_empresa");

// Limpieza previa (útil si necesitas volver a ejecutar el script desde cero)
db.equipos.deleteMany({});
db.empleados.deleteMany({});
db.areas.deleteMany({});

// --- Áreas ---
var areaSistemas = db.areas.insertOne({ nombre: "Sistemas", responsable: "Juan Pérez" }).insertedId;
var areaContabilidad = db.areas.insertOne({ nombre: "Contabilidad", responsable: "María Gómez" }).insertedId;
var areaRRHH = db.areas.insertOne({ nombre: "Recursos Humanos", responsable: "Carlos Ruiz" }).insertedId;

// --- Empleados ---
var empAna = db.empleados.insertOne({
  nombre: "Ana Torres",
  dni: "45678912",
  cargo: "Analista de Sistemas",
  area_id: areaSistemas,
  fecha_ingreso: new Date("2023-03-01")
}).insertedId;

var empLuis = db.empleados.insertOne({
  nombre: "Luis Fernández",
  dni: "41234567",
  cargo: "Contador",
  area_id: areaContabilidad,
  fecha_ingreso: new Date("2022-06-15")
}).insertedId;

var empRosa = db.empleados.insertOne({
  nombre: "Rosa Medina",
  dni: "40011223",
  cargo: "Asistente de RRHH",
  area_id: areaRRHH,
  fecha_ingreso: new Date("2024-01-10")
}).insertedId;

// --- Equipos ---
db.equipos.insertOne({
  codigo_inventario: "PC-001",
  tipo: "laptop",
  marca: "Dell",
  modelo: "Latitude 5420",
  numero_serie: "SN-DL-0001",
  estado: "activo",
  area_id: areaSistemas,
  asignado_a: empAna,
  especificaciones: { ram: "16GB", cpu: "Intel i7", almacenamiento: "512GB SSD" },
  historial_mantenimiento: [
    { fecha: new Date("2025-02-10"), tipo_mantenimiento: "Limpieza interna", tecnico: "Soporte TI", costo: 30 },
    { fecha: new Date("2025-08-15"), tipo_mantenimiento: "Cambio de batería", tecnico: "Soporte TI", costo: 120 }
  ]
});

db.equipos.insertOne({
  codigo_inventario: "PR-001",
  tipo: "impresora",
  marca: "HP",
  modelo: "LaserJet Pro M404",
  numero_serie: "SN-HP-0002",
  estado: "activo",
  area_id: areaContabilidad,
  asignado_a: null,
  especificaciones: { velocidad_ppm: "38", conexion: "red" },
  historial_mantenimiento: [
    { fecha: new Date("2025-05-01"), tipo_mantenimiento: "Cambio de tóner", tecnico: "Proveedor externo", costo: 80 }
  ]
});

db.equipos.insertOne({
  codigo_inventario: "SW-001",
  tipo: "switch",
  marca: "Cisco",
  modelo: "Catalyst 2960",
  numero_serie: "SN-CS-0003",
  estado: "activo",
  area_id: areaSistemas,
  asignado_a: null,
  especificaciones: { puertos: "24", velocidad: "1Gbps" },
  historial_mantenimiento: []
});

db.equipos.insertOne({
  codigo_inventario: "PC-002",
  tipo: "laptop",
  marca: "Lenovo",
  modelo: "ThinkPad T14",
  numero_serie: "SN-LN-0004",
  estado: "en mantenimiento",
  area_id: areaRRHH,
  asignado_a: empRosa,
  especificaciones: { ram: "8GB", cpu: "Intel i5", almacenamiento: "256GB SSD" },
  historial_mantenimiento: [
    { fecha: new Date("2026-06-20"), tipo_mantenimiento: "Reparación de pantalla", tecnico: "Soporte TI", costo: 250 }
  ]
});

db.equipos.insertOne({
  codigo_inventario: "PC-003",
  tipo: "laptop",
  marca: "HP",
  modelo: "ProBook 440",
  numero_serie: "SN-HP-0005",
  estado: "dado de baja",
  area_id: areaContabilidad,
  asignado_a: empLuis,
  especificaciones: { ram: "8GB", cpu: "Intel i5", almacenamiento: "500GB HDD" },
  historial_mantenimiento: [
    { fecha: new Date("2024-11-05"), tipo_mantenimiento: "Diagnóstico general", tecnico: "Soporte TI", costo: 40 }
  ]
});

print("Datos de prueba insertados correctamente.");
print("Área Sistemas: " + areaSistemas);
print("Área Contabilidad: " + areaContabilidad);
print("Área RRHH: " + areaRRHH);
