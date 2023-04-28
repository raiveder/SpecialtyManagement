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
                return Database.Entities.ArrearsLessons.Where(x => x.Id == Id).ToList().Count;
            }
        }

        public string LessonsString
        {
            get
            {
                string lessons = string.Empty;

                foreach (ArrearsLessons item in Database.Entities.ArrearsLessons.Where(x => x.Id == Id))
                {
                    lessons += item.Lessons.ShortName + ", ";
                }

                return lessons.Substring(0, lessons.Length - 2);
            }
        }
    }
}