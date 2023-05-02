using System;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для GroupAddPage.xaml
    /// </summary>
    public partial class GroupAddPage : Page
    {
        private Groups _group;

        public GroupAddPage()
        {
            InitializeComponent();
        }

        public GroupAddPage(Groups group)
        {
            InitializeComponent();

            TBHeader.Text = "Изменение группы";
            BtnAdd.Content = "Сохранить";

            _group = group;

            TBoxGroup.Text = _group.Group;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new GroupsShowPage());
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;

                if (_group == null)
                {
                    Database.Entities.Groups.Add(new Groups()
                    {
                        Group = TBoxGroup.Text
                    });

                    isUpdate = false;
                }
                else
                {
                    _group.Group = TBoxGroup.Text;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();

                    if (isUpdate)
                    {
                        MessageBox.Show("Данные успешно обновлены", "Группы", MessageBoxButton.OK, MessageBoxImage.Information);
                        Navigation.Frame.Navigate(new GroupsShowPage());
                    }
                    else
                    {
                        MessageBox.Show("Группа успешно добавлена", "Группы", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _group = null;
                }
                catch (Exception)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении группы произошла ошибка", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (TBoxGroup.Text.Length == 0)
            {
                MessageBox.Show("Введите группу студента", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}