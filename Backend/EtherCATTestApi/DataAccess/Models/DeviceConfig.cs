﻿using System.ComponentModel.DataAnnotations;

namespace EtherCATTestApi.DataAccess.Models
{
    public class DeviceConfig
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public ushort VendorId { get; set; }
        
        [Required]
        public uint ProductCode { get; set; }
        
        [Required]
        public ushort RevisionNo { get; set; }
        
        public string? OrderCode { get; set; }
        
        public string? DeviceName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public List<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }
}