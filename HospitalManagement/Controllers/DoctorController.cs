using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(IDoctorRepository doctorRepository, ILogger<DoctorController> logger)
        {
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var doctors = await _doctorRepository.GetAll();
                _logger.LogInformation("Retrieved all doctors successfully.");
                return View(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving doctors.");
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
                var doctor = await _doctorRepository.GetDoctor(id.Value);
                if (doctor == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of doctor with ID: {DoctorId}.", id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of doctor with ID: {DoctorId}.", id);
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
        public async Task<IActionResult> Create([Bind("DoctorId,DoctorName,Specialization,DocImageUrl")] Doctor doctor, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await image.CopyToAsync(ms);
                        doctor.DocImageUrl = ms.ToArray();
                    }
                }

                try
                {
                    await _doctorRepository.AddDoctor(doctor);
                    _logger.LogInformation("Doctor created successfully. Doctor ID: {DoctorId}", doctor.DoctorId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating a doctor.");
                    throw;
                }
            }
            return View(doctor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var doctor = await _doctorRepository.GetDoctor(id.Value);
                if (doctor == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of doctor with ID: {DoctorId} for editing.", id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of doctor with ID: {DoctorId} for editing.", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorId,DoctorName,Specialization,DocImageUrl")] Doctor doctor, IFormFile image)
        {
            if (id != doctor.DoctorId)
            {
                return NotFound();
            }
            ModelState.Remove("DoctorUserId");
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await image.CopyToAsync(ms);
                        doctor.DocImageUrl = ms.ToArray();
                    }
                }

                try
                {
                    await _doctorRepository.UpdateDoctor(doctor);
                    _logger.LogInformation("Doctor details updated successfully. Doctor ID: {DoctorId}", doctor.DoctorId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating doctor details. Doctor ID: {DoctorId}", doctor.DoctorId);
                    throw;
                }
            }
            return View(doctor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var doctor = await _doctorRepository.GetDoctor(id.Value);
                if (doctor == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Retrieved details of doctor with ID: {DoctorId} for deletion.", id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details of doctor with ID: {DoctorId} for deletion.", id);
                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _doctorRepository.RemoveDoctor(id);
                _logger.LogInformation("Doctor deleted successfully. Doctor ID: {DoctorId}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting doctor. Doctor ID: {DoctorId}", id);
                throw;
            }
        }

    }
}
