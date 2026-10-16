using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay2.Data;
using RaceDay2.Models;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryRepository _repository;

        public CategoriesController(CategoryRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return BadRequest("Category name is required.");

            try
            {
                int id = await _repository.CreateAsync(category);
                return Ok(new { CategoryID = id, Message = "Category created." });
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return Conflict("Could not create category. Check its details.");
            }
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return await _repository.DeleteAsync(id)
                    ? NoContent()
                    : NotFound();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return Conflict("This category may be linked to existing events.");
            }
        }
    }
}
