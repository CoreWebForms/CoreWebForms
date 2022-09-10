using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI.WebControls;

public class TextBox : WebControl
{
    public TextBox()
        : base(HtmlTextWriterTag.Input)
    {
    }
}

