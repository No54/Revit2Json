using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rvt2Json.App.UI.View;
using System.IO;
using System.Windows.Forms;

namespace Rvt2Json.App
{
    /// <summary>
    /// Accroding to threejs R122 JSON-Object-Scene-format-4 https://github.com/mrdoob/three.js/wiki
    /// Inspired by https://github.com/va3c/RvtVa3c
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var filename = string.IsNullOrEmpty(doc.PathName)? 
                            doc.Title: 
                            Path.GetFileNameWithoutExtension(doc.PathName);
            var isrvt = doc.OwnerFamily != null ? false : true;
           
            var view = doc.ActiveView;
            if (view is View3D)
            {
                var wnd = new ConfigWnd();
                wnd.ShowDialog();
                if (wnd.DialogResult == true)
                {
                    var instancechecked = wnd.InstanceChecked;
                    var typecheced = wnd.TypeChecked;

                    var dialog = new SaveFileDialog()
                    {
                        Filter = "Json File(*.json)|",
                        FilterIndex = 1,
                        RestoreDirectory = true,
                        FileName = $"{filename}.json"
                    };
                    var result = dialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        var outputpath = dialog.FileName.ToString();
                        var context = new CustomJsonContext(doc, outputpath, isrvt, instancechecked, typecheced);
                        var exporter = new CustomExporter(doc, context)
                        {
                            ShouldStopOnError = false,
                        };
                        exporter.Export(view as View3D);
                    }
                }
            }
            else
            {
                TaskDialog.Show("Warning", "Please switch to 3D View");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
