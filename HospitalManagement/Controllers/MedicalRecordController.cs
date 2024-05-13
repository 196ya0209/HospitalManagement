using HospitalManagement.Models;
using HospitalManagement.Models.ViewModels;
using HospitalManagement.Repository;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = "Admin, Doctor")]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly ILogger<MedicalRecordController> _logger;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        public MedicalRecordController(IMedicalRecordRepository medicalRecordRepository, ILogger<MedicalRecordController> logger, IPatientRepository patient, IDoctorRepository doctorRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _logger = logger;
            _patientRepository = patient;
            _doctorRepository = doctorRepository;
        }
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var medicalRecords = await _medicalRecordRepository.GetAllMedicalRecordsAsync();

                _logger.LogInformation("Retrieved all medical records successfully.");
                return View(medicalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving medical records.");
                throw;
            }
        }
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var medicalRecord = await _medicalRecordRepository.GetMedicalRecordByIdAsync(id.Value);
                if (medicalRecord == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of medical record with ID: {MedicalRecordId}.", id);
                return View(medicalRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of medical record with ID: {MedicalRecordId}.", id);
                throw;
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var patients = await _patientRepository.GetAll();
                var doctors = await _doctorRepository.GetAll();

                var viewModel = new MedicalRecordViewModel
                {
                    Patients = patients.Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName }),
                    Doctors = doctors.Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName })
                };

                _logger.LogInformation("Fetched patients and doctors for creating a new medical record.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patients and doctors for creating a new medical record.");
                throw;
            }
        }

        // POST: MedicalRecord/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");

            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = new MedicalRecord
                    {
                        PatientId = viewModel.PatientId,
                        DoctorId = viewModel.DoctorId,
                        Diagnosis = viewModel.Diagnosis,
                        Treatment = viewModel.Treatment
                    };

                    await _medicalRecordRepository.AddMedicalRecordAsync(medicalRecord);
                    _logger.LogInformation("Medical record created successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating a new medical record.");
                    throw;
                }
            }            

            return View(viewModel);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                MedicalRecord medicalRecord = await _medicalRecordRepository.GetMedicalRecordByIdAsync(id.Value);
                if (medicalRecord == null)
                {
                    return NotFound();
                }

                var viewModel = new MedicalRecordViewModel
                {
                    MedicalRecordId = medicalRecord.RecordId,
                    PatientId = medicalRecord.PatientId,
                    DoctorId = medicalRecord.DoctorId,
                    Diagnosis = medicalRecord.Diagnosis,
                    Treatment = medicalRecord.Treatment,
                    DateCreated = medicalRecord.DateCreated
                };

                var patients = await _patientRepository.GetAll();
                var doctors = await _doctorRepository.GetAll();

                viewModel.Patients = patients.Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.PatientName });
                viewModel.Doctors = doctors.Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = d.DoctorName });

                _logger.LogInformation("Retrieved medical record details for editing.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving medical record details for editing.");
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,MedicalRecordViewModel viewModel)
        {
            ModelState.Remove("Patients");
            ModelState.Remove("Doctors");
            if (id != viewModel.MedicalRecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = new MedicalRecord
                    {
                        RecordId = viewModel.MedicalRecordId,
                        PatientId = viewModel.PatientId,
                        DoctorId = viewModel.DoctorId,
                        Diagnosis = viewModel.Diagnosis,
                        Treatment = viewModel.Treatment,
                        DateCreated = viewModel.DateCreated
                    };

                    await _medicalRecordRepository.UpdateMedicalRecordAsync(medicalRecord);
                    _logger.LogInformation("Medical record details updated successfully.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating medical record details.");
                    throw;
                }
            }

            return View(viewModel);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var medicalRecord = await _medicalRecordRepository.GetMedicalRecordByIdAsync(id.Value);
                if (medicalRecord == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of medical record with ID: {MedicalRecordId} for deletion.", id);
                return View(medicalRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of medical record with ID: {MedicalRecordId} for deletion.", id);
                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _medicalRecordRepository.DeleteMedicalRecordAsync(id);
                _logger.LogInformation("Medical record deleted successfully. Medical Record ID: {MedicalRecordId}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting medical record. Medical Record ID: {MedicalRecordId}", id);
                throw;
            }
        }

        public async Task<IActionResult> MedicalRecordsForPatient(int patientId)
        {
            try
            {
                var medicalRecords = await _medicalRecordRepository.GetMedicalRecordsForPatientAsync(patientId);
                _logger.LogInformation("Retrieved all medical records for patient with ID: {PatientId} successfully.");
                return View("Index", medicalRecords); // Assuming you have an Index view for medical records
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving medical records for patient with ID: {PatientId}.", patientId);
                throw;
            }
        }

        public async Task<IActionResult> MedicalRecordsForDoctor(int doctorId)
        {
            try
            {
                var medicalRecords = await _medicalRecordRepository.GetMedicalRecordsForDoctorAsync(doctorId);
                _logger.LogInformation("Retrieved all medical records for doctor with ID: {DoctorId} successfully.");
                return View("Index", medicalRecords); // Assuming you have an Index view for medical records
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving medical records for doctor with ID: {DoctorId}.", doctorId);
                throw;
            }
        }

    }
}
