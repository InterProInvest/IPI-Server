using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IWorkstationAuditService workstationAuditService, ILogger<AuditController> logger)
        {
            _workstationAuditService = workstationAuditService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationEvent>>> GetEvents()
        {
            return await _workstationAuditService.GetWorkstationEventsAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationEvent>>> GetFilteredEvents(WorkstationEventFilter workstationEventFilter)
        {
            return await _workstationAuditService.GetFilteredWorkstationEventsAsync(workstationEventFilter);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationSession>>> GetSessions()
        {
            return await _workstationAuditService.GetWorkstationSessionsAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationSession>>> GetFilteredSessions(WorkstationSessionFilter workstationSessionFilter)
        {
            return await _workstationAuditService.GetFilteredWorkstationSessionsAsync(workstationSessionFilter);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDayAndEmployee>>> GetSummaryByDayAndEmployees()
        {
            return await _workstationAuditService.GetSummaryByDayAndEmployeesAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDayAndEmployee>>> GetFilteredSummaryByDayAndEmployees(SummaryFilter summaryFilter)
        {
            return await _workstationAuditService.GetFilteredSummaryByDaysAndEmployeesAsync(summaryFilter);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByEmployees>>> GetSummaryByEmployee()
        {
            return await _workstationAuditService.GetSummaryByEmployeesAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByEmployees>>> GetFilteredSummaryByEmployees(SummaryFilter summaryFilter)
        {
            return await _workstationAuditService.GetFilteredSummaryByEmployeesAsync(summaryFilter);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDepartments>>> GetSummaryByDepartments()
        {
            return await _workstationAuditService.GetSummaryByDepartmentsAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDepartments>>> GetFilteredSummaryByDepartments(SummaryFilter summaryFilter)
        {
            return await _workstationAuditService.GetFilteredSummaryByDepartmentsAsync(summaryFilter);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByWorkstations>>> GetSummaryByWorkstations()
        {
            return await _workstationAuditService.GetSummaryByWorkstationsAsync();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByWorkstations>>> GetFilteredSummaryByWorkstations(SummaryFilter summaryFilter)
        {
            return await _workstationAuditService.GetFilteredSummaryByWorkstationsAsync(summaryFilter);
        }
    }
}