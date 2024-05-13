using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PatientController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientRepository patientRepository, ILogger<PatientController> logger)
        {
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var patient = await _patientRepository.GetAll();
                _logger.LogInformation("Retrieved all patients successfully.");
                return View(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving patients.");
                throw;
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var doctor = await _patientRepository.GetPatient(id.Value);
                if (doctor == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of patient with ID: {PatientId}.", id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of patient with ID: {PatientId}.", id);
                throw;
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,PatientName,PatientAge,Gender, AdmissionDate, WardNumber, IsActive")] Patient patient)
        {
            if (ModelState.IsValid)
            {                
                try
                {
                    await _patientRepository.AddPatient(patient);
                    _logger.LogInformation("Patient created successfully. Patient ID: {PatientId}", patient.PatientId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating a doctor.");
                    throw;
                }
            }
            return View(patient);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var patient = await _patientRepository.GetPatient(id.Value);
                if (patient == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of patient with ID: {PatientId} for editing.", id);
                return View(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of patient with ID: {PatientId} for editing.", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,PatientName,PatientAge,Gender, AdmissionDate, WardNumber, IsActive")] Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }
            ModelState.Remove("DoctorUserId");
            if (ModelState.IsValid)
            {

                try
                {
                    await _patientRepository.UpdatePatient(patient);
                    _logger.LogInformation("Patient details updated successfully. Doctor ID: {PatientId}", patient.PatientId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating doctor details. Doctor ID: {DoctorId}", patient.PatientId);
                    throw;
                }
            }
            return View(patient);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var doctor = await _patientRepository.GetPatient(id.Value);
                if (doctor == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of Patient with ID: {PatientId} for deletion.", id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of Patient with ID: {PatientId} for deletion.", id);
                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _patientRepository.RemovePatient(id);
                _logger.LogInformation("Patient deleted successfully. Patient ID: {PatientId}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Patient. Patient ID: {PatientId}", id);
                throw;
            }
        }

    }
}
