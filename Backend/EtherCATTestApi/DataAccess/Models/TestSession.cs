﻿using System.ComponentModel.DataAnnotations;

namespace EtherCATTestApi.DataAccess.Models
{
    public class TestSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string SessionName { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [Required]
        public int DeviceConfigId { get; set; }
        
        // Navigation properties
        public DeviceConfig DeviceConfig { get; set; }
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}