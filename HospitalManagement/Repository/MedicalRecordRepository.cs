using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Repository
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MedicalRecordRepository> _logger;
        public MedicalRecordRepository(ApplicationDbContext db, ILogger<MedicalRecordRepository> logger)
        {
            _db = db;
            _logger = logger;

        }
        public async Task AddMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            try
            {
                await _db.MedicalRecords.AddAsync(medicalRecord);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error in AddMedicalRecordAsync");
            }            
        }

        public async Task DeleteMedicalRecordAsync(int id)
        {
            try
            {
                var medicalRecord = await _db.MedicalRecords.FindAsync(id);
                if (medicalRecord != null)
                {
                    _db.MedicalRecords.Remove(medicalRecord);
                    await _db.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMedicalRecordAsync");
            }                        
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsAsync()
        {
            return await _db.MedicalRecords.ToListAsync();
        }

        public async Task<MedicalRecord?> GetMedicalRecordByIdAsync(int id)
        {
            return await _db.MedicalRecords.FindAsync(id);
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsForDoctorAsync(int doctorId)
        {
            return await _db.MedicalRecords.Where(m => m.DoctorId == doctorId).ToListAsync();
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsForPatientAsync(int patientId)
        {
            return await _db.MedicalRecords.Where(m => m.PatientId == patientId).ToListAsync();
        }

        public async Task UpdateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            try
            {
                _db.Entry(medicalRecord).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateMedicalRecordAsync");
            }            
        }
    }
}
