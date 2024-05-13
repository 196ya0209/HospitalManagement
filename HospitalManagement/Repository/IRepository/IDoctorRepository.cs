using HospitalManagement.Models;

namespace HospitalManagement.Repository.IRepository
{
    public interface IDoctorRepository
    {
        public Task<List<Doctor>> GetAll();
        public Task<Doctor> GetDoctor(int doctorId);
        public Task AddDoctor(Doctor doctor);
        public Task AddDoctorUser(Doctor doctor);
        public Task UpdateDoctor(Doctor doctor);
        public Task<bool> RemoveDoctor(int doctorId);       
    }
}
