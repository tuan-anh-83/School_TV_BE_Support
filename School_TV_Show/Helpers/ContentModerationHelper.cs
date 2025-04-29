using System.Reflection;

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

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj) as string;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var matchedWord = _bannedWords
                        .FirstOrDefault(word => value.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedWord != null)
                    {
                        return (true, $"Nội dung trường '{prop.Name}' chứa từ cấm '{matchedWord}', vui lòng chỉnh sửa.");
                    }
                }
            }

            return (false, null);
        }
    }
}
