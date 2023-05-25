using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Word = Microsoft.Office.Interop.Word;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearsPrimaryCreateDocumentPage.xaml
    /// </summary>
    public partial class ArrearsPrimaryCreateDocumentPage : Page
    {
        private const int IdTypeArrear = 1; // Id первичной задолженности.
        private Filter _filter;
        private static int s_idPM; // Id типа дисциплины ПМ.
        private static List<Arrears> s_arrears; // Список задолженностей.
        private static List<List<Teachers>> s_teachers; // Список учителей.
        private static List<DistributionLessons> s_distributions; // Распределение дисциплин и преподавателей на группы.
        private static List<Lessons> s_lessons; // Список дисциплин (нужен для ПМ).
        private static List<string> s_typesLessons; // Список типов дисциплин для отображения (учебные дисциплины или ПМ).
        private static List<string> s_dates; // Список дат.
        private static List<string> s_times; // Список времён.
        private static List<string> s_audiences; // Список аудиторий.
        private int _indexViewItem = 0; // Индекс отображаемого элемента ListView.
        private bool _IsFirstShowMessage = true; // True - сообщение о выборе преподавателей отображается впервые, в противном случае - false.

        public ArrearsPrimaryCreateDocumentPage(Filter filter, List<Arrears> arrears)
        {
            InitializeComponent();

            _filter = filter;
            s_arrears = arrears;
            s_idPM = Database.Entities.TypesLessons.FirstOrDefault(x => x.Type == "ПМ").Id;
            s_teachers = new List<List<Teachers>>();
            s_distributions = new List<DistributionLessons>();
            s_lessons = new List<Lessons>();
            s_typesLessons = new List<string>();
            s_dates = new List<string>();
            s_times = new List<string>();
            s_audiences = new List<string>();

            foreach (Arrears arrear in s_arrears)
            {
                List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrear, IdTypeArrear);
                List<Lessons> lessonsPM = CutPMFromLessons(lessons);

                if (lessonsPM.Count == 0)
                {
                    for (int i = 0; i < lessons.Count; i++)
                    {
                        if (!IsDistributionContains(lessons[i]))
                        {
                            List<Teachers> teachers = GetTeachersForGroupAndLessons(lessons[i], arrear.Students.Groups);

                            if (!IsTeacherContains(teachers[0]))
                            {
                                s_teachers.Add(teachers);
                                s_typesLessons.Add("Учебные дисциплины");
                                s_lessons.Add(lessons[i]);
                                s_dates.Add(string.Empty);
                                s_times.Add(string.Empty);
                                s_audiences.Add(string.Empty);
                            }
                        }
                        else
                        {
                            ActionWithDistributionAlreadyAdded(lessons[i], arrear);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lessonsPM.Count; i++)
                    {
                        if (!IsDistributionContains(lessonsPM[i]))
                        {
                            List<Teachers> teachers = GetTeachersForGroupAndLessons(lessonsPM[i], arrear.Students.Groups);

                            if (!IsTeacherContains(teachers))
                            {
                                s_teachers.Add(teachers);
                                s_typesLessons.Add(lessonsPM[i].ShortName);
                                s_lessons.Add(lessonsPM[i]);
                                s_dates.Add(string.Empty);
                                s_times.Add(string.Empty);
                                s_audiences.Add(string.Empty);
                            }
                        }
                        else
                        {
                            ActionWithDistributionAlreadyAdded(lessonsPM[i], arrear);
                        }
                    }
                }
            }

            ListView.ItemsSource = s_teachers;
        }

        /// <summary>
        /// Удаляет дисциплины ПМ из списка.
        /// </summary>
        /// <param name="lessons">список дисциплин.</param>
        /// <returns>Удалённые дисциплины ПМ.</returns>
        private static List<Lessons> CutPMFromLessons(List<Lessons> lessons)
        {
            List<Lessons> lessonsPM = lessons.Where(x => x.TypesLessons.Id == s_idPM).ToList();

            foreach (Lessons item in lessonsPM)
            {
                lessons.Remove(item);
            }

            return lessonsPM;
        }

        /// <summary>
        /// Проверяет, содержит ли список _distributions элемент с такой же дисциплиной.
        /// </summary>
        /// <param name="lesson">дисциплина.</param>
        /// <returns>True, если список _distributions содержит элемент с такой же дисциплиной, в противном случае - false.</returns>
        private bool IsDistributionContains(Lessons lesson)
        {
            foreach (DistributionLessons item in s_distributions)
            {
                if (item.Lessons == lesson)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Проверяет, содержит ли список _distributions указанный элемент по данным.
        /// </summary>
        /// <param name="distribution">распределение.</param>
        /// <returns>True, если список _distributions содержит элемент с такой же дисциплиной, в противном случае - false.</returns>
        private bool IsDistributionContains(DistributionLessons distribution)
        {
            foreach (DistributionLessons item in s_distributions)
            {
                if (item.Lessons == distribution.Lessons && item.Groups == distribution.Groups && item.Teachers == distribution.Teachers)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Получает список преподавателей, которые ведут указанную дисциплину в указанной группе.
        /// </summary>
        /// <param name="lesson">дисциплина.</param>
        /// <param name="group">группа.</param>
        /// <returns>Список преподавателей.</returns>
        private List<Teachers> GetTeachersForGroupAndLessons(Lessons lesson, Groups group)
        {
            List<Teachers> teachers = new List<Teachers>();
            int idLesson = lesson.Id;
            int idGroup = group.Id;

            List<DistributionLessons> distributions = GetDistributionsForGroupAndLesson(lesson, group);
            if (distributions.Count != 0)
            {
                foreach (DistributionLessons item in distributions)
                {
                    teachers.Add(Database.Entities.Teachers.FirstOrDefault(x => x.Id == item.IdTeacher));

                    item.Groups = group;
                    item.IdGroup = group.Id;
                    s_distributions.Add(item);
                }
            }
            else
            {
                teachers = GetChoiceTeachers(lesson);
                foreach (Teachers item in teachers)
                {
                    s_distributions.Add(new DistributionLessons()
                    {
                        Teachers = item,
                        Groups = group,
                        Lessons = lesson,
                        IdTeacher = item.Id,
                        IdGroup = group.Id,
                        IdLesson = lesson.Id
                    });
                }
            }

            return teachers;
        }

        /// <summary>
        /// Получает распределение дисциплин и преподавателей в конкретной группе.
        /// </summary>
        /// <param name="lesson">дисциплина.</param>
        /// <param name="group">группа.</param>
        /// <returns>Распределение дисциплин и преподавателей.</returns>
        private List<DistributionLessons> GetDistributionsForGroupAndLesson(Lessons lesson, Groups group)
        {
            try
            {
                int idLesson = lesson.Id;
                int idGroup = group.Id;
                List<DistributionLessons> distributions = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson && x.Groups.Id == idGroup).ToList();

                if (distributions.Count == 0)
                {
                    string lastGroupString = Convert.ToInt32(group.Group[0].ToString()) - 1 + group.Group.Substring(1, group.Group.Length - 2);
                    Groups lastGroup = Database.Entities.Groups.FirstOrDefault(x => x.Group.Substring(0, 2) == lastGroupString);
                    distributions = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson && x.Groups.Id == lastGroup.Id).ToList();
                }
                if (distributions.Count == 0)
                {
                    return new List<DistributionLessons>();
                }

                return distributions;
            }
            catch
            {
                return new List<DistributionLessons>();
            }
        }

        /// <summary>
        /// Получает список выбранных пользователей преподавателей.
        /// </summary>
        /// <param name="lesson">дисциплина.</param>
        /// <returns>Список выбранных преподавателей.</returns>
        private List<Teachers> GetChoiceTeachers(Lessons lesson)
        {
            if (_IsFirstShowMessage)
            {
                MessageBox.Show("Для некоторых дисциплин не были найдены преподаватели. Выберите их вручную", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                _IsFirstShowMessage = false;
            }

            List<Teachers> teachers = new List<Teachers>();

            if (lesson.IdType == s_idPM)
            {
                while (true)
                {
                    ChoiceElementsWindow window = new ChoiceElementsWindow(teachers, lesson.FullName, Database.Entities.Teachers.ToList());
                    window.ShowDialog();

                    if ((bool)window.DialogResult)
                    {
                        break;
                    }
                    MessageBox.Show("Для продолжения работы выберите преподавателей \"" + lesson.FullName + "\"", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                teachers.Add(new Teachers());
                while (true)
                {
                    ChoiceElementWindow window = new ChoiceElementWindow(teachers[0], lesson.FullName);
                    window.ShowDialog();

                    if ((bool)window.DialogResult)
                    {
                        break;
                    }
                    MessageBox.Show("Для продолжения работы выберите преподавателя \"" + lesson.FullName + "\"", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            List<Teachers> teachersFromDB = new List<Teachers>();
            foreach (Teachers item in teachers)
            {
                teachersFromDB.Add(Database.Entities.Teachers.FirstOrDefault(x => x.Id == item.Id));
            }

            return teachersFromDB;
        }

        /// <summary>
        /// Проверяет, содержится ли указанный преподаватель в списке преподавателей.
        /// </summary>
        /// <param name="teacher">преподаватель.</param>
        /// <returns>True, если указанный преподаватель содержится в списке преподавателей, в противном случае - false.</returns>
        private bool IsTeacherContains(Teachers teacher)
        {
            foreach (List<Teachers> item in s_teachers.Where(x => x.Count == 1))
            {
                if (item.Contains(teacher))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Проверяет, содержатся ли указанные преподаватели в списке преподавателей как одно целое (ведут 1 ПМ).
        /// </summary>
        /// <param name="teachers">преподаватели.</param>
        /// <returns>True, если указанные преподаватели содержатся в списке преподавателей, в противном случае - false.</returns>
        private bool IsTeacherContains(List<Teachers> teachers)
        {
            foreach (List<Teachers> item in s_teachers.Where(x => x.Count == teachers.Count))
            {
                bool checkFullyContains = true;

                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] != teachers[i])
                    {
                        checkFullyContains = false;
                        break;
                    }
                }

                if (checkFullyContains)
                {
                    return true;
                }
            }

            return false;
        }

        private void ActionWithDistributionAlreadyAdded(Lessons lesson, Arrears arrear)
        {
            List<DistributionLessons> distributions = s_distributions.Where(x => x.Lessons == lesson && x.Groups != arrear.Students.Groups).ToList();
            foreach (DistributionLessons item in distributions)
            {
                DistributionLessons temp = new DistributionLessons()
                {
                    Lessons = item.Lessons,
                    Groups = arrear.Students.Groups,
                    Teachers = item.Teachers,
                    IdLesson = item.IdLesson,
                    IdGroup = arrear.Students.Groups.Id,
                    IdTeacher = item.IdTeacher
                };

                if (!IsDistributionContains(temp))
                {
                    s_distributions.Add(temp);
                }
            }
        }

        private void TBTypeLessons_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            tb.Text = s_typesLessons[GetIndexTeacher(tb.DataContext as List<Teachers>)];
        }

        private void TBTeachers_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            List<Teachers> teachers = s_teachers[_indexViewItem++];

            if (teachers.Count == 1)
            {
                tb.Text = teachers[0].ShortName;
            }
            else
            {
                foreach (Teachers item in teachers)
                {
                    tb.Text += item.ShortName + "\n";
                }
                tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
            }
        }

        private void DPDate_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            datePicker.DisplayDateStart = DateTime.Now.AddDays(1);
            datePicker.DisplayDateEnd = DateTime.Now.AddMonths(1);

            DateTime dateWeekend = (DateTime)datePicker.DisplayDateStart;

            if (dateWeekend.DayOfWeek != DayOfWeek.Sunday)
            {
                for (DateTime date = dateWeekend; date <= (DateTime)datePicker.DisplayDateEnd; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dateWeekend = date;
                        break;
                    }
                }
            }

            for (DateTime date = dateWeekend; date <= (DateTime)datePicker.DisplayDateEnd; date = date.AddDays(7))
            {
                datePicker.BlackoutDates.Add(new CalendarDateRange(date));
            }
        }

        private void DPDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            DateTime date = datePicker.SelectedDate.Value;

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Дата назначения пересдачи не может быть в воскресенье", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                datePicker.IsDropDownOpen = true;
            }
            else
            {
                s_dates[GetIndexTeacher(datePicker.DataContext as List<Teachers>)] = date.ToString("d");
            }
        }

        private void TBoxTime_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (box.Text == "Время")
            {
                box.Foreground = Brushes.Black;
                box.Text = string.Empty;
            }
        }

        private void TBoxTime_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (box.Text.Length > 0)
            {
                box.Foreground = Brushes.Black;
                if (Regex.IsMatch(box.Text, @"^(([0-1][0-9])|([2][0-3])):([0-5][0-9])$"))
                {
                    s_times[GetIndexTeacher(box.DataContext as List<Teachers>)] = box.Text;
                }
                else
                {
                    MessageBox.Show("Введите корректное время (2 цифры до \":\" и 2 цифры после", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    box.Focus();
                }
            }
            else
            {
                box.Foreground = Brushes.Gray;
                box.Text = "Время";
            }
        }

        private void TBoxAudience_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            s_audiences[GetIndexTeacher(box.DataContext as List<Teachers>)] = box.Text;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            new CreateDocumentWindow().ShowDialog();
        }

        /// <summary>
        /// Генерирует документ Word для первичных задолженностей.
        /// </summary>
        public static void CreateDocument(Word.Application app)
        {
            try
            {
                List<Groups> groups = Arrears.GetGroupsWithArrears(s_arrears, 1);

                Word.Document document = new Word.Document();
                document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
                document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
                document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
                document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

                for (int i = 0; i < groups.Count; i++)
                {
                    List<Arrears> arrears = new List<Arrears>();

                    foreach (Arrears item in s_arrears)
                    {
                        if (item.Students.IdGroup == groups[i].Id)
                        {
                            arrears.Add(item);
                        }
                    }
                    Arrears.DeleteArrearsNotMatchByType(arrears, 1);

                    Word.Paragraph paragraphHeader = document.Paragraphs.Add();
                    Word.Range rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = "Протокол";
                    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 20;
                    rangeHeader.Bold = 1;
                    paragraphHeader.Space1();
                    paragraphHeader.SpaceAfter = 0;
                    paragraphHeader.SpaceBefore = 0;
                    paragraphHeader.FirstLineIndent = 0;
                    paragraphHeader.RightIndent = 0;
                    paragraphHeader.LeftIndent = 0;
                    rangeHeader.InsertParagraphAfter();

                    paragraphHeader = document.Paragraphs.Add();
                    rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = "ознакомления с графиком ликвидации задолженностей по итогам";
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 14;
                    rangeHeader.Bold = 0;
                    rangeHeader.InsertParagraphAfter();

                    paragraphHeader = document.Paragraphs.Add();
                    rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = $"промежуточной аттестации за {arrears[0].SemesterSequenceNumberRoman} семестр {arrears[0].StartYear}-{arrears[0].StartYear + 1} учебного года в группе";
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 14;
                    rangeHeader.Bold = 0;
                    rangeHeader.InsertParagraphAfter();

                    paragraphHeader = document.Paragraphs.Add();
                    rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = $"{groups[i].Group},";
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 16;
                    rangeHeader.Bold = 1;
                    rangeHeader.InsertParagraphAfter();

                    paragraphHeader = document.Paragraphs.Add();
                    rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = $"специальность {Database.Entities.Specialty.FirstOrDefault().FullName}";
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 14;
                    rangeHeader.Bold = 0;
                    paragraphHeader.SpaceAfter = 16;
                    rangeHeader.InsertParagraphAfter();

                    paragraphHeader = document.Paragraphs.Add();
                    rangeHeader = paragraphHeader.Range;
                    rangeHeader.Text = "Список обучающихся, имеющих задолженности, и перечень учебных дисциплин";
                    rangeHeader.Font.Name = "Times New Roman";
                    rangeHeader.Font.Size = 14;
                    rangeHeader.Bold = 0;
                    rangeHeader.Underline = Word.WdUnderline.wdUnderlineSingle;
                    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    rangeHeader.InsertParagraphAfter();
                    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    paragraphHeader.SpaceAfter = 0;

                    Word.Paragraph paragraphStudents = document.Paragraphs.Add();
                    Word.Range rangeStudents = paragraphStudents.Range;
                    Word.Table tableStudents = document.Tables.Add(rangeStudents, arrears.Count + 1, 5);
                    tableStudents.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    tableStudents.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    tableStudents.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    tableStudents.Range.Font.Name = "Times New Roman";
                    tableStudents.Range.Font.Size = 12;
                    tableStudents.Rows[1].Range.Bold = 1;
                    tableStudents.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    paragraphStudents.Space1();
                    paragraphStudents.SpaceAfter = 0;
                    paragraphStudents.SpaceBefore = 0;
                    paragraphStudents.FirstLineIndent = 0;
                    paragraphStudents.RightIndent = 0;
                    paragraphStudents.LeftIndent = 0;

                    List<Students> tempStudents = new List<Students>();
                    foreach (Arrears item in arrears)
                    {
                        tempStudents.Add(item.Students);
                    }
                    float[] widths = new float[5];
                    // Для корректной ширины столбцов задаётся текст, длина которого показывает, какое количество символов будет отображено в одной строке.
                    // Это количество подсчитано на основании реальных данных таблицы.
                    tableStudents.Cell(1, 1).Range.Text = new string('a', 1);
                    tableStudents.Cell(1, 2).Range.Text = new string('a', GetMaxLengthSurnameAndName(tempStudents) + 1); // + 1 для сохранения места под пробел.
                    tableStudents.Cell(1, 3).Range.Text = new string('a', 1);
                    tableStudents.Cell(1, 4).Range.Text = new string('a', 18);
                    tableStudents.Cell(1, 5).Range.Text = new string('a', 8);
                    for (int j = 5; j >= 1; j--)
                    {
                        tableStudents.Columns[j].AutoFit();
                        widths[j - 1] = tableStudents.Columns[j].Width;
                    };
                    tableStudents.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                    Thread.Sleep(100);
                    tableStudents.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
                    tableStudents.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);
                    float tempWidth = tableStudents.Columns[5].Width;
                    tableStudents.Columns[5].Width = widths[4];
                    tableStudents.Columns[4].Width += tempWidth - tableStudents.Columns[5].Width;
                    tableStudents.Columns[2].SetWidth(widths[1], Word.WdRulerStyle.wdAdjustProportional);

                    tableStudents.Cell(1, 1).Range.Text = "№";
                    tableStudents.Cell(1, 2).Range.Text = "ФИО";
                    tableStudents.Cell(1, 3).Range.Text = "Кол-во задолж.";
                    tableStudents.Cell(1, 4).Range.Text = "Учебные дисциплины";
                    tableStudents.Cell(1, 5).Range.Text = "Подпись студента";

                    List<Lessons> allLessons = new List<Lessons>(); // Список всех дисциплин, по которым есть задолженность в текущей группе.
                    int number = 1;
                    for (int j = 0; j < arrears.Count; j++)
                    {
                        List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrears[j], IdTypeArrear);
                        foreach (Lessons item in lessons)
                        {
                            if (!allLessons.Contains(item))
                            {
                                allLessons.Add(item);
                            }
                        }
                        arrears[j].SequenceNumber = number++;

                        tableStudents.Cell(j + 2, 1).Range.Text = arrears[j].SequenceNumber.ToString();
                        tableStudents.Cell(j + 2, 2).Range.Text = arrears[j].Students.FullName;
                        tableStudents.Cell(j + 2, 3).Range.Text = arrears[j].CountArrears.ToString();
                        tableStudents.Cell(j + 2, 4).Range.Text = GetLessonsInString(lessons);
                        tableStudents.Rows[j + 2].Range.Bold = 0;
                        tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        tableStudents.Cell(j + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                        tableStudents.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        tableStudents.Cell(j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    }

                    Word.Paragraph paragraphShedule = document.Paragraphs.Add();
                    Word.Range rangeShedule = paragraphShedule.Range;
                    rangeShedule.Text = "График работы преподавателей";
                    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    rangeShedule.Font.Name = "Times New Roman";
                    rangeShedule.Font.Size = 16;
                    rangeShedule.Bold = 1;
                    paragraphShedule.Space1();
                    paragraphShedule.SpaceAfter = 0;
                    paragraphShedule.SpaceBefore = 36;
                    paragraphShedule.FirstLineIndent = 0;
                    paragraphShedule.RightIndent = 0;
                    paragraphShedule.LeftIndent = 0;
                    rangeShedule.InsertParagraphAfter();
                    paragraphShedule.SpaceBefore = 0;

                    paragraphShedule = document.Paragraphs.Add();
                    rangeShedule = paragraphShedule.Range;
                    rangeShedule.Text = "с обучающимися, имеющими задолженности";
                    rangeShedule.Font.Name = "Times New Roman";
                    rangeShedule.Font.Size = 14;
                    rangeShedule.Bold = 0;
                    paragraphShedule.SpaceAfter = 18;
                    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    rangeShedule.InsertParagraphAfter();
                    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    paragraphShedule.SpaceAfter = 0;

                    List<Lessons> lessonsPM = CutPMFromLessons(allLessons);
                    List<Teachers> teachers = GetAllTeachersForGroupWithLessons(groups[i].Id, allLessons);

                    Word.Paragraph paragraphTeachers = document.Paragraphs.Add();
                    Word.Range rangeTeachers = paragraphTeachers.Range;
                    Word.Table tableTeachers = document.Tables.Add(rangeTeachers, teachers.Count + 1, 4);
                    tableTeachers.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    tableTeachers.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    tableTeachers.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    tableTeachers.Range.Font.Name = "Times New Roman";
                    tableTeachers.Range.Font.Size = 12;
                    tableTeachers.Rows[1].Range.Bold = 1;
                    tableTeachers.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    paragraphTeachers.Space1();
                    paragraphTeachers.SpaceAfter = 0;
                    paragraphTeachers.SpaceBefore = 0;
                    paragraphTeachers.FirstLineIndent = 0;
                    paragraphTeachers.RightIndent = 0;
                    paragraphTeachers.LeftIndent = 0;
                    
                    tableTeachers.Cell(1, 1).Range.Text = new string('a', GetMaxLengthFullName(teachers, GetAllTeachersForGroupWithLessons(groups[i].Id, lessonsPM)));
                    tableTeachers.Cell(1, 2).Range.Text = new string('a', 18);
                    tableTeachers.Cell(1, 3).Range.Text = new string('a', 11);
                    tableTeachers.Cell(1, 4).Range.Text = new string('a', 13);

                    widths = new float[4];
                    for (int j = 4; j >= 1; j--)
                    {
                        tableTeachers.Columns[j].AutoFit();
                        widths[j - 1] = tableTeachers.Columns[j].Width;
                    };
                    tableTeachers.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                    Thread.Sleep(100);
                    tableTeachers.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
                    tableTeachers.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);
                    tempWidth = tableTeachers.Columns[4].Width;
                    tableTeachers.Columns[4].Width = widths[3];
                    tableTeachers.Columns[2].Width += tempWidth - tableTeachers.Columns[4].Width;

                    tableTeachers.Cell(1, 1).Range.Text = "ФИО преподавателя";
                    tableTeachers.Cell(1, 2).Range.Text = "Учебные дисциплины";
                    tableTeachers.Cell(1, 3).Range.Text = "Дни недели, числа";
                    tableTeachers.Cell(1, 4).Range.Text = "Время, № ауд.";

                    for (int j = 0; j < teachers.Count; j++)
                    {
                        List<Lessons> lessons = GetLessonsForTeacherAndGroup(groups[i].Id, teachers[j], allLessons);
                        int index = GetIndexTeacher(teachers[j]);

                        tableTeachers.Cell(j + 2, 1).Range.Text = teachers[j].FullName;
                        tableTeachers.Cell(j + 2, 2).Range.Text = GetLessonsInString(lessons);
                        tableTeachers.Cell(j + 2, 3).Range.Text = s_dates[index];
                        tableTeachers.Cell(j + 2, 4).Range.Text = s_times[index] + ", " + GetAudienceInString(s_audiences[index]);
                        tableTeachers.Rows[j + 2].Range.Bold = 0;
                        tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                        tableStudents.Cell(j + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                        tableTeachers.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        tableTeachers.Cell(j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }

                    if (lessonsPM.Count > 0)
                    {
                        for (int j = 0; j < lessonsPM.Count; j++)
                        {
                            int index = s_lessons.IndexOf(lessonsPM[j]);
                            tableTeachers.Rows.Add();

                            tableTeachers.Cell(teachers.Count + j + 2, 1).Range.Text = GetTeachersInString(s_teachers[index]);
                            tableTeachers.Cell(teachers.Count + j + 2, 2).Range.Text = lessonsPM[j].FullName;
                            tableTeachers.Cell(teachers.Count + j + 2, 3).Range.Text = s_dates[index];
                            tableTeachers.Cell(teachers.Count + j + 2, 4).Range.Text = s_times[index] + ", " + GetAudienceInString(s_audiences[index]);
                            tableTeachers.Rows[j + 2].Range.Bold = 0;
                            tableTeachers.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                            tableTeachers.Cell(j + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                            tableTeachers.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            tableTeachers.Cell(j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                    }

                    for (int j = 0; j < arrears.Count; j++)
                    {
                        Word.Paragraph paragraphLines = document.Paragraphs.Add();
                        Word.Range rangeLines = paragraphLines.Range;
                        if (j == 0)
                        {
                            rangeLines.Text = "Число, подпись обучающихся:     ___________________________";
                        }
                        else
                        {
                            rangeLines.Text = "___________________________";
                        }
                        paragraphLines.Space1();
                        paragraphLines.SpaceAfter = 0;
                        paragraphLines.SpaceBefore = 16;
                        paragraphLines.FirstLineIndent = 0;
                        paragraphLines.RightIndent = app.CentimetersToPoints(3);
                        paragraphLines.LeftIndent = 0;
                        rangeLines.Font.Name = "Times New Roman";
                        rangeLines.Font.Size = 14;
                        rangeLines.Bold = 0;
                        rangeLines.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                        rangeLines.InsertParagraphAfter();
                        paragraphLines.SpaceBefore = 0;
                    }

                    if (i != groups.Count - 1)
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("занято"))
                {
                    MessageBox.Show("Не изменяйте документ, пока он не будет сформирован", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (!ex.Message.ToLower().Contains("вызов был отклонен"))
                {
                    MessageBox.Show("При формировании документа возникла ошибка\nТекст ошибки: " + ex.Message, "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            app.Visible = true;
        }

        /// <summary>
        /// Возвращает список всех преподавателей, которые ведут указанные дисциплины в указанной группе.
        /// </summary>
        /// <param name="idGroup">группа.</param>
        /// <param name="lessons">дисциплины.</param>
        /// <returns>Список преподавателей, которые ведут указанные дисциплины в указанной группе.</returns>
        private static List<Teachers> GetAllTeachersForGroupWithLessons(int idGroup, List<Lessons> lessons)
        {
            List<Teachers> teachers = new List<Teachers>();

            foreach (DistributionLessons item in s_distributions.Where(x => x.IdGroup == idGroup))
            {
                if (lessons.Contains(item.Lessons) && !teachers.Contains(item.Teachers))
                {
                    teachers.Add(item.Teachers);
                }
            }

            return teachers;
        }

        /// <summary>
        /// Возвращает список предметов, которые ведёт преподаватель в указанной группе.
        /// </summary>
        /// <param name="idGroup">группа.</param>
        /// <param name="teacher">преподаватель.</param>
        /// <param name="lessonsSource">начальный список дисциплин.</param>
        /// <returns>Список всех предметов, которые ведёт преподаватель в указанной группе.</returns>
        private static List<Lessons> GetLessonsForTeacherAndGroup(int idGroup, Teachers teacher, List<Lessons> lessonsSource)
        {
            List<Lessons> lessons = new List<Lessons>();

            foreach (DistributionLessons item in s_distributions.Where(x => x.IdGroup == idGroup && x.IdTeacher == teacher.Id))
            {
                if (lessonsSource.Contains(item.Lessons))
                {
                    lessons.Add(item.Lessons);
                }
            }

            return lessons;
        }

        /// <summary>
        /// Получает максимальную длинну фамилии и имени человека.
        /// </summary>
        /// <param name="students">список студентов.</param>
        /// <returns>Максимальная длинна фамилии и имени человека.</returns>
        private static int GetMaxLengthSurnameAndName(List<Students> students)
        {
            int length = 0;

            foreach (Students item in students)
            {
                if (item.SurnameAndName.Length > length)
                {
                    length = item.SurnameAndName.Length;
                }
            }

            return length;
        }

        /// <summary>
        /// Возвращает дисциплины в виде строки.
        /// </summary>
        /// <param name="lessons">список дисциплин.</param>
        /// <returns>Строка с дисциплинами.</returns>
        public static string GetLessonsInString(List<Lessons> lessons)
        {
            string lessonsString = string.Empty;

            foreach (Lessons item in lessons)
            {
                lessonsString += item.FullName + ",\n";
            }

            if (lessonsString.Length > 2)
            {
                return lessonsString.Substring(0, lessonsString.Length - 2);
            }
            return lessonsString;
        }

        public static string GetAudienceInString(string text)
        {
            if (int.TryParse(text, out int result))
            {
                return "ауд. " + result;
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// Возвращает индекс преподавателя из списка.
        /// </summary>
        /// <param name="teacher">преподаватель.</param>
        /// <returns>Индекс преподавателя из списка</returns>
        private static int GetIndexTeacher(Teachers teacher)
        {
            foreach (List<Teachers> item in s_teachers.Where(x => x.Count == 1))
            {
                if (item[0] == teacher)
                {
                    return s_teachers.IndexOf(item);
                }
            }

            return -1;
        }

        /// <summary>
        /// Возвращает индекс преподавателя из списка.
        /// </summary>
        /// <param name="teachers">преподаватель.</param>
        /// <returns>Индекс преподавателя из списка</returns>
        private static int GetIndexTeacher(List<Teachers> teachers)
        {
            foreach (List<Teachers> item in s_teachers.Where(x => x.Count == teachers.Count))
            {
                bool checkFullyContains = true;

                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] != teachers[i])
                    {
                        checkFullyContains = false;
                        break;
                    }
                }

                if (checkFullyContains)
                {
                    return s_teachers.IndexOf(item);
                }
            }

            return -1;
        }

        /// <summary>
        /// Возвращает список преподавателей в виде строки.
        /// </summary>
        /// <param name="teachers">список преподавателей.</param>
        /// <returns>Список преподавателей.</returns>
        private static string GetTeachersInString(List<Teachers> teachers)
        {
            string result = string.Empty;

            foreach (Teachers item in teachers)
            {
                result += item.FullName + ",\n";
            }
            return result.Substring(0, result.Length - 2);
        }

        /// <summary>
        /// Получает количество символов максимальной длины полного имени человека.
        /// </summary>
        /// <param name="teachers">список преподавателей.</param>
        /// <returns>Количество символов.</returns>
        private static int GetMaxLengthFullName(List<Teachers> teachers, List<Teachers> teachersPM)
        {
            int length = 0;

            foreach (Teachers item in teachers)
            {
                if (item.FullName.Length > length)
                {
                    length = item.FullName.Length;
                }
            }

            foreach (Teachers item in teachersPM)
            {
                if (teachersPM.Last() != item)
                {
                    if (item.FullName.Length + 1 > length) // + 1 для сохранения места под запятую.
                    {
                        length = item.FullName.Length;
                    }
                }
                else
                {
                    if (item.FullName.Length > length)
                    {
                        length = item.FullName.Length;
                    }
                }
            }

            return length;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
        }

        private void DPDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            DateTime date = datePicker.SelectedDate.Value;

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Дата назначения пересдачи не может быть в воскресенье", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                s_dates[GetIndexTeacher(datePicker.DataContext as List<Teachers>)] = date.ToString("d");
            }
        }
    }
}