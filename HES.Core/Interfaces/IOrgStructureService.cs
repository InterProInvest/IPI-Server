using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IOrgStructureService
    {
        IQueryable<Company> QueryOfCompany();
        Task<Company> GetCompanyByIdAsync(string id);
        Task<List<Company>> GetCompaniesAsync();
        Task CreateCompanyAsync(Company company);
        Task EditCompanyAsync(Company company);
        Task DeleteCompanyAsync(string id);
        IQueryable<Department> DepartmentQuery();
        Task<Department> GetDepartmentByIdAsync(string id);
        Task<List<Department>> GetDepartmentsAsync();
        Task CreateDepartmentAsync(Department department);
        Task EditDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(string id);
        IQueryable<Position> PositionQuery();
        Task<Position> GetPositionByIdAsync(string id);
        Task<List<Position>> GetPositionsAsync();
        Task CreatePositionAsync(Position position);
        Task EditPositionAsync(Position position);
        Task DeletePositionAsync(string id);
    }
}