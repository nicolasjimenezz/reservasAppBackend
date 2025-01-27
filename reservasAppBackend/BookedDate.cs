using System.ComponentModel.DataAnnotations.Schema;

namespace reservasAppBackend
{
    public class BookedDate
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }

        [ForeignKey("RegisteredUser")]
        public int IdRegisteredUser { get; set; }
        public virtual RegisteredUser RegisteredUser { get; set; }
    }
}
