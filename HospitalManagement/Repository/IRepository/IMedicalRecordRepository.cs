using HospitalManagement.Models;

namespace HospitalManagement.Repository.IRepository
{
    public interface IMedicalRecordRepository
    {
        Task AddMedicalRecordAsync(MedicalRecord medicalRecord);        
        Task<MedicalRecord> GetMedicalRecordByIdAsync(int id);
        Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsAsync();
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsForPatientAsync(int patientId);
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsForDoctorAsync(int doctorId);
        Task UpdateMedicalRecordAsync(MedicalRecord medicalRecord);        
        Task DeleteMedicalRecordAsync(int id);
    }
}
