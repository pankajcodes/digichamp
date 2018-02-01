using DigiChamps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DigiChamps.Controllers
{
    public class SchoolAPIController : ApiController
    {
        DigiChampsEntities DbContext = new DigiChampsEntities();

        /// <summary>
        /// Get the school detail according to the school id
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        public SchoolModel.SchoolInformationOutput GetSchool(Guid? schoolId)
        {

            SchoolModel.SchoolInformationOutput objOutput = new SchoolModel.SchoolInformationOutput();
            List<SchoolModel.SchoolInformation> objList = new List<SchoolModel.SchoolInformation>();

            var schoolList = new List<tbl_DC_School_Info>();
            if (schoolId != null)
            {
                //var schoolUser=  DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == userId).SingleOrDefault();
                ////if(schoolUser!=null && (schoolUser.SchoolId!=null|| schoolUser.SchoolId!=Guid.Empty))
                ////{
                schoolList = DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == schoolId && x.IsActive == true).OrderByDescending(x => x.CreationDate).ToList();
                if (schoolList == null || schoolList.Count == 0)
                {
                    objOutput.Message = "Your school is not active,Please contact to admin.";
                    objOutput.ResultCount = 0;
                    return objOutput;
                }
                ////}

            }
            else
            {
                schoolList = DbContext.tbl_DC_School_Info.Where(x => x.IsActive == true).OrderByDescending(x=>x.CreationDate).ToList();

            }
            if (schoolList != null || schoolList.Count > 0)
            {
                foreach (var item in schoolList)
                {
                    SchoolModel.SchoolInformation objSchoolInformation = new SchoolModel.SchoolInformation();
                    objSchoolInformation.SchoolName = item.SchoolName;
                    objSchoolInformation.DocumentaryVideo = item.SchoolVideo;
                    objSchoolInformation.Information = item.SchoolDescription;
                    objSchoolInformation.IsActive = (bool)item.IsActive;
                    objSchoolInformation.Logo = item.SchoolLogo;
                    objSchoolInformation.SchoolId = item.SchoolId;
                    objSchoolInformation.ThumbnailPath = item.SchoolThumbnail;
                    objList.Add(objSchoolInformation);


                }
                objOutput.Message = "Successfull";
                objOutput.ResultCount = objList.Count;
                objOutput.schoolInformation = objList;
            }
            else
            {
                objOutput.Message = "No result found";
                objOutput.ResultCount = 0;
                // objOutput.schoolInformation = objList;
            }
            return objOutput;
        }

        public SchoolModel.SchoolInformation GetSchoolBySchoolId(Guid? schoolId)
        {
            SchoolModel.SchoolInformation objSchoolInformation = new SchoolModel.SchoolInformation();
            if (schoolId != null)
            {
              var schoolInfoDetail=  DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == schoolId && x.IsActive == true).OrderByDescending(x => x.CreationDate).FirstOrDefault();
              if (schoolInfoDetail != null) 
              {
                  objSchoolInformation.SchoolName = schoolInfoDetail.SchoolName;
                  objSchoolInformation.DocumentaryVideo = schoolInfoDetail.SchoolVideo;
                  objSchoolInformation.Information = schoolInfoDetail.SchoolDescription;
                  objSchoolInformation.IsActive = (bool)schoolInfoDetail.IsActive;
                  objSchoolInformation.Logo = schoolInfoDetail.SchoolLogo;
                  objSchoolInformation.SchoolId = schoolInfoDetail.SchoolId;
                  objSchoolInformation.ThumbnailPath = schoolInfoDetail.SchoolThumbnail;                 
              }
            }

            return objSchoolInformation;
        }
        /// <summary>
        /// Get the school detail according to the school id
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        public SchoolModel.AssignTeacherOutput GetAssignTacher(SchoolModel.InputModel input)
        {

            SchoolModel.AssignTeacherOutput objOutput = new SchoolModel.AssignTeacherOutput();
            SchoolModel.AssignTeacher objAssignTeacher = new SchoolModel.AssignTeacher();


            if ((input.SchoolId != Guid.Empty) && (input.ClassId != Guid.Empty))
            {
                try
                {
                    var assignteacherDetail = DbContext.tbl_DC_School_AssingTeacher.Where(x => x.ClassId == input.ClassId && x.SchoolId == input.SchoolId && x.SectionId == input.SectionId && x.IsActive == true).SingleOrDefault();
                    if (assignteacherDetail != null)
                    {
                        // add teacher id in assign table
                        //var teacherDetail=DbContext.tbl_DC_SchoolUser.Where(x=>x.UserId==assignteacherDetail.tea)  
                        var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == input.ClassId).SingleOrDefault();
                        if (classDetail != null)
                        {
                            objAssignTeacher.ClassName = classDetail.ClassName;
                            objOutput.Message = "Successfull";
                            objOutput.ResultCount = 1;
                            objOutput.AssignDetail = objAssignTeacher;
                            //need to add other field of teachers here
                        }
                        else
                        {
                            objOutput.Message = "No result found";
                            objOutput.ResultCount = 0;
                            // objOutput.schoolInformation = objList;

                        }
                    }
                    else
                    {
                        objOutput.Message = "No result found";
                        objOutput.ResultCount = 0;
                        // objOutput.schoolInformation = objList;

                    }
                }
                catch (Exception ex)
                {

                    objOutput.Message = "There is some error.";
                    objOutput.ResultCount = 0;
                }

            }
            else
            {
                objOutput.Message = "please send the required parameter.";
                objOutput.ResultCount = 0;
                // objOutput.schoolInformation = objList;

            }



            return objOutput;
        }

        public SchoolModel.ExamScheduleOutput ExamSchedule(SchoolModel.InputModel input)
        {
            SchoolModel.ExamScheduleOutput objOutput = new SchoolModel.ExamScheduleOutput();

            List<SchoolModel.CreateExamModel> objList = new List<SchoolModel.CreateExamModel>();

            if ((input.SchoolId != Guid.Empty) && (input.ClassId != Guid.Empty))
            {
                try
                {
                    var ExamSchedule = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.ClassId == input.ClassId && x.SchoolId == input.SchoolId && x.DateOfExam >= DateTime.Now).ToList();
                    if (ExamSchedule != null && ExamSchedule.Count > 0)
                    {
                        foreach (var item in ExamSchedule)
                        {
                            SchoolModel.CreateExamModel objCreateExamModel = new SchoolModel.CreateExamModel();
                            var examType = DbContext.tbl_DC_School_ExamType.Where(x => x.SchoolId == input.SchoolId && x.ExamTypeId == item.ExamTypeId).SingleOrDefault();
                            var subject = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId).SingleOrDefault();
                            if (examType != null)
                            {
                                objCreateExamModel.ExamTypeName = examType.ExamTypeName;
                                objCreateExamModel.StartDate = (DateTime)item.DateOfExam;
                                objCreateExamModel.SubjectName = subject.SubjectName;
                                objList.Add(objCreateExamModel);
                            }
                        }
                        objOutput.Message = "Successfull";
                        objOutput.ResultCount = objList.Count();
                        objOutput.ExamScheduleList = objList;
                    }
                    else
                    {
                        objOutput.Message = "No result found";
                        objOutput.ResultCount = 0;
                        // objOutput.schoolInformation = objList;

                    }
                }
                catch (Exception ex)
                {
                    objOutput.Message = "There is some error.";
                    objOutput.ResultCount = 0;

                }
            }
            else
            {
                objOutput.Message = "please send the required parameter.";
                objOutput.ResultCount = 0;
                // objOutput.schoolInformation = objList;

            }
            return objOutput;
        }
        /// <summary>
        /// daily homework detail
        /// </summary>
        /// <param name="schoolId"></param>
        /// <param name="classId"></param>
        /// <param name="sectionId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public SchoolModel.DailyHomeWorkOutput DailyHome(SchoolModel.InputModel input)
        {
            SchoolModel.DailyHomeWorkOutput objOutput = new SchoolModel.DailyHomeWorkOutput();
            List<SchoolModel.HomeWorkModel> objList = new List<SchoolModel.HomeWorkModel>();

            if ((input.SchoolId != Guid.Empty) && (input.ClassId != Guid.Empty) && (input.SectionId != Guid.Empty))
            {
                var homeWorkList = DbContext.tbl_DC_School_Homework.Where(x => x.ClassId == input.ClassId && x.SchoolId == input.SchoolId && x.SectionId == input.SectionId && x.CreatedDate == input.Startdate).ToList();
                if (homeWorkList != null && homeWorkList.Count > 0)
                {
                    foreach (var item in homeWorkList)
                    {
                        try
                        {
                            var subject = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SchoolId).SingleOrDefault();
                            if (subject != null)
                            {
                                SchoolModel.HomeWorkModel objHomeWorkModel = new SchoolModel.HomeWorkModel();
                                objHomeWorkModel.Description = item.HomeworkDetail;
                                // objHomeWorkModel.SubjectName = subject.SubjectName;
                                objHomeWorkModel.Date = (DateTime)item.CreatedDate;
                                objList.Add(objHomeWorkModel);
                            }

                        }
                        catch (Exception ex)
                        {

                            objOutput.Message = "There is some error occurs.";
                            objOutput.ResultCount = 0;
                        }

                    }
                    objOutput.Message = "Successfull";
                    objOutput.ResultCount = objList.Count();
                    objOutput.HomeWorkList = objList;
                }
                else
                {
                    objOutput.Message = "No result found";
                    objOutput.ResultCount = 0;
                }

            }
            else
            {
                objOutput.Message = "please send the required parameter.";
                objOutput.ResultCount = 0;
            }

            return objOutput;
        }
        /// <summary>
        /// Get the study material
        /// </summary>
        /// <param name="schoolId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public SchoolModel.StudyMaterialOutput StudyMaterial(SchoolModel.InputModel input)
        {
            SchoolModel.StudyMaterialOutput objOutput = new SchoolModel.StudyMaterialOutput();
            List<SchoolModel.StudyMaterialModel> objList = new List<SchoolModel.StudyMaterialModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty) && (input.ClassId != Guid.Empty))
                {

                    var studyMaterial = DbContext.tbl_DC_School_StudyMaterial.Where(x => x.ClassId == input.ClassId && x.SchoolId == input.SchoolId).ToList();
                    if (studyMaterial != null && studyMaterial.Count > 0)
                    {
                        foreach (var item in studyMaterial)
                        {
                            var subject = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId).SingleOrDefault();
                            if (subject != null)
                            {
                                SchoolModel.StudyMaterialModel objStudyMaterialModel = new SchoolModel.StudyMaterialModel();
                                objStudyMaterialModel.Topic = item.Topic;
                                //objStudyMaterialModel.MaterialType = item.FileType;
                                objStudyMaterialModel.Id = item.StudyMaterialId;
                                objList.Add(objStudyMaterialModel);
                            }
                        }
                        objOutput.Message = "Successfull";
                        objOutput.ResultCount = objList.Count();
                        objOutput.StudyMaterialList = objList;
                    }
                    else
                    {
                        objOutput.Message = "No result found";
                        objOutput.ResultCount = 0;
                    }
                }
                else
                {
                    objOutput.Message = "please send the required parameter.";
                    objOutput.ResultCount = 0;
                }

            }
            catch (Exception ex)
            {

                objOutput.Message = "There is some error occurs.";
                objOutput.ResultCount = 0;
            }

            return objOutput;
        }
        /// <summary>
        /// Get the study material detail on the basis of study material id
        /// </summary>
        /// <param name="schoolId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public SchoolModel.StudyMaterialDetailOutput StudyMaterialDetail(SchoolModel.InputModel input)
        {
            SchoolModel.StudyMaterialDetailOutput objOutput = new SchoolModel.StudyMaterialDetailOutput();
            List<SchoolModel.StudyMaterialModel> objList = new List<SchoolModel.StudyMaterialModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty) && (input.StudyMaterialId != Guid.Empty))
                {

                    var studyMaterial = DbContext.tbl_DC_School_StudyMaterial.Where(x => x.StudyMaterialId == input.StudyMaterialId && x.SchoolId == input.SchoolId).SingleOrDefault();
                    if (studyMaterial != null)
                    {


                        SchoolModel.StudyMaterialModel objStudyMaterialModel = new SchoolModel.StudyMaterialModel();
                        objStudyMaterialModel.Image = studyMaterial.FilePath;
                        objStudyMaterialModel.MaterialType = studyMaterial.FileType;
                        objList.Add(objStudyMaterialModel);

                        objOutput.Message = "Successfull";
                        objOutput.ResultCount = objList.Count();
                        objOutput.StudyMaterialDetail = objStudyMaterialModel;



                    }
                    else
                    {
                        objOutput.Message = "No result found";
                        objOutput.ResultCount = 0;
                    }
                }
                else
                {
                    objOutput.Message = "please send the required parameter.";
                    objOutput.ResultCount = 0;
                }

            }
            catch (Exception ex)
            {

                objOutput.Message = "There is some error occurs.";
                objOutput.ResultCount = 0;
            }

            return objOutput;
        }
        /// <summary>
        /// Get the school notice according to the class.
        /// </summary>
        /// <param name="schoolId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public SchoolModel.SchoolNoticeOutput SchoolNotice(SchoolModel.InputModel input)
        {
            SchoolModel.SchoolNoticeOutput objOutput = new SchoolModel.SchoolNoticeOutput();
            List<SchoolModel.MessageCreation> objList = new List<SchoolModel.MessageCreation>();
            try
            {
                if ((input.SchoolId != Guid.Empty) && (input.ClassId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var schoolNoticeList = DbContext.tbl_DC_School_MessageCreation.Where(x => x.ClassId == input.ClassId && x.SchoolId == input.SchoolId && x.MassageDisplayDate == date).ToList();
                    if (schoolNoticeList != null && schoolNoticeList.Count() > 0)
                    {
                        foreach (var item in schoolNoticeList)
                        {
                            SchoolModel.MessageCreation objMessageCreation = new SchoolModel.MessageCreation();
                            objMessageCreation.Message = item.MassageDisplay;
                            objMessageCreation.Image = item.ImagePath;

                            //objMessageCreation.DisplayDate = (DateTime)item.MassageDisplayDate;
                            objList.Add(objMessageCreation);
                        }
                        objOutput.Message = "Successfull";
                        objOutput.ResultCount = objList.Count();
                        objOutput.MessageCreationList = objList;
                    }

                    else
                    {
                        objOutput.Message = "No result found";
                        objOutput.ResultCount = 0;
                    }
                }
                else
                {
                    objOutput.Message = "please send the required parameter.";
                    objOutput.ResultCount = 0;
                }
            }
            catch (Exception ex)
            {

                objOutput.Message = "There is some error.";
                objOutput.ResultCount = 0;
            }
            return objOutput;
        }


        /// <summary>
        /// Function to get Class list
        /// SK
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.HomeWorkModel> GetHomeWorkList(SchoolModel.InputModel input)
        {


            List<SchoolModel.HomeWorkModel> objList = new List<SchoolModel.HomeWorkModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var HomeWorkList = DbContext.tbl_DC_School_Homework.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (HomeWorkList != null && HomeWorkList.Count() > 0)
                    {
                        foreach (var item in HomeWorkList)
                        {
                            //var SectionDetail = DbContext.tbl_DC_School_s.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            SchoolModel.HomeWorkModel objOutput = new SchoolModel.HomeWorkModel();
                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {
                                objOutput.ClassName = classDetail.ClassName;

                            }
                            objOutput.Description = item.HomeworkDetail;
                            objOutput.Date = (DateTime)item.CreatedDate;
                            objOutput.Id = item.HomeworkId;
                            objOutput.SchoolId = (Guid)item.SchoolId;
                            if (schoolDetail != null)
                            {
                                objOutput.SubjectName = schoolDetail.SubjectName;
                            }

                            objList.Add(objOutput);
                        }
                        // objOutput.HomeWork = objList;

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }



        /// <summary>
        /// Function to get exam type list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //    public List<SchoolModel.ExamType> GetExamTypeList(SchoolModel.InputModel input)
        //    {

        //        SchoolModel.ExamType objOutput = new SchoolModel.ExamType();
        //        List<SchoolModel.ExamType> objList = new List<SchoolModel.ExamType>();
        //        try
        //        {
        //            if ((input.SchoolId != Guid.Empty))
        //            {
        //                var date = DateTime.Now;
        //                var examTypelist = DbContext.tbl_DC_School_ExamType.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).OrderBy(x => x.SubjectId).ToList();
        //                if (examTypelist != null && examTypelist.Count() > 0)
        //                {
        //                    foreach (var item in examTypelist)
        //                    {
        //                        objOutput.ExamTypeId = item.ExamTypeId;
        //                        objOutput.ExamTypenname = item.ExamTypeName;
        //                        objList.Add(objOutput);
        //                    }

        //                }
        //            }

        //        }
        //        catch (Exception ex)
        //        {


        //        }
        //        return objList;

        //}




        /// <summary>
        /// Function to get message list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.MessageCreation> GetMessageList(SchoolModel.InputModel input)
        {


            List<SchoolModel.MessageCreation> objList = new List<SchoolModel.MessageCreation>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var messageList = DbContext.tbl_DC_School_MessageCreation.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (messageList != null && messageList.Count() > 0)
                    {
                        foreach (var item in messageList)
                        {
                            SchoolModel.MessageCreation objOutput = new SchoolModel.MessageCreation();

                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {

                                objOutput.ClassName = classDetail.ClassName;
                            }

                            objOutput.Message = item.MassageDisplay;
                            objOutput.MessageId = item.MessageId;
                            objOutput.MessageDisplayDate = (DateTime)item.MassageDisplayDate;

                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (examType != null)
                            //{
                            //    objOutput.ExamTypeName = examType.ExamTypeName;
                            //}




                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }
        /// <summary>
        /// GetCLass
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.CreateClass> GetClassList(SchoolModel.InputModel input)
        {


            List<SchoolModel.CreateClass> objList = new List<SchoolModel.CreateClass>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var classList = DbContext.tbl_DC_School_Class.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (classList != null && classList.Count() > 0)
                    {
                        foreach (var item in classList)
                        {
                            //item.
                            SchoolModel.CreateClass objOutput = new SchoolModel.CreateClass();
                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {

                                objOutput.ClassName = classDetail.ClassName;
                            }
                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (SectionDetail != null)
                            //{
                            //    objOutput.SectionName = SectionDetail.SectionName;
                            //}
                            // objOutput.SectionName = item.;
                            objOutput.Id = item.ClassId;
                            // objOutput.MessageDisplayDate = (DateTime)item.MassageDisplayDate;
                            //





                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }




        /// <summary>
        /// Function to get assign teacher list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.AssignTeacher> GetAssignTeacherList(SchoolModel.InputModel input)
        {


            List<SchoolModel.AssignTeacher> objList = new List<SchoolModel.AssignTeacher>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var assingTeacherlist = DbContext.tbl_DC_School_AssingTeacher.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (assingTeacherlist != null && assingTeacherlist.Count() > 0)
                    {
                        foreach (var item in assingTeacherlist)
                        {
                            SchoolModel.AssignTeacher objOutput = new SchoolModel.AssignTeacher();
                            var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId && x.IsActive == true).SingleOrDefault();
                            if (schoolDetail != null)
                            {

                                objOutput.SubjectName = schoolDetail.SubjectName;
                            }

                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {

                                objOutput.ClassName = classDetail.ClassName;
                            }
                            var teacherDetail = DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == item.TeacherId && x.IsActive == true).SingleOrDefault();
                            if (teacherDetail != null)
                            {

                                objOutput.TeacherName = teacherDetail.UserFirstname + teacherDetail.UserLastname;
                            }
                            objOutput.AssignmentId = item.Id;


                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (examType != null)
                            //{
                            //    objOutput.ExamTypeName = examType.ExamTypeName;
                            //}




                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }



        /// <summary>
        /// Function to get study material list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.StudyMaterialModel> GetStudyMaterialList(SchoolModel.InputModel input)
        {


            List<SchoolModel.StudyMaterialModel> objList = new List<SchoolModel.StudyMaterialModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var studyMateriallist = DbContext.tbl_DC_School_StudyMaterial.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (studyMateriallist != null && studyMateriallist.Count() > 0)
                    {
                        foreach (var item in studyMateriallist)
                        {
                            SchoolModel.StudyMaterialModel objOutput = new SchoolModel.StudyMaterialModel();
                            var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId && x.IsActive == true).SingleOrDefault();
                            if (schoolDetail != null)
                            {

                                objOutput.SubjectName = schoolDetail.SubjectName;
                            }

                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {

                                objOutput.ClassName = classDetail.ClassName;
                            }
                            objOutput.Id = item.StudyMaterialId;
                            objOutput.Material = item.Topic;

                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (examType != null)
                            //{
                            //    objOutput.ExamTypeName = examType.ExamTypeName;
                            //}




                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }

        /// <summary>
        /// Function to get topper list Is 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public List<SchoolModel.ToppersWayModel> GetToppersWay(SchoolModel.InputModel input)
        {


            List<SchoolModel.ToppersWayModel> objList = new List<SchoolModel.ToppersWayModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var Topperswaylist = DbContext.tbl_DC_ToppersWay.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (Topperswaylist != null && Topperswaylist.Count() > 0)
                    {
                        foreach (var item in Topperswaylist)
                        {
                            SchoolModel.ToppersWayModel objOutput = new SchoolModel.ToppersWayModel();
                            //var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SchoolId == item.SchoolId && x.IsActive == true).SingleOrDefault();
                            //if (schoolDetail != null)
                            //{

                            //    objOutput.SubjectName = schoolDetail.SubjectName;
                            //}

                            var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == item.ClassId && x.IsActive == true).SingleOrDefault();
                            if (classDetail != null)
                            {

                                objOutput.ClassName = classDetail.ClassName;
                            }
                            objOutput.Id = item.ToppersId;
                            objOutput.path = item.FileURL;


                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (examType != null)
                            //{
                            //    objOutput.ExamTypeName = examType.ExamTypeName;
                            //}




                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }


        /// <summary>
        /// Function to get Exam list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.CreateExamModel> GetExamListList(SchoolModel.InputModel input)
        {


            List<SchoolModel.CreateExamModel> objList = new List<SchoolModel.CreateExamModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var examlist = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (examlist != null && examlist.Count() > 0)
                    {
                        foreach (var item in examlist)
                        {
                            SchoolModel.CreateExamModel objOutput = new SchoolModel.CreateExamModel();
                            var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SubjectId && x.IsActive == true).SingleOrDefault();
                            if (schoolDetail != null)
                            {
                                objOutput.SubjectName = schoolDetail.SubjectName;
                            }
                            var examType = DbContext.tbl_DC_School_ExamType.Where(x => x.ExamTypeId == item.ExamTypeId && x.IsActive == true).SingleOrDefault();
                            if (examType != null)
                            {
                                objOutput.ExamTypeName = examType.ExamTypeName;
                            }
                            //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
                            //if (examType != null)
                            //{
                            //    objOutput.ExamTypeName = examType.ExamTypeName;
                            //}
                            objOutput.Id = item.ExamScheduleId;
                            objOutput.StartDate = (DateTime)item.DateOfExam;
                            objOutput.TimeSlot = item.TimeSlot;



                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }


        /// <summary>
        /// Function to get Subject list
        /// SK
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.CreateSubject> GetSubjectList(SchoolModel.InputModel input)
        {


            List<SchoolModel.CreateSubject> objList = new List<SchoolModel.CreateSubject>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var schoollist = DbContext.tbl_DC_School_Subject.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (schoollist != null && schoollist.Count() > 0)
                    {
                        foreach (var item in schoollist)
                        {
                            SchoolModel.CreateSubject objOutput = new SchoolModel.CreateSubject();
                            objOutput.SubjectId = item.SubjectId;
                            objOutput.SubjectName = item.SubjectName;
                            objOutput.SchoolId = (Guid)item.SchoolId;
                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;
        }

        /// <summary>
        /// Function to get exam type list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<SchoolModel.ExamTypeModel> GetExamTypeList(SchoolModel.InputModel input)
        {


            List<SchoolModel.ExamTypeModel> objList = new List<SchoolModel.ExamTypeModel>();
            try
            {
                if ((input.SchoolId != Guid.Empty))
                {
                    var date = DateTime.Now;
                    var examTypelist = DbContext.tbl_DC_School_ExamType.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
                    if (examTypelist != null && examTypelist.Count() > 0)
                    {
                        foreach (var item in examTypelist)
                        {
                            SchoolModel.ExamTypeModel objOutput = new SchoolModel.ExamTypeModel();
                            objOutput.ExamTypeId = item.ExamTypeId;
                            objOutput.ExamTypeName = item.ExamTypeName;
                            objOutput.SchoolId = (Guid)item.SchoolId;
                            objList.Add(objOutput);
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return objList;

        }
        /// <summary>
        /// Function to get Exam list
        /// AM
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        ////public List<SchoolModel.CreateExamModel> GetExamListList(SchoolModel.InputModel input)
        ////{

        ////    SchoolModel.CreateExamModel objOutput = new SchoolModel.CreateExamModel();
        ////    List<SchoolModel.CreateExamModel> objList = new List<SchoolModel.CreateExamModel>();
        ////    try
        ////    {
        ////        if ((input.SchoolId != Guid.Empty))
        ////        {
        ////            var date = DateTime.Now;
        ////            var examlist = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.SchoolId == input.SchoolId && x.IsActive == true).ToList();
        ////            if (examlist != null && examlist.Count() > 0)
        ////            {
        ////                foreach (var item in examlist)
        ////                {
        ////                    var schoolDetail = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == item.SchoolId && x.IsActive == true).SingleOrDefault();
        ////                    if (schoolDetail != null)
        ////                    {
        ////                        objOutput.SubjectName = schoolDetail.SubjectName;
        ////                    }
        ////                    var examType = DbContext.tbl_DC_School_ExamType.Where(x => x.ExamTypeId == item.ExamTypeId && x.IsActive == true).SingleOrDefault();
        ////                    if (examType != null)
        ////                    {
        ////                        objOutput.ExamTypeName = examType.ExamTypeName;
        ////                    }
        ////                    //var SectionDetail = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.Sec && x.IsActive == true).SingleOrDefault();
        ////                    //if (examType != null)
        ////                    //{
        ////                    //    objOutput.ExamTypeName = examType.ExamTypeName;
        ////                    //}
        ////                    objOutput.ExamId = item.ExamScheduleId;
        ////                    objOutput.StartDate =(DateTime) item.DateOfExam;
        ////                    objOutput.TimeSlot = item.TimeSlot;



        ////                    objList.Add(objOutput);
        ////                }

        ////            }
        ////        }

        ////    }
        ////    catch (Exception ex)
        ////    {


        ////    }
        ////    return objList;
        ////}
        //// /// <summary>
        /// Function to get User list depending on user role
        /// SK
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //public List<SchoolModel.SchoolAdminOrPrincipleModel> GetUserList(SchoolModel.InputModel input)
        //{

        //    SchoolModel.SchoolAdminOrPrincipleModel objOutput = new SchoolModel.SchoolAdminOrPrincipleModel();
        //    List<SchoolModel.SchoolAdminOrPrincipleModel> objList = new List<SchoolModel.SchoolAdminOrPrincipleModel>();
        //    try
        //    {
        //        if ((input.SchoolId != Guid.Empty))
        //        {
        //            var date = DateTime.Now;
        //            var userlist = DbContext.tbl_DC_SchoolUser.Where(x => x.SchoolId == input.SchoolId && x.UserRole == input.RoleName && x.IsActive == true).ToList();
        //            if (userlist != null && userlist.Count() > 0)
        //            {
        //                foreach (var item in userlist)
        //                {
        //                    objOutput.Id = item.UserId;
        //                    objOutput.FirstName = item.UserFirstname;
        //                    objOutput.LastName = item.UserLastname;
        //                    objOutput.Image = item.UserProfilePhoto;
        //                    objOutput.EmailAddress = item.UserEmailAddress;
        //                    objOutput.PhoneNumber = item.UserPhoneNumber;

        //                    objList.Add(objOutput);
        //                }

        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {


        //    }
        //    return objList;
        //}


    }

}
