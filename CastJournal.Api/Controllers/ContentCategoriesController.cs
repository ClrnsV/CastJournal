using CastJournal.Application.DTOs.ContentCategories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContentCategoriesController : ControllerBase
{
    private readonly IContentCategoryService _categoryService;
    private readonly IAuditService _auditService;

    public ContentCategoriesController(IContentCategoryService categoryService, IAuditService auditService)
    {
        _categoryService = categoryService;
        _auditService = auditService;
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    // GET api/contentcategories?type=GearType — open to any logged-in user,
    // since regular users need these lists to populate dropdowns when logging a catch.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryType? type, [FromQuery] bool includeInactive = false)
    {
        var categories = await _categoryService.GetAllAsync(type, includeInactive);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateContentCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.CategoryCreated, "ContentCategory", category.Id.ToString(),
            $"Created {category.Type} category '{category.Name}'", GetClientIp());

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);
        if (category == null) return NotFound();

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.CategoryUpdated, "ContentCategory", id.ToString(),
            $"Updated category '{category.Name}'", GetClientIp());

        return Ok(category);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _categoryService.DeleteAsync(id);
        if (!success) return NotFound();

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.CategoryDeleted, "ContentCategory", id.ToString(), ipAddress: GetClientIp());

        return NoContent();
    }
}