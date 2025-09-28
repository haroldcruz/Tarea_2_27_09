using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Tarea2.Models;

namespace Tarea2.Controllers
{
    [Authorize] // Protege todo el controlador, solo usuarios autenticados pueden acceder
    public class TareasController : Controller
    {
        // Ruta del archivo JSON donde se guardan las tareas
        private string RutaArchivo => Server.MapPath("~/App_Data/tareas.json");

        // Lee las tareas desde el archivo JSON
        private List<Tarea> LeerTareas()
        {
            if (!System.IO.File.Exists(RutaArchivo))
                return new List<Tarea>();

            var json = System.IO.File.ReadAllText(RutaArchivo);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Tarea>();

            return JsonConvert.DeserializeObject<List<Tarea>>(json);
        }

        // Guarda la lista de tareas en el archivo JSON
        private void GuardarTareas(List<Tarea> tareas)
        {
            System.IO.File.WriteAllText(RutaArchivo, JsonConvert.SerializeObject(tareas, Formatting.Indented));
        }

        // GET: Tareas/Lista
        public ActionResult Lista()
        {
            var tareas = LeerTareas(); // todas las tareas
            return View(tareas);
        }

        // GET: Tareas/Nueva
        public ActionResult Nueva()
        {
            return View();
        }

        // POST: Tareas/Nueva
        [HttpPost]
        public ActionResult Nueva(Tarea tarea)
        {
            // Validación mínima del servidor
            if (tarea == null || string.IsNullOrWhiteSpace(tarea.Titulo)
                || string.IsNullOrWhiteSpace(tarea.Descripcion)
                || string.IsNullOrWhiteSpace(tarea.Lenguajes)
                || string.IsNullOrWhiteSpace(tarea.Categoria))
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, mensaje = "Complete todos los campos obligatorios." });
                ViewBag.Error = "Complete todos los campos obligatorios.";
                return View(tarea);
            }

            var tareas = LeerTareas();

            tarea.Id = Guid.NewGuid().ToString();
            tarea.Autor = User.Identity.Name ?? "anon";
            tarea.Calificaciones = tarea.Calificaciones ?? new List<Calificacion>();

            tareas.Add(tarea);
            GuardarTareas(tareas);

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, mensaje = "Tarea registrada correctamente" });
            }

            TempData["AlertaNuevaTarea"] = true;
            return RedirectToAction("Lista");
        }

        // GET: Tareas/Editar/{id}
        public ActionResult Editar(string id)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null) return HttpNotFound();
            if (tarea.Autor != User.Identity.Name) return new HttpStatusCodeResult(403);

            return View(tarea);
        }

        // POST: Tareas/Editar
        [HttpPost]
        public ActionResult Editar(Tarea tareaEditada)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == tareaEditada.Id);
            if (tarea == null) return HttpNotFound();
            if (tarea.Autor != User.Identity.Name) return new HttpStatusCodeResult(403);

            // Actualiza los campos editables
            tarea.Titulo = tareaEditada.Titulo;
            tarea.Descripcion = tareaEditada.Descripcion;
            tarea.Lenguajes = tareaEditada.Lenguajes;
            tarea.Categoria = tareaEditada.Categoria;
            tarea.UrlRepositorio = tareaEditada.UrlRepositorio;
            GuardarTareas(tareas);

            TempData["AlertaEditarTarea"] = true;
            return RedirectToAction("Lista");
        }

        // GET: Tareas/Eliminar/{id}
        public ActionResult Eliminar(string id)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null) return HttpNotFound();
            if (tarea.Autor != User.Identity.Name) return new HttpStatusCodeResult(403);

            tareas.Remove(tarea);
            GuardarTareas(tareas);

            TempData["AlertaEliminarTarea"] = true;
            return RedirectToAction("Lista");
        }

        // GET: Tareas/Calificar/{id}
        public ActionResult Calificar(string id)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null) return HttpNotFound();

            // Evitar que el autor califique su propia tarea
            if (tarea.Autor == User.Identity.Name)
                return new HttpStatusCodeResult(403);

            var usuario = User.Identity.Name;
            var calificacionExistente = tarea.Calificaciones.FirstOrDefault(c => c.Usuario == usuario);

            ViewBag.CalificacionExistente = calificacionExistente;
            return View(tarea);
        }

        // POST: Tareas/Calificar
        [HttpPost]
        public ActionResult Calificar(string id, int puntuacion, string comentario)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null)
                return Json(new { success = false, mensaje = "Tarea no encontrada" });

            if (tarea.Autor == User.Identity.Name)
                return Json(new { success = false, mensaje = "No puedes calificar tu propia tarea" });

            var usuario = User.Identity.Name;

            // Actualiza o agrega calificación
            var calificacion = tarea.Calificaciones.FirstOrDefault(c => c.Usuario == usuario);
            if (calificacion != null)
            {
                calificacion.Puntuacion = puntuacion;
                calificacion.Comentario = comentario;
                calificacion.Fecha = DateTime.Now;
            }
            else
            {
                tarea.Calificaciones.Add(new Calificacion
                {
                    Usuario = usuario,
                    Puntuacion = puntuacion,
                    Comentario = comentario,
                    Fecha = DateTime.Now
                });
            }

            GuardarTareas(tareas);

            return Json(new { success = true, mensaje = "Calificación guardada correctamente" });
        }

        // GET: Tareas/Estadistica
        public ActionResult Estadistica()
        {
            var usuario = User.Identity.Name;
            var tareas = LeerTareas()
                .Where(t => t.Autor == usuario)
                .ToList();

            // Calcula promedio de cada tarea
            foreach (var tarea in tareas)
            {
                tarea.PromedioCalificacion = tarea.Calificaciones.Any()
                    ? tarea.Calificaciones.Average(c => c.Puntuacion)
                    : 0;
            }

            return View(tareas);
        }

        // GET: Tareas/Detalle/{id}
        public ActionResult Detalle(string id)
        {
            var tareas = LeerTareas();
            var tarea = tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null) return HttpNotFound();

            return View(tarea); // Muestra todos los detalles de la tarea
        }
    }
}
