// MIT License.

using System.Web.UI;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Collections;

namespace SystemWebUISample.Pages;

public partial class ObjectDataSourceSample : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    static readonly List<NorthwindEmployee> employees = new List<NorthwindEmployee>() {
        new NorthwindEmployee(1), new NorthwindEmployee(2),
        new NorthwindEmployee(3), new NorthwindEmployee(4),
        new NorthwindEmployee(5), new NorthwindEmployee(6),
        new NorthwindEmployee(7), new NorthwindEmployee(8),
        new NorthwindEmployee(9), new NorthwindEmployee(10),
        new NorthwindEmployee(11), new NorthwindEmployee(12),
        new NorthwindEmployee(13), new NorthwindEmployee(14),
        new NorthwindEmployee(15), new NorthwindEmployee(16),
        new NorthwindEmployee(17), new NorthwindEmployee(18),
        new NorthwindEmployee(19), new NorthwindEmployee(20)
    };
    // Returns a collection of NorthwindEmployee objects.
    public static ICollection GetAllEmployees(int startIndex, int maxRows)
    {
        ArrayList al = new ArrayList();
        if (maxRows <= 0)
        {
            maxRows = employees.Count;
        }
        if (startIndex < 0 || startIndex >= 20)
        {
            return al;
        }
        for (int i = startIndex; i < startIndex + maxRows && i < 20; i++)
        {
            al.Add(employees[i]);
        }
        return al;
    }
}


public class NorthwindEmployee
{

    public NorthwindEmployee(int id)
    {
        ID = id.ToString();
        lastName = "last_name_" + id + "test";
        firstName = "first_name" + id + "test";
        title = "title_" + id + "test";
        titleOfCourtesy = " titleOfCourtesy_" + id + "test";
        reportsTo = -1;
    }

    private object ID;
    public string EmployeeID
    {
        get { return ID.ToString(); }
        set { ID = value; }
    }

    private string lastName;
    public string LastName
    {
        get { return lastName; }
        set { lastName = value; }
    }

    private string firstName;
    public string FirstName
    {
        get { return firstName; }
        set { firstName = value; }
    }

    private string title;
    public String Title
    {
        get { return title; }
        set { title = value; }
    }

    private string titleOfCourtesy;
    public string Courtesy
    {
        get { return titleOfCourtesy; }
        set { titleOfCourtesy = value; }
    }

    private int reportsTo;
    public int Supervisor
    {
        get { return reportsTo; }
        set { reportsTo = value; }
    }

    public bool Save()
    {
        return true;
    }
}

internal class NorthwindDataException : Exception
{
    public NorthwindDataException(string msg) : base(msg) { }
}
