using System;
using System.Collections.Generic;
using System.Linq;

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
                if (DateTime.Today > new DateTime(DateTime.Today.Year, 6, 28) && DateTime.Today < new DateTime(DateTime.Today.Year, 12, 29))
                {
                    year = DateTime.Today.Year;
                    semesterNumber = 1;
                }
                else if (DateTime.Today > new DateTime(DateTime.Today.Year, 12, 28) && DateTime.Today < new DateTime(DateTime.Today.Year + 1, 1, 1))
                {
                    year = DateTime.Today.Year;
                    semesterNumber = 2;
                }
                else
                {
                    year = DateTime.Today.Year - 1;
                    semesterNumber = 2;
                }
            }
            else
            {
                if (DateTime.Today > new DateTime(DateTime.Today.Year, 6, 28) && DateTime.Today < new DateTime(DateTime.Today.Year, 12, 29))
                {
                    year = DateTime.Today.Year - 1;
                    semesterNumber = 2;
                }
                else if (DateTime.Today > new DateTime(DateTime.Today.Year, 12, 28) && DateTime.Today < new DateTime(DateTime.Today.Year + 1, 1, 1))
                {
                    year = DateTime.Today.Year;
                    semesterNumber = 1;
                }
                else
                {
                    year = DateTime.Today.Year - 1;
                    semesterNumber = 1;
                }
            }
        }

        /// <summary>
        /// Удаляет задолженности, которые не соответствуют выбранному типу, из списка.
        /// </summary>
        /// <param name="arrears">список задолженностей.</param>
        /// <param name="idType">типа задолженности.</param>
        public static void DeleteArrearsNotMatchByType(List<Arrears> arrears, int idType)
        {
            List<Arrears> arrearsToRemove = new List<Arrears>();

            foreach (Arrears item in arrears)
            {
                int countLessons = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == item.Id && x.IdType == idType).Count();

                if (countLessons == 0)
                {
                    arrearsToRemove.Add(item);
                }
                else
                {
                    item.CountArrears = countLessons;
                }
            }

            foreach (Arrears item in arrearsToRemove)
            {
                arrears.Remove(item);
            }
        }

        /// <summary>
        /// Возвращает список групп, у студентов которых есть задолженности.
        /// </summary>
        /// <param name="typeArrears">тип задолженности.</param>
        /// <returns>Список групп, у студентов которых есть задолженности определённого типа.</returns>
        public static List<Groups> GetGroupsWithArrears(List<Arrears> arrears, int typeArrears)
        {
            List<Groups> groups = new List<Groups>();

            List<Arrears> tempArrears = new List<Arrears>();
            tempArrears.AddRange(arrears);

            DeleteArrearsNotMatchByType(tempArrears, typeArrears);

            foreach (Arrears arrear in tempArrears)
            {
                if (!groups.Contains(arrear.Students.Groups))
                {
                    groups.Add(arrear.Students.Groups);
                }
            }

            return groups;
        }

        /// <summary>
        /// Возвращает список дисциплин, по которым у студента есть задолженность.
        /// </summary>
        /// <param name="arrear">задолженность.</param>
        /// <param name="idType">тип задолженности.</param>
        /// <returns>Список дисциплин, по которым у студента есть задолженность определённого типа.</returns>
        public static List<Lessons> GetLessonsForArrearsByType(Arrears arrear, int? idType)
        {
            List<ArrearsLessons> arrearLessons = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id).ToList();

            if (idType != null)
            {
                arrearLessons = arrearLessons.Where(x => x.IdType == idType && x.IsLiquidated == false).ToList();
            }

            List<Lessons> lessons = new List<Lessons>();
            foreach (ArrearsLessons item in arrearLessons)
            {
                lessons.Add(item.Lessons);
            }

            return lessons.OrderBy(x => x.FullName).ToList();
        }
    }
}