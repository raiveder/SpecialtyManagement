namespace SpecialtyManagement
{
    public partial class Lessons
    {
        public string ShortName
        {
            get => TypesLessons.Type + " " + Code;
        }

        public string FullName
        {
            get => TypesLessons.Type + " " + Code + " " + Name;
        }
    }
}