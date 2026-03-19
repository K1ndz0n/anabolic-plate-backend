using System.Diagnostics.Eventing.Reader;

namespace RecipesAPI.Services.Utils
{
    public class Filter
    {
        public string? Search { get; set; }
        public float? MinRating { get; set; } = 0;
        public float MaxRating { get; set; } = 5;
        public int? MinOpinionCount { get; set; } = 0;
        public int? MaxOpinionCount { get; set; } = int.MaxValue;
        public int? MinKcal { get; set; } = 0;
        public int? MaxKcal { get; set; } = int.MaxValue;
        public int? MinProtein { get; set; } = 0;
        public int? MaxProtein { get; set; } = int.MaxValue;
        public int? MinCarbs { get; set; } = 0;
        public int? MaxCarbs { get; set; } = int.MaxValue;
        public int? MinFat { get; set; } = 0;
        public int? MaxFat { get; set; } = int.MaxValue;
        public string? OrderBy { get; set; } = "id";
        public string? Order { get; set; } = "desc";
        public bool HasNutrition { get; set; } = false;
    }
}
