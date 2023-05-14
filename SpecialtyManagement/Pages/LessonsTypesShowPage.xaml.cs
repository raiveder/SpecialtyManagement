﻿using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для LessonsTypesShowPage.xaml
    /// </summary>
    public partial class LessonsTypesShowPage : Page
    {
        private Filter _filter;

        public LessonsTypesShowPage(Filter filter)
        {
            InitializeComponent();

            _filter = filter;
            SetFilter();
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
        private void SetFilter()
        {
            List<TypesLessons> types = Database.Entities.TypesLessons.ToList();

            if (TBoxFind.Text.Length > 0)
            {
                types = types.Where(x => x.Type.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            int number = 1;
            foreach (TypesLessons item in types)
            {
                item.SequenceNumber = number++;
            }

            DGTypesLessons.ItemsSource = types;

            if (types.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру типов дисциплин не найдено", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            LessonTypeAddWindow window = new LessonTypeAddWindow(DGTypesLessons.SelectedItem as TypesLessons);
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                SetFilter();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LessonTypeAddWindow window = new LessonTypeAddWindow();
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                SetFilter();
            }
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (TypesLessons item in DGTypesLessons.SelectedItems)
            {
                Database.Entities.TypesLessons.Remove(item);
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
                    "При удалении " + (DGTypesLessons.SelectedItems.Count == 1 ? "типов дисциплин" : "типа дисциплин") + " возникла ошибка",
                    "Типы дисциплин",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DGTypesLessons_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMTypesLessons.IsOpen = true;
        }

        private void DGTypesLessons_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMTypesLessons_Closed(object sender, RoutedEventArgs e)
        {
            DGTypesLessons.SelectedItems.Clear();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new LessonsShowPage(_filter));
        }
    }
}