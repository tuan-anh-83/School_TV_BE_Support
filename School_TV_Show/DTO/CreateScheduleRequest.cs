﻿using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateScheduleRequest
    {
        [Required(ErrorMessage = "ProgramID is required.")]
        public int ProgramID { get; set; }

        [Required(ErrorMessage = "StartTime is required.")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required.")]
        public string EndTime { get; set; }
        public bool IsReplay { get; set; } = false;

        [Required(ErrorMessage = "Thumbnail is required.")]
        public IFormFile ThumbnailFile { get; set; }
    }
}
