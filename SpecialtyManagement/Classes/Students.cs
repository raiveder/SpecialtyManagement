using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement
{
    public partial class Students
    {
        public int? SequenceNumber { get; set; }

        public string SurnameAndName
        {
            get
            {
                return Surname + " " + Name;
            }
            set { }
        }

        public string ShortName
        {
            get
            {
                if (Patronymic == null)
                {
                    return Surname + " " + Name[0] + ".";
                }
                return Surname + " " + Name[0] + ". " + Patronymic[0] + ".";
            }
            set { }
        }

        public string FullName
        {
            get
            {
                if (Patronymic == null)
                {
                    return SurnameAndName;
                }
                return SurnameAndName + " " + Patronymic;
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
            if (File.Exists(path))
            {
                string[] text = new string[0];

                using (StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        Array.Resize(ref text, text.Length + 1);
                        text[text.Length - 1] = reader.ReadLine();
                    }
                }

                return GetStudentsFromText(text);
            }
            else
            {
                MessageBox.Show("Файл не найден", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new List<Students>();
            }
        }

        /// <summary>
        /// Получает список студентов из текста файла.
        /// </summary>
        /// <param name="text">исходный текст файла построчно.</param>
        /// <returns>Список студентов.</returns>
        private static List<Students> GetStudentsFromText(string[] text)
        {
            List<Students> students = new List<Students>();

            for (int i = text[0].Contains("Фамилия") ? 1 : 0; i < text.Length; i++) // Пропуск строки заголовков.
            {
                Students student = GetStudentFromString(text[i]);

                if (student != null)
                {
                    if (student.Surname != null)
                    {
                        students.Add(student);
                    }
                    else if (student.Note != null)
                    {
                        students.Last().Note += "\n" + student.Note;
                    }
                }
                else
                {
                    MessageBox.Show("При чтении файла произошла ошибка. Проверьте корректность заполнения файла", "Студенты", MessageBoxButton.OK, MessageBoxImage.Information);
                    students = new List<Students>();
                }
            }

            return students;
        }

        /// <summary>
        /// Получает объект класса Students путём преобразования данных из строки.
        /// </summary>
        /// <param name="text">строка для преобразования.</param>
        /// <returns>Объект класса Students.</returns>
        private static Students GetStudentFromString(string text)
        {
            try
            {
                string[] array = text.Split(';');
                string[] fullName = array[1].Split(' ');

                if (fullName[0] != string.Empty && fullName[1] != string.Empty && array[2] != string.Empty) // Если строка содержит полноценные данные о студенте.
                {
                    return new Students()
                    {
                        Surname = fullName[0].Trim(),
                        Name = fullName[1].Trim() == string.Empty ? null : fullName[1].Trim(),
                        Patronymic = fullName[2].Trim(),
                        Birthday = Convert.ToDateTime(array[2].Trim()),
                        Note = array[3].Trim() == string.Empty ? null : array[3].Trim()
                    };
                }
                else if (fullName.Length == 1 && fullName[0] == string.Empty && array[2] == string.Empty && array[3] != string.Empty) // Если строка содержит только примечание.
                {
                    return new Students()
                    {
                        Note = array[3].Trim() == string.Empty ? null : array[3].Trim()
                    };
                }
                else if (fullName.Length == 1 && fullName[1] == string.Empty && array[2] == string.Empty && array[3] == string.Empty) // Если строка пустая.
                {
                    return new Students();
                }
                else // Если строка некорректная.
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}