﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiChamps.Models
{
    public class UploadFileDetailModel
    {
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public string VideoName { get; set; }
        public string VideoPath { get; set; }
    }

    public class SchoolModel
    {      

        public class SchoolAdminOrPrincipleModel
        {
            public List<SchoolAdminOrPrincipleModel> schoolInformation { get; set; }
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            //  [System.Web.Mvc.Remote("IsEmailExist", "School",
            // ErrorMessage = "Email Id already in use")] 
            public string EmailAddress { get; set; }

            public string Password { get; set; }
            //[Compare("Password",ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public string Image { get; set; }
            public bool IsActive { get; set; }
            public Guid SchoolId { get; set; }
        }

        public class HomeWorkModel
        {
            public List<HomeWorkModel> HomeWork { get; set; }
            public Guid Id { get; set; }
            public Guid ClassId { get; set; }
            public string ClassName { get; set; }
            public string SubjectName { get; set; }
            public Guid SectionId { get; set; }
            public DateTime Date { get; set; }
            public string TimeSlot { get; set; }
            public Guid SubjectId { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public Guid SchoolId { get; set; }
        }

        public class CreateExamModel
        {
            public Guid Id { get; set; }
            public Guid ExamType { get; set; }
            public Guid ClassId { get; set; }
            public Guid SubjectId { get; set; }
            public Guid SectionId { get; set; }
            public DateTime StartDate { get; set; }
            public int Class_Id { get; set; }
            public string TimeSlot { get; set; }
            public float TotalMarks { get; set; }
            public bool IsActive { get; set; }
            [Required(ErrorMessage="Please Select Exam Date")]
            public DateTime DateofExam { get; set; }
            public string SubjectName { get; set; }


            //public string Id { get; set; }
            ////add by am
            //public Guid ExamId { get; set; }

            // public int ExamType { get; set; }
            public string ExamTypeName { get; set; }
            //public int ClassId { get; set; }
            //public Guid SubjectId { get; set; }
            //public string SubjectName { get; set; }
            //public int SectionId { get; set; }
            //public DateTime StartDate { get; set; }
            //public string TimeSlot { get; set; }
            //public float TotalMarks { get; set; }
            //public bool IsActive { get; set; }
        }

        public class MessageCreation
        {
            public int ClassId { get; set; }
            //public string Id { get; set; }
            //public int SectionId { get; set; }
            //public string Message { get; set; }
            //public string Image { get; set; }
            //public bool IsActive { get; set; }
            public DateTime MessageDisplayDate { get; set; }


            public Guid MessageId { get; set; }
            public string Id { get; set; }
            public int SectionId { get; set; }
            public string Message { get; set; }
            public string Image { get; set; }

            public string ClassName { get; set; }

            public string SectionName { get; set; }
            public bool IsActive { get; set; }
            // public DateTime DisplayDate { get; set; }
        }


        public class ToppersWayModel
        {
            public Guid School { get; set; }
            public Guid ClassId { get; set; }
            public Guid Id { get; set; }
            public string path { get; set; }
            public string Type { get; set; }
            public bool IsActive { get; set; }
            public string ClassName { get; set; }

        }

        public class SchoolInformation
        {
            public string ThumbnailPath { get; set; }
            public Guid SchoolId { get; set; }
            [Required(ErrorMessage = "School Name is required")]
            public string SchoolName { get; set; }
            public string Information { get; set; }
            public string Logo { get; set; }
            public string DocumentaryVideo { get; set; }
            public bool IsActive { get; set; }
            [DisplayName("Select Image File to Upload")]
            public HttpPostedFileBase ImageFileUpload { get; set; }
            [DisplayName("Select Video File to Upload")]
            public HttpPostedFileBase VideoFileUpload { get; set; }
        }

        public class CreateSubject
        {
            public Guid Id { get; set; }
            public string SubjectName { get; set; }
            public bool IsActive { get; set; }
            public Guid SchoolId { get; set; }
            public Guid SubjectId { get; set; }

        }
        public class CreateClass
        {
            public Guid SchoolId { get; set; }
            public Guid Id { get; set; }
            public string ClassName { get; set; }
            public string SectionName { get; set; }
            public bool IsActive { get; set; }
            public List<SelectListItem> Section { get; set; }
            public List<SelectListItem> Class { get; set; }
        }


        public class StudyMaterialModel
        {
            public Guid Id { get; set; }
            public Guid ClassId { get; set; }
            public Guid SubjectId { get; set; }
            public string Topic { get; set; }
            public string Material { get; set; }
            public string Image { get; set; }
            public string SubjectName { get; set; }

            public string ClassName { get; set; }
            public string MaterialType { get; set; }
            public bool IsActive { get; set; }



        }
        public class ExamType
        {
            public Guid ExamTypeId { get; set; }
            public string ExamTypenname { get; set; }
            public bool IsActive { get; set; }
            public Guid SchoolId { get; set; }
            public Guid SubjectId { get; set; }
        }


        public class AssignTeacher
        {
            public Guid AssignmentId { get; set; }
            public Guid SchoolId { get; set; }
            public Guid ClassId { get; set; }
            public int Class_Id { get; set; }
            public Guid SectionId { get; set; }
            public Guid SubjectId { get; set; }
            public Guid TeacherId { get; set; }
            public SelectList Section { get; set; }
            public SelectList Class { get; set; }
            public SelectList Teacher { get; set; }
            public string ClassName { get; set; }
            public string SubjectName { get; set; }
            public string TeacherName { get; set; }
            public string SectionName { get; set; }
            public string EmailAddress { get; set; }
            public string ImageUrl { get; set; }
            public bool IsActive { get; set; }
            //public Guid SchoolId { get; set; }

        }
        public class InputModel
        {
            public Guid? SchoolNId { get; set; }
            public Guid SchoolId { get; set; }
            public Guid ClassId { get; set; }
            public Guid SectionId { get; set; }
            public Guid SubjectId { get; set; }
            public Guid TeacherId { get; set; }
            public Guid StudyMaterialId { get; set; }
            public DateTime Startdate { get; set; }
        }
        public class ExamTypeModel
        {
            public Guid ExamTypeId { get; set; }
            public string ExamTypeName { get; set; }
            public Guid SchoolId { get; set; }
        }
      

        #region  API Model

        public class SchoolInformationOutput
        {
            public List<SchoolInformation> schoolInformation { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }

        public class AssignTeacherOutput
        {
            public AssignTeacher AssignDetail { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }
        public class ExamScheduleOutput
        {
            public List<SchoolModel.CreateExamModel> ExamScheduleList { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }

        public class DailyHomeWorkOutput
        {
            public List<SchoolModel.HomeWorkModel> HomeWorkList { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }

        public class StudyMaterialOutput
        {
            public List<SchoolModel.StudyMaterialModel> StudyMaterialList { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }

        public class StudyMaterialDetailOutput
        {
            public SchoolModel.StudyMaterialModel StudyMaterialDetail { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }

        public class SchoolNoticeOutput
        {
            public List<SchoolModel.MessageCreation> MessageCreationList { get; set; }
            public string Message { get; set; }

            public int ResultCount { get; set; }
        }
        #endregion
    }
}