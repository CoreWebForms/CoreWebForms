//Mit license

namespace System.Web.UI.WebControls;

public sealed class MenuEventArgs : CommandEventArgs {
    private readonly MenuItem _item;
    private readonly object _commandSource;

    public MenuEventArgs(MenuItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
        _item = item;
        _commandSource = commandSource;
    }

    public MenuEventArgs(MenuItem item) : this(item, null, new CommandEventArgs(string.Empty, null)) {
    }

    public object CommandSource => _commandSource;

    public MenuItem Item => _item;
}
