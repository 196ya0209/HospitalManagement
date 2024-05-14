using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HospitalManagement.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(ApplicationDbContext db, ILogger<PaymentRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            try
            {
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while adding a payment");
                throw;
            }
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment != null)
            {
                _db.Payments.Remove(payment);
                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Payment with ID {PaymentId} not found", id);
            }
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _db.Payments.ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _db.Payments.FindAsync(id);
        }
      

        public async Task UpdatePaymentAsync(Payment payment)
        {
            try
            {
                _db.Entry(payment).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating a payment");
                throw;
            }
        }
    }
}
