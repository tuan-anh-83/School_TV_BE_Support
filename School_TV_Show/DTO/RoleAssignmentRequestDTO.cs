using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class RoleAssignmentRequestDTO
    {
        [Required(ErrorMessage = "RoleID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "RoleID must be a positive integer.")]
        public int RoleID { get; set; }
    }
}
