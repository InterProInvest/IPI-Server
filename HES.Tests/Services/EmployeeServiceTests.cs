using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Tests.Builders;
using HES.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
[assembly: TestCollectionOrderer("Xunit.Extensions.Ordering.CollectionOrderer", "Xunit.Extensions.Ordering")]
namespace HES.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly EmployeeBuilder employeeBuilder;
        private readonly IEmployeeService employeeService;
        private readonly string testId = "10";
        private readonly string testFullName = "Test10 Hideez10";

        public EmployeeServiceTests()
        {
            employeeBuilder = new EmployeeBuilder();
            employeeService = new ServicesBuilder("hes_test").EmployeeService;
        }

        [Fact, Order(1)]
        public async Task CreateEmployeeAsync()
        {
            var testEmployees = employeeBuilder.GetTestEmployees();
            foreach (var item in testEmployees)
            {
                await employeeService.CreateEmployeeAsync(item);
            }
            var result = await employeeService.EmployeeQuery().ToListAsync();

            Assert.NotEmpty(result);
            Assert.Equal(result.Count, testEmployees.Count);
        }

        [Fact, Order(2)]
        public async Task GetEmployeesAsync()
        {
            var expectedCount = 100;
            var count = await employeeService.GetEmployeesCountAsync(new DataLoadingOptions<EmployeeFilter>()
            {
                SearchText = string.Empty,
                Filter = null
            });
            var result = await employeeService.GetEmployeesAsync(new DataLoadingOptions<EmployeeFilter>()
            {
                Skip = 0,
                Take = count,
                SortDirection = ListSortDirection.Ascending,
                SortedColumn = nameof(Employee.Id),
                SearchText = string.Empty,
                Filter = null
            });

            Assert.NotEmpty(result);
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact, Order(3)]
        public async Task GetEmployeeByIdAsync()
        {
            var result = await employeeService.GetEmployeeByIdAsync(testId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
            Assert.Equal(result.FullName, testFullName);
        }

        [Fact, Order(4)]
        public async Task CheckEmployeeNameExistAsync()
        {
            var employee = new Employee() { FirstName = "Test10", LastName = "Hideez10" };

            var result = await employeeService.CheckEmployeeNameExistAsync(employee);

            Assert.True(result);
        }

        [Fact, Order(5)]
        public async Task EditEmployeeAsync()
        {
            var employee = await employeeService.GetEmployeeByIdAsync(testId);
            employee.FirstName = "Test00";
            employee.LastName = "Hideez00";
            await employeeService.EditEmployeeAsync(employee);
            var result = await employeeService.GetEmployeeByIdAsync(testId);

            Assert.NotNull(employee);
            Assert.Equal(employee, result);
        }

        [Fact, Order(6)]
        public async Task DeleteEmployeeAsync()
        {
            await employeeService.DeleteEmployeeAsync(testId);
            var result = await employeeService.GetEmployeeByIdAsync(testId);

            Assert.Null(result);
        }
    }
}