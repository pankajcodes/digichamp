//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigiChamps.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_DC_SchoolUser
    {
        public tbl_DC_SchoolUser()
        {
            this.tbl_DC_School_AssingTeacher = new HashSet<tbl_DC_School_AssingTeacher>();
        }
    
        public System.Guid UserId { get; set; }
        public Nullable<System.Guid> SchoolId { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserProfilePhoto { get; set; }
        public string UserRole { get; set; }
        public string UserPhoneNumber { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual tbl_DC_School_Info tbl_DC_School_Info { get; set; }
        public virtual ICollection<tbl_DC_School_AssingTeacher> tbl_DC_School_AssingTeacher { get; set; }
    }
}
