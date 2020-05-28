using Xunit;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using HES.Tests.Builders;
using HES.Tests.Helpers;
using Xunit.Extensions.Ordering;

namespace HES.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly EmployeeBuilder employeeBuilder;
        private readonly IEmployeeService employeeService;
        private readonly string employeeId = "123456789";
        private readonly string employeeFullName = "Test Hideez";

        public EmployeeServiceTests()
        {
            employeeBuilder = new EmployeeBuilder();
            employeeService = new ServicesBuilder("hes_test").EmployeeService;
        }

        [Order(1)]
        [Fact]
        public async Task CreateEmployeeAsync()
        {
            var employee = employeeBuilder.GetEmployeeWithName(employeeId, employeeFullName);
            var result = await employeeService.CreateEmployeeAsync(employee);
            Assert.NotNull(result);
        }

        [Order(2)]
        [Fact]
        public async Task GetEmployeeByIdAsync()
        {
            var result = await employeeService.GetEmployeeByIdAsync(employeeId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, employeeId);
            Assert.Equal(result.FullName, employeeFullName);
        }
    }
}
