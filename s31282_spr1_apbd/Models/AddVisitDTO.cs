namespace s31282_spr1_apbd.Models
{

    public class ServiceDTO
    {
        public string serviceName { get; set; }
        public decimal serviceFee { get; set; }
    }


    public class AddVisitDTO
    {
        public int visitID {  get; set; }
        public int clientID { get; set; }
        public string mechanicLicenceNumber { get; set; }
        public List<ServiceDTO> services { get; set; }
    }
}
