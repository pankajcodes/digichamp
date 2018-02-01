using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigiChamps.Models;
using System.IO;
using DigiChamps.DigiChampsEnum;
using System.Net.Mail;
using DigiChamps.Common;

namespace DigiChamps.Controllers
{
    public class SchoolController : Controller
    {
        //
        // GET: /School/
        DigiChampsEntities DbContext = new DigiChampsEntities();

        public ActionResult Index()
        {
            return View();
        }

        #region SchoolDetail
        public ActionResult GetSchoolList()
        {
            SchoolAPIController obj = new SchoolAPIController();
            SchoolModel.SchoolInformationOutput objOutput = new SchoolModel.SchoolInformationOutput();
            objOutput = obj.GetSchool(null);

            return View(objOutput.schoolInformation);
        }

        /// <summary>
        /// Get school detail
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult AddSchool(Guid? Id)
        {
            SchoolModel.SchoolInformation objSchoolInformation = new SchoolModel.SchoolInformation();

            if (Id != null)
            {
                SchoolAPIController schoolApi = new SchoolAPIController();
                return View(schoolApi.GetSchoolBySchoolId(Id));
            }

            return View(objSchoolInformation);
        }

        /// <summary>
        /// Add school
        /// </summary>
        /// <param name="objSchoolInformation"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSchool(SchoolModel.SchoolInformation objSchoolInformation)
        {

            //Add School 
            if (objSchoolInformation.SchoolId == Guid.Empty || objSchoolInformation.SchoolId == null)
            {
                objSchoolInformation.SchoolId = Guid.NewGuid();
                objSchoolInformation.IsActive = true;
                #region UploadFile
                FileHelper fileHelper = new FileHelper();
                string browserType = Request.Browser.Browser.ToUpper();
                UploadFileDetailModel uploadFileDetailModel = fileHelper.UploadDoc(Request.Files, "School", objSchoolInformation.SchoolId.ToString(), "Image", browserType);
                objSchoolInformation.Logo = !string.IsNullOrEmpty(uploadFileDetailModel.ImageName) ? uploadFileDetailModel.ImagePath + "/" + uploadFileDetailModel.ImageName : string.Empty;
                objSchoolInformation.DocumentaryVideo = !string.IsNullOrEmpty(uploadFileDetailModel.VideoName) ? uploadFileDetailModel.ImagePath + "/" + uploadFileDetailModel.ImageName : string.Empty;
                //objSchoolInformation.Logo = uploadFileDetailModel.ImageName;
                #endregion

                DbContext.Sp_DC_SchoolInfo(objSchoolInformation.SchoolId, objSchoolInformation.SchoolName, objSchoolInformation.Information, objSchoolInformation.Logo, objSchoolInformation.DocumentaryVideo, objSchoolInformation.ThumbnailPath, DateTime.Now, DateTime.Now, objSchoolInformation.IsActive);
                TempData["Message"] = "School Added Successfully.";
            }
            else
            {
                DbContext.Sp_DC_SchoolInfo(objSchoolInformation.SchoolId, objSchoolInformation.SchoolName, objSchoolInformation.Information, objSchoolInformation.Logo, objSchoolInformation.DocumentaryVideo, objSchoolInformation.ThumbnailPath, DateTime.Now, DateTime.Now, objSchoolInformation.IsActive);
                TempData["Message"] = "School Updated Successfully.";
            }
            return RedirectToAction("GetSchoolList", "School");
        }

        #endregion

        #region SchoolAdminDetail
        public ActionResult GetSchoolAdminList()
        {
            SchoolModel.SchoolAdminOrPrincipleModel objOutput = new SchoolModel.SchoolAdminOrPrincipleModel();
            List<SchoolModel.SchoolAdminOrPrincipleModel> objList = new List<SchoolModel.SchoolAdminOrPrincipleModel>();


            try
            {
                Guid schoolId = new Guid(Session["id"].ToString());
                var schoolAdminList = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "SchoolAdmin" && x.IsActive == true && x.SchoolId == schoolId).OrderByDescending(x => x.CreatedDate).ToList();
                if (schoolAdminList.Any())
                {
                    foreach (var item in schoolAdminList)
                    {
                        SchoolModel.SchoolAdminOrPrincipleModel objSchoolInformation = new SchoolModel.SchoolAdminOrPrincipleModel();
                        objSchoolInformation.Id = item.UserId;
                        objSchoolInformation.SchoolId = (Guid)item.SchoolId;
                        objSchoolInformation.FirstName = item.UserFirstname;
                        objSchoolInformation.LastName = item.UserLastname;
                        objSchoolInformation.IsActive = (bool)item.IsActive;
                        objSchoolInformation.Image = item.UserProfilePhoto;
                        //objSchoolInformation.ph = item.UserPhoneNumber;
                        objSchoolInformation.EmailAddress = item.UserEmailAddress;
                        objSchoolInformation.Password = item.UserPassword;
                        objList.Add(objSchoolInformation);


                    }
                }
                objOutput.schoolInformation = objList;
            }

            catch (Exception ex)
            {

            }
            return View(objOutput.schoolInformation);
        }

