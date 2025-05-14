using System.Reflection;
using System.Text.RegularExpressions;

namespace School_TV_Show.Helpers
{
    public static class ContentModerationHelper
    {
        private static readonly List<string> _bannedWords;

        static ContentModerationHelper()
        {
            var basePath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(basePath, "Files", "badwords.txt");

            if (File.Exists(filePath))
            {
                _bannedWords = File.ReadAllLines(filePath)
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Select(x => x.Trim())
                                   .ToList();
            }
            else
            {
                _bannedWords = new List<string>();
            }
        }

        public static (bool HasViolation, string? Message) ValidateAllStringProperties(object obj)
        {
            if (obj == null)
                return (true, "Request body không được null.");

            var properties = obj.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.PropertyType == typeof(string));

            if (_bannedWords == null || !_bannedWords.Any())
                return (false, null);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Precompile regex pattern for each banned word
                    foreach (var bannedWord in _bannedWords)
                    {
                        string pattern = $@"\b{Regex.Escape(bannedWord)}\b";
                        try
                        {
                            if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                            {
                                return (true, $"Nội dung trường '{prop.Name}' chứa từ cấm '{bannedWord}', vui lòng chỉnh sửa.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error in case of invalid regex pattern (though it shouldn't happen)
                            Console.WriteLine($"Error while matching banned word: {bannedWord}. Exception: {ex.Message}");
                        }
                    }
                }
            }

            return (false, null);
        }
    }
}
