using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using HES.Core.Interfaces;
using Microsoft.Extensions.Logging;
using HES.Core.Models.API;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            return await _templateService.GetTemplatesAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Template>> GetTemplateById(string id)
        {
            var template = await _templateService.GetByIdAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return template;
        }

        [HttpPost]
        public async Task<ActionResult<Template>> CreateTemplate(CreateTemplateDto templateDto)
        {
            Template createdTemplate;

            try
            {
                var template = new Template()
                {
                    Name = templateDto.Name,
                    Urls = templateDto.Urls,
                    Apps = templateDto.Apps
                };

                createdTemplate = await _templateService.CreateTmplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetTemplateById", new { id = createdTemplate.Id }, createdTemplate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditTemplate(string id, EditTemplateDto templateDto)
        {
            if (id != templateDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var template = new Template()
                {
                    Id = templateDto.Id,
                    Name = templateDto.Name,
                    Urls = templateDto.Urls,
                    Apps = templateDto.Apps
                };

                await _templateService.EditTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Template>> DeleteTemplate(string id)
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            await _templateService.DeleteTemplateAsync(id);

            return template;
        }
    }
}
