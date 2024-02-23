// MIT License.

namespace System.Web.UI.WebControls
{
    /// <devdoc>
    /// <para>Provides data for the <see langword='DayRender'/> event of a <see cref='Calendar'/>.
    /// </para>
    /// </devdoc>
    public sealed class DayRenderEventArgs
    {
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='DayRenderEventArgs'/> class.</para>
        /// </devdoc>
        public DayRenderEventArgs(TableCell cell, CalendarDay day)
        {
            Day = day;
            Cell = cell;
        }

        public DayRenderEventArgs(TableCell cell, CalendarDay day, string selectUrl)
        {
            Day = day;
            Cell = cell;
            SelectUrl = selectUrl;
        }

        /// <devdoc>
        ///    <para>Gets the cell that contains the day. This property is read-only.</para>
        /// </devdoc>
        public TableCell Cell { get; }

        /// <devdoc>
        ///    <para>Gets the day to render. This property is read-only.</para>
        /// </devdoc>
        public CalendarDay Day { get; }

        public string SelectUrl { get; }
    }
}
