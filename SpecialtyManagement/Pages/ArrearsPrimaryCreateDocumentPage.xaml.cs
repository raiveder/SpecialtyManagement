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
        private int _idPM;
        private Filter _filter;
        private List<Arrears> _arrears; // Список задолженностей.
        private List<List<Teachers>> _teachers = new List<List<Teachers>>(); // Список учителей.



        // Переделать _teachers на 3-х мерный список и изменить метод GetAllTeachersForGroupWithLessons().



        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин.
        private List<string> _typesLessons = new List<string>(); // Список типов дисциплин для отображения (учебные дисциплины или ПМ).
        private List<string> _dates = new List<string>(); // Список дат.
        private List<string> _times = new List<string>(); // Список времён.
        private List<string> _audiences = new List<string>(); // Список аудиторий.
        private int _indexViewItem = 0; // Индекс отображаемого элемента ListView.

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
                Navigation.Frame.Navigate(new ArrearsShowPage());
            }

            foreach (Arrears arrear in _arrears)
            {
                List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrear, IdTypeArrear);
                List<Lessons> lessonsPM = CutPMFromLessons(lessons);

                if (lessonsPM.Count == 0)
                {
                    for (int i = 0; i < lessons.Count; i++)
                    {
                        int idLesson = lessons[i].Id;
                        Teachers teacher = new Teachers();

                        try
                        {
                            DistributionLessons distributionLessons = Database.Entities.DistributionLessons.FirstOrDefault(x => x.IdLesson == idLesson &&
                            x.Groups.Id == arrear.Students.IdGroup);
                            if (distributionLessons == null)
                            {
                                string currentGroupString = arrear.Students.Groups.Group;
                                string lastGroupString = Convert.ToInt32(currentGroupString[0].ToString()) - 1 + currentGroupString.Substring(1, currentGroupString.Length - 2);
                                Groups lastGroup = Database.Entities.Groups.FirstOrDefault(x => x.Group.Substring(0, 2) == lastGroupString);
                                distributionLessons = Database.Entities.DistributionLessons.FirstOrDefault(x => x.IdLesson == idLesson &&
                                x.Groups.Id == lastGroup.Id);
                            }

                            teacher = Database.Entities.Teachers.FirstOrDefault(x => x.Id == distributionLessons.IdTeacher);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Для дисциплины \"" + lessons[i].FullName + "\" не был найден преподаватель. Выберите его вручную", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ChoiceElementWindow window = new ChoiceElementWindow(teacher, "Выбор преподавателя");

                            while (true)
                            {
                                window.ShowDialog();

                                if ((bool)window.DialogResult)
                                {
                                    Teachers tempTeacher = Database.Entities.Teachers.FirstOrDefault(x => x.Id == teacher.Id);
                                    teacher = tempTeacher;
                                    break;
                                }
                                else
                                {
                                    MessageBox.Show("Для продолжения работы выберите преподавателя дисциплины \"" + lessons[i].FullName + "\"", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }

                        if (!IsTeacherContains(teacher))
                        {
                            _teachers.Add(new List<Teachers>() { teacher });
                            _typesLessons.Add("Учебные дисциплины");
                            _lessons.Add(lessons[i]);
                            _dates.Add(string.Empty);
                            _times.Add(string.Empty);
                            _audiences.Add(string.Empty);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lessonsPM.Count; i++)
                    {
                        int idLesson = lessonsPM[i].Id;
                        List<DistributionLessons> distributionsLessons = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson &&
                        x.Groups.Id == arrear.Students.IdGroup).ToList();
                        if (distributionsLessons.Count == 0)
                        {
                            string currentGroupString = arrear.Students.Groups.Group;
                            string lastGroupString = Convert.ToInt32(currentGroupString[0].ToString()) - 1 + currentGroupString.Substring(1, currentGroupString.Length - 2);
                            Groups lastGroup = Database.Entities.Groups.FirstOrDefault(x => x.Group.Substring(0, 2) == lastGroupString);
                            distributionsLessons = Database.Entities.DistributionLessons.Where(x => x.IdLesson == idLesson &&
                            x.Groups.Id == lastGroup.Id).ToList();
                        }

                        List<Teachers> teachers = new List<Teachers>();
                        foreach (DistributionLessons item in distributionsLessons)
                        {
                            teachers.Add(item.Teachers);
                        }

                        if (!IsTeacherContains(teachers))
                        {
                            _teachers.Add(teachers);
                            _typesLessons.Add(lessonsPM[i].ShortName);
                            _lessons.Add(lessonsPM[i]);
                            _dates.Add(string.Empty);
                            _times.Add(string.Empty);
                            _audiences.Add(string.Empty);
                        }
                    }
                }
            }

            ListView.ItemsSource = _teachers;
        }

        /// <summary>
        /// Удаляет дисциплины ПМ из списка.
        /// </summary>
        /// <param name="lessons">список дисциплин.</param>
        /// <returns>Удалённые дисциплины ПМ.</returns>
        private List<Lessons> CutPMFromLessons(List<Lessons> lessons)
        {
            List<Lessons> lessonsPM = lessons.Where(x => x.TypesLessons.Id == _idPM).ToList();

            foreach (Lessons item in lessonsPM)
            {
                lessons.Remove(item);
            }

            return lessonsPM;
        }

        /// <summary>
        /// Проверяет, содержится ли указанный преподаватель в списке преподавателей.
        /// </summary>
        /// <param name="teacher">преподаватель.</param>
        /// <returns>True, если указанный преподаватель содержится в списке преподавателей, в противном случае - false.</returns>
        private bool IsTeacherContains(Teachers teacher)
        {
            foreach (List<Teachers> item in _teachers.Where(x => x.Count == 1))
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
            foreach (List<Teachers> item in _teachers.Where(x => x.Count == teachers.Count))
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

        private void TBTypeLessons_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            tb.Text = _typesLessons[GetIndexTeacher(tb.DataContext as List<Teachers>)];
        }

        private void TBTeachers_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            List<Teachers> teachers = _teachers[_indexViewItem++];

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
        }

        private void DPDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            DateTime date = datePicker.SelectedDate.Value;

            if (date < DateTime.Today)
            {
                MessageBox.Show("Дата назначения пересдачи не может быть ранее, чем сегодня", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                datePicker.IsDropDownOpen = true;
            }
            else
            {
                _dates[GetIndexTeacher(datePicker.DataContext as List<Teachers>)] = date.ToString("d");
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
                    _times[GetIndexTeacher(box.DataContext as List<Teachers>)] = box.Text;
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
            _audiences[GetIndexTeacher(box.DataContext as List<Teachers>)] = box.Text;
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
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"промежуточной аттестации за {arrears[0].SemesterSequenceNumberRoman} семестр {arrears[0].StartYear}-{arrears[0].StartYear + 1} учебного года в группе";
                rangeHeader.Font.Name = "Times New Roman";
                rangeHeader.Font.Size = 14;
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
                paragraphHeader.SpaceAfter = 16;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "Список обучающихся, имеющих задолженности, и перечень учебных дисциплин";
                rangeHeader.Font.Name = "Times New Roman";
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
                tableStudents.Cell(1, 2).Range.Text = new string('a', GetMaxLengthSurnameAndName(tempStudents)); // Для задания ширины столбца по максимальной длине контента.
                tableStudents.Columns[2].AutoFit();
                widths[1] = tableStudents.Columns[2].Width;
                for (int j = 1; j <= 5; j++)
                {
                    if (j == 2)
                    {
                        continue;
                    }
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

                tableTeachers.Cell(1, 1).Range.Text = new string('a', GetMaxLengthFullName(teachers)); // Для корректной ширины столбцов задаётся текст, длина которого
                tableTeachers.Cell(1, 2).Range.Text = new string('a', 18); // показывает, какое количество символов будет отображено в одной строке.
                tableTeachers.Cell(1, 3).Range.Text = new string('a', 11); // Это количество подсчитано на основании реальных данных таблицы.
                tableTeachers.Cell(1, 4).Range.Text = new string('a', 13);

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
                    foreach (Lessons item in CutPMFromLessons(lessons))
                    {
                        if (!lessonsPM.Contains(item))
                        {
                            lessonsPM.Add(item);
                        }
                    };
                    int index = GetIndexTeacher(teachers[j]);

                    tableTeachers.Cell(j + 2, 1).Range.Text = teachers[j].FullName;
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

                        tableTeachers.Cell(teachers.Count + j + 2, 1).Range.Text = GetTeachersInString(_teachers[index]);
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
                    paragraphLines.Space1();
                    paragraphLines.SpaceAfter = 0;
                    paragraphLines.SpaceBefore = 16;
                    paragraphLines.FirstLineIndent = 0;
                    paragraphLines.RightIndent = app.CentimetersToPoints(3);
                    paragraphLines.LeftIndent = 0;
                    rangeLines.Font.Name = "Times New Roman";
                    rangeLines.Font.Size = 14;
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

        /// <summary>
        /// Получает максимальную длинну фамилии и имени человека.
        /// </summary>
        /// <param name="students">список студентов.</param>
        /// <returns>Максимальная длинна фамилии и имени человека.</returns>
        private int GetMaxLengthSurnameAndName(List<Students> students)
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

            return lessonsString.Substring(0, lessonsString.Length - 2);
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
        private int GetIndexTeacher(Teachers teacher)
        {
            foreach (List<Teachers> item in _teachers.Where(x => x.Count == 1))
            {
                if (item[0] == teacher)
                {
                    return _teachers.IndexOf(item);
                }
            }

            return -1;
        }

        /// <summary>
        /// Возвращает индекс преподавателя из списка.
        /// </summary>
        /// <param name="teachers">преподаватель.</param>
        /// <returns>Индекс преподавателя из списка</returns>
        private int GetIndexTeacher(List<Teachers> teachers)
        {
            foreach (List<Teachers> item in _teachers.Where(x => x.Count == teachers.Count))
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
                    return _teachers.IndexOf(item);
                }
            }

            return -1;
        }

        /// <summary>
        /// Возвращает список преподавателей в виде строки.
        /// </summary>
        /// <param name="teachers">список преподавателей.</param>
        /// <returns>Список преподавателей.</returns>
        private string GetTeachersInString(List<Teachers> teachers)
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
        }
    }
}