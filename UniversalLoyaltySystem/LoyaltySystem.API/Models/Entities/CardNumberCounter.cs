namespace LoyaltySystem.API.Models.Entities;

public class CardNumberCounter
{
    public int OrgId { get; set; }
    public int ProgramId { get; set; }
    public long NextSeq { get; set; } = 1;
    public byte[] RowVersion { get; set; } = System.Array.Empty<byte>();
}
