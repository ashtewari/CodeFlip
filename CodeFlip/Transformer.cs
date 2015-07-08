using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Engine = Microsoft.VisualStudio.TextTemplating.Engine;

namespace AshTewari.CodeFlip
{    
    internal class Transformer
    {
        internal void Transform(DTE2 dte)
        {
            var objCursorTextPoint = GetCursorTextPoint(dte);
            if ((objCursorTextPoint != null))
            {
                var classElement = CodeSpecificElementFromPoint(dte, objCursorTextPoint, vsCMElement.vsCMElementClass);

                if (classElement != null)
                {
                    Debug.WriteLine(string.Format("Class Clicked : {0}", classElement.FullName));
                    Generate(dte, classElement);
                }
            }
            else
            {
                Debug.WriteLine("No object recognized at the cursor.");
            }
        }

        private EnvDTE.TextPoint GetCursorTextPoint(DTE2 dte)
        {
            VirtualPoint point = null;

            try
            {
                var textDocument = dte.ActiveDocument.Object() as EnvDTE.TextDocument;
                point = textDocument.Selection.ActivePoint;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
                throw;
            }

            return point;
        }

        private CodeElement CodeSpecificElementFromPoint(DTE2 dte, TextPoint objCursorTextPoint, vsCMElement elementType)
        {
            try
            {
                return dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElementFromPoint(objCursorTextPoint, elementType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generates the specified type to compare.
        /// </summary>
        /// <param name="dte">The DTE.</param>
        /// <param name="codeElement">The code element.</param>
        private void Generate(DTE2 dte, EnvDTE.CodeElement codeElement)
        {
            var templateFiles = GetTemplateFilesToProcess();

            if (templateFiles.Length == 0) return;            

            var result = ProcessTemplateFiles2(codeElement, templateFiles);

            WriteOutput(result);
        }

        private static string[] GetTemplateFilesToProcess()
        {
            OpenFileDialog openFileFDialog = new OpenFileDialog();
            openFileFDialog.InitialDirectory = Utils.GetInstalledDirectoryName();

            openFileFDialog.Multiselect = true;
            openFileFDialog.ShowDialog();

            return openFileFDialog.FileNames;            
        }

        private static string ProcessTemplateFiles2(CodeElement codeElement, string[] templateFiles)
        {
            var customHost = new CodeFlipCustomHost();
            var sessionHost = customHost as ITextTemplatingSessionHost;

            // Create a Session in which to pass parameters:
            sessionHost.Session = sessionHost.CreateSession();

            // Add parameter values to the Session:
            sessionHost.Session["codeElement"] = codeElement;

            string result = string.Empty;

            Engine engine = new Engine();
            foreach (var templateFile in templateFiles)
            {
                customHost.TemplateFileValue = templateFile;
                result = engine.ProcessTemplate(File.ReadAllText(templateFile), sessionHost as ITextTemplatingEngineHost);

                foreach (var message in customHost.Errors)
                {
                    result += string.Format("\nError while processing template: {0}", templateFile);
                    result += string.Format("\n{0}", message);
                }
            }

            return result;           
        }

        private static string ProcessTemplateFiles(CodeElement codeElement, string[] templateFiles)
        {
            var t4 = ServiceProvider.GlobalProvider.GetService(typeof (STextTemplating)) as ITextTemplating;
            //var t4 = new CodeFlipCustomHost();

            var host = t4 as ITextTemplatingSessionHost;
            //// var engine = t4 as ITextTemplatingEngineHost;

            // Create a Session in which to pass parameters:
            host.Session = host.CreateSession();

            // Add parameter values to the Session:
            host.Session["CodeElement"] = codeElement;

            var cb = new RunT4Callback();
            string result = string.Empty;

            foreach (var templateFile in templateFiles)
            {
                result = t4.ProcessTemplate(templateFile, File.ReadAllText(templateFile), cb);

                foreach (var message in cb.errorMessages)
                {
                    result += string.Format("\nError while processing template: {0}", templateFile);
                    result += string.Format("\n{0}", message);
                }
            }

            return result;
        }

        private static void WriteOutput(string result)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Guid.NewGuid().ToString(), ".txt"));

            File.WriteAllText(tempFile, result);

            System.Diagnostics.Process.Start(tempFile);
        }
    }
}
