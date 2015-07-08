using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using EnvDTE;
using EnvDTE80;

namespace AshTewari.CodeFlip
{
    public sealed class Utils
    {
        public Utils()
        {
        }

        public static IList<System.String> GetAllProperties(EnvDTE.CodeElement element)
        {
            IList<System.String> props = new List<System.String>();
            for (int i = 1; i <= element.Children.Count; i++)
            {
                var member = element.Children.Item(i);
                if (member.Kind == vsCMElement.vsCMElementProperty)
                {
                    props.Add(member.Name);
                }
            }
            return props;
        }

        public static IDictionary<System.String, System.String> GetPropertiesWithTypes(EnvDTE.CodeElement element)
        {
            IDictionary<System.String, System.String> result = new Dictionary<System.String, System.String>();
            for (int i = 1; i <= element.Children.Count; i++)
            {
                var member = element.Children.Item(i);
                if (member.Kind == vsCMElement.vsCMElementProperty)
                {
                    var codeProperty2 = member as CodeProperty2;
                    result.Add(member.Name, codeProperty2 == null ? "" : codeProperty2.Type.AsString);
                }
            }
            return result;
        }

        public static IList<System.String> GetFields(EnvDTE.CodeElement element)
        {
            IList<System.String> props = new List<System.String>();
            for (int i = 1; i <= element.Children.Count; i++)
            {
                var member = element.Children.Item(i);
                if (member.Kind == vsCMElement.vsCMElementVariable)
                {
                    props.Add(member.Name);
                }
            }
            return props;
        }

        public static string TrimTrailingChars(string input, int howMany)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return input.Substring(0, input.Length - howMany);
        }

        public static string LowercaseFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            char[] result = input.ToCharArray();
            result[0] = char.ToLower(result[0]);

            return new string(result);
        }

        public static string UppercaseFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            char[] result = input.ToCharArray();
            result[0] = char.ToUpper(result[0]);

            return new string(result);
        }

        internal void LoadDefaultTemplates()
        {
            var templatesFolder = GetTemplatesFolder();
            var files = Directory.GetFiles(templatesFolder);
            foreach (var file in files)
            {
                Debug.WriteLine(file);
            }
        }

        internal static string GetTemplatesFolder()
        {
            return Path.Combine(GetInstalledDirectoryName(), "Templates");
        }

        internal static string GetInstalledDirectoryName()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetSolutionDirectory(DTE2 dte)
        {
            return Directory.GetParent(dte.Solution.FullName).FullName;
        }
    }
}