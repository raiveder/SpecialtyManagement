using System.Linq;

namespace SpecialtyManagement
{
    public partial class Arrears
    {
        public int SequenceNumber { get; set; }

        public int CountArrears
        {
            get
            {
                return Database.Entities.ArrearsLessons.Where(x => x.IdArrear == Id).ToList().Count;
            }
        }

        public string LessonsString
        {
            get
            {
                string lessons = string.Empty;

                foreach (ArrearsLessons item in Database.Entities.ArrearsLessons.Where(x => x.IdArrear == Id))
                {
                    lessons += item.Lessons.ShortName + ", ";
                }

                if (lessons.Length > 0)
                {
                    return lessons.Substring(0, lessons.Length - 2);
                }
                return lessons;
            }
        }
    }
}