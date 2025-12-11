﻿using System.ComponentModel.DataAnnotations;

namespace EtherCATTestApi.DataAccess.Models
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string TestName { get; set; }
        
        [Required]
        public bool Passed { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public string? Details { get; set; }
        
        [Required]
        public DateTime TestTime { get; set; }
        
        [Required]
        public int TestSessionId { get; set; }
        
        public TestSession TestSession { get; set; }
    }
}