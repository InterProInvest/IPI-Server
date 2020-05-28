using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HES.Core.Entities;

namespace HES.Tests.Builders
{
    public class EmployeeBuilder
    {
        private Random _random;

        public EmployeeBuilder()
        {
            _random = new Random();
        }

        public Employee GetEmptyEmployee(bool needId = true)
        {
            return new Employee() { Id = needId ? Guid.NewGuid().ToString() : null };
        }

        public Employee GetEmployeeWithName(string id, string fullName)
        {
            var words = fullName.Split(' ');
            return new Employee()
            {
                Id = id,
                FirstName = words.First(),
                LastName = words.Last()
            };
        }

        public Employee GetRandomEmployee()
        {
            return new Employee()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = GetRandomWord(3),
                LastName = GetRandomWord(6),
                Email = GetRandomEmail(),
                PhoneNumber = GetRandomNumber(),
                LastSeen = GetRandomDate(),
                ActiveDirectoryGuid = Guid.NewGuid().ToString(),
                DepartmentId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                PrimaryAccountId = Guid.NewGuid().ToString(),
            };
        }

        public Employee GetEmployeeCopy(Employee employee)
        {
            return new Employee()
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                LastSeen = employee.LastSeen,
                ActiveDirectoryGuid = employee.ActiveDirectoryGuid,
                DepartmentId = employee.DepartmentId,
                PositionId = employee.PositionId,
                PrimaryAccountId = employee.PrimaryAccountId
            };
        }

        private string GetRandomNumber()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("+380");


            for (int i = 0; i < 10; i++)
                stringBuilder.Append(_random.Next(0, 10));

            return stringBuilder.ToString();
        }

        private DateTime GetRandomDate()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(_random.Next(range));
        }

        private string GetRandomWord(int minLenght, int maxLenght = 20)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int wordLenght = _random.Next(minLenght, maxLenght);

            for (int i = 0; i < wordLenght; i++)
            {
                char tmp = (char)_random.Next(97, 123);
                stringBuilder.Append(tmp);
            }

            return stringBuilder.ToString();
        }

        private string GetRandomEmail()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (_random.Next(0, 101) >= 50)
                stringBuilder.Append(_random.Next(1, 20));

            stringBuilder.Append(GetRandomWord(5));

            if (_random.Next(0, 101) >= 50)
            {
                stringBuilder.Append('.');
                stringBuilder.Append(GetRandomWord(5));
            }
            else
                stringBuilder.Append(_random.Next(1, 10));

            stringBuilder.Append('@');
            stringBuilder.Append(GetRandomWord(2, 3));
            stringBuilder.Append('.');
            stringBuilder.Append(GetRandomWord(2, 3));


            return stringBuilder.ToString();
        }

        public List<Employee> GetTestEmployees()
        {
            var employees = new List<Employee>();

            for (int i = 0; i < 100; i++)
            {
                employees.Add(new Employee()
                {
                    Id = $"{i}",
                    FirstName = $"Test{i}",
                    LastName = $"Hideez{i}",
                    Email = $"th.{i}@hideez.com",
                    PhoneNumber = $"380{i}"
                });
            }
            return employees;
        }
    }
}
