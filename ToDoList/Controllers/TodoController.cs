using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Models.InputTodo;

namespace ToDoList.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        public TodoController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var todos = await _context.Todos.ToListAsync();

            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var todos = await _context.Todos.FindAsync(id);

            if (todos == null)
            {
                return NotFound();
            }

            return Ok(todos);
        }


        [HttpPost("Add")]
        public async Task<IActionResult> Add(TodoInput todoInput)
        {
            var currentUser = HttpContext.User.Identity as ClaimsIdentity;

            var userClaims = currentUser.Claims;
            var users = new List<string>();

            foreach (var claim in userClaims)
            {
                users.Add(claim.Value);
            }

            var authenticatedUser = await userManager.FindByEmailAsync(users[0]);

            var todo = new Todo
            {
                Title = todoInput.Title,
                Description = todoInput.Description,
                UserId = authenticatedUser.Id
            };
            _ = _context.Todos.Add(todo);
            _ = await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(TodoInput todoInput, int id)
        {
            var todos = await _context.Todos.FindAsync(id);

            if (todos == null)
            {
                return BadRequest();
            }

            todos.Description = todoInput.Description;
            todos.Title = todoInput.Title;

            _ = await _context.SaveChangesAsync();

            return Ok(todos);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<Todo>> Delete(int id)
        {
            var todos = await _context.Todos.FindAsync(id);
            if (todos == null)
            {
                return NotFound();
            }

            _ = _context.Todos.Remove(todos);
            _ = await _context.SaveChangesAsync();

            return todos;
        }


    }
}
