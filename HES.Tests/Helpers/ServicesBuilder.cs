using HES.Core.Entities;
using HES.Core.Services;
using HES.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HES.Tests.Helpers
{
    public class ServicesBuilder
    {
        private readonly ApplicationDbContext _dbContext;

        public ServicesBuilder(string dbName)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
​
            _dbContext = new ApplicationDbContext(dbOptions);
        }

        public EmployeeService GetEmployeeService()
        {
            var _empRepository = new Repository<Employee>(_dbContext);

            return new EmployeeService(_empRepository, null, null, null, null, null, null, null, null, null);
        }
    }
}
