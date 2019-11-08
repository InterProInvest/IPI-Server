using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IOrgStructureService
    {
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        IQueryable<Position> PositionQuery();
        Task CreateCompanyAsync(Company company);
        Task EditCompanyAsync(Company company);
        Task DeleteCompanyAsync(string id);
        Task CreateDepartmentAsync(Department department);
        Task EditDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(string id);
        Task CreatePositionAsync(Position position);
        Task EditPositionAsync(Position position);
        Task DeletePositionAsync(string id);
    }
}