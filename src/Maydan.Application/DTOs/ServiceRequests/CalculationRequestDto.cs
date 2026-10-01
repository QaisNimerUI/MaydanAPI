namespace Maydan.Application.DTOs.ServiceRequests;

public class CalculationRequestDto
{
    public int ServiceId { get; set; }
    public int RequestedWorkers { get; set; }
    public int ShiftsOrDaysCount { get; set; }
}
