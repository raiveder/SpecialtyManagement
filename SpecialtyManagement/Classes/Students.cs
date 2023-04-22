using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace SpecialtyManagement
{
    public partial class Students
    {

        public string FullName
        {
            get => Surname + " " + Name + " " + Patronymic;
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
                            MessageBox.Show("При чтении файла произошла ошибка. Проверьте кооректность заполнения файла.");
                            return new List<Students>();
                        }

                        students.Add(student);
                    }
                }
            }
            else
            {
                MessageBox.Show("Файл не найден.");
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
            string[] array = text.Split(';');
            string[] fullName = array[0].Split(' ');

            try
            {
                if (fullName.Length > 1) // Если ФИО записано в одном поле.
                {
                    return new Students()
                    {
                        Surname = fullName[0],
                        Name = fullName[1],
                        Patronymic = fullName[2],
                        Birthday = Convert.ToDateTime(array[1]),
                        Note = array[2]
                    };
                }
                else
                {
                    return new Students()
                    {
                        Surname = array[0],
                        Name = array[1],
                        Patronymic = array[2],
                        Birthday = Convert.ToDateTime(array[3]),
                        Note = array[4]
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