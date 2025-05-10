using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using Reflectly.Entity;
using Reflectly.Service;
using System.Security.Claims;

namespace Reflectly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReflectionController : Controller
    {
        private readonly Reflection_Service _reflectionService;

        public ReflectionController(Reflection_Service reflection_Service)
        {
            _reflectionService = reflection_Service;
        }

        // Lấy tất cả Reflection
        [Authorize]
        [HttpGet("getreflection")]
        public async Task<IActionResult> GetReflection()
        {
            var reflections = await _reflectionService.GetAllAsync();
            return Ok(JsonConvert.SerializeObject(reflections));
        }

        // Thêm Reflection mới
        [Authorize]
        [HttpPost("addreflection")]
        public async Task<IActionResult> AddReflection([FromBody] Reflection reflection)
        {
            if (reflection == null)
            {
                return BadRequest("Reflection cannot be null");
            }

            await _reflectionService.CreateAsync(reflection);
            return CreatedAtAction(nameof(GetReflection), new { id = reflection.id }, reflection);
        }

        // Sửa Reflection
        [Authorize]
        [HttpPut("updatereflection/{id}")]
        public async Task<IActionResult> UpdateReflection(string id, [FromBody] Reflection reflection)
        {
            if (reflection == null)
            {
                return BadRequest("Reflection cannot be null");
            }

            var result = await _reflectionService.UpdateAsync(id, reflection);
            if (result.ModifiedCount == 0)
            {
                return NotFound($"Reflection with id {id} not found");
            }

            return NoContent(); // Trả về 204 No Content
        }

        // Xóa Reflection
        [Authorize]
        [HttpDelete("deleterelection/{id}")]
        public async Task<IActionResult> DeleteReflection(string id)
        {
            var result = await _reflectionService.DeleteAsync(id);
            if (result.DeletedCount == 0)
            {
                return NotFound($"Reflection with id {id} not found");
            }

            return NoContent(); // Trả về 204 No Content
        }

        // Tìm kiếm Reflection theo id
        [Authorize]
        [HttpGet("getreflectionbyid/{id}")]
        public async Task<IActionResult> GetReflectionById(string id)
        {
            var reflection = await _reflectionService.GetByIdAsync(id);
            if (reflection == null)
            {
                return NotFound($"Reflection with id {id} not found");
            }

            return Ok(JsonConvert.SerializeObject(reflection));
        }

        // Tìm kiếm Reflection theo description
        [Authorize]
        [HttpGet("getreflectionbydescription")]
        public async Task<IActionResult> GetReflectionByDescription(string description)
        {
            var reflections = await _reflectionService.GetByDescriptionAsync(description);
            return Ok(JsonConvert.SerializeObject(reflections));
        }

        // Tìm kiếm Reflection theo category_id
        [Authorize]
        [HttpGet("getreflectionbycategory/{categoryId}")]
        public async Task<IActionResult> GetReflectionByCategoryId(int categoryId)
        {
            var reflections = await _reflectionService.GetByCategoryIdAsync(categoryId);
            return Ok(JsonConvert.SerializeObject(reflections));
        }
    }
}

