﻿using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            CBTypes.SelectedValue = _lesson.IdType;
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

            List<Teachers> teachers = Database.Entities.Teachers.ToList();
            teachers.Sort((x, y) => x.FullName.CompareTo(y.FullName));
            LBTeachers.ItemsSource = teachers;
            LBTeachers.SelectedValuePath = "Id";
            LBTeachers.DisplayMemberPath = "FullName";

            List<TypesLessons> types = Database.Entities.TypesLessons.ToList();
            types.Sort((x, y) => x.Type.ToLower().CompareTo(y.Type.ToLower()));
            CBTypes.ItemsSource = types;
            CBTypes.SelectedValuePath = "Id";
            CBTypes.DisplayMemberPath = "Type";
        }

        private void TBoxCode_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if ((CBTypes.SelectedItem as TypesLessons).Type == "ПМ")
            {
                bool isAdded = false;

                foreach (DistributionLessons item in Database.Entities.DistributionLessons
                    .Where(x => x.Lessons.TypesLessons.Type != "ОП" && x.Lessons.Code.Substring(0, 2) == TBoxCode.Text))
                {
                    if (!IsTeacherContains(item.Teachers, item.Groups))
                    {
                        _teachers.Add(item.Teachers);
                        _groups.Add(item.Groups);

                        if (!isAdded)
                        {
                            isAdded = true;
                        }
                    }
                }

                if (isAdded)
                {
                    LVTeachers.ItemsSource = new List<Teachers>();
                    UpdateListView();
                }
            }
        }

        /// <summary>
        /// Проверяет наличие преподавателя в списке добавленных (с учётом группы).
        /// </summary>
        /// <param name="teacher">преподаватель.</param>
        /// <param name="group">группа.</param>
        /// <returns>True, если совпадение найдено, в противном случае - false.</returns>
        private bool IsTeacherContains(Teachers teacher, Groups group)
        {
            for (int i = 0; i < _teachers.Count; i++)
            {
                if (_teachers[i].FullName == teacher.FullName && _groups[i].Id == group.Id)
                {
                    return true;
                }
            }

            return false;
        }

        private void LBTeachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBTeachers.SelectedIndex != -1)
            {
                Teachers teacher = LBTeachers.SelectedItem as Teachers;

                Groups group = new Groups();
                ChoiceElementWindow window = new ChoiceElementWindow(group, teacher.FullName);
                window.ShowDialog();

                if ((bool)window.DialogResult)
                {
                    if (!IsTeacherContains(teacher, group))
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

        private void BtnDeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            Button tb = sender as Button;
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
            SortTeachersByGroups(_teachers, _groups);
            List<Teachers> tempTeachers = new List<Teachers>();
            tempTeachers.AddRange(_teachers);

            _indexGroup = 0;
            LVTeachers.ItemsSource = tempTeachers;
        }

        /// <summary>
        /// Сортирует список преподавателей по группам.
        /// </summary>
        /// <param name="teachers">преподаватели.</param>
        /// <param name="groups">группы.</param>
        private void SortTeachersByGroups(List<Teachers> teachers, List<Groups> groups)
        {
            List<Teachers> tempTeachers = new List<Teachers>();
            List<Groups> tempGroups = new List<Groups>();
            tempTeachers.AddRange(teachers);
            tempGroups.AddRange(groups);
            teachers.Clear();
            groups.Clear();

            Dictionary<int, string> teachersWithKey = new Dictionary<int, string>();
            for (int i = 0; i < tempTeachers.Count; i++)
            {
                teachersWithKey.Add(i, $"{tempGroups[i].Group} {tempTeachers[i].ShortName}");
            }

            teachersWithKey = teachersWithKey.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in teachersWithKey)
            {
                teachers.Add(tempTeachers[item.Key]);
                groups.Add(tempGroups[item.Key]);
            }
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
                        IdType = (int)CBTypes.SelectedValue,
                        Code = TBoxCode.Text,
                        Name = TBoxName.Text
                    };

                    Database.Entities.Lessons.Add(_lesson);

                    isUpdate = false;
                }
                else
                {
                    _lesson.IdType = (int)CBTypes.SelectedValue;
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
                        Navigation.Frame.Navigate(new LessonsShowPage(_filter));
                    }

                    _lesson = null;
                    CBTypes.SelectedIndex = -1;
                    TBoxCode.Text = string.Empty;
                    TBoxName.Text = string.Empty;
                    _groups.Clear();
                    _teachers.Clear();
                    _indexGroup = 0;
                    LVTeachers.ItemsSource = new List<Teachers>();
                }
                catch (Exception ex)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка\nТекст ошибки: " + ex.Message, "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении дисциплины произошла ошибка\nТекст ошибки: " + ex.Message, "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (CBTypes.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите тип дисциплины", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxCode.Text.Length == 0)
            {
                MessageBox.Show("Введите код дисциплины", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (TBoxCode.Text.Length < 2)
            {
                MessageBox.Show("Код дисциплины не может быть короче 2-х символов. Если он состоит из одной цифры, то поставьте в начало \"0\"", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите наименование дисциплины", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_lesson == null && Database.Entities.Lessons.FirstOrDefault(x => x.IdType == (int)CBTypes.SelectedValue && x.Code == TBoxCode.Text) != null)
            {
                MessageBox.Show("Дисциплина с таким кодом уже есть в базе данных, для изменения списка преподавателей отредактируйте её", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_lesson != null && Database.Entities.Lessons.FirstOrDefault(x => x.Id != _lesson.Id && x.IdType == (int)CBTypes.SelectedValue && x.Code == TBoxCode.Text) != null)
            {
                MessageBox.Show("Дисциплина с таким кодом уже есть в базе данных, для изменения списка преподавателей отредактируйте её", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
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