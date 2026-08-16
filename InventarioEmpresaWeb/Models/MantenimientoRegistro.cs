using System;
using MongoDB.Bson.Serialization.Attributes;

namespace InventarioEmpresaWeb.Models
{
    /// <summary>
    /// Representa un registro embebido dentro del array
    /// "historial_mantenimiento" de un Equipo.
    /// </summary>
    public class MantenimientoRegistro
    {
        [BsonElement("fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("tipo_mantenimiento")]
        public string TipoMantenimiento { get; set; }

        [BsonElement("tecnico")]
        public string Tecnico { get; set; }

        [BsonElement("costo")]
        public double Costo { get; set; }
    }
}
