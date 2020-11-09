using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

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
                result = $"{category.Trim()} {typename.Trim()} {elem.Name.Trim()}({elem.Id.IntegerValue})";
            }
            return result;
        }

        public static int ColorToInt(Color color)
        {
            return color.Red << 16 | color.Green << 8 | color.Blue;
        }

        public static Dictionary<string, string> GetUserData(bool isrvt, 
                                                             bool instancechecked, 
                                                             bool typechecked,
                                                             Element elem)
        {
            var userdata = new Dictionary<string, string>();
            var doc = elem.Document;
            if (isrvt)
            {
                var result = new List<Parameter>();
                if (instancechecked && typechecked)
                {
                    var plist = elem.Parameters;
                    foreach (Parameter p in plist)
                    {
                        if (result.FirstOrDefault(x => x.Definition.Name == p.Definition.Name) == null)
                        {
                            result.Add(p);
                            var key = p.Definition.Name;
                            string val;
                            if (StorageType.String == p.StorageType)
                            {
                                val = p.AsString();
                            }
                            else
                            {
                                val = p.AsValueString();
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                userdata.Add(key, val);
                            }
                        }
                    }

                    var elementTypeId = elem.GetTypeId();
                    if (elementTypeId != ElementId.InvalidElementId)
                    {
                        var elementType = doc.GetElement(elementTypeId) as ElementType;
                        var etplist = elementType.Parameters;
                        foreach (Parameter et in etplist)
                        {
                            if (result.FirstOrDefault(x => x.Definition.Name == et.Definition.Name) == null)
                            {
                                result.Add(et);
                                var key = $"Type {et.Definition.Name}";
                                string val;
                                if (StorageType.String == et.StorageType)
                                {
                                    val = et.AsString();
                                }
                                else
                                {
                                    val = et.AsValueString();
                                }
                                if (!string.IsNullOrEmpty(val))
                                {
                                    userdata.Add(key, val);
                                }
                            }
                        }
                    }
                }
                if (instancechecked && !typechecked)
                {
                    var plist = elem.Parameters;
                    foreach (Parameter p in plist)
                    {
                        if (result.FirstOrDefault(x => x.Definition.Name == p.Definition.Name) == null)
                        {
                            result.Add(p);
                            var key = p.Definition.Name;
                            string val;
                            if (StorageType.String == p.StorageType)
                            {
                                val = p.AsString();
                            }
                            else
                            {
                                val = p.AsValueString();
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                userdata.Add(key, val);
                            }
                        }
                    }
                }
                if (!instancechecked && typechecked)
                {
                    var elementTypeId = elem.GetTypeId();
                    if (elementTypeId != ElementId.InvalidElementId)
                    {
                        var elementType = doc.GetElement(elementTypeId) as ElementType;
                        var etplist = elementType.Parameters;
                        foreach (Parameter et in etplist)
                        {
                            if (result.FirstOrDefault(x => x.Definition.Name == et.Definition.Name) == null)
                            {
                                result.Add(et);
                                var key = $"Type {et.Definition.Name}";
                                string val;
                                if (StorageType.String == et.StorageType)
                                {
                                    val = et.AsString();
                                }
                                else
                                {
                                    val = et.AsValueString();
                                }
                                if (!string.IsNullOrEmpty(val))
                                {
                                    userdata.Add(key, val);
                                }
                            }
                        }
                    }
                }
            }
            return userdata;
        }
    }
}
