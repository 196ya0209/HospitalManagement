using HospitalManagement.CustomExceptions;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Models.ViewModels;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = "Admin, Doctor, Patient")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<AppointmentController> _logger;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        public AppointmentController(IAppointmentRepository appointmentRepository, ILogger<AppointmentController> logger, IPatientRepository patientRepository, IDoctorRepository doctorRepository, IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _appointmentRepository = appointmentRepository;
            _logger = logger;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> PaymentSuccess(int id)
        {
            Appointment appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);            
            return View(appointment);
        }
        //Same Date Conflict
        private async Task<bool> IsAppointmentConflict(AppointmentViewModel viewModel)
        {
            var existingAppointments = await _appointmentRepository.GetAllAppointmentsAsync(); // Get all existing appointments
            var newAppointmentDate = viewModel.DateScheduled.Date;
            
            foreach (var existingAppointment in existingAppointments)
            {
                if (existingAppointment.DateScheduled.Date == newAppointmentDate)
                {
                    Log.Error("Conflict in Date with Doctor and Patient" + newAppointmentDate);
                    throw new AppointmentConflictException();
                }
            }            
            return false;
        }

        public async Task<IActionResult> BookAppointment(int id)
        {            
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            return View(appointment);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AppointmentForDoctor()
        {           
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _appointmentRepository.GetAppointmentsForDoctorAsync(userId);
            return View(appointments);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AppointmentForPatient()
        {            
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _appointmentRepository.GetAppointmentsForPatientAsync(userId);            
            return View(appointments);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> CreateForPatient()
        {
            var viewModel = new AppointmentViewModel
            {
                Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName }),
                Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName })
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForPatient(AppointmentViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (ModelState.IsValid)
            {
                try
                {
                    //bool isConflict = await IsAppointmentConflict(viewModel);
                    //if (isConflict)
                    //{
                        //return RedirectToAction("Index", "Appointment");
                    //}
                    string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    var patient = await _db.Patients.FirstOrDefaultAsync(d => d.PatientUserId == userId)!;
                    var appointment = new Appointment
                    {
                        PatientId = patient.PatientId,
                        DoctorId = viewModel.DoctorId,
                        DateScheduled = viewModel.DateScheduled,
                        IsAccepted = viewModel.IsAccepted,
                        PaymentStatus = viewModel.PaymentStatus,
                        AmountPaid = viewModel.AmountPaid
                    };

                    await _appointmentRepository.AddAppointmentAsync(appointment);

                    Log.Information("Appointment created successfully.");

                    return RedirectToAction("Payment", "Appointment", new { id = appointment.AppointmentId });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while creating a new appointment.");
                    throw;
                }
            }

            viewModel.Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName });
            viewModel.Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName });

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,                
                DoctorId = appointment.DoctorId,
                DateScheduled = appointment.DateScheduled,
                IsAccepted = appointment.IsAccepted,
                PaymentStatus = appointment.PaymentStatus,
                AmountPaid = appointment.AmountPaid
            };

            return View(viewModel);            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(int id, AppointmentViewModel viewModel)
        {            
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    appointment.PaymentStatus = true;
                    appointment.AmountPaid = 500;

                    await _appointmentRepository.UpdateAppointmentAsync(appointment);

                    return RedirectToAction("PaymentSuccess","Appointment", new { id = appointment.AppointmentId }); 
                }
                catch (Exception ex)
                {
                    // Log error
                    Log.Error(ex.Message);
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(viewModel);
        }


        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            return View(appointments);
        }
        
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new AppointmentViewModel
            {
                Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName }),
                Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName })
            };

            return View(viewModel);
        }
        
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (ModelState.IsValid)
            {
                try
                {
                    //bool isConflict = await IsAppointmentConflict(viewModel);
                    //if (isConflict)
                    //{
                    //    return RedirectToAction("Index", "Appointment");
                    //}
                    var appointment = new Appointment
                    {
                        PatientId = viewModel.PatientId,
                        DoctorId = viewModel.DoctorId,
                        DateScheduled = viewModel.DateScheduled,
                        IsAccepted = viewModel.IsAccepted,
                        PaymentStatus = viewModel.PaymentStatus,
                        AmountPaid = viewModel.AmountPaid
                    };

                    await _appointmentRepository.AddAppointmentAsync(appointment);
                    Log.Information("Appointment created successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while creating a new appointment.");
                    throw;
                }
            }

            viewModel.Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName });
            viewModel.Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName });

            return View(viewModel);
        }
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                Appointment appointment = await _appointmentRepository.GetAppointmentByIdAsync(id.Value);
                if (appointment == null)
                {
                    return NotFound();
                }

                var viewModel = new AppointmentViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    DateScheduled = appointment.DateScheduled,
                    IsAccepted = appointment.IsAccepted,
                    PaymentStatus = appointment.PaymentStatus,
                    AmountPaid = appointment.AmountPaid,
                    Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName }),
                    Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName })
                };

                Log.Information("Retrieved appointment details for editing.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while retrieving appointment details for editing.");
                throw;
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (id != viewModel.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //bool isConflict = await IsAppointmentConflict(viewModel);
                    //if (isConflict)
                    //{
                    //    return RedirectToAction("Index", "Appointment");
                    //}
                    var appointment = new Appointment
                    {
                        AppointmentId = viewModel.AppointmentId,
                        PatientId = viewModel.PatientId,
                        DoctorId = viewModel.DoctorId,
                        DateScheduled = viewModel.DateScheduled,
                        IsAccepted = viewModel.IsAccepted,
                        PaymentStatus = viewModel.PaymentStatus,
                        AmountPaid = viewModel.AmountPaid
                    };

                    await _appointmentRepository.UpdateAppointmentAsync(appointment);
                    Log.Information("Appointment details updated successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while updating appointment details.");
                    throw;
                }
            }

            viewModel.Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName });
            viewModel.Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName });

            return View(viewModel);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditForDoctor(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                Appointment appointment = await _appointmentRepository.GetAppointmentByIdAsync(id.Value);
                if (appointment == null)
                {
                    return NotFound();
                }

                var viewModel = new AppointmentViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    DateScheduled = appointment.DateScheduled,
                    IsAccepted = appointment.IsAccepted,
                    PaymentStatus = appointment.PaymentStatus,
                    AmountPaid = appointment.AmountPaid,
                    Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName }),
                    Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName })
                };

                Log.Information("Retrieved appointment details for editing.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while retrieving appointment details for editing.");
                throw;
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForDoctor(int id, AppointmentViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (id != viewModel.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = new Appointment
                    {
                        AppointmentId = viewModel.AppointmentId,
                        PatientId = viewModel.PatientId,
                        DoctorId = viewModel.DoctorId,
                        DateScheduled = viewModel.DateScheduled,
                        IsAccepted = viewModel.IsAccepted,
                        PaymentStatus = viewModel.PaymentStatus,
                        AmountPaid = viewModel.AmountPaid
                    };

                    await _appointmentRepository.UpdateAppointmentAsync(appointment);
                    Log.Information("Appointment details updated successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while updating appointment details.");
                    throw;
                }
            }

            viewModel.Patients = (await _patientRepository.GetAll()).Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName });
            viewModel.Doctors = (await _doctorRepository.GetAll()).Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName });

            return View(viewModel);
        }

        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        [Authorize(Roles = "Doctor, Patient")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _appointmentRepository.DeleteAppointmentAsync(id);
                Log.Information("Appointment deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while deleting an appointment");
                throw;
            }
        }

        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> DeleteForDoctor(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        [Authorize(Roles = "Doctor, Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForDoctorConfirmed(int id)
        {
            try
            {
                await _appointmentRepository.DeleteAppointmentAsync(id);
                Log.Information("Appointment deleted successfully.");
                return RedirectToAction(nameof(AppointmentForDoctor));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while deleting an appointment");
                throw;
            }
        }
    }
}
