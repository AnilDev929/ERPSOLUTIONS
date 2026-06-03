namespace SchoolERP.Models.DTOS
{
    public class StudentAddressDTO
    {
        public int StudentId { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
    }
}
