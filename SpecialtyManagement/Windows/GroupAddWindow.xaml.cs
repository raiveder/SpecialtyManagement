using SpecialtyManagement.Pages;
using System;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupAddWindow.xaml
    /// </summary>
    public partial class GroupAddWindow : Window
    {
        private Groups _group;
        private GroupsShowPage _page;

        public GroupAddWindow(GroupsShowPage page)
        {
            UploadPage(page);
        }

        public GroupAddWindow(Groups group, GroupsShowPage page)
        {
            UploadPage(page);
            _group = group;

            TBHeader.Text = "Изменение группы";
            TBoxGroup.Text = _group.Group;
            BtnAdd.Content = "Сохранить";
        }

        /// <summary>
        /// Настраивает элементы управления окна.
        /// </summary>
        /// <param name="page">экземпляр страницы, из которой было вызвано данное окно.</param>
        private void UploadPage(GroupsShowPage page)
        {
            InitializeComponent();
            _page = page;
            Navigation.SPDimming.Visibility = Visibility.Visible;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;
                if (_group == null)
                {
                    _group = new Groups()
                    {
                        Group = TBoxGroup.Text
                    };
                    Database.Entities.Groups.Add(_group);
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
                    _page.UpdateView();

                    if (isUpdate)
                    {
                        Close();
                    }

                    _group = null;
                    TBoxGroup.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка\nТекст ошибки:" + ex.Message, "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении группы произошла ошибка\nТекст ошибки: " + ex.Message, "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Введите название группы", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_group == null && Database.Entities.Groups.FirstOrDefault(x => x.Group == TBoxGroup.Text) != null)
            {
                MessageBox.Show("Данная группа уже есть в базе данных", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_group != null && Database.Entities.Groups.FirstOrDefault(x => x.Id != _group.Id && x.Group == TBoxGroup.Text) != null)
            {
                MessageBox.Show("Данная группа уже есть в базе данных, для изменения названия отредактируйте её", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Navigation.SPDimming.Visibility = Visibility.Collapsed;
        }
    }
}