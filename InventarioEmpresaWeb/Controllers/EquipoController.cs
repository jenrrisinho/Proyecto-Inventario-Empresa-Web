using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using InventarioEmpresaWeb.Data;
using InventarioEmpresaWeb.Models;

namespace InventarioEmpresaWeb.Controllers
{
    public class EquipoController : Controller
    {
        private readonly IMongoCollection<Equipo> _equipos;
        private readonly IMongoCollection<Area> _areas;
        private readonly IMongoCollection<Empleado> _empleados;

        public EquipoController(DBConnection db)
        {
            _equipos = db.Database.GetCollection<Equipo>("equipos");
            _areas = db.Database.GetCollection<Area>("areas");
            _empleados = db.Database.GetCollection<Empleado>("empleados");
        }

        public IActionResult Index(string areaId, string estado)
        {
            var filtroBuilder = Builders<Equipo>.Filter;
            var filtros = new List<FilterDefinition<Equipo>>();

            if (!string.IsNullOrEmpty(areaId))
                filtros.Add(filtroBuilder.Eq(e => e.AreaId, areaId));
            if (!string.IsNullOrEmpty(estado))
                filtros.Add(filtroBuilder.Eq(e => e.Estado, estado));

            FilterDefinition<Equipo> filtro = filtros.Count > 0 ? filtroBuilder.And(filtros) : filtroBuilder.Empty;

            var equipos = _equipos.Find(filtro).ToList();
            var areas = _areas.Find(_ => true).ToList();
            var empleados = _empleados.Find(_ => true).ToList();

            foreach (var eq in equipos)
            {
                var area = areas.FirstOrDefault(a => a.Id == eq.AreaId);
                eq.AreaNombre = area != null ? area.Nombre : "";

                var emp = empleados.FirstOrDefault(x => x.Id == eq.AsignadoA);
                eq.EmpleadoNombre = emp != null ? emp.Nombre : "(sin asignar)";
            }

            ViewBag.Areas = new SelectList(areas, "Id", "Nombre", areaId);
            ViewBag.Estados = new SelectList(new List<string> { "activo", "en mantenimiento", "dado de baja" }, estado);

            return View(equipos);
        }

        private void CargarListasDesplegables(string areaSeleccionada, string empleadoSeleccionado)
        {
            ViewBag.Areas = new SelectList(_areas.Find(_ => true).ToList(), "Id", "Nombre", areaSeleccionada);
            ViewBag.Empleados = new SelectList(_empleados.Find(_ => true).ToList(), "Id", "Nombre", empleadoSeleccionado);
        }

        private Dictionary<string, string> ParsearEspecificaciones(string texto)
        {
            var resultado = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(texto)) return resultado;

