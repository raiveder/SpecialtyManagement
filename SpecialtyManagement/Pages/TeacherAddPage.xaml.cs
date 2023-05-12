using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeacherAddPage.xaml
    /// </summary>
    public partial class TeacherAddPage : Page
    {
        private Filter _filter;
        private Teachers _teacher;
        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин, которые ведёт преподаватель.
        private List<Groups> _groups = new List<Groups>(); // Список групп, у которых преподаватель ведёт выбранную дисциплину.
        private int _indexGroup = 0; // Индекс для отображения группы из списка _groups.

        public TeacherAddPage(Filter filter)
        {
            UploadPage(filter);
        }

        public TeacherAddPage(Filter filter, Teachers teacher)
        {
            UploadPage(filter);

            TBHeader.Text = "Изменение преподавателя";
            BtnAdd.Content = "Сохранить";

            _teacher = teacher;

            TBoxSurname.Text = _teacher.Surname;
            TBoxName.Text = _teacher.Name;
            TBoxPatronymic.Text = _teacher.Patronymic;

            foreach (DistributionLessons item in Database.Entities.DistributionLessons.Where(x => x.IdTeacher == _teacher.Id))
            {
                _lessons.Add(item.Lessons);
                _groups.Add(item.Groups);
            }

            LVLessons.ItemsSource = _lessons;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage(Filter filter)
        {
            InitializeComponent();
            _filter = filter;

            LBLessons.ItemsSource = Database.Entities.Lessons.ToList();
            LBLessons.SelectedValuePath = "Id";
            LBLessons.DisplayMemberPath = "FullName";
        }

        private void LBLessons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBLessons.SelectedIndex != -1)
            {
                Lessons lesson = LBLessons.SelectedItem as Lessons;

                Groups group = new Groups();
                ChoiceGroupWindow window = new ChoiceGroupWindow(group, lesson.FullName);
                window.ShowDialog();

                if ((bool)window.DialogResult)
                {
                    bool checkContains = false;
                    for (int i = 0; i < _lessons.Count; i++)
                    {
                        if (_lessons[i].FullName == lesson.FullName && _groups[i].Id == group.Id)
                        {
                            checkContains = true;
                            break;
                        }
                    }

                    if (!checkContains)
                    {
                        _lessons.Add(lesson);
                        _groups.Add(group);

                        UpdateListView();
                    }
                    else
                    {
                        MessageBox.Show("Преподаватель уже ведёт выбранную дисциплину у группы " + group.Group, "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                LBLessons.SelectedIndex = -1;
            }
        }

        private void TBGroup_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBlock).Text = "(" + _groups[_indexGroup++].Group + ")";
        }

        private void TBDeleteLesson_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            int idLesson = Convert.ToInt32(tb.Uid);
            StackPanel spParent = tb.Parent as StackPanel;
            TextBlock tbGroup = spParent.Children[1] as TextBlock;
            string groupString = tbGroup.Text.Substring(1, tbGroup.Text.Length - 2);
            Lessons lesson = Database.Entities.Lessons.FirstOrDefault(x => x.Id == idLesson);

            for (int i = 0; i < _lessons.Count; i++)
            {
                if (_lessons[i] == lesson && _groups[i].Group == groupString)
                {
                    _lessons.RemoveAt(i);
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
            List<Lessons> tempLessons = new List<Lessons>();
            int indexNumber = 0;

            foreach (Lessons item in _lessons)
            {
                item.SequenceNumber = indexNumber++;
                tempLessons.Add(item);
            }

            _indexGroup = 0;
            LVLessons.ItemsSource = tempLessons;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;

                if (_teacher == null)
                {
                    _teacher = new Teachers()
                    {
                        Surname = TBoxSurname.Text,
                        Name = TBoxName.Text,
                        Patronymic = TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text
                    };

                    Database.Entities.Teachers.Add(_teacher);

                    isUpdate = false;
                }
                else
                {
                    _teacher.Surname = TBoxSurname.Text;
                    _teacher.Name = TBoxName.Text;
                    _teacher.Patronymic = TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();

                    SaveTeacherLessons();

                    if (isUpdate)
                    {
                        MessageBox.Show("Данные успешно обновлены", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Information);
                        Navigation.Frame.Navigate(new TeahersShowPage(_filter));
                    }
                    else
                    {
                        MessageBox.Show("Преподаватель успешно добавлен", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _teacher = null;
                }
                catch (Exception)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении преподавателя произошла ошибка", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            Regex regexText = new Regex(@"^[А-Я][а-я]+");

            if (TBoxSurname.Text.Length == 0)
            {
                MessageBox.Show("Введите фамилию преподавателя", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxSurname.Text))
            {
                MessageBox.Show("Введите фамилию преподавателя корректно", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите имя преподавателя", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxName.Text))
            {
                MessageBox.Show("Введите имя преподавателя корректно", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxPatronymic.Text) && TBoxPatronymic.Text.Length > 0)
            {
                MessageBox.Show("Введите отчество преподавателя корректно", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_teacher == null && Database.Entities.Teachers.FirstOrDefault(x => x.Surname == TBoxSurname.Text &&
            x.Name == TBoxName.Text && x.Patronymic == (TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text)) != null)
            {
                MessageBox.Show("Данный преподаватель уже есть в базе данных", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Сохраняет данные о дисциплинах, которые ведёт преподаватель.
        /// </summary>
        /// <returns>True - если сохранение прошло успешно, в противном случае - false.</returns>
        private void SaveTeacherLessons()
        {
            Database.Entities.DistributionLessons.RemoveRange(Database.Entities.DistributionLessons.Where(x => x.IdTeacher == _teacher.Id));

            for (int i = 0; i < _lessons.Count; i++)
            {
                Database.Entities.DistributionLessons.Add(new DistributionLessons()
                {
                    IdTeacher = _teacher.Id,
                    IdLesson = _lessons[i].Id,
                    IdGroup = _groups[i].Id
                });
            }

            Database.Entities.SaveChanges();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new TeahersShowPage(_filter));
        }
    }
}