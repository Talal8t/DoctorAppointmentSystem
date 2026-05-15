using DAMS.Models;
using DAMS.Services;
using DAMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DAMS.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AuthController : Controller
    {
        private readonly MiddleMan _middleMan;

        public AuthController(MiddleMan middleMan)
        {
            _middleMan = middleMan;
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match");
                    return View(model);
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    Role = "Patient"
                };

                _middleMan.RegisterFlow(user);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        // ================= LOGIN =================

        [HttpGet]
        // ================= LOGIN GET =================
        [HttpGet]
        public IActionResult Login()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            if (userId != null && !string.IsNullOrWhiteSpace(role))
                return RedirectLoggedInUser(role);

            return View();
        }

        // ================= LOGIN POST =================
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _middleMan.LoginFlow(model.Email, model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials";
                return View(model);
            }

            // ================= SESSION =================
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Name", user.Name);

            // ================= ROLE BASED ROUTING =================

            if (user.Role == "Patient")
                return RedirectToAction("RoutePatient");

            if (user.Role == "Doctor")
                return RedirectToAction("RouteDoctor");

            if (user.Role == "Admin")
                return RedirectToAction("RouteAdmin");

            return RedirectToAction("Login");
        }

        // ================= ROLE ROUTING =================

        public IActionResult RoutePatient()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool exists = _middleMan.PatientExistsByUserId(userId);

            if (!exists)
                return RedirectToAction("CompletePatientProfile", "Patient");

            return RedirectToAction("Dashboard", "Patient");
        }

        public IActionResult RouteDoctor()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool exists = _middleMan.DoctorExistsByUserId(userId);

            if (!exists)
                return RedirectToAction("CompleteDoctorProfile", "Doctor");

            return RedirectToAction("Dashboard", "Doctor");
        }

        public IActionResult RouteAdmin()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool exists = _middleMan.AdminExistsByUserId(userId);

            if (!exists)
                return RedirectToAction("CompleteAdminProfile", "Admin");

            return RedirectToAction("Dashboard", "Admin");
        }



        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private IActionResult RedirectLoggedInUser(string role)
        {
            if (role == "Patient")
                return RedirectToAction("Dashboard", "Patient");

            if (role == "Doctor")
                return RedirectToAction("Dashboard", "Doctor");

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Login");
        }
    }
}
