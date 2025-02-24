<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
public class ProductContext : System.Data.Entity.DbContext
{
    public System.Data.Entity.DbSet<Category> Categories { get; set; }
    public System.Data.Entity.DbSet<Product> Products { get; set; }
}

public class Category
{
}

public class Product
{
}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<body>
    <h1>Entity Framework compilation</h1>
    <p>
        If this shows, then types from EF were able to be compiled.
    </p>
</body>
</html>
