using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Account;
using HES.Tests.Builders;
using HES.Tests.Helpers;
using HES.Web;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
[assembly: TestCollectionOrderer("Xunit.Extensions.Ordering.CollectionOrderer", "Xunit.Extensions.Ordering")]
namespace HES.Tests.Services
{
    public class EmployeeServiceTests : IClassFixture<CustomWebAppFactory<Startup>>
    {
        private readonly IEmployeeService _employeeService;
        private readonly EmployeeBuilder _employeeBuilder;
        private readonly CustomWebAppFactory<Startup> _factory;

        public EmployeeServiceTests(CustomWebAppFactory<Startup> factory)
        {
            _factory = factory;
            _employeeService = _factory.GetEmployeeService();
            _employeeBuilder = new EmployeeBuilder();
        }

        [Fact, Order(1)]
        public async Task CreateEmployeeAsync()
        {
            var testEmployees = _employeeBuilder.GetTestEmployees();
            foreach (var item in testEmployees)
            {
                await _employeeService.CreateEmployeeAsync(item);
            }
            var result = await _employeeService.EmployeeQuery().ToListAsync();

            Assert.NotEmpty(result);
            Assert.Equal(result.Count, testEmployees.Count);
        }

        [Fact, Order(2)]
        public async Task GetEmployeesAsync()
        {
            var expectedCount = 100;
            var count = await _employeeService.GetEmployeesCountAsync(new DataLoadingOptions<EmployeeFilter>()
            {
                SearchText = string.Empty,
                Filter = null
            });
            var result = await _employeeService.GetEmployeesAsync(new DataLoadingOptions<EmployeeFilter>()
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

        [Theory, Order(3)]
        [InlineData("10", "Test10 Hideez10")]
        public async Task GetEmployeeByIdAsync(string testId, string fullName)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(testId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, testId);
            Assert.Equal(result.FullName, fullName);
        }

        [Fact, Order(4)]
        public async Task CheckEmployeeNameExistAsync()
        {
            var employee = new Employee() { FirstName = "Test10", LastName = "Hideez10" };

            var result = await _employeeService.CheckEmployeeNameExistAsync(employee);

            Assert.True(result);
        }

        [Theory, Order(5)]
        [InlineData("10")]
        public async Task EditEmployeeAsync(string testId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(testId);
            employee.FirstName = "Test00";
            employee.LastName = "Hideez00";
            await _employeeService.EditEmployeeAsync(employee);
            var result = await _employeeService.GetEmployeeByIdAsync(testId);

            Assert.NotNull(employee);
            Assert.Equal(employee, result);
        }

        [Theory, Order(6)]
        [InlineData("10")]
        public async Task DeleteEmployeeAsync(string testId)
        {
            await _employeeService.DeleteEmployeeAsync(testId);
            var result = await _employeeService.GetEmployeeByIdAsync(testId);

            Assert.Null(result);
        }


        [Fact, Order(7)]
        public async Task CreatePersonalAccountAsync()
        {
            var account = new PersonalAccount()
            {
                Name = "stackoverflow",
                Urls = "stackoverflow.com",
                Login = "login1",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = "20"
            };
            var result = await _employeeService.CreatePersonalAccountAsync(account);

            Assert.NotNull(result.Id);
            Assert.True(result.Kind == AccountKind.WebApp);
        }

        [Fact, Order(8)]
        public async Task CreateWorkstationLocalAccountAsync()
        {
            var account = new WorkstationAccount()
            {
                Name = "local",
                UserName = "user_local",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = "20",
                Type = WorkstationAccountType.Local
            };
            var result = await _employeeService.CreateWorkstationAccountAsync(account);

            Assert.NotNull(result.Id);
            Assert.True(result.Kind == AccountKind.Workstation);
        }

        [Fact, Order(9)]
        public async Task CreateWorkstationDomainAccountAsync()
        {
            var account = new WorkstationDomain()
            {
                Name = "domain",
                UserName = "user_domain",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = "20",
            };
            var result = await _employeeService.CreateWorkstationAccountAsync(account);

            Assert.NotNull(result.Id);
            Assert.True(result.Kind == AccountKind.Workstation);
        }

        [Fact, Order(10)]
        public async Task CreateWorkstationMSAccountAsync()
        {
            var account = new WorkstationAccount()
            {
                Name = "ms",
                UserName = "user_ms",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = "20",
                Type = WorkstationAccountType.Microsoft
            };

            var result = await _employeeService.CreateWorkstationAccountAsync(account);

            Assert.NotNull(result.Id);
            Assert.True(result.Kind == AccountKind.Workstation);
        }

        [Fact, Order(11)]
        public async Task CreateWorkstationAzureAccountAsync()
        {
            var account = new WorkstationAccount()
            {
                Name = "azure",
                UserName = "user_azure",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = "20",
                Type = WorkstationAccountType.AzureAD
            };
            var result = await _employeeService.CreateWorkstationAccountAsync(account);

            Assert.NotNull(result.Id);
            Assert.True(result.Kind == AccountKind.Workstation);
        }


        [Fact, Order(12)]
        public async Task GetAccountsAsync()
        {

            var accountsCount = await _employeeService.GetAccountsCountAsync(string.Empty, "20");

            var result = await _employeeService.GetAccountsAsync(0, accountsCount, nameof(Account.Id), ListSortDirection.Ascending, string.Empty, "20");

            Assert.NotEmpty(result);
            Assert.Equal(accountsCount, result.Count);
        }

        [Fact, Order(13)]
        public async Task GetAccountsByEmployeeIdAsync()
        {
            var result = await _employeeService.GetAccountsByEmployeeIdAsync("20");

            Assert.NotNull(result);
            Assert.All(result, x => Assert.True(x.EmployeeId == "20"));
        }

        [Fact, Order(14)]
        public async Task SetAsWorkstationAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsAsync(0, 100, nameof(Account.Id), ListSortDirection.Ascending, string.Empty, "20");
            var accountId = accounts.FirstOrDefault(x => x.Name == "azure").Id;

            await _employeeService.SetAsWorkstationAccountAsync("20", accountId);

            var employee = await _employeeService.GetEmployeeByIdAsync("20");

            Assert.True(employee.PrimaryAccountId == accountId);
        }

