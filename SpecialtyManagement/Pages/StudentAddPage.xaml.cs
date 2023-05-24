using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для StudentAddPage.xaml
    /// </summary>
    public partial class StudentAddPage : Page
    {
        private Students _student;
        private Filter _filter;

        public StudentAddPage(Filter filter)
        {
            UploadPage(filter);
        }

        public StudentAddPage(Filter filter, Students student)
        {
            UploadPage(filter);

            TBHeader.Text = "Изменение студента";
            BtnAdd.Content = "Сохранить";

            _student = student;

            TBoxSurname.Text = _student.Surname;
            TBoxName.Text = _student.Name;
            TBoxPatronymic.Text = _student.Patronymic;
            CBGroups.SelectedValue = _student.IdGroup;
            DPBirthday.SelectedDate = _student.Birthday;
            TBoxNote.Text = _student.Note;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage(Filter filter)
        {
            InitializeComponent();

            _filter = filter;

            DPBirthday.DisplayDateEnd = DateTime.Now.AddYears(-14);

            CBGroups.ItemsSource = Database.Entities.Groups.ToList();
            CBGroups.SelectedValuePath = "Id";
            CBGroups.DisplayMemberPath = "Group";
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;

                if (_student == null)
                {
                    Database.Entities.Students.Add(new Students()
                    {
                        Surname = TBoxSurname.Text,
                        Name = TBoxName.Text,
                        Patronymic = TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text,
                        IdGroup = (int)CBGroups.SelectedValue,
                        Birthday = DPBirthday.SelectedDate.Value,
                        Note = TBoxNote.Text.Length == 0 ? null : TBoxNote.Text,
                        IsExpelled = false,
                        IsAcademic = false
                    });

                    isUpdate = false;
                }
                else
                {
                    _student.Surname = TBoxSurname.Text;
                    _student.Name = TBoxName.Text;
                    _student.Patronymic = TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text;
                    _student.IdGroup = (int)CBGroups.SelectedValue;
                    _student.Birthday = DPBirthday.SelectedDate.Value;
                    _student.Note = TBoxNote.Text.Length == 0 ? null : TBoxNote.Text;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();

                    if (isUpdate)
                    {
                        Navigation.Frame.Navigate(new StudentsShowPage(_filter));
                    }

                    _student = null;
                }
                catch (Exception ex)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка\nТекст ошибки: " + ex.Message, "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении студента произошла ошибка\nТекст ошибки: " + ex.Message, "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Введите фамилию студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxSurname.Text))
            {
                MessageBox.Show("Введите фамилию студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите имя студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxName.Text))
            {
                MessageBox.Show("Введите имя студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxPatronymic.Text) && TBoxPatronymic.Text.Length > 0)
            {
                MessageBox.Show("Введите отчество студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (CBGroups.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите группу студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (DPBirthday.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_student == null && Database.Entities.Students.FirstOrDefault(x => x.Surname == TBoxSurname.Text &&
            x.Name == TBoxName.Text && x.Patronymic == (TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text) &&
            x.IdGroup == (int)CBGroups.SelectedValue && x.Birthday == DPBirthday.SelectedDate.Value &&
            x.Note == (TBoxNote.Text.Length == 0 ? null : TBoxNote.Text)) != null)
            {
                MessageBox.Show("Данный студент уже есть в базе данных", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_student != null && Database.Entities.Students.FirstOrDefault(x => x.Id != _student.Id && x.Surname == TBoxSurname.Text &&
            x.Name == TBoxName.Text && x.Patronymic == (TBoxPatronymic.Text.Length == 0 ? null : TBoxPatronymic.Text) &&
            x.IdGroup == (int)CBGroups.SelectedValue && x.Birthday == DPBirthday.SelectedDate.Value &&
            x.Note == (TBoxNote.Text.Length == 0 ? null : TBoxNote.Text)) != null)
            {
                MessageBox.Show("Другой такой же студент уже есть в базе данных", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new StudentsShowPage(_filter));
        }
    }
}