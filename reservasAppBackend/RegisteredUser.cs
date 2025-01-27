using static System.Runtime.InteropServices.JavaScript.JSType;

namespace reservasAppBackend
{
    public class RegisteredUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; }
        //public List<BookedDate> BookedDates { get; set; }
        public virtual ICollection<BookedDate> BookedDates { get; set; }

    }
}
