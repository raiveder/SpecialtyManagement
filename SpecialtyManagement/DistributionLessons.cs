//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpecialtyManagement
{
    using System;
    using System.Collections.Generic;
    
    public partial class DistributionLessons
    {
        public int Id { get; set; }
        public int IdTeacher { get; set; }
        public int IdLesson { get; set; }
        public int IdGroup { get; set; }
    
        public virtual Groups Groups { get; set; }
        public virtual Lessons Lessons { get; set; }
        public virtual Teachers Teachers { get; set; }
    }
}
