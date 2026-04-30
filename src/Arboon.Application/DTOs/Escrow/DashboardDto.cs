namespace Arboon.Application.DTOs.Escrow;

public class DashboardStatsDto
{
    public decimal FrozenTotal { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
}

public class DashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<EscrowResponseDto> RecentEscrows { get; set; } = new();
}
