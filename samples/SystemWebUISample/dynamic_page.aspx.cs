// MIT License.

using System.Web.UI;
using System;
using System.ComponentModel;
using System.Collections.Generic;


namespace SystemWebUISample.Pages;

public partial class DynamicPage : Page
{
    protected string TestValue1 = "Hello there!";

    protected string GetText(string value)
        => $"{value}: {GetCount(value)}";

    private int GetCount(string value)
    {
        if (ViewState[value] is int count)
        {
            ViewState[value] = ++count;
            return count;
        }
        else
        {
            ViewState[value] = 1;
            return 1;
        }
    }


    protected void BindData()
    {
        var list = new List<Employee>()
        {
            new Employee { FirstName = "Foo", LastName = "Bar", EmpId = 1000, City = "TestCity", Email="Hello@hello.com", DateOfJoining="01/01/2000"},
            new Employee { FirstName = "FooNext", LastName = "BarNext", EmpId = 1001, City = "TestCity2",Email="Hello@hello.com", DateOfJoining="01/01/2001"},
            new Employee { FirstName = "FooNextNext", LastName = "BarNextNext", EmpId = 1010, City = "TestCity3", Email="Hello@hello.com", DateOfJoining="01/01/2002"},
            new Employee { FirstName = "FooNextNextNext", LastName = "BarNextNextNext", EmpId = 2000, City = "TestCity4", Email="Hello@hello.com",DateOfJoining="01/01/2003"},
        };
        var bindingList = new BindingList<Employee>(list);
        Grid.DataSource = bindingList;
        Grid.DataBind();
    }
    
    class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int EmpId { get; set; }
        public string City { get; set; }
        public string DateOfJoining { get; set; }
        public string Email { get; set; }
    }

}
