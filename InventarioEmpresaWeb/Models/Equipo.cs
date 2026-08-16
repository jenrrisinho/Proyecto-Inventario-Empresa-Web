using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventarioEmpresaWeb.Models
{
    public class Equipo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigo_inventario")]
        public string CodigoInventario { get; set; }

        [BsonElement("tipo")]
        public string Tipo { get; set; }

        [BsonElement("marca")]
        public string Marca { get; set; }

        [BsonElement("modelo")]
        public string Modelo { get; set; }

        [BsonElement("numero_serie")]
        public string NumeroSerie { get; set; }

        [BsonElement("estado")]
        public string Estado { get; set; }

        [BsonElement("area_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AreaId { get; set; }

        [BsonElement("asignado_a")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AsignadoA { get; set; }

        [BsonElement("especificaciones")]
        public Dictionary<string, string> Especificaciones { get; set; } = new Dictionary<string, string>();

        [BsonElement("historial_mantenimiento")]
        public List<MantenimientoRegistro> HistorialMantenimiento { get; set; } = new List<MantenimientoRegistro>();

        // Propiedades solo para las vistas; no se guardan en Mongo.
        [BsonIgnore]
        public string EspecificacionesTexto { get; set; }

        [BsonIgnore]
        public string AreaNombre { get; set; }

        [BsonIgnore]
        public string EmpleadoNombre { get; set; }
    }
}
