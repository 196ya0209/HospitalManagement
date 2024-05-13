using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Models.ViewModels;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

                    _logger.LogInformation("Appointment created successfully.");

                    return RedirectToAction("Payment", "Appointment", new { id = appointment.AppointmentId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating a new appointment.");
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
            //if (id != viewModel.AppointmentId)
            //{
            //    return NotFound();
            //}
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

                    return RedirectToAction("Index", "Home"); // Redirect to appropriate page after updating
                }
                catch (Exception ex)
                {
                    // Log error
                    return RedirectToAction("Index", "Home"); // Redirect to appropriate page after error
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
                    _logger.LogInformation("Appointment created successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating a new appointment.");
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

                _logger.LogInformation("Retrieved appointment details for editing.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving appointment details for editing.");
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
                    _logger.LogInformation("Appointment details updated successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating appointment details.");
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

                _logger.LogInformation("Retrieved appointment details for editing.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving appointment details for editing.");
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
                    _logger.LogInformation("Appointment details updated successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating appointment details.");
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
                _logger.LogInformation("Appointment deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting an appointment");
                throw;
            }
        }
    }
}
