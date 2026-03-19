using RecipesAPI.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RecipesAPI.Services.Utils
{
    public static class ProfanityFilter
    {
        static readonly HashSet<string> bannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kurw",
            "kurew",
            "pierd",
            "chuj",
            "jeb",
            "pizd",
            "dup",
            "jeban",
            "kutas",
            "suk"
        };

        public static bool ContainsProfanity(string text)
        {
            if (text == null)
                return false;

            return bannedWords.Any(word => Regex.IsMatch(text, 
                Regex.Escape(word), 
                RegexOptions.IgnoreCase));
        }

        public static bool RecipeContainsProfanity(CreateRecipeDto recipeDto)
        {
            if (ProfanityFilter.ContainsProfanity(recipeDto.Name)
                || ProfanityFilter.ContainsProfanity(recipeDto.Description)
                || ProfanityFilter.ContainsProfanity(recipeDto.Steps))
            {
                return true;
            }

            foreach (var ingredient in recipeDto.Ingredients)
            {
                if (ProfanityFilter.ContainsProfanity(ingredient.Name))
                    return true;
            }

            return false;
        }

        public static string FilterText(string text)
        {
            if (text == null)
                return text;

            foreach (string word in bannedWords)
            {
                string replacement = new string('*', word.Length);
                text = Regex.Replace(text,
                    Regex.Escape(word),
                    replacement,
                    RegexOptions.IgnoreCase);
            }

            return text;
        }
    }
}
