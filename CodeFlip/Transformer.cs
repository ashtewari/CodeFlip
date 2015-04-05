using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;

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
            OpenFileDialog openFileFDialog = new OpenFileDialog();
            openFileFDialog.InitialDirectory = Directory.GetParent(dte.Solution.FullName).FullName;
            openFileFDialog.ShowDialog();
            string templateFile = openFileFDialog.FileName;

            if (string.IsNullOrWhiteSpace(templateFile)) return;

            var cb = new RunT4Callback();

            var t4 = ServiceProvider.GlobalProvider.GetService(typeof(STextTemplating)) as ITextTemplating;
            var host = t4 as ITextTemplatingSessionHost;
            //// var engine = t4 as ITextTemplatingEngineHost;

            // Create a Session in which to pass parameters:
            host.Session = host.CreateSession();
            
            // Add parameter values to the Session:
            host.Session["CodeElement"] = codeElement;

            var result = t4.ProcessTemplate(templateFile, File.ReadAllText(templateFile), cb);
            
            foreach (var message in cb.errorMessages)
            {                
                result += string.Format("\n{0}", message);
            }

            var tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Guid.NewGuid().ToString(), ".txt"));

            File.WriteAllText(tempFile, result);

            System.Diagnostics.Process.Start(tempFile);
        }
    }
}
