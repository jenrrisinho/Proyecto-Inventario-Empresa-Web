using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventarioEmpresaWeb.Models
{
    public class Empleado
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("dni")]
        public string Dni { get; set; }

        [BsonElement("cargo")]
        public string Cargo { get; set; }

        [BsonElement("area_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AreaId { get; set; }

        [BsonElement("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; }

        // Propiedad solo para mostrar en las vistas; no se guarda en Mongo.
        [BsonIgnore]
        public string AreaNombre { get; set; }
    }
}
