using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Word = Microsoft.Office.Interop.Word;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearsShowPage.xaml
    /// </summary>
    public partial class ArrearsShowPage : Page
    {
        public ArrearsShowPage()
        {
            UploadPage();

            RBCurrentSemester.IsChecked = true;
            CBGroup.SelectedIndex = 0;
            CBType.SelectedIndex = 0;
            CBSort.SelectedIndex = 0;
        }

        public ArrearsShowPage(Filter filter)
        {
            UploadPage();

            if (filter.IsCurrentSemester)
            {
                RBLastSemester.IsChecked = false;
                RBCurrentSemester.IsChecked = true;
            }
            else
            {
                RBLastSemester.IsChecked = true;
                RBCurrentSemester.IsChecked = false;
            }

            TBoxFind.Text = filter.FindText;
            CBGroup.SelectedIndex = filter.IndexGroup;
            CBType.SelectedIndex = filter.IndexType;
            CBSort.SelectedIndex = filter.IndexSort;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        private void UploadPage()
        {
            InitializeComponent();

            List<Groups> groups = new List<Groups>()
            {
                new Groups()
                {
                    Id = 0,
                    Group = "Все группы"
                }
            };

            groups.AddRange(Database.Entities.Groups.ToList());

            CBGroup.ItemsSource = groups;
            CBGroup.SelectedValuePath = "Id";
            CBGroup.DisplayMemberPath = "Group";

            List<TypesArrears> typesArrears = new List<TypesArrears>()
            {
                new TypesArrears()
                {
                    Id = 0,
                    Type = "Все типы"
                }
            };

            typesArrears.AddRange(Database.Entities.TypesArrears.ToList());

            CBType.ItemsSource = typesArrears;
            CBType.SelectedValuePath = "Id";
            CBType.DisplayMemberPath = "Type";
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter();
        }

        private void RBLastSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBCurrentSemester.IsChecked = false;
            SetFilter();
        }

        private void RBCurrentSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBLastSemester.IsChecked = false;
            SetFilter();
        }

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
        private void SetFilter()
        {
            Arrears.GetYearAndSemester(out int year, out int semesterNumber, (bool)RBCurrentSemester.IsChecked);

            List<Arrears> arrears = Database.Entities.Arrears.Where(x => x.StartYear == year && x.SemesterNumber == semesterNumber).ToList();

            if (CBType.SelectedIndex > 0)
            {
                DeleteArrearsNotMatchByType(arrears, (int)CBType.SelectedValue);
            }
            else
            {
                foreach (Arrears item in arrears)
                {
                    item.CountArrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == item.Id).Count();
                }
            }

            if (CBGroup.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.Students.IdGroup == Convert.ToInt32(CBGroup.SelectedValue)).ToList();
            }

            if (TBoxFind.Text.Length > 0)
            {
                arrears = arrears.Where(x => x.Students.FullName.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            switch (CBSort.SelectedIndex)
            {
                case 1:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    break;
                case 2:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    break;
                case 3:
                    arrears.Sort((x, y) => x.CountArrears.CompareTo(y.CountArrears));
                    break;
                case 4:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    arrears.Reverse();
                    break;
                case 5:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    arrears.Reverse();
                    break;
                case 6:
                    arrears.Sort((x, y) => x.CountArrears.CompareTo(y.CountArrears));
                    arrears.Reverse();
                    break;
                default:
                    break;
            }

            int number = 1;
            foreach (Arrears item in arrears)
            {
                item.SequenceNumber = number++;
            }

            DGArrears.ItemsSource = arrears;

            if (DGArrears.Items.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру задолженностей не найдено", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Удаляет задолженности, которые не соответствуют выбранному типу, из списка.
        /// </summary>
        /// <param name="arrears">список задолженностей.</param>
        /// <param name="idType">индекс типа задолженности.</param>
        private void DeleteArrearsNotMatchByType(List<Arrears> arrears, int idType)
        {
            List<Arrears> arrearsToRemove = new List<Arrears>();

            foreach (Arrears item in arrears)
            {
                int countLessons = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == item.Id && x.IdType == idType).Count();

                if (countLessons == 0)
                {
                    arrearsToRemove.Add(item);
                }
                else
                {
                    item.CountArrears = countLessons;
                }
            }

            foreach (Arrears item in arrearsToRemove)
            {
                arrears.Remove(item);
            }
        }

        private void WPLessons_Loaded(object sender, RoutedEventArgs e)
        {
            WrapPanel panel = sender as WrapPanel;
            int id = Convert.ToInt32(panel.Uid);
            panel.Children.Clear();

            List<ArrearsLessons> arrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == id).ToList();

            if (CBType.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.IdType == (int)CBType.SelectedValue).ToList();
            }

            foreach (ArrearsLessons item in arrears)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = item.Lessons.ShortName
                };

                if (!item.IsLiquidated)
                {
                    if (item.IdType == 2)
                    {
                        tb.Foreground = Brushes.Red; // Комиссионная задолженность.
                    }

                    switch (item.IdReason)
                    {
                        case 1:
                            tb.Foreground = Brushes.Green; // Задолженность, не сданная по уважительной причине.
                            break;
                        case 2:
                            tb.Foreground = Brushes.PaleVioletRed; // Задолженность, не сданная по причине академического отпуска.
                            break;
                        case 3:
                            tb.Foreground = Brushes.Brown; // Задолженность, не сданная по причине отчисления.
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    tb.Foreground = Brushes.Blue; // Ликвидированная задолженность.
                }

                tb.Text += ", ";

                panel.Children.Add(tb);
            }

            if (panel.Children.Count > 0) // Удаление последней запятой.
            {
                TextBlock lastBlock = panel.Children[panel.Children.Count - 1] as TextBlock;
                lastBlock.Text = lastBlock.Text.Substring(0, lastBlock.Text.Length - 2);
            }
        }

        private void DGArrears_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DGArrears.SelectedItems.Count == 0)
            {
                return;
            }

            MIChange.Visibility = Visibility.Visible;
            MIDelete.Visibility = Visibility.Visible;

            if (DGArrears.SelectedItems.Count > 1)
            {
                MIChange.Visibility = Visibility.Collapsed;
            }

            CMArrears.IsOpen = true;
        }

        private void DGArrears_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            Filter filter = new Filter()
            {
                FindText = TBoxFind.Text,
                IndexType = CBType.SelectedIndex,
                IndexGroup = CBGroup.SelectedIndex,
                IsCurrentSemester = (bool)RBCurrentSemester.IsChecked,
                IndexSort = CBSort.SelectedIndex
            };

            Navigation.Frame.Navigate(new ArrearAddPage(filter, DGArrears.SelectedItem as Arrears));
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (Arrears item in DGArrears.SelectedItems)
            {
                Database.Entities.Arrears.Remove(item);
            }

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "При удалении " + (DGArrears.SelectedItems.Count == 1 ? "задолженности" : "задолженностей") + " возникла ошибка",
                    "Задолженности",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void CMArrears_Closed(object sender, RoutedEventArgs e)
        {
            DGArrears.SelectedItems.Clear();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Filter filter = new Filter()
            {
                FindText = TBoxFind.Text,
                IndexType = CBType.SelectedIndex,
                IndexGroup = CBGroup.SelectedIndex,
                IsCurrentSemester = (bool)RBCurrentSemester.IsChecked,
                IndexSort = CBSort.SelectedIndex
            };

            Navigation.Frame.Navigate(new ArrearAddPage(filter));
        }

        private void MIPrimaryArrears_Click(object sender, RoutedEventArgs e)
        {
            List<Groups> groups = GetGroupsWithArrears(1);

            Word.Application app = new Word.Application();
            Word.Document document = new Word.Document();
            document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            for (int i = 0; i < groups.Count; i++)
            {
                List<Arrears> arrears = new List<Arrears>();

                foreach (Arrears item in DGArrears.Items)
                {
                    if (item.Students.IdGroup == groups[i].Id)
                    {
                        arrears.Add(item);
                    }
                }

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
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"промежуточной аттестации за {arrears[0].SemesterSequenceNumberRoman} семестр {arrears[0].StartYear}-{arrears[0].StartYear + 1} учебного года в группе";
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"{groups[i].Group},";
                rangeHeader.Font.Size = 18;
                rangeHeader.Bold = 1;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"специальность {Database.Entities.Specialty.FirstOrDefault().FullName}";
                paragraphHeader.SpaceAfter = 16;
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "Список обучающихся, имеющих задолженности, и перечень учебных дисциплин";
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

                int number = 1;
                for (int j = 0; j < arrears.Count; j++)
                {
                    arrears[j].SequenceNumber = number++;

                    List<Lessons> lessons = GetLessonsForArrears(arrears[j], 1);
                    string lessonsString = string.Empty;
                    foreach (Lessons item in lessons)
                    {
                        lessonsString += item.FullName + ", ";
                    }

                    tableStudents.Cell(j + 2, 1).Range.Text = arrears[j].SequenceNumber.ToString();
                    tableStudents.Cell(j + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableStudents.Cell(j + 2, 2).Range.Text = arrears[j].Students.FullName;
                    tableStudents.Cell(j + 2, 3).Range.Text = arrears[j].CountArrears.ToString();
                    tableStudents.Cell(j + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    tableStudents.Cell(j + 2, 4).Range.Text = lessonsString;
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

                Word.Paragraph paragraphTeachers = document.Paragraphs.Add();
                Word.Range rangeTeachers = paragraphTeachers.Range;
                Word.Table tableTeachers = document.Tables.Add(rangeTeachers, arrears.Count + 1, 3);
                tableTeachers.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                tableTeachers.Cell(1, 1).Range.Text = "Имя";
                tableTeachers.Cell(1, 2).Range.Text = "Фамилия";
                tableTeachers.Cell(1, 3).Range.Text = "Дата рождения";

                tableTeachers.Rows[1].Range.Bold = 1;
                tableTeachers.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int j = 0; j < arrears.Count; j++)
                {
                    tableTeachers.Cell(j + 2, 1).Range.Text = arrears[j].Students.Name;
                    tableTeachers.Cell(j + 2, 2).Range.Text = arrears[j].Students.Surname;
                    tableTeachers.Cell(j + 2, 3).Range.Text = arrears[j].Students.Birthday.ToString("d");
                }

                for (int j = 0; j < 5; j++)
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

            app.Visible = true;
            //document.SaveAs2(@"C:\Users\User\Desktop\test.docx");
            //Marshal.ReleaseComObject(document);
            //Marshal.ReleaseComObject(app);
        }

        private void MIComissionArrears_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Возвращает список групп, у студентов которых есть задолженности.
        /// </summary>
        /// <param name="typeArrears">тип задолженности.</param>
        /// <returns>Список групп, у студентов которых есть задолженности определённого типа.</returns>
        private List<Groups> GetGroupsWithArrears(int typeArrears)
        {
            List<Groups> groups = new List<Groups>();

            List<Arrears> arrears = new List<Arrears>();
            foreach (Arrears item in DGArrears.Items)
            {
                arrears.Add(item);
            }

            DeleteArrearsNotMatchByType(arrears, typeArrears);

            foreach (Arrears arrear in arrears)
            {
                if (!groups.Contains(arrear.Students.Groups))
                {
                    groups.Add(arrear.Students.Groups);
                    //foreach (ArrearsLessons arrearlesson in Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id))
                    //{
                    //    if (arrearlesson.IdType == typeArrears)
                    //    {
                    //        groups.Add(arrear.Students.Groups);
                    //        break;
                    //    }
                    //}
                }
            }

            return groups;
        }

        /// <summary>
        /// Возвращает список дисциплин, по которым у студента есть задолженность.
        /// </summary>
        /// <param name="arrear">задолженность.</param>
        /// <param name="idType">тип задолженности.</param>
        /// <returns>Список дисциплин, по которым у студента есть задолженность определённого типа.</returns>
        private List<Lessons> GetLessonsForArrears(Arrears arrear, int? idType)
        {
            List<ArrearsLessons> arrearLessons = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id).ToList();

            if (idType != null)
            {
                arrearLessons = arrearLessons.Where(x => x.IdType == idType).ToList();
            }

            List<Lessons> lessons = new List<Lessons>();
            foreach (ArrearsLessons item in arrearLessons)
            {
                lessons.Add(item.Lessons);
            }

            return lessons;
        }
    }
}