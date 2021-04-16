using System.ComponentModel.DataAnnotations;


namespace FamilySite.UI.MVC.Models
{
    public class ContactViewModel
    {
        //fields

        //properties
        [StringLength(100, ErrorMessage = "Less than 100 characters")]
        [Required(ErrorMessage = "* Required")]
        public string Name { get; set; }

        [Required(ErrorMessage ="* Required")]
        [EmailAddress(ErrorMessage = "* Please enter valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "* Required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "* Required")]
        public string Message { get; set; }
        //Get: ContactView
    }
}