using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IOrgStructureService
    {
        IQueryable<Company> CompanyQuery();
        Task<Company> GetCompanyByIdAsync(string id);
        Task<List<Company>> GetCompaniesAsync();
        Task<Company> CreateCompanyAsync(Company company);
        Task EditCompanyAsync(Company company);
        Task UnchangedCompanyAsync(Company company);
        Task DeleteCompanyAsync(string id);
        Task DetachCompaniesAsync(List<Company> companies);
        IQueryable<Department> DepartmentQuery();
        Task<List<Department>> GetDepartmentsAsync();
        Task<List<Department>> GetDepartmentsByCompanyIdAsync(string id);
        Task<Department> GetDepartmentByIdAsync(string id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task EditDepartmentAsync(Department department);
        Task UnchangedDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(string id);
        IQueryable<Position> PositionQuery();
        Task<List<Position>> GetPositionsAsync();
        Task<Position> GetPositionByIdAsync(string id);
        Task<Position> CreatePositionAsync(Position position);
        Task EditPositionAsync(Position position);
        Task UnchangedPositionAsync(Position position);
        Task DeletePositionAsync(string id);
        Task DetachPositionsAsync(List<Position> positions);
    }
}