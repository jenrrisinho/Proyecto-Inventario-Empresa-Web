// ============================================================
// 04_aggregations_reference.js
// Las mismas 3 consultas de agregación que usa la aplicación C#
// en la pestaña "Reportes". Pruébalas aquí primero, en mongosh
// o en MongoDB Compass, para entender qué hacen antes de verlas
// ejecutarse desde la aplicación.
// ============================================================

db = db.getSiblingDB("inventario_empresa");

// 1) Área con sus empleados y equipos (cambia "Sistemas" por el área que quieras)
db.areas.aggregate([
  { $match: { nombre: "Sistemas" } },
  { $lookup: { from: "empleados", localField: "_id", foreignField: "area_id", as: "empleados" } },
  { $lookup: { from: "equipos", localField: "_id", foreignField: "area_id", as: "equipos" } }
]);

// 2) Conteo de equipos por estado
db.equipos.aggregate([
  { $group: { _id: "$estado", cantidad: { $sum: 1 } } },
  { $sort: { cantidad: -1 } }
]);

// 3) Costo total de mantenimiento por equipo
db.equipos.aggregate([
  { $unwind: "$historial_mantenimiento" },
  { $group: {
      _id: { codigo: "$codigo_inventario" },
      costo_total: { $sum: "$historial_mantenimiento.costo" },
      cantidad_mantenimientos: { $sum: 1 }
  }},
  { $sort: { costo_total: -1 } }
]);
