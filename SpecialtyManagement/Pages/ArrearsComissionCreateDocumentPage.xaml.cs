using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин.
        private List<Lessons> _lessonsSource = new List<Lessons>(); // Список дисциплин, по которым есть комисионные задолженности.
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
            paragraphHeader.SpaceAfter = 16;
            paragraphHeader.SpaceBefore = 10;
            paragraphHeader.FirstLineIndent = 0;
            paragraphHeader.RightIndent = 0;
            paragraphHeader.LeftIndent = 0;
            rangeHeader.InsertParagraphAfter();
            paragraphHeader.SpaceAfter = 0;
            paragraphHeader.SpaceBefore = 0;

            for (int i = 0; i < _lessons.Count; i++)
            {
                Word.Paragraph paragraphDescription = document.Paragraphs.Add();
                Word.Range rangeDescription = paragraphDescription.Range;
                rangeDescription.Text = (i + 1) + ".\tПрошу провести комиссионную пересдачу учебной дисциплины";
                int indexWordsUnderlineStart = rangeDescription.Words.Count + 1;
                rangeDescription.Text += $" {_lessons[i].FullName} ";
                int indexWordsUnderlineEnd = rangeDescription.Words.Count - 1;
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
                paragraphDescription.SpaceAfter = 0;
                paragraphDescription.SpaceBefore = 0;
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
            //List<Groups> groups = Arrears.GetGroupsWithArrears(_arrears, 1);

            //Word.Document document = new Word.Document();
            //document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            //document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            //document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            //document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            //for (int i = 0; i < groups.Count; i++)
            //{
            //    List<Arrears> arrears = new List<Arrears>();

            //    foreach (Arrears item in _arrears)
            //    {
            //        if (item.Students.IdGroup == groups[i].Id)
            //        {
            //            arrears.Add(item);
            //        }
            //    }
            //    Arrears.DeleteArrearsNotMatchByType(arrears, 1);

            //    Word.Paragraph paragraphHeader = document.Paragraphs.Add();
            //    Word.Range rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = "Протокол";
            //    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 20;
            //    rangeHeader.Bold = 1;
            //    paragraphHeader.Space1();
            //    paragraphHeader.SpaceAfter = 0;
            //    paragraphHeader.SpaceBefore = 0;
            //    paragraphHeader.FirstLineIndent = 0;
            //    paragraphHeader.RightIndent = 0;
            //    paragraphHeader.LeftIndent = 0;
            //    rangeHeader.InsertParagraphAfter();

            //    paragraphHeader = document.Paragraphs.Add();
            //    rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = "ознакомления с графиком ликвидации задолженностей по итогам";
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 14;
            //    rangeHeader.InsertParagraphAfter();

            //    paragraphHeader = document.Paragraphs.Add();
            //    rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = $"промежуточной аттестации за {arrears[0].SemesterSequenceNumberRoman} семестр {arrears[0].StartYear}-{arrears[0].StartYear + 1} учебного года в группе";
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 14;
            //    rangeHeader.InsertParagraphAfter();

            //    paragraphHeader = document.Paragraphs.Add();
            //    rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = $"{groups[i].Group},";
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 16;
            //    rangeHeader.Bold = 1;
            //    rangeHeader.InsertParagraphAfter();

            //    paragraphHeader = document.Paragraphs.Add();
            //    rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = $"специальность {Database.Entities.Specialty.FirstOrDefault().FullName}";
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 14;
            //    paragraphHeader.SpaceAfter = 16;
            //    rangeHeader.InsertParagraphAfter();

            //    paragraphHeader = document.Paragraphs.Add();
            //    rangeHeader = paragraphHeader.Range;
            //    rangeHeader.Text = "Список обучающихся, имеющих задолженности, и перечень учебных дисциплин";
            //    rangeHeader.Font.Name = "Times New Roman";
            //    rangeHeader.Font.Size = 14;
            //    rangeHeader.Underline = Word.WdUnderline.wdUnderlineSingle;
            //    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            //    rangeHeader.InsertParagraphAfter();
            //    rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    paragraphHeader.SpaceAfter = 0;

            //    Word.Paragraph paragraphStudents = document.Paragraphs.Add();
            //    Word.Range rangeStudents = paragraphStudents.Range;
            //    Word.Table tableStudents = document.Tables.Add(rangeStudents, arrears.Count + 1, 5);
            //    tableStudents.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            //    tableStudents.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            //    tableStudents.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
            //    tableStudents.Range.Font.Name = "Times New Roman";
            //    tableStudents.Range.Font.Size = 12;
            //    tableStudents.Rows[1].Range.Bold = 1;
            //    tableStudents.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    paragraphStudents.Space1();
            //    paragraphStudents.SpaceAfter = 0;
            //    paragraphStudents.SpaceBefore = 0;
            //    paragraphStudents.FirstLineIndent = 0;
            //    paragraphStudents.RightIndent = 0;
            //    paragraphStudents.LeftIndent = 0;

            //    List<Students> tempStudents = new List<Students>();
            //    foreach (Arrears item in arrears)
            //    {
            //        tempStudents.Add(item.Students);
            //    }

            //    float[] widths = new float[5];
            //    tableStudents.Cell(1, 2).Range.Text = new string('a', GetMaxLengthSurnameAndName(tempStudents)); // Для задания ширины столбца по максимальной длине контента.
            //    tableStudents.Columns[2].AutoFit();
            //    widths[1] = tableStudents.Columns[2].Width;
            //    for (int j = 1; j <= 5; j++)
            //    {
            //        if (j == 2)
            //        {
            //            continue;
            //        }
            //        tableStudents.Cell(1, j).Range.Text = "1"; // Для корректной ширины столбцов задаётся текст минимальной длины.
            //        tableStudents.Columns[j].AutoFit();
            //        widths[j - 1] = tableStudents.Columns[j].Width;
            //    };
            //    tableStudents.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
            //    Thread.Sleep(100);
            //    tableStudents.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
            //    tableStudents.Columns[2].SetWidth(widths[1], Word.WdRulerStyle.wdAdjustProportional);
            //    tableStudents.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);
            //    float tempWidth = tableStudents.Columns[5].Width;
            //    tableStudents.Columns[5].Width = widths[4];
            //    tableStudents.Columns[4].Width += tempWidth - tableStudents.Columns[5].Width;

            //    tableStudents.Cell(1, 1).Range.Text = "№";
            //    tableStudents.Cell(1, 2).Range.Text = "ФИО";
            //    tableStudents.Cell(1, 3).Range.Text = "Кол-во задолж.";
            //    tableStudents.Cell(1, 4).Range.Text = "Учебные дисциплины";
            //    tableStudents.Cell(1, 5).Range.Text = "Подпись студента";

            //    List<Lessons> allLessons = new List<Lessons>(); // Список всех дисциплин, по которым есть задолженность в текущей группе.
            //    int number = 1;
            //    for (int j = 0; j < arrears.Count; j++)
            //    {
            //        List<Lessons> lessons = Arrears.GetLessonsForArrearsByType(arrears[j], 1);
            //        foreach (Lessons item in lessons)
            //        {
            //            if (!allLessons.Contains(item))
            //            {
            //                allLessons.Add(item);
            //            }
            //        }
            //        string lessonsString = GetLessonsInString(lessons);
            //        arrears[j].SequenceNumber = number++;

            //        tableStudents.Cell(j + 2, 1).Range.Text = arrears[j].SequenceNumber.ToString();
            //        tableStudents.Cell(j + 2, 2).Range.Text = arrears[j].Students.FullName;
            //        tableStudents.Cell(j + 2, 3).Range.Text = arrears[j].CountArrears.ToString();
            //        tableStudents.Cell(j + 2, 4).Range.Text = lessonsString;
            //        tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //        tableStudents.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    }

            //    Word.Paragraph paragraphShedule = document.Paragraphs.Add();
            //    Word.Range rangeShedule = paragraphShedule.Range;
            //    rangeShedule.Text = "График работы преподавателей";
            //    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    rangeShedule.Font.Name = "Times New Roman";
            //    rangeShedule.Font.Size = 16;
            //    rangeShedule.Bold = 1;
            //    paragraphShedule.Space1();
            //    paragraphShedule.SpaceAfter = 0;
            //    paragraphShedule.SpaceBefore = 36;
            //    paragraphShedule.FirstLineIndent = 0;
            //    paragraphShedule.RightIndent = 0;
            //    paragraphShedule.LeftIndent = 0;
            //    rangeShedule.InsertParagraphAfter();
            //    paragraphShedule.SpaceBefore = 0;

            //    paragraphShedule = document.Paragraphs.Add();
            //    rangeShedule = paragraphShedule.Range;
            //    rangeShedule.Text = "с обучающимися, имеющими задолженности";
            //    rangeShedule.Font.Name = "Times New Roman";
            //    rangeShedule.Font.Size = 14;
            //    paragraphShedule.SpaceAfter = 18;
            //    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            //    rangeShedule.InsertParagraphAfter();
            //    rangeShedule.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    paragraphShedule.SpaceAfter = 0;

            //    List<Teachers> teachers = GetAllTeachersForGroupWithLessons(groups[i].Id, allLessons);

            //    Word.Paragraph paragraphTeachers = document.Paragraphs.Add();
            //    Word.Range rangeTeachers = paragraphTeachers.Range;
            //    Word.Table tableTeachers = document.Tables.Add(rangeTeachers, teachers.Count + 1, 4);
            //    tableTeachers.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            //    tableTeachers.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            //    tableTeachers.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
            //    tableTeachers.Range.Font.Name = "Times New Roman";
            //    tableTeachers.Range.Font.Size = 12;
            //    tableTeachers.Rows[1].Range.Bold = 1;
            //    tableTeachers.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    paragraphTeachers.Space1();
            //    paragraphTeachers.SpaceAfter = 0;
            //    paragraphTeachers.SpaceBefore = 0;
            //    paragraphTeachers.FirstLineIndent = 0;
            //    paragraphTeachers.RightIndent = 0;
            //    paragraphTeachers.LeftIndent = 0;

            //    tableTeachers.Cell(1, 1).Range.Text = new string('a', GetMaxLengthFullName(teachers)); // Для корректной ширины столбцов задаётся текст, длина которого
            //    tableTeachers.Cell(1, 2).Range.Text = new string('a', 18); // показывает, какое количество символов будет отображено в одной строке.
            //    tableTeachers.Cell(1, 3).Range.Text = new string('a', 11); // Это количество подсчитано на основании реальных данных таблицы.
            //    tableTeachers.Cell(1, 4).Range.Text = new string('a', 13);

            //    widths = new float[4];
            //    for (int j = 1; j <= 4; j++)
            //    {
            //        tableTeachers.Columns[j].AutoFit();
            //        widths[j - 1] = tableTeachers.Columns[j].Width;
            //    };
            //    tableTeachers.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);
            //    Thread.Sleep(100);
            //    tableTeachers.Columns[1].SetWidth(widths[0], Word.WdRulerStyle.wdAdjustProportional);
            //    tableTeachers.Columns[3].SetWidth(widths[2], Word.WdRulerStyle.wdAdjustProportional);
            //    tempWidth = tableTeachers.Columns[4].Width;
            //    tableTeachers.Columns[4].Width = widths[3];
            //    tableTeachers.Columns[2].Width += tempWidth - tableTeachers.Columns[4].Width;

            //    tableTeachers.Cell(1, 1).Range.Text = "ФИО преподавателя";
            //    tableTeachers.Cell(1, 2).Range.Text = "Учебные дисциплины";
            //    tableTeachers.Cell(1, 3).Range.Text = "Дни недели, числа";
            //    tableTeachers.Cell(1, 4).Range.Text = "Время, № ауд.";

            //    List<Lessons> lessonsPM = new List<Lessons>();
            //    for (int j = 0; j < teachers.Count; j++)
            //    {
            //        List<Lessons> lessons = GetLessonsForTeacherAndGroup(groups[i].Id, teachers[j], allLessons);
            //        foreach (Lessons item in CutPMFromLessons(lessons))
            //        {
            //            if (!lessonsPM.Contains(item))
            //            {
            //                lessonsPM.Add(item);
            //            }
            //        };
            //        int index = GetIndexTeacher(teachers[j]);

            //        tableTeachers.Cell(j + 2, 1).Range.Text = teachers[j].FullName;
            //        tableTeachers.Cell(j + 2, 2).Range.Text = GetLessonsInString(lessons);
            //        tableTeachers.Cell(j + 2, 3).Range.Text = _dates[index];
            //        tableTeachers.Cell(j + 2, 4).Range.Text = _times[index] + ", " + GetAudienceInString(_audiences[index]);
            //        tableTeachers.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //        tableTeachers.Cell(j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    }

            //    if (lessonsPM.Count > 0)
            //    {
            //        for (int j = 0; j < lessonsPM.Count; j++)
            //        {
            //            int index = _lessons.IndexOf(lessonsPM[j]);
            //            tableTeachers.Rows.Add();

            //            tableTeachers.Cell(teachers.Count + j + 2, 1).Range.Text = GetTeachersInString(_teachers[index]);
            //            tableTeachers.Cell(teachers.Count + j + 2, 2).Range.Text = lessonsPM[j].FullName;
            //            tableTeachers.Cell(teachers.Count + j + 2, 3).Range.Text = _dates[index];
            //            tableTeachers.Cell(teachers.Count + j + 2, 4).Range.Text = _times[index] + ", " + GetAudienceInString(_audiences[index]);
            //            tableTeachers.Cell(teachers.Count + j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //            tableTeachers.Cell(teachers.Count + j + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //        }
            //    }

            //    for (int j = 0; j < arrears.Count; j++)
            //    {
            //        Word.Paragraph paragraphLines = document.Paragraphs.Add();
            //        Word.Range rangeLines = paragraphLines.Range;
            //        if (j == 0)
            //        {
            //            rangeLines.Text = "Число, подпись обучающихся:     ___________________________";
            //        }
            //        else
            //        {
            //            rangeLines.Text = "___________________________";
            //        }
            //        paragraphLines.Space1();
            //        paragraphLines.SpaceAfter = 0;
            //        paragraphLines.SpaceBefore = 16;
            //        paragraphLines.FirstLineIndent = 0;
            //        paragraphLines.RightIndent = app.CentimetersToPoints(3);
            //        paragraphLines.LeftIndent = 0;
            //        rangeLines.Font.Name = "Times New Roman";
            //        rangeLines.Font.Size = 14;
            //        rangeLines.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            //        rangeLines.InsertParagraphAfter();
            //        paragraphLines.SpaceBefore = 0;
            //    }

            //    if (i != groups.Count - 1)
            //    {
            //        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            //    }
            //}
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
        }
    }
}