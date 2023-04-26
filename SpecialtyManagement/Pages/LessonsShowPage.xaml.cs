﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для LessonsShowPage.xaml
    /// </summary>
    public partial class LessonsShowPage : Page
    {
        public LessonsShowPage()
        {
            UploadPage();

            CBType.SelectedIndex = 0;
        }

        public LessonsShowPage(Filter filter)
        {
            UploadPage();

            TBoxFind.Text = filter.FindText;
            CBType.SelectedIndex = filter.IndexType;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage()
        {
            InitializeComponent();

            List<TypesLessons> types = new List<TypesLessons>()
            {
                new TypesLessons()
                {
                    Id = 0,
                    Type = "Все типы"
                }
            };

            types.AddRange(Database.Entities.TypesLessons.ToList());

            CBType.ItemsSource = types;
            CBType.SelectedValuePath = "Id";
            CBType.DisplayMemberPath = "Type";
        }

        private void CBType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter();
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        private void SetFilter()
        {
            List<Lessons> lessons = Database.Entities.Lessons.ToList();

            if (TBoxFind.Text.Length > 0)
            {
                lessons = lessons.Where(x => x.FullName.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            if (CBType.SelectedIndex > 0)
            {
                lessons = lessons.Where(x => x.IdType == (int)CBType.SelectedValue).ToList();
            }

            int number = 1;

            foreach (Lessons item in lessons)
            {
                item.SequenceNumber = number++;
            }

            DGLessons.ItemsSource = lessons;

            if (lessons.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру дисциплин не найдено", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Filter filter = new Filter()
            {
                FindText = TBoxFind.Text,
                IndexType = CBType.SelectedIndex
            };

            Navigation.Frame.Navigate(new LessonAddPage(filter));
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            if (DGLessons.SelectedItems.Count == 1)
            {
                Filter filter = new Filter()
                {
                    FindText = TBoxFind.Text,
                    IndexType = CBType.SelectedIndex
                };

                Navigation.Frame.Navigate(new LessonAddPage(filter, DGLessons.SelectedItem as Lessons));
            }
            else
            {
                MessageBox.Show("Выберите одну дисциплину", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (Lessons item in DGLessons.SelectedItems)
            {
                Database.Entities.Lessons.Remove(item);
            }

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "При удалении " + (DGLessons.SelectedItems.Count == 1 ? "дисциплины" : "дисциплин") + " возникла ошибка",
                    "Дисциплины",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DGLessons_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMLessons.IsOpen = true;
        }

        private void DGLessons_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMLessons_Closed(object sender, RoutedEventArgs e)
        {
            DGLessons.SelectedItems.Clear();
        }
    }
}
