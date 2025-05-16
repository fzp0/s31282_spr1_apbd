namespace s31282_spr1_apbd.Models
{

    public class ClientDTO
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
    }

    public class MechanicDTO
    {
        public int mechanicId { get; set; }
        public string licenceNumber { get; set; }
    }

    public class ServiceVisitDTO
    {
        public string name { get; set; }
        public decimal serviceFee { get; set; }
    }

  
    public class GetVisitByIdDTO
    {
        public DateTime date {  get; set; }
        public ClientDTO client { get; set; }
        public MechanicDTO mechanic { get; set; }

        public List<ServiceVisitDTO> visitServices { get; set; }
    }
}
