using System;

namespace SpecialtyManagement
{
    public partial class Arrears
    {
        public int SequenceNumber { get; set; }

        public int CountArrears { get; set; }

        public string SemesterSequenceNumberRoman
        {
            get
            {
                switch (SemesterSequenceNumber)
                {
                    case 1: return "I";
                    case 2: return "II";
                    case 3: return "III";
                    case 4: return "IV";
                    case 5: return "V";
                    case 6: return "VI";
                    case 7: return "VII";
                    case 8: return "VIII";
                    default: return "0";
                }
            }
        }

        /// <summary>
        /// Вычисляет начальный год учебного года и номер семестра.
        /// </summary>
        /// <param name="year">возвращаемый год начала учебного года.</param>
        /// <param name="semesterNumber">возвращаемый номер семестра.</param>
        /// <param name="isCurrentSemester">true - текущий семестр, false - прошлый.</param>
        public static void GetYearAndSemester(out int year, out int semesterNumber, bool isCurrentSemester)
        {
            if (isCurrentSemester)
            {
                if (DateTime.Today >= new DateTime(DateTime.Today.Year, 9, 1) && DateTime.Today <= new DateTime(DateTime.Today.Year, 12, 31))
                {
                    year = DateTime.Today.Year;
                    semesterNumber = 1;
                }
                else
                {
                    year = DateTime.Today.Year - 1;
                    semesterNumber = 2;
                }
            }
            else
            {
                if (DateTime.Today >= new DateTime(DateTime.Today.Year, 9, 1) && DateTime.Today <= new DateTime(DateTime.Today.Year, 12, 31))
                {
                    year = DateTime.Today.Year;
                    semesterNumber = 2;
                }
                else
                {
                    year = DateTime.Today.Year - 1;
                    semesterNumber = 1;
                }
            }
        }

        /// <summary>
        /// Проверяет, является ли указанный семестр текущим.
        /// </summary>
        /// <param name="semesterNumber">номер семестра.</param>
        /// <returns>True - семестр является текущим, в противном случае - false.</returns>
        public static bool IsCurrentSemester(int semesterNumber)
        {
            if (DateTime.Today >= new DateTime(DateTime.Today.Year, 9, 1) && DateTime.Today <= new DateTime(DateTime.Today.Year, 12, 31))
            {
                if (semesterNumber == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (semesterNumber == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}