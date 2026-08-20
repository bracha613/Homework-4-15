using Homework_4_15.Data;
using Homework_4_15.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Homework_4_15.Web.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment _environment;
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=ImageSharePassword;Integrated Security=true;Trust Server Certificate=true;";

        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(string password, IFormFile imageFile)
        {
            var db = new ImageDB(_connectionString);
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string fullPath = Path.Combine(_environment.WebRootPath, "images", fileName);
            using FileStream fs = new FileStream(fullPath, FileMode.CreateNew);
            imageFile.CopyTo(fs);
            Image image = new Image
            {
                FileName = fileName,
                Password = password
            };
            db.Add(image);

            UploadViewModel vm = new UploadViewModel
            {
                Image = image
            };
            return View(vm);
        }

        public IActionResult ViewImage(int id)
        {
            var db = new ImageDB(_connectionString);
            var vm = new ViewImageViewModel();
            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }
            if (!HasPermissionToView(id))
            {
                vm.HasPermissionToView = false;
                vm.Image = new Image { Id = id };
            }
            else
            {
                vm.HasPermissionToView = true;
                db.IncrementViews(id);
                var image = db.GetById(id);
                if(image == null)
                {
                    return RedirectToAction("Index");
                }

                vm.Image = image;
            }
                return View(vm);
        }

        [HttpPost]
        public IActionResult ViewImage(int id, string password)
        {
            var db = new ImageDB(_connectionString);
            var image = db.GetById(id);

            if (image == null)
            {
                return RedirectToAction("Index");
            }

            if (password != image.Password)
            {
                TempData["message"] = "Invalid password!";
            }
            else
            {
                var allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
                if (allowedIds == null)
                {
                    allowedIds = new List<int>();
                }
                allowedIds.Add(id);
                HttpContext.Session.Set("allowedids", allowedIds);
            }

            return Redirect($"/home/viewimage?id={id}");
        }

        private bool HasPermissionToView(int Id)
        {
            var allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
            if(allowedIds == null)
            {
                return false;
            }
            return allowedIds.Contains(Id);
        }
    }

}
