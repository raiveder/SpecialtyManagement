using System;
using System.Collections.Generic;
using System.Linq;
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

            List<ArrearsLessons> arrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == id).ToList();

            if (CBType.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.IdArrear == id && x.IdType == (int)CBType.SelectedValue).ToList();
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
            Word.Application app = new Word.Application();
            Word.Document document = new Word.Document();
            document.PageSetup.LeftMargin = app.CentimetersToPoints(1.25F);
            document.PageSetup.RightMargin = app.CentimetersToPoints(0.75F);
            document.PageSetup.TopMargin = app.CentimetersToPoints(0.5F);
            document.PageSetup.BottomMargin = app.CentimetersToPoints(0.25F);

            List<Arrears> arrears = new List<Arrears>();
            foreach (Arrears item in DGArrears.Items)
            {
                arrears.Add(item);
            }

            List<Students> students = Database.Entities.Students.ToList();
            students.RemoveRange(0, 10);

            for (int i = 0; i < 5; i++)
            {
                Word.Paragraph paragraphHeader = document.Paragraphs.Add();
                Word.Range rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "Протокол";
                rangeHeader.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                rangeHeader.Font.Size = 24;
                rangeHeader.Bold = 1;
                rangeHeader.Paragraphs.Space1();
                if (i == 0)
                {
                    paragraphHeader.TabHangingIndent(1);
                    paragraphHeader.TabIndent(-1);
                }
                else
                {
                    rangeHeader.ParagraphFormat.RightIndent = 0;
                }
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "ознакомления с графиком ликвидации задолженностей по итогам";
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = $"промежуточной аттестации за {i} семестр 2021-2022 учебного года в группе";
                rangeHeader.InsertParagraphAfter();

                paragraphHeader = document.Paragraphs.Add();
                rangeHeader = paragraphHeader.Range;
                rangeHeader.Text = "21П,";
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
                rangeHeader.InsertParagraphAfter();
                paragraphHeader.SpaceAfter = 0;

                Word.Paragraph paragraphStudents = document.Paragraphs.Add();
                Word.Range rangeStudents = paragraphStudents.Range;
                Word.Table tableStudents = document.Tables.Add(rangeStudents, students.Count + 1, 3);
                tableStudents.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableStudents.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableStudents.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                tableStudents.Cell(1, 1).Range.Text = "Фамилия";
                tableStudents.Cell(1, 2).Range.Text = "Имя";
                tableStudents.Cell(1, 3).Range.Text = "Дата рождения";

                tableStudents.Rows[1].Range.Bold = 1;
                tableStudents.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int j = 0; j < students.Count; j++)
                {
                    tableStudents.Cell(j + 2, 1).Range.Text = students[j].Surname;
                    tableStudents.Cell(j + 2, 2).Range.Text = students[j].Name;
                    tableStudents.Cell(j + 2, 3).Range.Text = students[j].Birthday.ToString("d");
                }

                Word.Paragraph paragraphShedule = document.Paragraphs.Add();
                Word.Range rangeShedule = paragraphShedule.Range;
                rangeShedule.Text = "График работы преподавателей";
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
                rangeShedule.InsertParagraphAfter();
                paragraphShedule.SpaceAfter = 0;

                Word.Paragraph paragraphTeachers = document.Paragraphs.Add();
                Word.Range rangeTeachers = paragraphTeachers.Range;
                Word.Table tableTeachers = document.Tables.Add(rangeTeachers, students.Count + 1, 3);
                tableTeachers.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                tableTeachers.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                tableTeachers.Cell(1, 1).Range.Text = "Имя";
                tableTeachers.Cell(1, 2).Range.Text = "Фамилия";
                tableTeachers.Cell(1, 3).Range.Text = "Дата рождения";

                tableTeachers.Rows[1].Range.Bold = 1;
                tableTeachers.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int j = 0; j < students.Count; j++)
                {

                    tableTeachers.Cell(j + 2, 1).Range.Text = students[j].Name;
                    tableTeachers.Cell(j + 2, 2).Range.Text = students[j].Surname;
                    tableTeachers.Cell(j + 2, 3).Range.Text = students[j].Birthday.ToString("d");
                }

                for (int j = 0; j < 5; j++)
                {
                    Word.Paragraph paragraphLines = document.Paragraphs.Add();
                    Word.Range rangeLines = paragraphLines.Range;
                    if (j == 0)
                    {
                        rangeLines.Text = "Число, подпись обучающихся:     ___________________________";
                        paragraphLines.SpaceBefore = 16;
                    }
                    else
                    {
                        rangeLines.Text = "___________________________";
                        paragraphLines.SpaceBefore = 16;
                    }
                    rangeLines.ParagraphFormat.RightIndent = app.CentimetersToPoints(4);
                    rangeLines.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    rangeLines.InsertParagraphAfter();
                    paragraphLines.SpaceBefore = 0;
                }

                if (i != 4)
                {
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }
            }

            app.Visible = true;
        }

        private void MIComissionArrears_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}