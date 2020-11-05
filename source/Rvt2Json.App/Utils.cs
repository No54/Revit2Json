using Autodesk.Revit.DB;

namespace Rvt2Json.App
{
    public static class Utils
    {
        public static string GetDescription4Element(Element elem)
        {
            var result = "<null>";
            if (elem != null)
            {
                var category = elem.Category.Name;
                var typename = string.Empty;
                var typeid = elem.GetTypeId();
                if (typeid != ElementId.InvalidElementId)
                {
                    var elemtype = elem.Document.GetElement(typeid);
                    if (elemtype != null)
                    {
                        typename = elemtype.Name;
                    }
                }
                result = $"{category} {typename} {elem.Name}({elem.Id.IntegerValue})";
            }
            return result;
        }

        public static int ColorToInt(Color color)
        {
            return color.Red << 16 | color.Green << 8 | color.Blue;
        }
    }
}
