namespace SaveForPerksAPI.Models
{
    public class CustomerDashboardDto
    {
        public CustomerProgressDto Progress { get; set; } = null!;
        public CustomerAchievementsDto Achievements { get; set; } = null!;
        public IEnumerable<CustomerActiveBusinessDto> Top3Businesses { get; set; } = new List<CustomerActiveBusinessDto>();
        public CustomerLast30DaysDto Last30Days { get; set; } = null!;
    }

    public class CustomerProgressDto
    {
        public int CurrentTotalPoints { get; set; }
        public int RewardsAvailable { get; set; }
    }

    public class CustomerAchievementsDto
    {
        public int LifetimeRewardsClaimed { get; set; }
        public int TotalPointsEarned { get; set; }
    }

    public class CustomerActiveBusinessDto
    {
        public BusinessDto Business { get; set; } = null!;
        public int Balance { get; set; }
        public int CostPoints { get; set; }
        public int RewardsAvailable { get; set; }
    }

    public class CustomerLast30DaysDto
    {
        public int PointsEarned { get; set; }
        public int ScansCompleted { get; set; }
        public int RewardsClaimed { get; set; }
    }
}