        [Fact, Order(15)]
        public async Task GetAccountByIdAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var accountId = accounts.FirstOrDefault(x => x.Name == "stackoverflow").Id;

            var account = await _employeeService.GetAccountByIdAsync(accountId);

            Assert.NotNull(account);
            Assert.True(account.Id == accountId);
        }

        [Fact, Order(16)]
        public async Task UnchangedPersonalAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var account = accounts.FirstOrDefault(x => x.Name == "stackoverflow");

            account.Name = "test"; 

            await _employeeService.UnchangedPersonalAccountAsync(account);
            
            Assert.True(account.Name != "test");
        }

        [Fact, Order(17)]
        public async Task EditPersonalAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var account = accounts.FirstOrDefault(x => x.Name == "stackoverflow");

            account.Name = "New name";
            await _employeeService.EditPersonalAccountAsync(account);

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var result = accountsResult.FirstOrDefault(x => x.Name == "New name");

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
        }

        [Fact, Order(18)]
        public async Task EditPersonalAccountPwdAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var account = accounts.FirstOrDefault(x => x.Name == "New name");

            await _employeeService.EditPersonalAccountPwdAsync(account, new AccountPassword
            {
                Password = "newPassword",
                ConfirmPassword = "newPassword"
            });

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var result = accountsResult.FirstOrDefault(x => x.Name == "New name");

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
            Assert.True(result.Password == "newPassword");
        }


        [Fact, Order(19)]
        public async Task EditPersonalAccountOtpAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var account = accounts.FirstOrDefault(x => x.Name == "New name");

            await _employeeService.EditPersonalAccountOtpAsync(account, new AccountOtp
            {
                OtpSecret = "newOtpSecret"
            });

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var result = accountsResult.FirstOrDefault(x => x.Name == "New name");

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
            Assert.True(result.OtpSecret == "newOtpSecret");
        }


        [Fact, Order(20)]
        public async Task DeleteAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var account = accounts.FirstOrDefault(x => x.Name == "New name");

            await _employeeService.DeleteAccountAsync(account.Id);

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync("20");
            var result = accountsResult.FirstOrDefault(x => x.Id == account.Id);

            Assert.Null(result);
        }

        // TODO
        // AddSharedAccountAsync
    }
}