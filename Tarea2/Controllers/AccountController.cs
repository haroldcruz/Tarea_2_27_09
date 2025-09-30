using System.Web.Mvc;
using System.Web.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Tarea2.Controllers
{
    public class AccountController : Controller
    {
        private string rutaArchivo = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data/usuarios.json");

        private List<Usuario> LeerUsuarios()
        {
            if (!System.IO.File.Exists(rutaArchivo) || string.IsNullOrWhiteSpace(System.IO.File.ReadAllText(rutaArchivo)))
            {
                System.IO.File.WriteAllText(rutaArchivo, "[]");
            }

            var json = System.IO.File.ReadAllText(rutaArchivo);
            return JsonConvert.DeserializeObject<List<Usuario>>(json);
        }


        private void GuardarUsuarios(List<Usuario> usuarios)
        {
            var json = JsonConvert.SerializeObject(usuarios, Formatting.Indented);
            System.IO.File.WriteAllText(rutaArchivo, json);
        }

        // GET: Account/Login
        public ActionResult Login() => View();

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var usuarios = LeerUsuarios();
            var usuario = usuarios.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (usuario != null)
            {
                FormsAuthentication.SetAuthCookie(usuario.Username, false);

                // Login exitoso
                Session["MensajeLogin"] = "¡Bienvenido! Inicio de sesión exitoso";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Error de login
                TempData["LoginError"] = "Usuario o contraseña incorrectos";
                return RedirectToAction("Login");
            }
        }

        // GET: Account/Register
        public ActionResult Register() => View();

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(string username, string password)
        {
            var usuarios = LeerUsuarios();

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
            {
                ViewBag.Error = "El nombre de usuario debe tener al menos 4 caracteres.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
                return View();
            }

            if (usuarios.Any(u => u.Username == username))
            {
                ViewBag.Error = "El usuario ya existe.";
                return View();
            }

            usuarios.Add(new Usuario { Username = username, Password = password });
            GuardarUsuarios(usuarios);

            // Mensaje de éxito
            Session["MensajeLogin"] = "Registro exitoso. Ahora puedes iniciar sesión.";

            return RedirectToAction("Login");
        }


        // GET: Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            // Cerrar sesión
            FormsAuthentication.SignOut();

            // Guardar mensaje de éxito para mostrar en Layout
            Session["MensajeLogin"] = "Has cerrado sesión correctamente";

            // Redirigir al login (o donde quieras)
            return RedirectToAction("Login", "Account");
        }

    }

}
