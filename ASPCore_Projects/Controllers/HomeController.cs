using ASPCore_Projects.Data;
using ASPCore_Projects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ASPCore_Projects.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> manager)
        {
            _logger = logger;
            _appContext = context;
            _userManager = manager;
        }

        public IActionResult Index()
        {
            var projects = _appContext.Projects.Include(p => p.Owner).ToList(); 

            return View(projects);
        }

        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            project.Owner = await _userManager.GetUserAsync(User);
            _appContext.Projects.Add(project);
            _appContext.SaveChanges();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> UpdateProject(int id)
        {
            var proj = await _appContext.Projects.FirstAsync(e => e.Id == id);

            return View(proj);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return View(project);
            }
            var proj = await _appContext.Projects.FindAsync(project.Id);
            proj.Name = project.Name;
            proj.Description = project.Description;
            proj.UpdatedAt = DateTime.Now;
            _appContext.SaveChanges();

            return RedirectToAction("ProjectDetails", new { id=project.Id });
        }
        public async Task<IActionResult> DeleteProject(int id)
        {
            var proj = await _appContext.Projects.FindAsync(id);
            if(proj != null)
            {
                _appContext.Tasks.RemoveRange(_appContext.Tasks.Where(t => t.ProjectId == proj.Id));
                _appContext.Projects.Remove(proj);
                _appContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var proj = await _appContext.Projects.Include(e=>e.Owner).Include(e=>e.Tasks).ThenInclude(e=>e.User).FirstAsync(e => e.Id == id);

            return View(proj);
        }
        public async Task<IActionResult> CreateTask(int id)
        {
            var proj = await _appContext.Projects.FirstAsync(e => e.Id == id);
            ViewBag.Users = new SelectList(await _appContext.Users.ToListAsync(), "Id", "UserName");
            return View(new MyTask { Project = proj, ProjectId = proj.Id });
        }
        [HttpPost]
        public async Task<IActionResult> CreateTask(MyTask task)
        {
            task.Id = 0;
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _appContext.Users.ToListAsync(), "Id", "UserName");

                return View(task);
            }
            _appContext.Tasks.Add(task);
            _appContext.Projects.Find(task.ProjectId).UpdatedAt = DateTime.Now;
            _appContext.SaveChanges();
            return RedirectToAction("ProjectDetails", new { id=task.ProjectId });
        }
        public async Task<IActionResult> DeleteTask(int id)
        {
            var t = await _appContext.Tasks.FindAsync(id);
            int projId = t.ProjectId;
            _appContext.Tasks.Remove(t);
            _appContext.Projects.Find(projId).UpdatedAt = DateTime.Now;
            _appContext.SaveChanges();

            return RedirectToAction("ProjectDetails", new { id = projId });
        }

        public async Task<IActionResult> CompleteTask(int id)
        {
            var t = _appContext.Tasks.Find(id);
            t.IsCompleted = true;
            _appContext.SaveChanges();
            return RedirectToAction("ProjectDetails", new { id = t.ProjectId });
        }

        public async Task<IActionResult> UpdateTask(int id)
        {
            var task = await _appContext.Tasks.FirstAsync(e => e.Id == id);
            ViewBag.Users = new SelectList(await _appContext.Users.ToListAsync(), "Id", "UserName");
            return View(task);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTask(MyTask task)
        {
            if (!ModelState.IsValid)
            {
                return View(task);
            }
            _appContext.Projects.Find(task.ProjectId).UpdatedAt = DateTime.Now;
            _appContext.Tasks.Update(task);
            _appContext.SaveChanges();

            return RedirectToAction("ProjectDetails", new { id = task.ProjectId });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}