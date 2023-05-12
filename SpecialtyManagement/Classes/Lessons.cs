namespace SpecialtyManagement
{
    public partial class Lessons
    {
        public int SequenceNumber { get; set; }

        public string ShortName
        {
            get
            {
                if (Code.Contains("."))
                {
                    return TypesLessons.Type + " " + Code;
                }
                else
                {
                    return TypesLessons.Type + "." + Code;
                }
            }
            set { }
        }

        public string FullName
        {
            get => ShortName + " " + Name;
            set { }
        }
    }
}