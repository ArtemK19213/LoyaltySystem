namespace LoyaltySystem.API.Models.Partner;

public class UpdateProgramRequest : CreateProgramRequest
{
    public bool? IsActive { get; set; }
}
