using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrgStructureController : ControllerBase
    {
        private readonly IOrgStructureService _orgStructureService;
        private readonly ILogger<OrgStructureController> _logger;

        public OrgStructureController(IOrgStructureService orgStructureService, ILogger<OrgStructureController> logger)
        {
            _orgStructureService = orgStructureService;
            _logger = logger;
        }

        #region Company

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await _orgStructureService.GetCompaniesAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompanyById(string id)
        {
            var company = await _orgStructureService.GetCompanyByIdAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        [HttpPost]
        public async Task<ActionResult<Company>> CreateCompany(CreateOrgStructureGenericDto companyDto)
        {
            Company createdCompany;
            try
            {
                var company = new Company()
                {
                    Name = companyDto.Name
                };

                createdCompany = await _orgStructureService.CreateCompanyAsync(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetCompanyById", new { id = createdCompany.Id }, createdCompany);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCompany(string id, EditOrgStructureGenericDto companyDto)
        {
            if (id != companyDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var company = new Company()
                {
                    Id = companyDto.Id,
                    Name = companyDto.Name
                };

                await _orgStructureService.EditCompanyAsync(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(string id)
        {
            var company = await _orgStructureService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            try
            {
                await _orgStructureService.DeleteCompanyAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return company;
        }

        #endregion

        #region Department

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            return await _orgStructureService.GetDepartmentsAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartmentByCompanyId(string id)
        {
            var departments = await _orgStructureService.GetDepartmentsByCompanyIdAsync(id);

            if (departments == null)
            {
                return NotFound();
            }

            return departments;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartmentById(string id)
        {
            var department = await _orgStructureService.GetDepartmentByIdAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(CreateDepartmentDto departmentDto)
        {
            Department createdDepartment;
            try
            {
                var department = new Department()
                {
                    Name = departmentDto.Name,
                    CompanyId = departmentDto.CompanyId
                };

                createdDepartment = await _orgStructureService.CreateDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetDepartmentById", new { id = createdDepartment.Id }, createdDepartment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditDepartment(string id, EditDepartmentDto departmentDto)
        {
            if (id != departmentDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var department = new Department()
                {
                    Id = departmentDto.Id,
                    Name = departmentDto.Name,
                    CompanyId = departmentDto.CompanyId
                };

                await _orgStructureService.EditDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> DeleteDepartment(string id)
        {
            var department = await _orgStructureService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            try
            {
                await _orgStructureService.DeleteDepartmentAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return department;
        }

        #endregion

        #region Position

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            return await _orgStructureService.GetPositionsAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPositionById(string id)
        {
            var position = await _orgStructureService.GetPositionByIdAsync(id);

            if (position == null)
            {
                return NotFound();
            }

            return position;
        }

        [HttpPost]
        public async Task<ActionResult<Company>> CreatePosition(CreateOrgStructureGenericDto positionDto)
        {
            Position createdPosition;
            try
            {
                var position = new Position()
                {
                    Name = positionDto.Name
                };

                createdPosition = await _orgStructureService.CreatePositionAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetPositionById", new { id = createdPosition.Id }, createdPosition);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPosition(string id, EditOrgStructureGenericDto positionDto)
        {
            if (id != positionDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var position = new Position()
                {
                    Id = positionDto.Id,
                    Name = positionDto.Name
                };

                await _orgStructureService.EditPositionAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeletePosition(string id)
        {
            var position = await _orgStructureService.GetCompanyByIdAsync(id);
            if (position == null)
            {
                return NotFound();
            }

            try
            {
                await _orgStructureService.DeletePositionAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return position;
        }

        #endregion
    }
}
