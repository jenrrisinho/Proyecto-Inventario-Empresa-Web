using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventarioEmpresaWeb.Models
{
    public class Area
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("responsable")]
        public string Responsable { get; set; }
    }
}
