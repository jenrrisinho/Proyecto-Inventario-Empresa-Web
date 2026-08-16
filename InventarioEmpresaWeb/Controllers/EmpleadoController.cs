using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using InventarioEmpresaWeb.Data;
using InventarioEmpresaWeb.Models;

namespace InventarioEmpresaWeb.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly IMongoCollection<Empleado> _empleados;
        private readonly IMongoCollection<Area> _areas;

        public EmpleadoController(DBConnection db)
        {
            _empleados = db.Database.GetCollection<Empleado>("empleados");
            _areas = db.Database.GetCollection<Area>("areas");
        }

        public IActionResult Index()
        {
            var empleados = _empleados.Find(_ => true).ToList();
            var areas = _areas.Find(_ => true).ToList();

            foreach (var emp in empleados)
            {
                var area = areas.FirstOrDefault(a => a.Id == emp.AreaId);
                emp.AreaNombre = area != null ? area.Nombre : "";
            }

            return View(empleados);
        }

        private SelectList ObtenerAreasParaSelect(string seleccionada)
        {
            var areas = _areas.Find(_ => true).ToList();
            return new SelectList(areas, "Id", "Nombre", seleccionada);
        }

        public IActionResult Create()
        {
            ViewBag.Areas = ObtenerAreasParaSelect(null);
            return View(new Empleado());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Empleado empleado)
        {
            if (string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Areas = ObtenerAreasParaSelect(empleado.AreaId);
                return View(empleado);
            }

            if (string.IsNullOrEmpty(empleado.AreaId)) empleado.AreaId = null;
            empleado.FechaIngreso = DateTime.UtcNow;

            try
            {
                _empleados.InsertOne(empleado);
                TempData["Mensaje"] = "Empleado registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                ViewBag.Areas = ObtenerAreasParaSelect(empleado.AreaId);
                return View(empleado);
            }
        }

        public IActionResult Edit(string id)
        {
            var empleado = _empleados.Find(e => e.Id == id).FirstOrDefault();
            if (empleado == null) return NotFound();
            ViewBag.Areas = ObtenerAreasParaSelect(empleado.AreaId);
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Empleado empleado)
        {
            if (string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Areas = ObtenerAreasParaSelect(empleado.AreaId);
                return View(empleado);
            }

            if (string.IsNullOrEmpty(empleado.AreaId)) empleado.AreaId = null;

            try
            {
                var filtro = Builders<Empleado>.Filter.Eq(e => e.Id, id);
                var update = Builders<Empleado>.Update
                    .Set(e => e.Nombre, empleado.Nombre)
                    .Set(e => e.Dni, empleado.Dni)
                    .Set(e => e.Cargo, empleado.Cargo)
                    .Set(e => e.AreaId, empleado.AreaId);

                _empleados.UpdateOne(filtro, update);
                TempData["Mensaje"] = "Empleado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                ViewBag.Areas = ObtenerAreasParaSelect(empleado.AreaId);
                return View(empleado);
            }
        }

        public IActionResult Delete(string id)
        {
            var empleado = _empleados.Find(e => e.Id == id).FirstOrDefault();
            if (empleado == null) return NotFound();
            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _empleados.DeleteOne(e => e.Id == id);
                TempData["Mensaje"] = "Empleado eliminado.";
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
    }
}
