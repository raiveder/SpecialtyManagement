using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для LessonAddPage.xaml
    /// </summary>
    public partial class LessonAddPage : Page
    {
        private Filter _filter;
        private Lessons _lesson;
        private List<Teachers> _teachers = new List<Teachers>(); // Список преподавателей, которые ведут выбранные дисциплины.
        private List<Groups> _groups = new List<Groups>(); // Список групп, у которых дисциплину ведёт выбранный преподаватель.
        private int _indexGroup = 0; // Индекс для отображения группы из списка _groups.

        public LessonAddPage(Filter filter)
        {
            UploadPage(filter);
        }

        public LessonAddPage(Filter filter, Lessons lesson)
        {
            UploadPage(filter);

            TBHeader.Text = "Изменение дисциплины";
            BtnAdd.Content = "Сохранить";

            _lesson = lesson;

            CBType.SelectedValue = _lesson.IdType;
            TBoxCode.Text = _lesson.Code;
            TBoxName.Text = _lesson.Name;

            foreach (DistributionLessons item in Database.Entities.DistributionLessons.Where(x => x.IdLesson == _lesson.Id))
            {
                _teachers.Add(item.Teachers);
                _groups.Add(item.Groups);
            }

            LVTeachers.ItemsSource = _teachers;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage(Filter filter)
        {
            InitializeComponent();
            _filter = filter;

            LBTeachers.ItemsSource = Database.Entities.Teachers.ToList();
            LBTeachers.SelectedValuePath = "Id";
            LBTeachers.DisplayMemberPath = "FullName";

            CBType.ItemsSource = Database.Entities.TypesLessons.ToList();
            CBType.SelectedValuePath = "Id";
            CBType.DisplayMemberPath = "Type";
        }

        private void LBTeachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBTeachers.SelectedIndex != -1)
            {
                Teachers teacher = LBTeachers.SelectedItem as Teachers;

                Groups group = new Groups();
                ChoiceGroupWindow window = new ChoiceGroupWindow(group, teacher.FullName);
                window.ShowDialog();

                if ((bool)window.DialogResult)
                {
                    bool checkContains = false;
                    for (int i = 0; i < _teachers.Count; i++)
                    {
                        if (_teachers[i].FullName == teacher.FullName && _groups[i].Id == group.Id)
                        {
                            checkContains = true;
                            break;
                        }
                    }

                    if (!checkContains)
                    {
                        _teachers.Add(teacher);
                        _groups.Add(group);

                        UpdateListView();
                    }
                    else
                    {
                        MessageBox.Show("Дисциплину уже ведёт выбранный преподаватель у группы " + group.Group, "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                LBTeachers.SelectedIndex = -1;
            }
        }

        private void TBGroup_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBlock).Text = "(" + _groups[_indexGroup++].Group + ")";
        }

        private void TBDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            int idTeacher = Convert.ToInt32(tb.Uid);
            StackPanel spParent = tb.Parent as StackPanel;
            TextBlock tbGroup = spParent.Children[1] as TextBlock;
            string groupString = tbGroup.Text.Substring(1, tbGroup.Text.Length - 2);
            Teachers teacher = Database.Entities.Teachers.FirstOrDefault(x => x.Id == idTeacher);

            for (int i = 0; i < _teachers.Count; i++)
            {
                if (_teachers[i] == teacher && _groups[i].Group == groupString)
                {
                    _teachers.RemoveAt(i);
                    _groups.RemoveAt(i);
                    break;
                }
            }

            UpdateListView();
        }

        /// <summary>
        /// Обновляет визуальное отображение ListView с дисциплинами.
        /// </summary>
        private void UpdateListView()
        {
            List<Teachers> tempTeachers = new List<Teachers>();
            tempTeachers.AddRange(_teachers);

            _indexGroup = 0;
            LVTeachers.ItemsSource = tempTeachers;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;

                if (_lesson == null)
                {
                    _lesson = new Lessons()
                    {
                        IdType = (int)CBType.SelectedValue,
                        Code = TBoxCode.Text,
                        Name = TBoxName.Text
                    };

                    Database.Entities.Lessons.Add(_lesson);

                    isUpdate = false;
                }
                else
                {
                    _lesson.IdType = (int)CBType.SelectedValue;
                    _lesson.Code = TBoxCode.Text;
                    _lesson.Name = TBoxName.Text;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();

                    SaveLessonTeachers();

                    if (isUpdate)
                    {
                        MessageBox.Show("Данные успешно обновлены", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Information);
                        Navigation.Frame.Navigate(new LessonsShowPage(_filter));
                    }
                    else
                    {
                        MessageBox.Show("Дисциплина успешно добавлена", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _lesson = null;
                }
                catch (Exception)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении дисциплины произошла ошибка", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей.
        /// </summary>
        /// <returns>True - если все данные заполнены корректно, в противном случае - false.</returns>
        private bool CheckFillData()
        {
            if (CBType.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите тип дисциплины", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxCode.Text.Length == 0)
            {
                MessageBox.Show("Введите код дисциплины корректно", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите наименование дисциплины", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_lesson == null && Database.Entities.Lessons.FirstOrDefault(x => x.IdType == (int)CBType.SelectedValue &&
            x.Code == TBoxCode.Text && x.Name == TBoxName.Text) != null)
            {
                MessageBox.Show("Данная дисциплина уже есть в базе данных", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Сохраняет данные о преподавателях, которые ведут дисциплины.
        /// </summary>
        /// <returns>True - если сохранение прошло успешно, в противном случае - false.</returns>
        private void SaveLessonTeachers()
        {
            Database.Entities.DistributionLessons.RemoveRange(Database.Entities.DistributionLessons.Where(x => x.IdLesson == _lesson.Id));

            for (int i = 0; i < _teachers.Count; i++)
            {
                Database.Entities.DistributionLessons.Add(new DistributionLessons()
                {
                    IdTeacher = _teachers[i].Id,
                    IdLesson = _lesson.Id,
                    IdGroup = _groups[i].Id
                });
            }

            Database.Entities.SaveChanges();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new LessonsShowPage(_filter));
        }
    }
}