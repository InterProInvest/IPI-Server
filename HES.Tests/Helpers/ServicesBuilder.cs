using HES.Core.Entities;
using HES.Core.Services;
using HES.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HES.Tests.Helpers
{
    public class ServicesBuilder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _dbName = "HesTest";

        public ServicesBuilder(string name)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options;
            _dbContext = new ApplicationDbContext(dbOptions);
        }
        
        public EmployeeService GetEmployeeService()
        {
            var _empRepository = new Repository<Employee>(_dbContext);

            return new EmployeeService(_empRepository, null, null, null, null, null, null, null, null, null);
        }
    }
}
