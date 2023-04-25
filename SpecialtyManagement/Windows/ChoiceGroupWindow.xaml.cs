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
        private Groups _group;
        private List<Students> _students;

        public string Text { get; set; }

        public ChoiceGroupWindow(Groups group)
        {
            UploadWindow();

            _group = group;
        }

        public ChoiceGroupWindow(List<Students> students)
        {
            UploadWindow();

            _students = students;
        }

        /// <summary>
        /// Настраивает элементы управления окна.
        /// </summary>
        private void UploadWindow()
        {
            InitializeComponent();
            DataContext = this;

            CBGroups.ItemsSource = Database.Entities.Groups.ToList();
            CBGroups.SelectedValuePath = "Id";
            CBGroups.DisplayMemberPath = "Group";
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (CBGroups.SelectedIndex != -1)
            {
                Groups tempGroup = CBGroups.SelectedItem as Groups;

                if (_students != null)
                {
                    foreach (Students item in _students)
                    {
                        item.IdGroup = tempGroup.Id;
                    }
                }
                else if (_group != null)
                {
                    _group.Id = tempGroup.Id;
                    _group.Group = tempGroup.Group;
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите группу", "Выбор группы", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
