using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Server.Models
{
    public class MessageModel
    {
        public int MessageId { get; set; }

        public string ChatId { get; set; }

        public int SenderId { get; set; }

        public int? ReceiverId { get; set; } // nullable for group

        public string Content { get; set; }

        public string MessageType { get; set; }
        // Private, Group, Broadcast

        public string Status { get; set; }
        // Sent, Delivered, Read

        public DateTime CreatedAt { get; set; }



    }
    public enum MessageType
    {
        Private,            // Patient → specific Doctor
        PatientToAdmin,     // Patient → broadcast all Admins
        AdminToDoctors,     // Admin → broadcast all Doctors
        DoctorToDoctors,    // Doctor → broadcast all Doctors
        DoctorToPatients,   // Doctor → broadcast all Patients
        DoctorToAll         // Doctor → broadcast everyone
    }
    public enum Role
    {
        Patient,
        Doctor,
        Admin
    }
}
