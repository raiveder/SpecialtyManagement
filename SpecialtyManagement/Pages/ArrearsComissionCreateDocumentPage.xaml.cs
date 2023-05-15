using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Word = Microsoft.Office.Interop.Word;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearsComissionCreateDocumentPage.xaml
    /// </summary>
    public partial class ArrearsComissionCreateDocumentPage : Page
    {
        private const int IdTypeArrear = 2; // Id комиссионной задолженности.
        private Filter _filter;
        private List<Arrears> _arrears; // Список задолженностей.
        private List<Lessons> _lessonsSource = new List<Lessons>(); // Список всех дисциплин, по которым есть комисионные задолженности.
        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин для отображения.
        private List<List<Teachers>> _teachers = new List<List<Teachers>>(); // Список учителей.
        private List<List<Students>> _students = new List<List<Students>>(); // Список студентов.
        private List<DateTime> _dates = new List<DateTime>(); // Список дат.
        private List<string> _times = new List<string>(); // Список времён.
        private List<string> _audiences = new List<string>(); // Список аудиторий.

        public ArrearsComissionCreateDocumentPage(Filter filter, List<Arrears> arrears)
        {
            InitializeComponent();

            _filter = filter;
            _arrears = arrears;
            Arrears.DeleteArrearsNotMatchByType(_arrears, IdTypeArrear);
            _lessonsSource = GetAllLessonsForArrearsByType(_arrears, IdTypeArrear);
        }

        /// <summary>
        /// Возвращает список всех дисциплин, по которым у студентов есть задолженности.
        /// </summary>
        /// <param name="arrears">список задолженностей.</param>
        /// <param name="idType">тип задолженности.</param>
        /// <returns>Список всех дисциплин, по которым у студентов есть задолженности определённого типа.</returns>
        private List<Lessons> GetAllLessonsForArrearsByType(List<Arrears> arrears, int? idType)
        {
            List<Lessons> lessons = new List<Lessons>();

            foreach (Arrears arrear in arrears)
            {
                List<ArrearsLessons> arrearLessons = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id).ToList();

                if (idType != null)
                {
                    arrearLessons = arrearLessons.Where(x => x.IdType == idType).ToList();
                }

                foreach (ArrearsLessons item in arrearLessons)
                {
                    if (!lessons.Contains(item.Lessons))
                    {
                        lessons.Add(item.Lessons);
                    }
                }
            }

            return lessons;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Lessons lesson = new Lessons();
            ChoiceElementWindow window = new ChoiceElementWindow(lesson, "Выберите дисциплину", _lessonsSource);
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                _lessons.Add(lesson);
                _students.Add(new List<Students>());
                _teachers.Add(new List<Teachers>());
                _dates.Add(new DateTime());
                _times.Add(string.Empty);
                _audiences.Add(string.Empty);

                UpdateListView();
            }
        }

        private void DPDate_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            int index = Convert.ToInt32(datePicker.Uid);

            datePicker.DisplayDateStart = DateTime.Now.AddDays(1);

            if (_dates[index] >= DateTime.Now)
            {
                datePicker.SelectedDate = _dates[index];
            }
        }

        private void TBoxTime_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.Text = _times[Convert.ToInt32(box.Uid)];
        }

        private void TBoxAudience_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.Text = _audiences[Convert.ToInt32(box.Uid)];
        }

        private void SPTeachers_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            int index = Convert.ToInt32(panel.Uid);

            if (_teachers[index].Count > 0)
            {
                foreach (var item in _teachers[index])
                {
                    panel.Children.Add(new TextBlock()
                    {
                        Text = item.ShortName,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                }

            (panel.Children[panel.Children.Count - 1] as TextBlock).Margin = new Thickness(0);
            }
        }

        private void SPStudents_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            int index = Convert.ToInt32(panel.Uid);

            if (_students[index].Count > 0)
            {
                foreach (var item in _students[index])
                {
                    panel.Children.Add(new TextBlock()
                    {
                        Text = item.ShortName,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                }

            (panel.Children[panel.Children.Count - 1] as TextBlock).Margin = new Thickness(0);
            }
        }

        private void BtnChangeStudents_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32((sender as Button).Uid);
            ChoiceElementsWindow window = new ChoiceElementsWindow(_students[index], "Выберите студентов", GetStudentsForLesson(_arrears, _lessons[index]));
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                UpdateListView();
            }
        }

        /// <summary>
        /// Получает список студентов, у которых есть задолженность по указанному предмету.
        /// </summary>
        /// <param name="arrears">список задолженностей.</param>
        /// <param name="lesson">дисциплина.</param>
        /// <returns>Список студентов, у которых есть задолженность по указанному предмету.</returns>
        private List<Students> GetStudentsForLesson(List<Arrears> arrears, Lessons lesson)
        {
            List<Students> students = new List<Students>();

            foreach (Arrears item in arrears)
            {
                ArrearsLessons arrearLesson = Database.Entities.ArrearsLessons.FirstOrDefault(x => x.Arrears.Id == item.Id && x.IdLesson == lesson.Id);

                if (arrearLesson != null && !students.Contains(arrearLesson.Arrears.Students))
                {
                    students.Add(arrearLesson.Arrears.Students);
                }
            }

            return students;
        }

        private void BtnChangeTeachers_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32((sender as Button).Uid);

            ChoiceElementsWindow window = new ChoiceElementsWindow(_teachers[index], "Выберите состав комиссии", Database.Entities.Teachers.ToList());
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                UpdateListView();
            }
        }

        private void DPDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            _dates[Convert.ToInt32(datePicker.Uid)] = datePicker.SelectedDate.Value;
        }

        private void TBoxTime_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (Regex.IsMatch(box.Text, @"^(([0-1][0-9])|([2][0-3])):([0-5][0-9])$"))
            {
                _times[Convert.ToInt32(box.Uid)] = box.Text;
            }
            else
            {
                MessageBox.Show("Введите корректное время (2 цифры до \":\" и 2 цифры после", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                box.Focus();
            }
        }

        private void TBoxAudience_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            _audiences[Convert.ToInt32(box.Uid)] = box.Text;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32((sender as Button).Uid);

            _lessons.RemoveAt(index);
            _students.RemoveAt(index);
            _teachers.RemoveAt(index);
            _dates.RemoveAt(index);
            _times.RemoveAt(index);
            _audiences.RemoveAt(index);

            UpdateListView();
        }

        /// <summary>
        /// Обновляет визуальное отображение ListView.
        /// </summary>
        private void UpdateListView()
        {
            List<Lessons> tempLessons = new List<Lessons>();
            tempLessons.AddRange(_lessons);
            int indexItem = 0;

            foreach (Lessons item in tempLessons)
            {
                item.SequenceNumber = indexItem++;
            }

            ListView.ItemsSource = tempLessons;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            //if (true) Делать проверку или нет.
            {
                Word.Application app = new Word.Application();
                CreateDocumentMemo(app.Application, TBoxSender.Text, TBoxRecipient.Text);
                CreateDocumentShedule(app.Application);
                app.Visible = true;
            }
        }

        /// <summary>
        /// Генерирует документ Word для служебной записки.
        /// </summary>
        /// <param name="app">экземпляр приложения Word.</param>
        private void CreateDocumentMemo(Word.Application app, string sender, string recipient)
        {
            Word.Document document = new Word.Document();
            document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            Word.Paragraph paragraphTitle = document.Paragraphs.Add();
            Word.Range rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = "Зам. руководителю по подготовке";
            rangeTitle.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            paragraphTitle.Space1();
            paragraphTitle.SpaceAfter = 0;
            paragraphTitle.SpaceBefore = 0;
            paragraphTitle.FirstLineIndent = 0;
            paragraphTitle.RightIndent = 0;
            paragraphTitle.LeftIndent = 0;
            paragraphTitle.TabIndent(9);
            rangeTitle.InsertParagraphAfter();

            paragraphTitle = document.Paragraphs.Add();
            rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = "специалистов";
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            rangeTitle.InsertParagraphAfter();

            paragraphTitle = document.Paragraphs.Add();
            rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = recipient;
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            rangeTitle.InsertParagraphAfter();

            paragraphTitle = document.Paragraphs.Add();
            rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = "от заведующей отделением";
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            rangeTitle.InsertParagraphAfter();

            paragraphTitle = document.Paragraphs.Add();
            rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = Database.Entities.Specialty.FirstOrDefault().Departament;
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            rangeTitle.InsertParagraphAfter();

            paragraphTitle = document.Paragraphs.Add();
            rangeTitle = paragraphTitle.Range;
            rangeTitle.Text = sender;
            rangeTitle.Font.Name = "Times New Roman";
            rangeTitle.Font.Size = 14;
            rangeTitle.Bold = 0;
            rangeTitle.InsertParagraphAfter();

            Word.Paragraph paragraphHeader = document.Paragraphs.Add();
            Word.Range rangeHeader = paragraphHeader.Range;
            rangeHeader.Text = "служебная записка.";
            rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            rangeHeader.Font.Name = "Times New Roman";
            rangeHeader.Font.Size = 14;
            rangeHeader.Bold = 0;
            paragraphHeader.Space1();
            paragraphHeader.SpaceAfter = 0;
            paragraphHeader.SpaceBefore = 10;
            paragraphHeader.FirstLineIndent = 0;
            paragraphHeader.RightIndent = 0;
            paragraphHeader.LeftIndent = 0;
            rangeHeader.InsertParagraphAfter();
            paragraphHeader.SpaceBefore = 0;

            for (int i = 0; i < _lessons.Count; i++)
            {
                Word.Paragraph paragraphDescription = document.Paragraphs.Add();
                Word.Range rangeDescription = paragraphDescription.Range;
                rangeDescription.Text = i + 1 + ".\tПрошу провести комиссионную пересдачу учебной дисциплины";
                int indexWordsUnderlineStart = rangeDescription.Words.Count + 1;
                rangeDescription.Text += $" {_lessons[i].FullName} ";
                int indexWordsUnderlineEnd = rangeDescription.Words.Count;
                rangeDescription.Text += $"для следующих обучающихся {GetGroupsInString(_students[i])} в {GetDayOfWeek(_dates[i])}, ";
                int indexWordsBackgroundStart = rangeDescription.Words.Count + 1;
                rangeDescription.Text += $"{_dates[i]:D}";
                int indexWordsBackgroundEnd = rangeDescription.Words.Count;
                rangeDescription.Text += $" в {_times[i]} в кабинете {_audiences[i]}:";
                rangeDescription.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                rangeDescription.Font.Name = "Times New Roman";
                rangeDescription.Font.Size = 14;
                rangeDescription.Bold = 0;
                paragraphDescription.Space1();
                paragraphDescription.SpaceAfter = 6;
                paragraphDescription.SpaceBefore = 16;
                paragraphDescription.FirstLineIndent = 0;
                paragraphDescription.RightIndent = 0;
                paragraphDescription.LeftIndent = 0;
                for (int j = indexWordsUnderlineStart; j <= indexWordsUnderlineEnd; j++)
                {
                    rangeDescription.Words[j].Underline = Word.WdUnderline.wdUnderlineSingle;
                }
                for (int j = indexWordsBackgroundStart; j <= indexWordsBackgroundEnd; j++)
                {
                    rangeDescription.Words[j].HighlightColorIndex = Word.WdColorIndex.wdBrightGreen;
                }
                rangeDescription.InsertParagraphAfter();
                paragraphDescription.SpaceAfter = 0;
                paragraphHeader.SpaceBefore = 0;

                Word.Paragraph paragraphStudents = document.Paragraphs.Add();
                Word.Range rangeStudents = paragraphStudents.Range;
                Word.Table tableStudents = document.Tables.Add(rangeStudents, _students[i].Count + 1, 4);
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

                float[] widths = new float[3];
                tableStudents.Cell(1, 1).Range.Text = "а";
                tableStudents.Columns[1].AutoFit();
                widths[0] = tableStudents.Columns[1].Width;

                tableStudents.Cell(1, 2).Range.Text = new string('a', GetMaxLengthFullName(_students[i])); // Для задания ширины столбца по максимальной длине контента.
                tableStudents.Columns[2].AutoFit();
                widths[1] = tableStudents.Columns[2].Width;

                tableStudents.Cell(1, 3).Range.Text = "Группа"; // Для корректной ширины столбцов задаётся текст минимальной длины.
                tableStudents.Columns[3].AutoFit();
                widths[2] = tableStudents.Columns[3].Width;

                tableStudents.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                Thread.Sleep(100);
                tableStudents.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
                tableStudents.Columns[2].SetWidth(widths[1], Word.WdRulerStyle.wdAdjustProportional);
                tableStudents.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);

                tableStudents.Cell(1, 1).Range.Text = "№";
                tableStudents.Cell(1, 2).Range.Text = "ФИО";
                tableStudents.Cell(1, 4).Range.Text = "Состав комиссии";

                int number = 1;
                for (int j = 0; j < _students[i].Count; j++)
                {
                    _students[i][j].SequenceNumber = number++;

                    tableStudents.Cell(j + 2, 1).Range.Text = _students[i][j].SequenceNumber.ToString();
                    tableStudents.Cell(j + 2, 2).Range.Text = _students[i][j].FullName;
                    tableStudents.Cell(j + 2, 3).Range.Text = _students[i][j].Groups.Group;
                    tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableStudents.Cell(j + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    tableStudents.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableStudents.Rows[j + 2].Range.Bold = 0;
                }
                tableStudents.Cell(2, 4).Range.Text = GetTeachersInString(_teachers[i]);
                if (_students[i].Count > 1)
                {
                    tableStudents.Cell(2, 4).Merge(tableStudents.Cell(_students[i].Count + 1, 4));
                }
            }
        }

        /// <summary>
        /// Генерирует документ Word для комиссионных задолженностей.
        /// </summary>
        /// <param name="app">экземпляр приложения Word.</param>
        private void CreateDocumentShedule(Word.Application app)
        {
            Word.Document document = new Word.Document();
            document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            List<Students> students = GetAllStudents();
            List<List<List<Teachers>>> teachers = new List<List<List<Teachers>>>();
            List<List<Lessons>> lessons = new List<List<Lessons>>();
            List<List<DateTime>> dates = new List<List<DateTime>>();
            List<List<string>> times = new List<List<string>>();
            List<List<string>> audiences = new List<List<string>>();

            for (int i = 0; i < students.Count; i++)
            {
                teachers.Add(new List<List<Teachers>>());
                lessons.Add(new List<Lessons>());
                dates.Add(new List<DateTime>());
                times.Add(new List<string>());
                audiences.Add(new List<string>());

                foreach (List<Students> listStudents in _students)
                {
                    foreach (Students item in listStudents)
                    {
                        if (item == students[i])
                        {
                            int index = _students.IndexOf(listStudents);
                            teachers[i].Add(new List<Teachers>());
                            lessons[i].Add(_lessons[index]);
                            dates[i].Add(_dates[index]);
                            times[i].Add(_times[index]);
                            audiences[i].Add(_audiences[index]);
                            foreach (Teachers teacher in _teachers[index])
                            {
                                if (!teachers[i][lessons[i].IndexOf(lessons[i].Last())].Contains(teacher))
                                {
                                    teachers[i][lessons[i].IndexOf(lessons[i].Last())].Add(teacher);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < students.Count; i++)
            {

                Word.Paragraph paragraphHeader = document.Paragraphs.Add();
                Word.Range rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "Индивидуальный график ликвидации";
                rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                rangeHeader.Font.Name = "Times New Roman";
                rangeHeader.Font.Size = 16;
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
                rangeHeader.Text = "академических задолженностей";
                rangeHeader.Font.Name = "Times New Roman";
                rangeHeader.Font.Size = 14;
                rangeHeader.Bold = 1;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "(комиссионные пересдачи)";
                rangeHeader.Font.Name = "Times New Roman";
                rangeHeader.Font.Size = 14;
                rangeHeader.Bold = 1;
                rangeHeader.InsertParagraphAfter();

                Word.Paragraph paragraphTitle = document.Paragraphs.Add();
                Word.Range rangeTitle = paragraphTitle.Range;
                rangeTitle.Text = $"Специальность: {Database.Entities.Specialty.FirstOrDefault().FullName}";
                rangeTitle.Font.Name = "Times New Roman";
                rangeTitle.Font.Size = 14;
                rangeTitle.Bold = 0;
                rangeTitle.Words[1].Bold = 1;
                rangeTitle.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                rangeTitle.InsertParagraphAfter();

                paragraphTitle = document.Paragraphs.Add();
                rangeTitle = paragraphTitle.Range;
                rangeTitle.Text = $"ФИО студента: {students[i].FullName}";
                rangeTitle.Font.Name = "Times New Roman";
                rangeTitle.Font.Size = 14;
                rangeTitle.Bold = 0;
                rangeTitle.Words[1].Bold = 1;
                rangeTitle.InsertParagraphAfter();

                paragraphTitle = document.Paragraphs.Add();
                rangeTitle = paragraphTitle.Range;
                rangeTitle.Text = $"Группа: {students[i].Groups.Group}";
                rangeTitle.Font.Name = "Times New Roman";
                rangeTitle.Font.Size = 14;
                rangeTitle.Bold = 0;
                rangeTitle.Words[1].Bold = 1;
                paragraphTitle.SpaceAfter = 18;
                rangeTitle.InsertParagraphAfter();
                paragraphTitle.SpaceAfter = 0;

                Word.Paragraph paragraphArrears = document.Paragraphs.Add();
                Word.Range rangeArrears = paragraphArrears.Range;
                Word.Table tableArrears = document.Tables.Add(rangeArrears, lessons[i].Count + 1, 3);
                tableArrears.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableArrears.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableArrears.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                tableArrears.Range.Font.Name = "Times New Roman";
                tableArrears.Range.Font.Size = 12;
                tableArrears.Rows[1].Range.Bold = 1;
                tableArrears.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                paragraphArrears.Space1();
                paragraphArrears.SpaceAfter = 0;
                paragraphArrears.SpaceBefore = 0;
                paragraphArrears.FirstLineIndent = 0;
                paragraphArrears.RightIndent = 0;
                paragraphArrears.LeftIndent = 0;

                float[] widths = new float[2];
                tableArrears.Cell(1, 1).Range.Text = new string('a', 18); // Для задания ширины столбца по максимальной длине контента.
                tableArrears.Columns[1].AutoFit();
                widths[0] = tableArrears.Columns[1].Width;

                tableArrears.Cell(1, 2).Range.Text = new string('a', GetMaxLengthFullName(teachers[i]));
                tableArrears.Columns[2].AutoFit();
                widths[1] = tableArrears.Columns[2].Width;

                tableArrears.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
                Thread.Sleep(100);
                tableArrears.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
                tableArrears.Columns[2].SetWidth(widths[1], Word.WdRulerStyle.wdAdjustProportional);

                tableArrears.Cell(1, 1).Range.Text = "Дата, время, аудитория";
                tableArrears.Cell(1, 2).Range.Text = "Состав комиссии";
                tableArrears.Cell(1, 3).Range.Text = "Учебная дисциплина, УП, ПП, ЭПМ";

                for (int j = 0; j < lessons[i].Count; j++)
                {
                    tableArrears.Cell(j + 2, 1).Range.Text = dates[i][j].ToString("dddd");
                    tableArrears.Cell(j + 2, 1).Range.Text += dates[i][j].ToString("d") + " г.";
                    tableArrears.Cell(j + 2, 1).Range.Text += times[i][j];
                    tableArrears.Cell(j + 2, 1).Range.Text += ArrearsPrimaryCreateDocumentPage.GetAudienceInString(audiences[i][j]);
                    tableArrears.Cell(j + 2, 2).Range.Text = GetTeachersInString(teachers[i][j]);
                    tableArrears.Cell(j + 2, 3).Range.Text = lessons[i][j].FullName;
                    tableArrears.Rows[j + 2].Range.Bold = 0;
                    tableArrears.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableArrears.Cell(j + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    tableArrears.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                Word.Paragraph paragraphShedule = document.Paragraphs.Add();
                Word.Range rangeShedule = paragraphShedule.Range;
                rangeShedule.Text = "С графиком ознакомлен:\t_________________ \t\tДата: _______________";
                rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                rangeShedule.Font.Name = "Times New Roman";
                rangeShedule.Font.Size = 12;
                rangeShedule.Bold = 1;
                paragraphShedule.Space1();
                paragraphShedule.SpaceAfter = 0;
                paragraphShedule.SpaceBefore = 30;
                paragraphShedule.FirstLineIndent = 0;
                paragraphShedule.RightIndent = 0;
                paragraphShedule.LeftIndent = 0;
                rangeShedule.InsertParagraphAfter();
                paragraphShedule.SpaceBefore = 0;

                paragraphShedule = document.Paragraphs.Add();
                rangeShedule = paragraphShedule.Range;
                rangeShedule.Text = "\t\t\t\t(подпись студента)";
                rangeShedule.Font.Name = "Times New Roman";
                rangeShedule.Font.Size = 12;
                rangeShedule.Bold = 1;
                paragraphShedule.SpaceAfter = 20;
                rangeShedule.InsertParagraphAfter();
                paragraphShedule.SpaceAfter = 0;

                if (i + 1 % 2 == 0 && i != students.Count - 1)
                {
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }
            }
        }

        /// <summary>
        /// Получает список групп студентов в виде строки.
        /// </summary>
        /// <param name="students">список студентов</param>
        /// <returns>Список групп в строку.</returns>
        private string GetGroupsInString(List<Students> students)
        {
            string groupsString = string.Empty;

            if (students.Count > 0)
            {
                List<Groups> tempListGroups = new List<Groups>();

                foreach (Students item in students)
                {
                    if (!tempListGroups.Contains(item.Groups))
                    {
                        tempListGroups.Add(item.Groups);
                        groupsString += item.Groups.Group + ", ";
                    }
                }

                if (tempListGroups.Count == 1)
                {
                    groupsString = "группы " + groupsString;
                }
                else
                {
                    groupsString = "групп " + groupsString;
                }

                groupsString = groupsString.Substring(0, groupsString.Length - 2);
            }

            return groupsString;
        }

        /// <summary>
        /// Получает список преподавателей в виде строки.
        /// </summary>
        /// <param name="teachers">список преподавателей</param>
        /// <returns>Список преподавателей в строку.</returns>
        private string GetTeachersInString(List<Teachers> teachers)
        {
            string teachersString = string.Empty;

            if (teachers.Count > 0)
            {
                List<Teachers> teachersTemp = new List<Teachers>();

                foreach (Teachers item in teachers)
                {
                    if (!teachersTemp.Contains(item))
                    {
                        teachersTemp.Add(item);
                        teachersString += item.FullName + ",\n";
                    }
                }

                teachersString = teachersString.Substring(0, teachersString.Length - 2);
            }

            return teachersString;
        }

        /// <summary>
        /// Получает день недели в винительном падеже.
        /// </summary>
        /// <param name="date">дата.</param>
        /// <returns>День недели в винительном падеже.</returns>
        private string GetDayOfWeek(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "понедельник";
                case DayOfWeek.Tuesday:
                    return "вторник";
                case DayOfWeek.Wednesday:
                    return "среду";
                case DayOfWeek.Thursday:
                    return "четверг";
                case DayOfWeek.Friday:
                    return "пятницу";
                case DayOfWeek.Saturday:
                    return "субботу";
                case DayOfWeek.Sunday:
                    return "воскресенье";
                default:
                    return "не определено";
            }
        }

        /// <summary>
        /// Получает максимальную длинну полного имени человека.
        /// </summary>
        /// <param name="students">список студентов.</param>
        /// <returns>Максимальная длинна полного имени человека.</returns>
        private int GetMaxLengthFullName(List<Students> students)
        {
            int length = 0;

            foreach (Students item in students)
            {
                if (item.FullName.Length > length)
                {
                    length = item.FullName.Length;
                }
            }

            return length;
        }

        /// <summary>
        /// Получает список всех студентов (единожды) из списка _students.
        /// </summary>
        /// <returns>Список студентов.</returns>
        private List<Students> GetAllStudents()
        {
            List<Students> students = new List<Students>();

            foreach (List<Students> list in _students)
            {
                foreach (Students item in list)
                {
                    if (!students.Contains(item))
                    {
                        students.Add(item);
                    }
                }
            }

            return students;
        }

        /// <summary>
        /// Получает количество символов максимальной длины полного имени человека.
        /// </summary>
        /// <param name="teachers">список списков преподавателей.</param>
        /// <returns>Количество символов.</returns>
        private int GetMaxLengthFullName(List<List<Teachers>> teachers)
        {
            int length = 0;

            foreach (List<Teachers> list in teachers)
            {
                foreach (Teachers item in list)
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
    }
}