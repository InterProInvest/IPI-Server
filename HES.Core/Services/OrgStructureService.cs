using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class OrgStructureService : IOrgStructureService
    {
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;
        private readonly IAsyncRepository<Position> _positionRepository;

        public OrgStructureService(IAsyncRepository<Company> companyRepository,
                                   IAsyncRepository<Department> departmentRepository,
                                   IAsyncRepository<Position> positionRepository)
        {
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public IQueryable<Position> PositionQuery()
        {
            return _positionRepository.Query();
        }

        public async Task CreateCompanyAsync(Company company)
        {
            if (company == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            await _companyRepository.AddAsync(company);
        }

        public async Task EditCompanyAsync(Company company)
        {
            if (company == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteCompanyAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                throw new Exception("Company does not exist.");
            }
            await _companyRepository.DeleteAsync(company);
        }

        public async Task CreateDepartmentAsync(Department department)
        {
            if (department == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            await _departmentRepository.AddAsync(department);
        }

        public async Task EditDepartmentAsync(Department department)
        {
            if (department == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            await _departmentRepository.UpdateAsync(department);
        }

        public async Task DeleteDepartmentAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                throw new Exception("Department does not exist.");
            }
            await _departmentRepository.DeleteAsync(department);
        }

        public async Task CreatePositionAsync(Position position)
        {
            if (position == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            await _positionRepository.AddAsync(position);
        }

        public async Task EditPositionAsync(Position position)
        {
            if (position == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            await _positionRepository.UpdateAsync(position);
        }

        public async Task DeletePositionAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                throw new Exception("Position does not exist.");
            }
            await _positionRepository.DeleteAsync(position);
        }
    }
}