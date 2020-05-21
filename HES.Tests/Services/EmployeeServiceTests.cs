using Moq;
using Xunit;
using System;
using HES.Core.Services;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using System.Threading.Tasks;

namespace HES.Tests.Services
{
    public class EmployeeServiceTests
    {
        public EmployeeServiceTests()
        {

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
            var employeeService = new EmployeeService(mock.Object, null,null, null, null, null, null, null, null, null);


            await Assert.ThrowsAsync<AlreadyExistException>(async () => await employeeService.EditEmployeeAsync(employee));
        }

        [Fact]
        public async Task EditEmployeeAsync_EmployeeIsNullSholdFail()
        {
            var mock = new Mock<IAsyncRepository<Employee>>();

            var employeeService = new EmployeeService(mock.Object, null, null, null, null, null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await employeeService.EditEmployeeAsync(null));
        }
    }
}
