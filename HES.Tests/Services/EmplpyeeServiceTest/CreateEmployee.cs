using HES.Core.Services;
using HES.Tests.Builders;
using HES.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace HES.Tests.Services.EmplpyeeServiceTest
{

    [Order(1)]
    public class CreateEmployee
    {
        private readonly EmployeeBuilder employeeBuilder;
        private readonly ServicesBuilder servicesBuilder;
        private readonly EmployeeService employeeService;

        public CreateEmployee()
        {
            servicesBuilder = new ServicesBuilder("q1");

            employeeBuilder = new EmployeeBuilder();

            employeeService = servicesBuilder.GetEmployeeService();
        }

        [Fact]
        [Order(1)]
        public async Task Create()
        {
            var employee1 = employeeBuilder.GetEmployeeWithName("123", "Alexey Leonov");
            var employee2 = employeeBuilder.GetEmployeeWithName("123", "Vitalii  Martyniuk");

            var result = await employeeService.CreateEmployeeAsync(employee1);
            var q = await employeeService.GetEmployeeByIdAsync(result.Id);

            Assert.Equal(result.Id, q.Id);
        }
    }
    
}
