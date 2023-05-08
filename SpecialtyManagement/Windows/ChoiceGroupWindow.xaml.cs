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

        public string Text { get; set; }

        public ChoiceGroupWindow(Groups group, string text)
        {
            UploadWindow();

            Text = text;
            _group = group;
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
                _group.Id = tempGroup.Id;
                _group.Group = tempGroup.Group;

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