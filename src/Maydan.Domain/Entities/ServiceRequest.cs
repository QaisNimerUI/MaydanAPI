using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maydan.Domain.Common;
using Maydan.Domain.Enums;

namespace Maydan.Domain.Entities
{
    public class ServiceRequest : SharedEntities
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int ProductionCompanyId { get; set; }
        public ProductionCompany ProductionCompany { get; set; } = null!;

        public int ServiceId { get; set; }

        public Service Service { get; set; } = null!;
        public int AssociationId { get; set; }
        public Association Association { get; set; } = null!;

         //Using CityId from Association.CityId to decide the relatd association's city

        //Duration and Time related properties
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ShiftsCount
        {
            get; set;
        } //e.g. DayCount

        //Workr related properties
        public int RequestedWorkersCount { get; set; } 
        public int SelectedWorkersCount { get; set; } = 0; 
        public int AttendanceFrequency { get; set; }         
        public string? AdditionalRequirements { get; set; }

        // Persisted coordinates for the requested location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        //Financial related properties
        public decimal UnitPriceSnapshot { get; set; }
        public decimal ExpectedTotalAmount { get; set; }

        //Status
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.PendingWorkerSelection;
        public string? IdempotencyKey { get; set; } //   prevent (Double-submit)
        public bool ReminderSent { get; set; } = false;

        public DateTime? CancelledAt { get; set; }

    }
}
