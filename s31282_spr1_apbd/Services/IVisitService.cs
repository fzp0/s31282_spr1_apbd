using s31282_spr1_apbd.Models;

namespace s31282_spr1_apbd.Services
{
    public interface IVisitService
    {
        Task<GetVisitByIdDTO> GetVisitByIdAsync(int id);
        Task AddVisitAsync(AddVisitDTO dto);
    }
}
