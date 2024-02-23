// MIT License.

using System.Web;
using SharedLibrary;

[assembly: PreApplicationStartMethod(typeof(Class1), nameof(Class1.Startup))]
//[assembly: PreApplicationStartMethod(typeof(Class1), nameof(Class1.Startup2))]
//[assembly: PreApplicationStartMethod(typeof(Class1), nameof(Class1.Startup3))]
[assembly: PreApplicationStartMethod(typeof(Class2), nameof(Class2.Start2))]
//[assembly: PreApplicationStartMethod(typeof(Class3), nameof(Class3.Start2))]
[assembly: PreApplicationStartMethod(typeof(Class3), nameof(Class3.StartStatic))]

namespace SharedLibrary;

public static class Class1
{
    public static void Startup()
    {
    }

    public static int Startup3()
    {
        return 5;
    }

    public static void Startup2(string input)
    {
    }
}

public class Class2
{
    public void Start2()
    {
    }
}

public class Class3
{
    public Class3(int i)
    {

    }

    public void Start2()
    {
    }

    public static void StartStatic()
    {
    }
}
