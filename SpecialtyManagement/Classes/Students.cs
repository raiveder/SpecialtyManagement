using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace SpecialtyManagement
{
    public partial class Students
    {
        public int? SequenceNumber { get; set; }

        public string FullName
        {
            get
            {
                if (Patronymic == null)
                {
                    return SurnameAndName;
                }
                return Surname + " " + Name + " " + Patronymic;
            }
            set { }
        }

        public string SurnameAndName
        {
            get
            {
                return Surname + " " + Name;
            }
            set { }
        }

        /// <summary>
        /// Считывает данные о студентах из файла ".csv".
        /// </summary>
        /// <param name="path">путь к файлу.</param>
        /// <returns>Список студентов.</returns>
        public static List<Students> GetStudentsFromFile(string path)
        {
            List<Students> students = new List<Students>();

            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path, Encoding.Default))
                {
                    reader.ReadLine(); // Пропуск строки заголовков.

                    while (!reader.EndOfStream)
                    {
                        Students student = ParseStringToStudent(reader.ReadLine());

                        if (student == null)
                        {
                            MessageBox.Show
                            (
                                "При чтении файла произошла ошибка. Проверьте корректность заполнения файла",
                                "Студенты",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                            return new List<Students>();
                        }

                        students.Add(student);
                    }
                }
            }
            else
            {
                MessageBox.Show("Файл не найден", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return students;
        }

        /// <summary>
        /// Преобразует строку с разделителями в объект класса Student.
        /// </summary>
        /// <param name="text">исходная строка.</param>
        /// <returns>Объект типа Students из данной строки.</returns>
        private static Students ParseStringToStudent(string text)
        {
            text = text.Trim();
            string[] array = text.Split(';');
            string[] fullName = array[1].Split(' ');

            try
            {
                if (fullName.Length > 1) // Если ФИО записано в одном поле.
                {
                    return new Students()
                    {
                        Surname = fullName[0],
                        Name = fullName[1],
                        Patronymic = fullName[2],
                        Birthday = Convert.ToDateTime(array[2]),
                        Note = array[3] == string.Empty ? null : array[3]
                    };
                }
                else
                {
                    return new Students()
                    {
                        Surname = array[1],
                        Name = array[2],
                        Patronymic = array[3],
                        Birthday = Convert.ToDateTime(array[4]),
                        Note = array[5] == string.Empty ? null : array[5]
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}