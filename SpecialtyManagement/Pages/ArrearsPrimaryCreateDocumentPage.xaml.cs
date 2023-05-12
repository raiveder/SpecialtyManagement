using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int IdTypeArrear = 1;
        private int _idPM;
        private Filter _filter;
        private List<Arrears> _arrears; // Список задолженностей.
        private List<Teachers> _teachers = new List<Teachers>(); // Список учителей.
        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин.
        private List<string> _typesLessons = new List<string>(); // Список типов дисциплин для отображения (учебные дисциплины или ПМ).
        private List<string> _dates = new List<string>(); // Список дат.
        private List<string> _times = new List<string>(); // Список времён.
        private List<string> _audiences = new List<string>(); // Список аудиторий.

        public ArrearsPrimaryCreateDocumentPage(Filter filter, List<Arrears> arrears)
        {
            InitializeComponent();

            _filter = filter;
            _arrears = arrears;
            Arrears.DeleteArrearsNotMatchByType(_arrears, IdTypeArrear);

            TypesLessons typeLesson = Database.Entities.TypesLessons.FirstOrDefault(x => x.Type == "ПМ");
            if (typeLesson != null)
            {
                _idPM = typeLesson.Id;
            }
            else
            {
                MessageBox.Show("Отсутствует тип дисциплины \"ПМ\". Добавьте его, прежде чем формировать протокол", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Навигация на добавление типа дисциплин.
                _idPM = 0;
            }

            //int number = 0;
            //foreach (Arrears arrear in _arrears)
            //{
            //    List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrear, IdTypeArrear);
            //    List<Lessons> lessonsPM = lessons.Where(x => x.IdType == _idPM).ToList();

            //    if (lessonsPM.Count == 0)
            //    {
            //        for (int i = 0; i < lessons.Count; i++)
            //        {
            //            int idLesson = lessons[i].Id;
            //            DistributionLessons distributionLessons = Database.Entities.DistributionLessons.FirstOrDefault(x => x.IdLesson == idLesson &&
            //            x.Groups.Id == arrear.Students.IdGroup);
            //            if (distributionLessons == null)
            //            {
            //                string currentGroupString = arrear.Students.Groups.Group;
            //                string lastGroupString = Convert.ToInt32(currentGroupString[0].ToString()) - 1 + currentGroupString.Substring(1, currentGroupString.Length - 2);
            //                Groups lastGroup = Database.Entities.Groups.FirstOrDefault(x => x.Group.Substring(0, 2) == lastGroupString);
            //                distributionLessons = Database.Entities.DistributionLessons.FirstOrDefault(x => x.IdLesson == idLesson &&
            //                x.Groups.Id == lastGroup.Id);
            //            }

            //            Teachers teacher = Database.Entities.Teachers.FirstOrDefault(x => x.Id == distributionLessons.IdTeacher);

            //            if (!_teachers.Contains(teacher))
            //            {
            //                _teachers.Add(teacher);
            //                _typesLessons.Add("Учебные дисциплины");
            //                _lessons.Add(lessons[i]);
            //                _dates.Add(string.Empty);
            //                _times.Add(string.Empty);
            //                _audiences.Add(string.Empty);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < lessonsPM.Count; i++)
            //        {
            //            int idLesson = lessonsPM[i].Id;
            //            List<DistributionLessons> distributionsLessons = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson &&
            //            x.Groups.Id == arrear.Students.IdGroup).ToList();
            //            if (distributionsLessons.Count == 0)
            //            {
            //                string currentGroupString = arrear.Students.Groups.Group;
            //                string lastGroupString = Convert.ToInt32(currentGroupString[0].ToString()) - 1 + currentGroupString.Substring(1, currentGroupString.Length - 2);
            //                Groups lastGroup = Database.Entities.Groups.FirstOrDefault(x => x.Group.Substring(0, 2) == lastGroupString);
            //                distributionsLessons = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson &&
            //                x.Groups.Id == lastGroup.Id).ToList();
            //            }

            //            List<Teachers> tempTeachers = new List<Teachers>();
            //            foreach (DistributionLessons item in distributionsLessons)
            //            {
            //                tempTeachers.Add(item.Teachers);
            //            }

            //            Teachers teacher = new Teachers();

            //            for (int j = 0; j < tempTeachers.Count; j++)
            //            {
            //                teacher.Surname += tempTeachers[j].FullName + ",\n"; // Для отображения в документе.
            //                teacher.Name += tempTeachers[j].ShortName + "\n"; // Для отображения в приложении.
            //            }
            //            teacher.Surname = teacher.Surname.Substring(0, teacher.Surname.Length - 2);
            //            teacher.Name = teacher.Name.Substring(0, teacher.Name.Length - 1);

            //            if (!_teachers.Contains(teacher))
            //            {
            //                teacher.SequenceNumber = number++;
            //                _teachers.Add(teacher);
            //                _typesLessons.Add(lessonsPM[i].ShortName);
            //                _lessons.Add(lessonsPM[i]);
            //                _dates.Add(string.Empty);
            //                _times.Add(string.Empty);
            //                _audiences.Add(string.Empty);
            //            }
            //        }
            //    }
            //}

            List<List<Teachers>> teachersd = new List<List<Teachers>>();
            ListView.ItemsSource = teachersd;
        }

        private void TBTypeLessons_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            int index = Convert.ToInt32(tb.Uid);
            tb.Text = _typesLessons[index];
        }

        private void DPDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            int index = Convert.ToInt32(datePicker.Uid);
            DateTime date = datePicker.SelectedDate.Value;

            if (date < DateTime.Today)
            {
                MessageBox.Show("Дата назначения пересдачи не может быть ранее, чем сегодня", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                datePicker.IsDropDownOpen = true;
            }
            else
            {
                _dates[index] = date.ToString("d");
            }
        }

        private void DPDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            //DatePicker datePicker = sender as DatePicker;
            //int index = Convert.ToInt32(datePicker.Uid);
            //DateTime date = datePicker.SelectedDate.Value;
            //
            //if (date < DateTime.Today)
            //{
            //    MessageBox.Show("Дата назначения пересдачи не может быть ранее, чем сегодня", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    datePicker.Focus();
            //}
            //else
            //{
            //    _dates[index] = date.ToString("d");
            //}
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
            int index = Convert.ToInt32(box.Uid);

            if (box.Text.Length > 0)
            {
                box.Foreground = Brushes.Black;
                if (Regex.IsMatch(box.Text, @"^(([0-1][0-9])|([2][0-3])):([0-5][0-9])$"))
                {
                    _times[index] = box.Text;
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

        private void TBoxAudience_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            _audiences[Convert.ToInt32(box.Uid)] = box.Text;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                Word.Application app = new Word.Application();
                CreateDocument(app);
                app.Visible = true;
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей.
        /// </summary>
        /// <returns>True - если все данные заполнены корректно, в противном случае - false.</returns>
        private bool CheckFillData()
        {
            //foreach (string item in _dates)
            //{
            //    if (item == string.Empty)
            //    {
            //        MessageBox.Show("Не все даты работы преподавателей выбраны", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return false;
            //    }
            //    else if (!DateTime.TryParse(item, out DateTime result))
            //    {
            //        MessageBox.Show("Проверьте корректность выбранных дат работы преподавателей", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return false;
            //    }
            //}

            //foreach (string item in _times)
            //{
            //    if (item == string.Empty)
            //    {
            //        MessageBox.Show("Не все времена работы преподавателей выбраны", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return false;
            //    }
            //    else if (!Regex.IsMatch(item, @"^(([0-1][0-9])|([2][0-3])):([0-5][0-9])$"))
            //    {
            //        MessageBox.Show("Проверьте корректность выбранного времени работы преподавателей", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return false;
            //    }
            //}

            //foreach (string item in _audiences)
            //{
            //    if (item == string.Empty)
            //    {
            //        MessageBox.Show("Не все аудитории выбраны", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return false;
            //    }
            //}

            return true;
        }

        /// <summary>
        /// Генерирует документ Word для первичных задолженностей.
        /// </summary>
        /// <param name="app">экземпляр приложения Word.</param>
        private void CreateDocument(Word.Application app)
        {
            List<Groups> groups = Arrears.GetGroupsWithArrears(_arrears, 1);

            Word.Document document = new Word.Document();
            document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            for (int i = 0; i < groups.Count; i++)
            {
                List<Arrears> arrears = new List<Arrears>();

                foreach (Arrears item in _arrears)
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
                rangeHeader.Font.Size = 24;
                rangeHeader.Bold = 1;
                rangeHeader.Paragraphs.Space1();
                paragraphHeader.FirstLineIndent = 0;
                rangeHeader.ParagraphFormat.RightIndent = 0;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "ознакомления с графиком ликвидации задолженностей по итогам";
                rangeHeader.Font.Size = 14;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"промежуточной аттестации за {arrears[0].SemesterSequenceNumberRoman} семестр {arrears[0].StartYear}-{arrears[0].StartYear + 1} учебного года в группе";
                rangeHeader.Font.Size = 14;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"{groups[i].Group},";
                rangeHeader.Font.Size = 16;
                rangeHeader.Bold = 1;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"специальность {Database.Entities.Specialty.FirstOrDefault().FullName}";
                rangeHeader.Font.Size = 14;
                paragraphHeader.SpaceAfter = 16;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "Список обучающихся, имеющих задолженности, и перечень учебных дисциплин";
                rangeHeader.Font.Size = 14;
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
                tableStudents.Range.Font.Size = 12;
                tableStudents.Rows[1].Range.Bold = 1;
                tableStudents.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                float[] widths = new float[5];
                for (int j = 1; j <= 5; j++)
                {
                    tableStudents.Cell(1, j).Range.Text = "1"; // Для корректной ширины столбцов задаётся текст минимальной длины.
                    tableStudents.Columns[j].AutoFit();
                    widths[j - 1] = tableStudents.Columns[j].Width;
                };
                tableStudents.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                Thread.Sleep(100);
                tableStudents.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
                tableStudents.Columns[2].SetWidth(widths[1], Word.WdRulerStyle.wdAdjustProportional);
                tableStudents.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);
                float tempWidth = tableStudents.Columns[5].Width;
                tableStudents.Columns[5].Width = widths[4];
                tableStudents.Columns[4].Width += tempWidth - tableStudents.Columns[5].Width;

                tableStudents.Cell(1, 1).Range.Text = "№";
                tableStudents.Cell(1, 2).Range.Text = "ФИО";
                tableStudents.Cell(1, 3).Range.Text = "Кол-во задолж.";
                tableStudents.Cell(1, 4).Range.Text = "Учебные дисциплины";
                tableStudents.Cell(1, 5).Range.Text = "Подпись студента";

                List<Lessons> allLessons = new List<Lessons>(); // Список всех дисциплин, по которым есть задолженность в текущей группе.
                int number = 1;
                for (int j = 0; j < arrears.Count; j++)
                {
                    List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrears[j], 1);
                    foreach (Lessons item in lessons)
                    {
                        if (!allLessons.Contains(item))
                        {
                            allLessons.Add(item);
                        }
                    }
                    string lessonsString = GetLessonsInString(lessons);
                    arrears[j].SequenceNumber = number++;

                    tableStudents.Cell(j + 2, 1).Range.Text = arrears[j].SequenceNumber.ToString();
                    tableStudents.Cell(j + 2, 2).Range.Text = arrears[j].Students.FullName;
                    tableStudents.Cell(j + 2, 3).Range.Text = arrears[j].CountArrears.ToString();
                    tableStudents.Cell(j + 2, 4).Range.Text = lessonsString;
                    tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableStudents.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                Word.Paragraph paragraphShedule = document.Paragraphs.Add();
                Word.Range rangeShedule = paragraphShedule.Range;
                rangeShedule.Text = "График работы преподавателей";
                rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                rangeShedule.Font.Size = 16;
                rangeShedule.Bold = 1;
                rangeShedule.Paragraphs.Space1();
                paragraphShedule.SpaceBefore = 36;
                rangeShedule.InsertParagraphAfter();
                paragraphShedule.SpaceBefore = 0;

                paragraphShedule = document.Paragraphs.Add();
                rangeShedule = paragraphShedule.Range;
                rangeShedule.Text = "с обучающимися, имеющими задолженности";
                rangeShedule.Font.Size = 16;
                paragraphShedule.SpaceAfter = 18;
                rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                rangeShedule.InsertParagraphAfter();
                rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                paragraphShedule.SpaceAfter = 0;

                List<Teachers> teachers = GetAllTeachersForGroupWithLessons(groups[i].Id, allLessons);

                Word.Paragraph paragraphTeachers = document.Paragraphs.Add();
                Word.Range rangeTeachers = paragraphTeachers.Range;
                Word.Table tableTeachers = document.Tables.Add(rangeTeachers, teachers.Count + 1, 4);
                tableTeachers.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                tableTeachers.Range.Font.Size = 12;
                tableTeachers.Rows[1].Range.Bold = 1;
                tableTeachers.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                tableTeachers.Cell(1, 1).Range.Text = new string('a', 20); // Для корректной ширины столбцов задаётся текст, длина которого
                                                                           //tableTeachers.Cell(1, 1).Range.Text = new string('a', GetMaxLengthFullName(teachers)); // Для корректной ширины столбцов задаётся текст, длина которого
                tableTeachers.Cell(1, 2).Range.Text = new string('a', 18); // показывает, какое количество символов будет отображено в одной строке.
                tableTeachers.Cell(1, 3).Range.Text = new string('a', 11); // Это количество подсчитано на основании реальных данных таблицы.
                tableTeachers.Cell(1, 4).Range.Text = new string('a', 13);
                Debug.WriteLine("After. Table " + (i + 1) + ". Width: " + tableTeachers.Columns[1].Width);

                widths = new float[4];
                for (int j = 1; j <= 4; j++)
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

                List<Lessons> lessonsPM = new List<Lessons>();
                for (int j = 0; j < teachers.Count; j++)
                {
                    List<Lessons> lessons = GetLessonsForTeacherAndGroup(groups[i].Id, teachers[j], allLessons);
                    foreach (Lessons item in GetPMFromLessons(lessons))
                    {
                        if (!lessonsPM.Contains(item))
                        {
                            lessonsPM.Add(item);
                        }
                    };
                    int index = _teachers.IndexOf(teachers[j]);

                    tableTeachers.Cell(j + 2, 1).Range.Text = teachers[j].Surname;
                    tableTeachers.Cell(j + 2, 2).Range.Text = GetLessonsInString(lessons);
                    tableTeachers.Cell(j + 2, 3).Range.Text = _dates[index];
                    tableTeachers.Cell(j + 2, 4).Range.Text = _times[index] + ", " + GetAudienceInString(_audiences[index]);
                    tableTeachers.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableTeachers.Cell(j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                if (lessonsPM.Count > 0)
                {
                    for (int j = 0; j < lessonsPM.Count; j++)
                    {
                        int index = _lessons.IndexOf(lessonsPM[j]);
                        tableTeachers.Rows.Add();

                        tableTeachers.Cell(teachers.Count + j + 2, 1).Range.Text = _teachers[index].Surname;
                        tableTeachers.Cell(teachers.Count + j + 2, 2).Range.Text = lessonsPM[j].FullName;
                        tableTeachers.Cell(teachers.Count + j + 2, 3).Range.Text = _dates[index];
                        tableTeachers.Cell(teachers.Count + j + 2, 4).Range.Text = _times[index] + ", " + GetAudienceInString(_audiences[index]);
                        tableTeachers.Cell(teachers.Count + j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        tableTeachers.Cell(teachers.Count + j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
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
                    paragraphLines.SpaceBefore = 16;
                    rangeLines.ParagraphFormat.RightIndent = app.CentimetersToPoints(3);
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

        /// <summary>
        /// Возвращает список всех преподавателей, которые ведут указанные дисциплины в указанной группе.
        /// </summary>
        /// <param name="idGroup">группа.</param>
        /// <param name="lessons">дисциплины.</param>
        /// <returns>Список преподавателей, которые ведут указанные дисциплины в указанной группе.</returns>
        private List<Teachers> GetAllTeachersForGroupWithLessons(int idGroup, List<Lessons> lessons)
        {
            List<Teachers> teachers = new List<Teachers>();

            foreach (DistributionLessons item in Database.Entities.DistributionLessons.Where(x => x.IdGroup == idGroup))
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
        private List<Lessons> GetLessonsForTeacherAndGroup(int idGroup, Teachers teacher, List<Lessons> lessonsSource)
        {
            List<Lessons> lessons = new List<Lessons>();

            foreach (DistributionLessons item in Database.Entities.DistributionLessons.Where(x => x.IdGroup == idGroup && x.IdTeacher == teacher.Id))
            {
                if (lessonsSource.Contains(item.Lessons))
                {
                    lessons.Add(item.Lessons);
                }
            }

            return lessons;
        }

        private int GetMaxLengthFullName(List<Teachers> teachers)
        {
            int length = 0;

            foreach (Teachers item in teachers)
            {
                if (item.FullName.Length > length)
                {
                    length = item.FullName.Length;
                }
            }

            return length;
        }

        /// <summary>
        /// Возвращает дисциплины в виде строки.
        /// </summary>
        /// <param name="lessons">список дисциплин.</param>
        /// <returns>Строка с дисциплинами.</returns>
        private string GetLessonsInString(List<Lessons> lessons)
        {
            string lessonsString = string.Empty;

            foreach (Lessons item in lessons)
            {
                lessonsString += item.FullName + ",\n";
            }

            return lessonsString.Substring(0, lessonsString.Length - 2);
        }

        private string GetAudienceInString(string text)
        {
            if (int.TryParse(text, out int result))
            {
                return "ауд." + result;
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// Удаляет дисциплины ПМ из списка.
        /// </summary>
        /// <param name="lessons">список дисциплин.</param>
        /// <returns>Удалённые дисциплины ПМ.</returns>
        private List<Lessons> GetPMFromLessons(List<Lessons> lessons)
        {
            List<Lessons> lessonsPM = lessons.Where(x => x.TypesLessons.Id == _idPM).ToList();

            foreach (Lessons item in lessonsPM)
            {
                lessons.Remove(item);
            }

            return lessonsPM;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
        }
    }
}