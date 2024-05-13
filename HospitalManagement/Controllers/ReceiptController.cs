using HospitalManagement.Models.ViewModels;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public ReceiptController(IAppointmentRepository appointmentRepository, IPatientRepository patientRepository, IDoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<IActionResult> Generate(int appointmentId)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            var patient = await _patientRepository.GetPatient(appointment.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            var doctor = await _doctorRepository.GetDoctor(appointment.DoctorId);
            if (doctor == null)
            {
                return NotFound();
            }

            var model = new ReceiptViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientName = patient.PatientName,
                DoctorName = doctor.DoctorName,
                AppointmentDate = appointment.DateScheduled,
                AmountPaid = appointment.AmountPaid
            };

            return View(model);
        }
    }
}
