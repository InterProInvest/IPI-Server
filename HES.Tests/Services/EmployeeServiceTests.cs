using Moq;
using Xunit;
using System;
using HES.Core.Services;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using HES.Tests.Builders;
using HES.Tests.Helpers;
using Xunit.Extensions.Ordering;
using Microsoft.Extensions.DependencyInjection;

namespace HES.Tests.Services
{
    [Order(2)]
    public class EmployeeServiceTests
    {
        private readonly EmployeeBuilder employeeBuilder;
        private readonly ServicesBuilder servicesBuilder;
        private readonly IEmployeeService employeeService;

        public EmployeeServiceTests()
        {
            servicesBuilder = new ServicesBuilder("q1");
            employeeBuilder = new EmployeeBuilder();
            employeeService = servicesBuilder.EmployeeService;
        }

        [Fact]
        public async Task CreateEmployee()
        {
            var employee1 = employeeBuilder.GetEmployeeWithName("123", "Alexey Leonov");
            var employee2 = employeeBuilder.GetEmployeeWithName("123", "Vitalii  Martyniuk");

            var result = await employeeService.CreateEmployeeAsync(employee1);
            var q = await employeeService.GetEmployeeByIdAsync(result.Id);

            Assert.Equal(result.Id, q.Id);
        }

        [Fact]
        public async Task EditEmployeeAsync_NoEmployeeExistsSholdFail()
        {
            var employee = new Employee();
            var mock = new Mock<IAsyncRepository<Employee>>();
            mock.Setup(r => r.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName && x.Id != employee.Id)).Returns(async () =>
            {
                await Task.CompletedTask;
                return true;
            });
            var employeeService = new EmployeeService(mock.Object, null, null, null, null, null, null, null);


            await Assert.ThrowsAsync<AlreadyExistException>(async () => await employeeService.EditEmployeeAsync(employee));
        }

        [Fact]
        public async Task EditEmployeeAsync_EmployeeIsNullSholdFail()
        {
            var mock = new Mock<IAsyncRepository<Employee>>();

            var employeeService = new EmployeeService(mock.Object, null, null, null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await employeeService.EditEmployeeAsync(null));
        }
    }
}
