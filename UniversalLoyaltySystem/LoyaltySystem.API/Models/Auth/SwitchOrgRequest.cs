using System.ComponentModel.DataAnnotations;


namespace LoyaltySystem.API.Models.Auth;


public class SwitchOrgRequest { [Required] public Guid OrgId { get; set; } }