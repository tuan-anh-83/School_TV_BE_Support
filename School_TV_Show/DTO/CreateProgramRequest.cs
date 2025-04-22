using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateProgramRequest
    {
        [Required]
        public int SchoolChannelID { get; set; }

        [Required(ErrorMessage = "Program name is required.")]
        [StringLength(100, ErrorMessage = "Program name cannot exceed 100 characters.")]
        public string ProgramName { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; }
    }
}
