using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using InventarioEmpresaWeb.Data;
using InventarioEmpresaWeb.Models;

namespace InventarioEmpresaWeb.Controllers
{
    public class ReporteController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _areas;
        private readonly IMongoCollection<BsonDocument> _equipos;
        private readonly IMongoCollection<Area> _areasTipadas;

        public ReporteController(DBConnection db)
        {
            _areas = db.Database.GetCollection<BsonDocument>("areas");
            _equipos = db.Database.GetCollection<BsonDocument>("equipos");
            _areasTipadas = db.Database.GetCollection<Area>("areas");
        }

        public IActionResult Index(string areaId)
        {
            ViewBag.Areas = new SelectList(_areasTipadas.Find(_ => true).ToList(), "Id", "Nombre", areaId);

            ViewBag.ReporteArea = !string.IsNullOrEmpty(areaId) ? EjecutarReporteArea(areaId) : null;
            ViewBag.ReporteEstados = EjecutarReporteConteoEstados();
            ViewBag.ReporteCostos = EjecutarReporteCostosMantenimiento();

            return View();
        }

        private List<BsonDocument> EjecutarReporteArea(string areaId)
        {
            var etapas = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(areaId))),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "empleados" },
                    { "localField", "_id" },
                    { "foreignField", "area_id" },
                    { "as", "empleados" }
                }),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "equipos" },
                    { "localField", "_id" },
                    { "foreignField", "area_id" },
                    { "as", "equipos" }
                })
            };

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = etapas;
            return _areas.Aggregate(pipeline).ToList();
        }

        private List<BsonDocument> EjecutarReporteConteoEstados()
        {
            var etapas = new List<BsonDocument>
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$estado" },
                    { "cantidad", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("cantidad", -1))
            };

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = etapas;
            return _equipos.Aggregate(pipeline).ToList();
        }

        private List<BsonDocument> EjecutarReporteCostosMantenimiento()
        {
            var etapas = new List<BsonDocument>
            {
                new BsonDocument("$unwind", "$historial_mantenimiento"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument { { "codigo", "$codigo_inventario" } } },
                    { "costo_total", new BsonDocument("$sum", "$historial_mantenimiento.costo") },
                    { "cantidad_mantenimientos", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("costo_total", -1))
            };

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = etapas;
            return _equipos.Aggregate(pipeline).ToList();
        }
    }
}
