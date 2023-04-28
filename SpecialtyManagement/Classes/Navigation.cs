using SpecialtyManagement.Classes;
using System.Windows.Controls;

namespace SpecialtyManagement
{
    /// <summary>
    /// Структура для сохранения настроек фильтра данных.
    /// </summary>
    public struct Filter
    {
        public string FindText { get; set; }
        public int IndexSort { get; set; }
        public int IndexGroup { get; set; }
        public int IndexType { get; set; }
        public bool HasNote { get; set; }
        public bool IsLastSemester { get; set; }
        public bool IsCurrentSemester { get; set; }
    }

    public class Navigation
    {
        public static Frame Frame;
        public static Setting Setting;
    }
}