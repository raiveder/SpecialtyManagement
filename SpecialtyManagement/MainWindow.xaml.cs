using SpecialtyManagement.Classes;
using SpecialtyManagement.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpecialtyManagement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Button> _buttonsMenu; // Список кнопок навигации меню.

        public MainWindow()
        {
            InitializeComponent();

            _buttonsMenu = SPMenu.Children.OfType<Button>().ToList();
            _buttonsMenu.Add(BtnSettings);
            SelectButton(_buttonsMenu[0]);

            if (!Database.CreateEntities(out string message))
            {
                if (message.ToLower().Contains("error: 26"))
                {
                    //try
                    //{
                    //    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    //    MessageBox.Show(ConfigurationManager.ConnectionStrings["SpecialtyManagementEntities"].ConnectionString);
                    //    var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
                    //    string connStr = connectionStringsSection.ConnectionStrings["SpecialtyManagementEntities"].ConnectionString;

                    //    MessageBox.Show("old String: " + connStr);
                    //    //int index = 0;
                    //    //for (int i = 0; i < connStr.Length - 11; i++)
                    //    //{
                    //    //    if (connStr[i] == '\\' && connStr[i + 1] == 'S' && connStr[i + 2] == 'Q' && connStr[i + 3] == 'L' && connStr[i + 4] == 'E' && connStr[i + 5] == 'X' &&
                    //    //        connStr[i + 6] == 'P' && connStr[i + 7] == 'R' && connStr[i + 8] == 'E' && connStr[i + 9] == 'S' && connStr[i + 10] == 'S')
                    //    //    {
                    //    //        index = i;
                    //    //        break;
                    //    //    }
                    //    //}
                    //    //MessageBox.Show("new String: " + connStr);
                    //    //connStr = connStr.Remove(index, 11);
                    //    connStr = "metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=.;initial catalog=SpecialtyManagement;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;";
                    //    MessageBox.Show("new String: " + connStr);
                    //    connectionStringsSection.ConnectionStrings["SpecialtyManagementEntities"].ConnectionString = connStr;

                    //    config.Save();
                    //    ConfigurationManager.RefreshSection("connectionStrings");
                    //    MessageBox.Show(ConfigurationManager.ConnectionStrings["SpecialtyManagementEntities"].ConnectionString);
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message);
                    //}
                    MessageBox.Show("Возникла ошибка при запуске приложения. Задайте имя сервера по одному из шаблонов:\n\"имя сервера\\SQLEXPRESS\"\n\"имя сервера\"\nТекст ошибки: " + message, "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("При подключении к БД возникла ошибка. Обратитесь к администратору\nТекст ошибки: " + message, "Подключение к базе данных");
                    Close();
                }
            }
            Navigation.Frame = MainFrame;
            Navigation.Setting = new Setting();
            Navigation.Frame.Navigate(new StudentsShowPage());

            DataContext = Navigation.Setting;
        }

        private void BtnStudents_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new StudentsShowPage());
        }

        private void BtnArrears_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new ArrearsShowPage());
        }

        private void BtnLessons_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new LessonsShowPage());
        }

        private void BtnTeachers_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new TeahersShowPage());
        }

        private void BtnGroups_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new GroupsShowPage());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new SettingsPage());
        }

        /// <summary>
        /// Выделяет кнопку на фоне остальных.
        /// </summary>
        /// <param name="currentButton">выбранная кнопка.</param>
        private void SelectButton(Button currentButton)
        {
            foreach (Button item in _buttonsMenu)
            {
                item.Background = ApplicationColor.ColorSecondary;
                item.Foreground = Brushes.Black;
                item.FontWeight = FontWeights.Regular;
            }

            currentButton.Background = ApplicationColor.ColorAccent;
            currentButton.Foreground = Brushes.White;
            currentButton.FontWeight = FontWeights.DemiBold;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}