using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChoiceGroupWindow.xaml
    /// </summary>
    public partial class ChoiceGroupWindow : Window
    {
        private List<Students> _students;

        public ChoiceGroupWindow(List<Students> students)
        {
            InitializeComponent();

            _students = students;

            CBGroups.ItemsSource = Database.Entities.Groups.ToList();
            CBGroups.SelectedValuePath = "Id";
            CBGroups.DisplayMemberPath = "Group";
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (CBGroups.SelectedIndex != -1)
            {
                int idGroup = Convert.ToInt32(CBGroups.SelectedValue);

                foreach (Students item in _students)
                {
                    item.IdGroup = idGroup;
                }

                Close();
            }
            else
            {
                MessageBox.Show("Выберите группу", "Выбор группы", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}