        /// <summary>
        /// Create admin profi;le
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult AddSchoolAdmin(Guid? Id)
        {
            SchoolModel.SchoolAdminOrPrincipleModel objSchoolAdminOrPrincipleModel = new SchoolModel.SchoolAdminOrPrincipleModel();
            if (Id != null && Id != Guid.Empty)
            {
                var admin = DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == Id).SingleOrDefault();
                objSchoolAdminOrPrincipleModel.Id = admin.UserId;
                objSchoolAdminOrPrincipleModel.Password = admin.UserPassword;
                objSchoolAdminOrPrincipleModel.EmailAddress = admin.UserEmailAddress;
                objSchoolAdminOrPrincipleModel.FirstName = admin.UserFirstname;
                objSchoolAdminOrPrincipleModel.Image = admin.UserProfilePhoto;
                objSchoolAdminOrPrincipleModel.LastName = admin.UserLastname;
                objSchoolAdminOrPrincipleModel.SchoolId = (Guid)admin.SchoolId;
            }
            ViewBag.School_Id = new SelectList(DbContext.tbl_DC_School_Info.Where(x => x.IsActive == true), "SchoolId", "SchoolName");

            return View(objSchoolAdminOrPrincipleModel);
        }

        [HttpPost]
        public ActionResult AddSchoolAdmin(SchoolModel.SchoolAdminOrPrincipleModel objSchoolAdminOrPrincipleModel)//CreatOrEditSchoolAdmin
        {
            try
            {
                objSchoolAdminOrPrincipleModel.SchoolId = new Guid(Session["id"].ToString());
                if (objSchoolAdminOrPrincipleModel.Id == Guid.Empty || objSchoolAdminOrPrincipleModel.Id == null)
                {
                    objSchoolAdminOrPrincipleModel.Id = Guid.NewGuid();
                    objSchoolAdminOrPrincipleModel.IsActive = true;

                    #region UploadFile
                    FileHelper fileHelper = new FileHelper();
                    string browserType = Request.Browser.Browser.ToUpper();
                    UploadFileDetailModel uploadFileDetailModel = fileHelper.UploadDoc(Request.Files, "School", objSchoolAdminOrPrincipleModel.SchoolId.ToString(), "Admin", browserType);
                    objSchoolAdminOrPrincipleModel.Image = !string.IsNullOrEmpty(uploadFileDetailModel.ImageName) ? uploadFileDetailModel.ImagePath + "/" + uploadFileDetailModel.ImageName : string.Empty;
                    TempData["Message"] = "School Admin Added Successfully.";

                    #endregion
                    TempData["Message"] = "School Admin Added Successfully.";
                }
                else
                {
                    objSchoolAdminOrPrincipleModel.IsActive = true;
                    TempData["Message"] = "School Admin Updated Successfully.";
                }
                int result = 0;
                result = DbContext.SP_DC_SchoolUser(objSchoolAdminOrPrincipleModel.Id, objSchoolAdminOrPrincipleModel.SchoolId, objSchoolAdminOrPrincipleModel.FirstName, objSchoolAdminOrPrincipleModel.LastName, objSchoolAdminOrPrincipleModel.EmailAddress, objSchoolAdminOrPrincipleModel.EmailAddress, objSchoolAdminOrPrincipleModel.Password, objSchoolAdminOrPrincipleModel.Image, "SchoolAdmin", "", DateTime.Now, DateTime.Now, objSchoolAdminOrPrincipleModel.IsActive);

                //---------------Mail Send functionality------------
                if (result > 0)
                {
                    string emailMsg = "Hi, this is to inform you your school admin credential are Username " + objSchoolAdminOrPrincipleModel.EmailAddress + " password " + objSchoolAdminOrPrincipleModel.Password + ".";
                    EmailHelper.SendEmail(objSchoolAdminOrPrincipleModel.EmailAddress, "Admin Credentials", emailMsg);
                }
                //Mail Send functionality
            }
            catch (Exception EX)
            {

            }
            return new JsonResult()
            {
                Data = true,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        #endregion

        #region TeacherDetail

        public ActionResult GetSchoolTeacherList()
        {
            SchoolModel.SchoolAdminOrPrincipleModel objOutput = new SchoolModel.SchoolAdminOrPrincipleModel();
            List<SchoolModel.SchoolAdminOrPrincipleModel> objList = new List<SchoolModel.SchoolAdminOrPrincipleModel>();


            try
            {
                Guid schoolId = new Guid(Session["id"].ToString());
                var teacherList = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "Teacher" && x.IsActive == true && x.SchoolId == schoolId).OrderByDescending(x => x.CreatedDate).ToList();
                if (teacherList.Any())
                {
                    foreach (var item in teacherList)
                    {
                        SchoolModel.SchoolAdminOrPrincipleModel objSchoolInformation = new SchoolModel.SchoolAdminOrPrincipleModel();
                        objSchoolInformation.Id = item.UserId;
                        objSchoolInformation.SchoolId = (Guid)item.SchoolId;
                        objSchoolInformation.FirstName = item.UserFirstname;
                        objSchoolInformation.LastName = item.UserLastname;
                        objSchoolInformation.IsActive = (bool)item.IsActive;
                        objSchoolInformation.Image = item.UserProfilePhoto;
                        //objSchoolInformation.ph = item.UserPhoneNumber;
                        objSchoolInformation.EmailAddress = item.UserEmailAddress;
                        objSchoolInformation.Password = item.UserPassword;
                        objList.Add(objSchoolInformation);
                    }
                }
                objOutput.schoolInformation = objList;
            }

            catch (Exception ex)
            {

            }
            return View(objOutput.schoolInformation);
        }
        /// <summary>
        /// Create teacher profi;le
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// 
        public ActionResult AddSchoolTeacher(Guid? Id)
        {

            SchoolModel.SchoolAdminOrPrincipleModel objSchoolAdminOrPrincipleModel = new SchoolModel.SchoolAdminOrPrincipleModel();
            if (Id != null && Id != Guid.Empty)
            {
                var teacher = DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == Id).SingleOrDefault();
                objSchoolAdminOrPrincipleModel.Id = teacher.UserId;
                objSchoolAdminOrPrincipleModel.Password = teacher.UserPassword;
                objSchoolAdminOrPrincipleModel.EmailAddress = teacher.UserEmailAddress;
                objSchoolAdminOrPrincipleModel.FirstName = teacher.UserFirstname;
                objSchoolAdminOrPrincipleModel.Image = teacher.UserProfilePhoto;
                objSchoolAdminOrPrincipleModel.LastName = teacher.UserLastname;
                objSchoolAdminOrPrincipleModel.SchoolId = (Guid)teacher.SchoolId;
            }
            return View(objSchoolAdminOrPrincipleModel);

        }


        //  return
        [HttpPost]
        public ActionResult AddSchoolTeacher(SchoolModel.SchoolAdminOrPrincipleModel objSchoolTeacherModel)//CreatOrEditTeacherProfile
        {
            try
            {
                objSchoolTeacherModel.SchoolId = new Guid(Session["id"].ToString());
                if (objSchoolTeacherModel.Id == Guid.Empty || objSchoolTeacherModel.Id == null)
                {
                    objSchoolTeacherModel.IsActive = true;
                    objSchoolTeacherModel.Id = Guid.NewGuid();
                    TempData["Message"] = "School Teacher Added Successfully.";
                }
                else
                {
                    objSchoolTeacherModel.IsActive = true;
                    TempData["Message"] = "School Teacher Updated Successfully.";
                }
                int result = DbContext.SP_DC_SchoolUser(objSchoolTeacherModel.Id, objSchoolTeacherModel.SchoolId, objSchoolTeacherModel.FirstName, objSchoolTeacherModel.LastName, objSchoolTeacherModel.EmailAddress, "", objSchoolTeacherModel.Password, objSchoolTeacherModel.Image, "Teacher", "", DateTime.Now, DateTime.Now, objSchoolTeacherModel.IsActive);

                //---------------Mail Send functionality------------
                if (result > 0)
                {
                    string emailMsg = "Hi, this is to inform you your school admin credential are Username " + objSchoolTeacherModel.EmailAddress + " password " + objSchoolTeacherModel.Password + ".";
                    EmailHelper.SendEmail(objSchoolTeacherModel.EmailAddress, "Admin Credentials", emailMsg);
                }
                //Mail Send functionality
            }
            catch (Exception EX)
            {

            }
            return RedirectToAction("GetSchoolTeacherList", "School");
        }
        #endregion

        #region PrincipalDetail
        public ActionResult GetSchoolPrincipleList()
        {
            SchoolModel.SchoolAdminOrPrincipleModel objOutput = new SchoolModel.SchoolAdminOrPrincipleModel();
            List<SchoolModel.SchoolAdminOrPrincipleModel> objList = new List<SchoolModel.SchoolAdminOrPrincipleModel>();

            try
            {
                Guid schoolId = new Guid(Session["id"].ToString());
                var schoolList = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "Principle" && x.IsActive == true && x.SchoolId == schoolId).ToList();
                if (schoolList.Any())
                {
                    foreach (var item in schoolList)
                    {
                        SchoolModel.SchoolAdminOrPrincipleModel objSchoolInformation = new SchoolModel.SchoolAdminOrPrincipleModel();
                        objSchoolInformation.Id = item.UserId;
                        objSchoolInformation.SchoolId = (Guid)item.SchoolId;
                        objSchoolInformation.FirstName = item.UserFirstname;
                        objSchoolInformation.LastName = item.UserLastname;
                        objSchoolInformation.IsActive = (bool)item.IsActive;
                        objSchoolInformation.Image = item.UserProfilePhoto;
                        //objSchoolInformation.ph = item.UserPhoneNumber;
                        objSchoolInformation.EmailAddress = item.UserEmailAddress;
                        objSchoolInformation.Password = item.UserPassword;
                        objList.Add(objSchoolInformation);
                    }
                    objOutput.schoolInformation = objList;
                }
            }

            catch (Exception ex)
            {

            }
            return View(objOutput.schoolInformation);
        }
        /// <summary>
        /// Create principle profi;le
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult AddSchoolPrincipal(Guid? Id)//CreateOrEditPrincipleProfile
        {
            SchoolModel.SchoolAdminOrPrincipleModel objSchoolAdminOrPrincipleModel = new SchoolModel.SchoolAdminOrPrincipleModel();
            if (Id != null && Id != Guid.Empty)
            {
                var principle = DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == Id).SingleOrDefault();
                objSchoolAdminOrPrincipleModel.Id = principle.UserId;
                objSchoolAdminOrPrincipleModel.Password = principle.UserPassword;
                objSchoolAdminOrPrincipleModel.EmailAddress = principle.UserEmailAddress;
                objSchoolAdminOrPrincipleModel.FirstName = principle.UserFirstname;
                objSchoolAdminOrPrincipleModel.Image = principle.UserProfilePhoto;
                objSchoolAdminOrPrincipleModel.LastName = principle.UserLastname;
                objSchoolAdminOrPrincipleModel.SchoolId = (Guid)principle.SchoolId;
            }
            return View(objSchoolAdminOrPrincipleModel);
        }
        /// <summary>
        /// //Insert data
        /// </summary>
        /// <param name="objSchoolAdminOrPrincipleModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSchoolPrincipal(SchoolModel.SchoolAdminOrPrincipleModel objSchoolPrincipalModel)
        {
            try
            {

                objSchoolPrincipalModel.SchoolId = new Guid(Session["id"].ToString());
                objSchoolPrincipalModel.Image = "";
                if (objSchoolPrincipalModel.Id == Guid.Empty || objSchoolPrincipalModel.Id == null)
                {
                    objSchoolPrincipalModel.IsActive = true;
                    objSchoolPrincipalModel.Id = Guid.NewGuid();
                    TempData["Message"] = "School Prinipal Added Successfully.";
                }
                else
                {
                    objSchoolPrincipalModel.IsActive = true;
                    TempData["Message"] = "School Principal Updated Successfully.";
                }
                int result = 0;
                result = DbContext.SP_DC_SchoolUser(objSchoolPrincipalModel.Id, objSchoolPrincipalModel.SchoolId, objSchoolPrincipalModel.FirstName, objSchoolPrincipalModel.LastName, objSchoolPrincipalModel.EmailAddress, "", objSchoolPrincipalModel.Password, objSchoolPrincipalModel.Image, "Principle", "", DateTime.Now, DateTime.Now, objSchoolPrincipalModel.IsActive);
                //---------------Mail Send functionality------------
                if (result > 0)
                {
                    string emailMsg = "Hi, this is to inform you your school admin credential are Username " + objSchoolPrincipalModel.EmailAddress + " password " + objSchoolPrincipalModel.Password + ".";
                    EmailHelper.SendEmail(objSchoolPrincipalModel.EmailAddress, "Admin Credentials", emailMsg);
                }
                //Mail Send functionality
            }
            catch
            {

            }

            return RedirectToAction("GetSchoolPrincipleList", "School");
        }
        #endregion



        /// <summary>SubjectId
        /// Create home work form
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult CreateOrEditHomeWork(string Id = "")
        {
            SchoolModel.HomeWorkModel objHomeWorkModel = new SchoolModel.HomeWorkModel();
            if (string.IsNullOrEmpty(Id))
            {

            }
            ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_School_Class.Where(x => x.IsActive == true), "ClassId", "ClassName");
            ViewBag.Section_Id = new SelectList(DbContext.tbl_DC_Class_Section.Where(x => x.IsActive == true), "SectionId", "SectionName");
            ViewBag.Subject_Id = new SelectList(DbContext.tbl_DC_School_Subject.Where(x => x.IsActive == true), "SubjectId", "SubjectName");
            return View(objHomeWorkModel);
        }
        [HttpPost]

        public ActionResult CreateOrEditHomeWork(SchoolModel.HomeWorkModel objHomeWorkModel)
        {
            try
            {
                tbl_DC_School_Homework obj = new tbl_DC_School_Homework();
                obj.HomeworkId = Guid.NewGuid();
                obj.SchoolId = new Guid(Session["id"].ToString());
                obj.HomeworkDetail = objHomeWorkModel.Description;
                obj.IsActive = true;
                obj.SectionId = objHomeWorkModel.SectionId;
                obj.CreatedDate = objHomeWorkModel.Date;
                obj.TimeSlot = objHomeWorkModel.TimeSlot;
                obj.SubjectId = objHomeWorkModel.SubjectId;
                obj.ClassId = objHomeWorkModel.ClassId;
                DbContext.tbl_DC_School_Homework.Add(obj);
                var id = DbContext.SaveChanges();

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("GetHomeWorkList", "School");
        }
        /// <summary>
        /// Create exam form
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult CreateOrEditCreateExam(Guid? Id)
        {
            SchoolModel.CreateExamModel objCreateExamModel = new SchoolModel.CreateExamModel();
            if (Id != null && Id != Guid.Empty)
            {
                var exam = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.ExamScheduleId == Id).SingleOrDefault();
                {
                    objCreateExamModel.Id = exam.ExamScheduleId;
                    objCreateExamModel.ClassId = (Guid)exam.ClassId;
                    if (exam.DateOfExam != null) ;
                    objCreateExamModel.StartDate = (DateTime)exam.DateOfExam;
                    objCreateExamModel.SubjectId = (Guid)exam.SubjectId;
                    objCreateExamModel.ExamType = (Guid)exam.ExamTypeId;


                }
            }
            ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_School_Class.Where(x => x.IsActive == true), "ClassId", "ClassName");
            ViewBag.ExamType_Id = new SelectList(DbContext.tbl_DC_School_ExamType.Where(x => x.IsActive == true), "ExamTypeId", "ExamTypeName");
            ViewBag.Subject_Id = new SelectList(DbContext.tbl_DC_School_Subject.Where(x => x.IsActive == true), "SubjectId", "SubjectName");

            return View(objCreateExamModel);
        }
        [HttpPost]
        public ActionResult CreateOrEditCreateExam(SchoolModel.CreateExamModel objCreateExamModel)
        {
            try
            {
                if (objCreateExamModel.Id != null && objCreateExamModel.Id != Guid.Empty)
                {
                    var exam = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.ExamScheduleId == objCreateExamModel.Id).SingleOrDefault();
                    {
                        exam.ExamScheduleId = objCreateExamModel.Id;
                        exam.ClassId = objCreateExamModel.ClassId;
                        exam.DateOfExam = objCreateExamModel.StartDate;
                        exam.SubjectId = objCreateExamModel.SubjectId;
                        exam.ExamTypeId = objCreateExamModel.ExamType;

                        DbContext.SaveChanges();
                    }
                }
                else
                {
                    tbl_DC_School_ExamSchedule objexam = new tbl_DC_School_ExamSchedule();
                    //school objam= new objam();

                    objexam.ClassId = objCreateExamModel.ClassId;
                    //  objexam.se = objCreateExamModel.SectionId;
                    objexam.ExamScheduleId = objCreateExamModel.Id;
                    objexam.IsActive = true;
                    objexam.DateOfExam = DateTime.Now;
                    objexam.TimeSlot = "";
                    objexam.TotalMarks = objCreateExamModel.TotalMarks.ToString(); ;
                    objexam.SubjectId = objCreateExamModel.SubjectId;
                    objexam.SchoolId = new Guid(Session["id"].ToString());
                    objexam.ExamTypeId = objexam.ExamTypeId;
                    objexam.CreatedDate = DateTime.Now;
                    DbContext.tbl_DC_School_ExamSchedule.Add(objexam);
                    var id = DbContext.SaveChanges();
                }



            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("GetExam", "School");
        }
        /// <summary>
        /// Create Message Creation
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult CreateOrEditMessageCreation(string Id = "")
        {
            SchoolModel.MessageCreation objMessageCreation = new SchoolModel.MessageCreation();
            if (string.IsNullOrEmpty(Id))
            {

            }
            ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_School_Class.Where(x => x.IsActive == true), "ClassId", "ClassName");
            ViewBag.Section_Id = new SelectList(DbContext.tbl_DC_Class_Section.Where(x => x.IsActive == true), "SectionId", "SectionName");
            return View(objMessageCreation);
        }

        [HttpGet]
        public JsonResult IsEmailExist(string eMail)
        {

            // bool isExist = DbContext.tbl_DC_SchoolUser.Where(u => u.UserEmailAddress.ToLowerInvariant().Equals(eMail.ToLower()))).FirstOrDefault() != null;
            bool isExist = DbContext.tbl_DC_SchoolUser.Where(x => x.UserEmailAddress.Equals(eMail) && x.IsActive == true).FirstOrDefault() != null; ; ;

            return Json(!isExist, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateOrEditMessageCreation(SchoolModel.MessageCreation objMessageCreation)
        {
            try
            {
                tbl_DC_School_MessageCreation objmsg = new tbl_DC_School_MessageCreation();
                objmsg.MessageId = Guid.NewGuid();
                objmsg.SchoolId = new Guid(Session["id"].ToString());
                objmsg.MassageDisplay = objMessageCreation.Message;
                objmsg.IsActive = true;
                objmsg.MassageDisplayDate = DateTime.Now;
                objmsg.CreatedDate = DateTime.Now;
                DbContext.tbl_DC_School_MessageCreation.Add(objmsg);
                var id = DbContext.SaveChanges();

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("GetMessageList", "School");
        }
        /// <summary>
        /// Insert Exam type
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult ExamType(Guid? Id)
        {
            SchoolModel.ExamType objExamType = new SchoolModel.ExamType();

            if (Id != null && Id != Guid.Empty)
            {
                var examType = DbContext.tbl_DC_School_ExamType.Where(x => x.ExamTypeId == Id).SingleOrDefault();
                if (examType != null)
                {
                    objExamType.ExamTypeId = examType.ExamTypeId;
                    objExamType.ExamTypenname = examType.ExamTypeName;
                }
            }
            return View(objExamType);
        }
        [HttpPost]

        public ActionResult ExamType(SchoolModel.ExamType objExamType)
        {
            try
            {
                if (objExamType.ExamTypeId != null && objExamType.ExamTypeId != Guid.Empty)
                {
                    var examType = DbContext.tbl_DC_School_ExamType.Where(x => x.ExamTypeId == objExamType.ExamTypeId).SingleOrDefault();
                    if (examType != null)
                    {
                        examType.ExamTypeId = objExamType.ExamTypeId;
                        examType.ExamTypeName = objExamType.ExamTypenname;

                        DbContext.SaveChanges();
                    }
                }
                else
                {
                    tbl_DC_School_ExamType objExmam = new tbl_DC_School_ExamType();
                    objExmam.ExamTypeName = objExamType.ExamTypenname;
                    objExmam.IsActive = true;
                    objExmam.ExamTypeId = Guid.NewGuid();
                    objExmam.SchoolId = new Guid(Session["id"].ToString());

                    DbContext.tbl_DC_School_ExamType.Add(objExmam);
                    DbContext.SaveChanges();
                }

                //  DbContext();
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("GetExamType", "School");
        }


        /// <summary>
        /// Create study material
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult CreateOrEditStudyMaterial(string Id = "")
        {
            SchoolModel.StudyMaterialModel objStudyMaterialModel = new SchoolModel.StudyMaterialModel();
            if (string.IsNullOrEmpty(Id))
            {

            }
            ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_School_Class.Where(x => x.IsActive == true), "ClassId", "ClassName");
            ViewBag.Subject_Id = new SelectList(DbContext.tbl_DC_School_Subject.Where(x => x.IsActive == true), "SubjectId", "SubjectName");

            return View(objStudyMaterialModel);
        }


        [HttpPost]
        public ActionResult CreateOrEditStudyMaterial(SchoolModel.StudyMaterialModel objStudyMaterialModel)
        {

            try
            {
                HttpFileCollectionBase files = Request.Files;
                objStudyMaterialModel.IsActive = true;
                if (objStudyMaterialModel.Id == Guid.Empty || objStudyMaterialModel.Id == null)
                {
                    objStudyMaterialModel.Id = Guid.NewGuid();
                }
                string fname = null;

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];

                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }




                    string path = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + "School\\StudyMaterial";
                    string subPath = "~/Upload/School/StudyMaterial/";
                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);
                    // objimage.Add(Path.Combine("/Upload/" + "Product/" + productId + "/" + fname));

                    file.SaveAs(Path.Combine(path, fname));
                    objStudyMaterialModel.Image = subPath + fname;

                    //Stream strm = file.InputStream;
                }

                //HttpFileCollectionBase files = Request.Files;
                //HttpPostedFileBase file = files;

                //string paths = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + "School\\StudyMaterial\\";
                //string subPaths = "~/Upload/School/StudyMaterial/";
                //bool s = System.IO.Directory.Exists(paths);

                //if (!s)
                //    System.IO.Directory.CreateDirectory(paths);
                //// objimage.Add(Path.Combine("/Upload/" + "Product/" + productId + "/" + fname));

                //file.SaveAs(Path.Combine(paths, objStudyMaterialModel.File));
                //objStudyMaterialModel.File = subPaths + objStudyMaterialModel.File;
                tbl_DC_School_StudyMaterial objstudy = new tbl_DC_School_StudyMaterial();
                objstudy.SchoolId = new Guid(Session["id"].ToString());
                objstudy.StudyMaterialId = Guid.NewGuid();
                objstudy.IsActive = true;
                objstudy.Topic = objStudyMaterialModel.Topic;
                objstudy.SubjectId = objStudyMaterialModel.SubjectId;
                objstudy.FilePath = objStudyMaterialModel.Image;
                string p = objStudyMaterialModel.Image;
                string e = Path.GetExtension(p);
                objstudy.FileType = e;
                objstudy.ClassId = objStudyMaterialModel.ClassId;
                objstudy.CreatedDate = DateTime.Now;
                DbContext.tbl_DC_School_StudyMaterial.Add(objstudy);
                DbContext.SaveChanges();
                //objStudyMaterialModel.SchoolId = new Guid(Session["id"].ToString());

                //CreateThumbnail(strm, fname);
                // objSchoolInformation.ThumbnailPath;
            }
            catch (Exception ex)
            {

            }

            return new JsonResult()
            {
                Data = true,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        public ActionResult CreateOrEditToppersWay(string Id = "")
        {
            SchoolModel.ToppersWayModel objToppersWayModel = new SchoolModel.ToppersWayModel();
            if (string.IsNullOrEmpty(Id))
            {

            }
            ViewBag.Class_Id = new SelectList(DbContext.tbl_DC_School_Class.Where(x => x.IsActive == true), "ClassId", "ClassName");

            return View(objToppersWayModel);
        }



        [HttpPost]
        public ActionResult CreateOrEditToppersWay(SchoolModel.ToppersWayModel objToppersWayModel)
        {
            try
            {
                tbl_DC_ToppersWay obj = new tbl_DC_ToppersWay();

                HttpFileCollectionBase files = Request.Files;
                obj.IsActive = true;
                if (obj.ToppersId == Guid.Empty || obj.ToppersId == null)
                {
                    obj.ToppersId = Guid.NewGuid();
                }
                string fname = null;

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];

                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }




                    string path = System.Web.HttpContext.Current.Server.MapPath("~/Upload/") + "School\\ToppersWay";
                    string subPath = "~/Upload/School/ToppersWay/";
                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);
                    // objimage.Add(Path.Combine("/Upload/" + "Product/" + productId + "/" + fname));

                    file.SaveAs(Path.Combine(path, fname));
                    // obj. = subPath + fname;
                    obj.FileURL = subPath + fname;
                    //Stream strm = file.InputStream;
                }


                obj.ToppersId = Guid.NewGuid();
                obj.SchoolId = new Guid(Session["id"].ToString());
                obj.ClassId = objToppersWayModel.ClassId;
                string p = objToppersWayModel.path;
                string e = Path.GetExtension(p);
                obj.FileType = e;

                DbContext.tbl_DC_ToppersWay.Add(obj);
                var id = DbContext.SaveChanges();

            }
            catch
            {

            }
            return new JsonResult()
            {
                Data = true,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult CreateOrEditSubject(Guid? Id)
        {
            SchoolModel.CreateSubject objCreateSubject = new SchoolModel.CreateSubject();
            if (Id != null && Id != Guid.Empty)
            {
                var subject = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == Id).SingleOrDefault();
                if (subject != null)
                {
                    objCreateSubject.SubjectId = subject.SubjectId;
                    objCreateSubject.SubjectName = subject.SubjectName;
                }
            }
            return View(objCreateSubject);
        }
        [HttpPost]
        public ActionResult CreateOrEditSubject(SchoolModel.CreateSubject input)
        {
            try
            {
                if (input.SubjectId != null && input.SubjectId != Guid.Empty)
                {
                    var subject = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == input.SubjectId).SingleOrDefault();
                    if (subject != null)
                    {
                        subject.SubjectId = input.SubjectId;
                        subject.SubjectName = input.SubjectName;
                        DbContext.SaveChanges();
                    }
                }
                else
                {

                    input.Id = Guid.NewGuid();
                    input.SchoolId = new Guid(Session["id"].ToString());
                    tbl_DC_School_Subject objtbl_DC_School_Subject = new tbl_DC_School_Subject();
                    objtbl_DC_School_Subject.SubjectId = input.Id;
                    objtbl_DC_School_Subject.SchoolId = input.SchoolId;
                    objtbl_DC_School_Subject.SubjectName = input.SubjectName;
                    objtbl_DC_School_Subject.CreatedDate = DateTime.Now;
                    objtbl_DC_School_Subject.IsActive = true;
                    var status = false;

                    DbContext.tbl_DC_School_Subject.Add(objtbl_DC_School_Subject);
                    var id = DbContext.SaveChanges();
                    if (id != null)
                    {
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("GetSubjectList", "School");

        }
        public ActionResult CreateOrEditClass(Guid? Id)
        {
            SchoolModel.CreateClass objCreateClass = new SchoolModel.CreateClass();

            var SectionList = Enum.GetValues(typeof(EnumValue.Section))
                                      .Cast<EnumValue.Section>()
                                      .ToList();
            List<SelectListItem> objCIFFOB = new List<SelectListItem>();
            foreach (var item in SectionList)
            {


                objCIFFOB.Add(new SelectListItem { Text = item.ToString(), Value = item.ToString() });


            }

            //var ClassList = Enum.GetValues(typeof(EnumValue.SchoolClass))
            //                        .Cast<EnumValue.SchoolClass>()
            //                        .ToList();
            List<SelectListItem> objclassList = new List<SelectListItem>();
            //foreach (var classItem in ClassList)
            //{

            //    string name = Enum.GetName(typeof(EnumValue.SchoolClass), classItem);
            //    objclassList.Add(new SelectListItem { Text = name, Value = name });


            //}
            for (int schoolClass = 1; schoolClass <= 12; schoolClass++)
            {

                // string name = Enum.GetName(typeof(EnumValue.SchoolClass), classItem);
                objclassList.Add(new SelectListItem { Text = schoolClass.ToString(), Value = schoolClass.ToString() });


            }

            objCreateClass.Section = objCIFFOB;
            objCreateClass.Class = objclassList;
            //  ViewBag.Accounts=
            if (Id != Guid.Empty && Id != null)
            {
                var schoolClass = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == Id).SingleOrDefault();

                objCreateClass.Id = schoolClass.ClassId;
                objCreateClass.ClassName = schoolClass.ClassName;
                var sectionName = DbContext.tbl_DC_Class_Section.Where(x => x.ClassId == Id).ToList();

                foreach (var section in sectionName)
                {
                    objCreateClass.SectionName += section.SectionName + ",";
                }
                objCreateClass.SectionName = objCreateClass.SectionName.TrimEnd(',');

            }
            return View(objCreateClass);
        }
        public ActionResult AddClass(SchoolModel.CreateClass input)
        {
            var status = false;
            if (input.Id != null && input.Id != Guid.Empty)
            {
                var classDetail = DbContext.tbl_DC_School_Class.Where(x => x.ClassId == input.Id).SingleOrDefault();
                classDetail.ClassName = input.ClassName;
                var sections = input.SectionName.Split(',');
                DbContext.SaveChanges();
                if (classDetail != null)
                {
                    if (sections != null && sections.Length > 0)
                    {
                        for (int sec = 0; sec < sections.Length; sec++)
                        {
                            var sectionName = sections[sec].ToString();
                            var classSection = DbContext.tbl_DC_Class_Section.Where(x => x.ClassId == classDetail.ClassId && x.SectionName == sectionName).SingleOrDefault();
                            if (classSection == null)
                            {
                                tbl_DC_Class_Section objtbl_DC_Class_Section = new tbl_DC_Class_Section();
                                var sectionId = Guid.NewGuid();
                                objtbl_DC_Class_Section.ClassId = input.Id;
                                objtbl_DC_Class_Section.CreatedDate = DateTime.Now;
                                objtbl_DC_Class_Section.IsActive = true;
                                objtbl_DC_Class_Section.SectionId = sectionId;
                                objtbl_DC_Class_Section.SectionName = Convert.ToString(sections[sec]);
                                DbContext.tbl_DC_Class_Section.Add(objtbl_DC_Class_Section);
                                var id = DbContext.SaveChanges();
                            }
                            else
                            {
                                if (classSection.IsActive == false)
                                {
                                    classSection.IsActive = true;
                                    DbContext.SaveChanges();
                                }
                            }
                        }

                        var classSectionDlt = DbContext.tbl_DC_Class_Section.Where(x => x.ClassId == classDetail.ClassId && x.IsActive == true).ToList();
                        if (classSectionDlt != null && classSectionDlt.Count > 0)
                        {
                            foreach (var item in classSectionDlt)
                            {
                                if (!input.SectionName.Contains(item.SectionName))
                                {
                                    item.IsActive = false;
                                    DbContext.SaveChanges();
                                }
                            }
                        }

                    }

                    //var class
                    //foreach(var item in classSection)
                    //{
                    //     var classSectiondlt = DbContext.tbl_DC_Class_Section.Where(x => x.SectionId == item.SectionId).SingleOrDefault();
                    //     //DbContext.tbl_DC_Class_Section.Remove(classSectiondlt);
                    //     //DbContext.SaveChanges();

                    //   if(classSectiondlt!=null)
                    //   {

                    //   }

                    //}


                }
            }
            else
            {

                input.Id = Guid.NewGuid();
                tbl_DC_School_Class objtbl_DC_School_Class = new tbl_DC_School_Class();
                objtbl_DC_School_Class.ClassId = input.Id;
                objtbl_DC_School_Class.ClassName = input.ClassName;
                objtbl_DC_School_Class.CreatedDate = DateTime.Now;
                objtbl_DC_School_Class.IsActive = true;
                objtbl_DC_School_Class.SchoolId = new Guid("CE83C8BA-FAD3-4ED7-AD98-36BB7D08D497");
                DbContext.tbl_DC_School_Class.Add(objtbl_DC_School_Class);

                var classId = DbContext.SaveChanges();
                if (classId != null)
                {


                    InsertSection(input);

                }


            }
            return new JsonResult()
            {
                Data = status,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //return View();
        }


        public bool InsertSection(SchoolModel.CreateClass input)
        {
            var status = false;
            var sections = input.SectionName.Split(',');
            if (sections != null && sections.Length > 0)
            {
                for (int sec = 0; sec < sections.Length; sec++)
                {
                    tbl_DC_Class_Section objtbl_DC_Class_Section = new tbl_DC_Class_Section();
                    var sectionId = Guid.NewGuid();
                    objtbl_DC_Class_Section.ClassId = input.Id;
                    objtbl_DC_Class_Section.CreatedDate = DateTime.Now;
                    objtbl_DC_Class_Section.IsActive = true;
                    objtbl_DC_Class_Section.SectionId = sectionId;
                    objtbl_DC_Class_Section.SectionName = Convert.ToString(sections[sec]);
                    DbContext.tbl_DC_Class_Section.Add(objtbl_DC_Class_Section);
                    var id = DbContext.SaveChanges();
                    if (id != null)
                    {
                        status = true;
                    }
                }
            }
            else
            {

            }
            return status;
        }
        /// <summary>
        /// ///Insert class
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]

        public ActionResult CreateOrEditClass(SchoolModel.CreateClass objCreateClass)
        {
            try
            {
                tbl_DC_School_Class objClass = new tbl_DC_School_Class();
                if (objCreateClass.Id == Guid.Empty || objCreateClass.Id == null)
                {
                    objCreateClass.Id = Guid.NewGuid();

                }

                objClass.ClassId = objCreateClass.Id;
                objClass.ClassName = objCreateClass.ClassName;
                objClass.IsActive = true;
                objClass.CreatedDate = DateTime.Now;
                objClass.SchoolId = new Guid(Session["id"].ToString());
                DbContext.tbl_DC_School_Class.Add(objClass);

            }

            catch
            {

            }
            return RedirectToAction("GetClassList", "School");
        }

        /// <summary>
        /// // Get List of all school for master admin
        /// </summary>
        /// <returns></returns>
        public ActionResult SchoolDashBoard(Guid id)
        {
            try
            {
                Session["id"] = id;
                ViewBag.SchoolName = DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == id).Select(x => x.SchoolName).FirstOrDefault();
                ViewBag.Description = DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == id).Select(x => x.SchoolDescription).FirstOrDefault();
                ViewBag.Logo = DbContext.tbl_DC_School_Info.Where(x => x.SchoolId == id).Select(x => x.SchoolLogo).FirstOrDefault();
                ViewBag.TeacherCount = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "Teacher").Count();
                // ViewBag.SchoolAdmin = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "SchoolAdmin").Count();
                // ViewBag.TeacherCount = DbContext.tbl_DC_SchoolUser.Where(x => x.UserRole == "Teacher").Count();
            }
            catch (Exception ex)
            {

            }
            return View();

        }

        #region GetListingrid







        public ActionResult DeleteUserList(Guid id)
        {

            var data = "";
            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_SchoolUser.Where(x => x.UserId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();
                data = result.UserRole;

            }

            catch (Exception ex)
            {



            }
            if (data == "SchoolAdmin")
                return RedirectToAction("GetSchoolAdminList", "School");
            if (data == "Teacher")
                return RedirectToAction("GetSchoolTeacherList", "School");
            else
                return RedirectToAction("GetSchoolPrincipleList", "School");
        }

        public ActionResult GetHomeWorkList()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.HomeWorkModel> objOutput = new List<SchoolModel.HomeWorkModel>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetHomeWorkList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }
        public ActionResult GetStudyMterial()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.StudyMaterialModel> objOutput = new List<SchoolModel.StudyMaterialModel>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetStudyMaterialList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }

        public ActionResult GetToppersWay()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.ToppersWayModel> objOutput = new List<SchoolModel.ToppersWayModel>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetToppersWay(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }

        public ActionResult GetClassList()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.CreateClass> objOutput = new List<SchoolModel.CreateClass>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetClassList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }
        public ActionResult GetMessageList()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.MessageCreation> objOutput = new List<SchoolModel.MessageCreation>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetMessageList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }

        public ActionResult GetSubjectList()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.CreateSubject> objOutput = new List<SchoolModel.CreateSubject>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetSubjectList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }

        public ActionResult GetExamType()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.ExamTypeModel> objOutput = new List<SchoolModel.ExamTypeModel>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetExamTypeList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }
        /// <summary>
        /// get the exam list
        /// </summary>
        /// <returns></returns>
        public ActionResult GetExam()
        {
            SchoolAPIController obj = new SchoolAPIController();
            List<SchoolModel.CreateExamModel> objOutput = new List<SchoolModel.CreateExamModel>();
            SchoolModel.InputModel objInput = new SchoolModel.InputModel();
            objInput.SchoolId = new Guid(Session["id"].ToString());
            objOutput = obj.GetExamListList(objInput);
            //  objOutput.HomeWork = objOutput;

            return View(objOutput);
        }


        #endregion
        public ActionResult DeleteHomeWork(Guid id)
        {

            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_School_Homework.Where(x => x.HomeworkId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();
                //data = result.UserRole;

            }

            catch (Exception ex)
            {

            }

            return RedirectToAction("GetHomeWorkList", "School");
        }




        public ActionResult DeleteExamType(Guid id)
        {

            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_School_ExamType.Where(x => x.ExamTypeId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();
                //data = result.UserRole;

            }

            catch (Exception ex)
            {

            }

            return RedirectToAction("GetExamType", "School");
        }

        public ActionResult DeleteExam(Guid id)
        {

            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_School_ExamSchedule.Where(x => x.ExamScheduleId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();
                //data = result.UserRole;

            }

            catch (Exception ex)
            {

            }

            return RedirectToAction("GetExam", "School");
        }

        public ActionResult DeleteSubject(Guid id)
        {

            try
            {
                // get data for same id 
                var result = DbContext.tbl_DC_School_Subject.Where(x => x.SubjectId == id && x.IsActive == true).FirstOrDefault();
                //                  //Set status false for delete
                result.IsActive = false;
                DbContext.SaveChanges();
                //data = result.UserRole;

            }

            catch (Exception ex)
            {

            }

            return RedirectToAction("GetHomeWorkList", "School");
        }
    }
}
