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
    
    public partial class Arrears
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Arrears()
        {
            this.ArrearsLessons = new HashSet<ArrearsLessons>();
        }
    
        public int Id { get; set; }
        public int IdStudent { get; set; }
        public int StartYear { get; set; }
        public int SemesterNumber { get; set; }
        public int SemesterSequenceNumber { get; set; }
    
        public virtual Students Students { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ArrearsLessons> ArrearsLessons { get; set; }
    }
}
