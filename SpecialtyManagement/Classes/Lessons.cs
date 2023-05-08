namespace SpecialtyManagement
{
    public partial class Lessons
    {
        public int SequenceNumber { get; set; }

        public string ShortName
        {
            get => TypesLessons.Type + " " + Code;
            set { }
        }

        public string FullName
        {
            get => TypesLessons.Type + " " + Code + " " + Name;
            set { }
        }
    }
}