            var lineas = texto.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var linea in lineas)
            {
                var partes = linea.Split(new char[] { '=' }, 2);
                if (partes.Length == 2)
                {
                    var clave = partes[0].Trim();
                    var valor = partes[1].Trim();
                    if (!string.IsNullOrEmpty(clave))
                    {
                        resultado[clave] = valor;
                    }
                }
            }
            return resultado;
        }

        private string EspecificacionesATexto(Dictionary<string, string> specs)
        {
            if (specs == null) return "";
            var lineas = specs.Select(kv => kv.Key + "=" + kv.Value);
            return string.Join(Environment.NewLine, lineas);
        }

        public IActionResult Create()
        {
            CargarListasDesplegables(null, null);
            var equipo = new Equipo();
            equipo.Estado = "activo";
            return View(equipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Equipo equipo)
        {
            if (string.IsNullOrWhiteSpace(equipo.CodigoInventario))
            {
                ModelState.AddModelError("CodigoInventario", "El código de inventario es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }

            if (string.IsNullOrEmpty(equipo.AreaId)) equipo.AreaId = null;
            if (string.IsNullOrEmpty(equipo.AsignadoA)) equipo.AsignadoA = null;
            equipo.Especificaciones = ParsearEspecificaciones(equipo.EspecificacionesTexto);
            equipo.HistorialMantenimiento = new List<MantenimientoRegistro>();

            try
            {
                _equipos.InsertOne(equipo);
                TempData["Mensaje"] = "Equipo registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoWriteException ex)
            {
                ModelState.AddModelError("", "La base de datos rechazó la operación (código duplicado o validación): " + ex.Message);
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }
        }

        public IActionResult Edit(string id)
        {
            var equipo = _equipos.Find(e => e.Id == id).FirstOrDefault();
            if (equipo == null) return NotFound();

            equipo.EspecificacionesTexto = EspecificacionesATexto(equipo.Especificaciones);
            CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
            return View(equipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Equipo equipo)
        {
            if (string.IsNullOrWhiteSpace(equipo.CodigoInventario))
            {
                ModelState.AddModelError("CodigoInventario", "El código de inventario es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }

            if (string.IsNullOrEmpty(equipo.AreaId)) equipo.AreaId = null;
            if (string.IsNullOrEmpty(equipo.AsignadoA)) equipo.AsignadoA = null;
            var especificaciones = ParsearEspecificaciones(equipo.EspecificacionesTexto);

            try
            {
                var filtro = Builders<Equipo>.Filter.Eq(e => e.Id, id);
                var update = Builders<Equipo>.Update
                    .Set(e => e.CodigoInventario, equipo.CodigoInventario)
                    .Set(e => e.Tipo, equipo.Tipo)
                    .Set(e => e.Marca, equipo.Marca)
                    .Set(e => e.Modelo, equipo.Modelo)
                    .Set(e => e.NumeroSerie, equipo.NumeroSerie)
                    .Set(e => e.Estado, equipo.Estado)
                    .Set(e => e.AreaId, equipo.AreaId)
                    .Set(e => e.AsignadoA, equipo.AsignadoA)
                    .Set(e => e.Especificaciones, especificaciones);

                _equipos.UpdateOne(filtro, update);
                TempData["Mensaje"] = "Equipo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoWriteException ex)
            {
                ModelState.AddModelError("", "La base de datos rechazó la operación: " + ex.Message);
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                CargarListasDesplegables(equipo.AreaId, equipo.AsignadoA);
                return View(equipo);
            }
        }

        public IActionResult Delete(string id)
        {
            var equipo = _equipos.Find(e => e.Id == id).FirstOrDefault();
            if (equipo == null) return NotFound();
            return View(equipo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _equipos.DeleteOne(e => e.Id == id);
                TempData["Mensaje"] = "Equipo eliminado.";
            }
            catch (MongoCommandException ex)
            {
                TempData["Error"] = "Operación no autorizada por el servidor: " + ex.Message;
            }
            catch (MongoException ex)
            {
                TempData["Error"] = "Error de MongoDB: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(string id)
        {
            var equipo = _equipos.Find(e => e.Id == id).FirstOrDefault();
            if (equipo == null) return NotFound();

            var area = _areas.Find(a => a.Id == equipo.AreaId).FirstOrDefault();
            equipo.AreaNombre = area != null ? area.Nombre : "";

            var emp = _empleados.Find(x => x.Id == equipo.AsignadoA).FirstOrDefault();
            equipo.EmpleadoNombre = emp != null ? emp.Nombre : "(sin asignar)";

            return View(equipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarMantenimiento(string id, string tipoMantenimiento, string tecnico, double costo)
        {
            if (string.IsNullOrWhiteSpace(tipoMantenimiento))
            {
                TempData["Error"] = "Indica el tipo de mantenimiento.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var registro = new MantenimientoRegistro
            {
                Fecha = DateTime.UtcNow,
                TipoMantenimiento = tipoMantenimiento,
                Tecnico = tecnico,
                Costo = costo
            };

            try
            {
                var filtro = Builders<Equipo>.Filter.Eq(e => e.Id, id);
                var update = Builders<Equipo>.Update.Push(e => e.HistorialMantenimiento, registro);
                _equipos.UpdateOne(filtro, update);
                TempData["Mensaje"] = "Mantenimiento agregado correctamente.";
            }
            catch (MongoCommandException ex)
            {
                TempData["Error"] = "Operación no autorizada por el servidor: " + ex.Message;
            }
            catch (MongoException ex)
            {
                TempData["Error"] = "No se pudo agregar el mantenimiento: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
