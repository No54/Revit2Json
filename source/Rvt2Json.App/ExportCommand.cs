using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rvt2Json.App.UI.View;
using System.IO;
using System.Windows.Forms;

namespace Rvt2Json.App
{
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
                        Filter = "Json文件(*.json)",
                        FilterIndex = 1,
                        RestoreDirectory = true,
                        FileName = $"{filename}.json"
                    };
                    var result = dialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        var outputpath = dialog.FileName.ToString();
                        var context = new CustomContext(doc, outputpath, isrvt, instancechecked, typecheced);
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
                TaskDialog.Show("注意", "请切换至三维视图");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
