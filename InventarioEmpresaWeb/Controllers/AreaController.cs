using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using InventarioEmpresaWeb.Data;
using InventarioEmpresaWeb.Models;

namespace InventarioEmpresaWeb.Controllers
{
    public class AreaController : Controller
    {
        private readonly IMongoCollection<Area> _areas;

        public AreaController(DBConnection db)
        {
            _areas = db.Database.GetCollection<Area>("areas");
        }

        public IActionResult Index()
        {
            var lista = _areas.Find(_ => true).ToList();
            return View(lista);
        }

        public IActionResult Create()
        {
            return View(new Area());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Area area)
        {
            if (string.IsNullOrWhiteSpace(area.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid) return View(area);

            try
            {
                _areas.InsertOne(area);
                TempData["Mensaje"] = "Área registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                return View(area);
            }
        }

        public IActionResult Edit(string id)
        {
            var area = _areas.Find(a => a.Id == id).FirstOrDefault();
            if (area == null) return NotFound();
            return View(area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Area area)
        {
            if (string.IsNullOrWhiteSpace(area.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid) return View(area);

            try
            {
                var filtro = Builders<Area>.Filter.Eq(a => a.Id, id);
                var update = Builders<Area>.Update
                    .Set(a => a.Nombre, area.Nombre)
                    .Set(a => a.Responsable, area.Responsable);

                _areas.UpdateOne(filtro, update);
                TempData["Mensaje"] = "Área actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                ModelState.AddModelError("", "Error de MongoDB: " + ex.Message);
                return View(area);
            }
        }

        public IActionResult Delete(string id)
        {
            var area = _areas.Find(a => a.Id == id).FirstOrDefault();
            if (area == null) return NotFound();
            return View(area);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _areas.DeleteOne(a => a.Id == id);
                TempData["Mensaje"] = "Área eliminada.";
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
