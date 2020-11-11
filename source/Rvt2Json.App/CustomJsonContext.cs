using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Rvt2Json.App.Model;
using Rvt2Json.App.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Rvt2Json.App
{
    public class CustomJsonContext : IExportContext
    {
        /// <summary>
        /// Scale entire top level BIM object node in JSON
        /// output. A scale of 1.0 will output the model in 
        /// millimetres. Currently we scale it to decimetres
        /// so that a typical model has a chance of fitting 
        /// into a cube with side length 100, i.e. 10 metres.
        /// </summary>
        readonly double scale_bim = 1.0;

        /// <summary>
        /// Scale applied to each vertex in each individual 
        /// BIM element. This can be used to scale the model 
        /// down from millimetres to metres, e.g.
        /// Currently we stick with millimetres after all
        /// at this level.
        /// </summary>
        readonly double scale_vertex = 1.0;

        /// <summary>
        /// If true, switch Y and Z coordinate 
        /// and flip X to negative to convert from
        /// Revit coordinate system to standard 3d
        /// computer graphics coordinate system with
        /// Z pointing out of screen, X towards right,
        /// Y up.
        /// </summary>
        readonly bool switch_coordinates = true;

        private Document doc;
        private string filename;
        private bool isrvt;
        private bool instancechecked;
        private bool typechecked;

        private string orignmaterialuuid = "18A30FF6-A3E4-4424-9E9E-01FD832753A2";

        private Json4ThreeModel container;
        //3 part of Json Parent Node
        private Dictionary<string, GeometryModel> geometries;
        private Dictionary<string, MaterialModel> materials;
        private Dictionary<string, ObjectModel> objects;
        //Transform
        private Stack<Transform> tfStack = new Stack<Transform>();
        //Current
        private ObjectModel currentobject;
        //Cache
        private Dictionary<string, List<XYZ>> XYZs;
        private Dictionary<string, List<int>> Indexs;
        private Dictionary<string, ObjectModel> cacheobjects;

        public CustomJsonContext(Document doc, string filename, bool isrvt, bool instancechecked, bool typechecked)
        {
            this.doc = doc;
            this.filename = filename;
            this.isrvt = isrvt;
            this.instancechecked = instancechecked;
            this.typechecked = typechecked;
        }

        public bool Start()
        {
            container = new Json4ThreeModel();

            geometries = new Dictionary<string, GeometryModel>();
            materials = new Dictionary<string, MaterialModel>();
            objects = new Dictionary<string, ObjectModel>();

            XYZs = new Dictionary<string, List<XYZ>>();
            Indexs = new Dictionary<string, List<int>>();
            cacheobjects = new Dictionary<string, ObjectModel>();

            tfStack.Push(Transform.Identity);

            //metadata
            var metadata = new MetadataModel()
            {
                version = "1.0",
                type = "Object",
                generator = "bim.frankliang Rvt2Json exporter"
            };
            container.metadata = metadata;

            //object
            var obj = new ObjectModel()
            {
                uuid = doc.ActiveView.UniqueId,
                name = $"BIM {doc.Title}",
                type = "Scene",
                matrix = new double[] {
                    scale_bim,0,0,0,
                    0,scale_bim,0,0,
                    0,0,scale_bim,0,
                    0,0,0,scale_bim
                }
            };
            container.obj = obj;
            return true;
        }

        //done
        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            // Setting our default LoD for the view
            // The scale goes from 0 to 10, but the value close to the edges
            // aren't really that usable, except maybe of experimenting
            node.LevelOfDetail = 5;
            Debug.WriteLine($"OnViewBegin:{node.NodeName}({node.ViewId.IntegerValue}):LOD:{node.LevelOfDetail}");
            return RenderNodeAction.Proceed;
        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            var elem = doc.GetElement(elementId);
            string uuid = elem.UniqueId;
            var currentelemmaterialuuid = string.Empty;
            if (elem.Category != null && elem.Category.Material != null)
            {
                currentelemmaterialuuid = elem.Category.Material.UniqueId;
            }
            else
            {
                currentelemmaterialuuid = orignmaterialuuid;
            }
            var elem_per_material = $"{uuid}-{currentelemmaterialuuid}";

            if (isrvt)
            {
                if (elem.Category == null || !Utils.HasSolid(elem) || elem is ElementType)
                {
                    return RenderNodeAction.Skip;
                }
            }
            else
            {

            }

            if (!materials.ContainsKey(currentelemmaterialuuid))
            {
                var origncolor = new Color(128, 128, 128);
                var materialmodel = new MaterialModel()
                {
                    uuid = currentelemmaterialuuid,
                    name = "Origin Material",
                    type = "MeshPhongMaterial",
                    color = Utils.ColorToInt(origncolor),
                    ambient = Utils.ColorToInt(origncolor),
                    emissive = 0,
                    specular = Utils.ColorToInt(origncolor),
                    shininess = 1,
                    opacity = 1,
                    transparent = false,
                    wireframe = false
                };
                materials.Add(currentelemmaterialuuid, materialmodel);
            }

            if (!cacheobjects.ContainsKey(elem_per_material))
            {
                currentobject = new ObjectModel()
                {
                    uuid = uuid,
                    name = Utils.GetDescription4Element(elem, isrvt),
                    type = "RevitElement",
                    matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
                    children = new List<ObjectModel>()
                    {
                        new ObjectModel(){
                                uuid = elem_per_material,
                                name = Utils.GetDescription4Element(elem,isrvt),
                                type = "Mesh",
                                matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
                                geometry = elem_per_material,
                                material = currentelemmaterialuuid,
                            }
                    },
                };
                if (instancechecked || typechecked)
                {
                    currentobject.userData = Utils.GetUserData(isrvt, instancechecked, typechecked, elem);
                }
                cacheobjects.Add(elem_per_material, currentobject);
            }
            return RenderNodeAction.Proceed;
        }


        /// <summary>
        /// This code demonstrates how to process material information
        /// https://www.revitapidocs.com/2018/9d2dc6b3-21a7-5362-2bf5-2cb11b42c2d4.htm
        /// </summary>
        /// <remarks>
        /// OnMaterial method can be invoked for every single out-coming mesh
        /// even when the material has not actually changed. Thus it is usually
        /// beneficial to store the current material and only get its attributes
        /// when the material actually changes.
        /// </remarks>
        public void OnMaterial(MaterialNode node)
        {
            var mid = node.MaterialId;
            var muid = string.Empty;
            if (ElementId.InvalidElementId != mid)
            {
                var m = doc.GetElement(node.MaterialId) as Material;
                if (m != null)
                {
                    muid = m.UniqueId;
                    var materialmodel = new MaterialModel()
                    {
                        uuid = muid,
                        name = m.Name,
                        type = "MeshPhongMaterial",
                        color = Utils.ColorToInt(m.Color),
                        ambient = Utils.ColorToInt(m.Color),
                        emissive = 0,
                        specular = Utils.ColorToInt(m.Color),
                        shininess = 1,
                        opacity = 0.01 * (double)(100 - m.Transparency),
                        transparent = 0 < m.Transparency,
                        wireframe = false
                    };
                    if (!materials.ContainsKey(muid))
                    {
                        materials.Add(muid, materialmodel);
                    }
                }
            }
        }

        /// <summary>
        /// This code demonstrates how to process face geometry
        /// https://www.revitapidocs.com/2018/9a9f37ae-c8c2-7355-2c3b-f26762951f1d.htm
        /// </summary>
        /// <remarks>
        /// This method is invoked only if the custom exporter was set to include faces.
        /// </remarks>
        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            // Get the get the actual geometric face and all information about it
            // and its edges by using standard API for Face and Edge
            Face theFace = node.GetFace();
            double area = theFace.Area;
            if (theFace.HasRegions)
            {
                IList<Face> regionedFaces = theFace.GetRegions();
            }

            // We can either skip this face or proceed with rendering it depending on 
            // whether our export process can handle face geometry or not. If we choose 
            // to proceed, we get calls to export tessellated meshes for this face.
            if (true == ExportAFace(theFace))
            {
                return RenderNodeAction.Skip;
            }
            Debug.WriteLine($"OnFaceBegin: {node.NodeName}");
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// Assuming this would be the method that processes faces and exports them in our proprietary format.
        /// https://www.revitapidocs.com/2018/9a9f37ae-c8c2-7355-2c3b-f26762951f1d.htm
        /// </summary>
        /// <remarks>
        /// For example, we can decide that our format supports planar faces only, but no curved surfaces.
        /// Or we can support basic surfaces only (planar, spherical, cylindrical), but not complex faces.
        /// This is, naturally, depending on what a particular custom exporter is designed to output.
        /// </remarks>
        /// <returns>
        /// Should return True if the face could be handled (exported), False otherwise.
        /// </returns>
        private bool ExportAFace(Face face)
        {
            return false;  // in this case, 
        }

        public void OnPolymesh(PolymeshTopology node)
        {
            var pts = node.GetPoints();
            var t = tfStack.Peek();
            pts = pts.Select(p => t.OfPoint(p)).ToList();

            List<XYZ> xyzlist;
            List<int> indexlist;
            if (currentobject != null)
            {
                var uuid = currentobject.children.FirstOrDefault().uuid;
                if (XYZs.ContainsKey(uuid))
                {
                    xyzlist = XYZs[uuid];
                    indexlist = Indexs[uuid];
                }
                else
                {
                    xyzlist = new List<XYZ>();
                    XYZs.Add(uuid, xyzlist);
                    indexlist = new List<int>();
                    Indexs.Add(uuid, indexlist);
                }
                foreach (PolymeshFacet facet in node.GetFacets())
                {
                    var v1 = facet.V1;
                    var pv1 = pts[v1];
                    var matcheditem = xyzlist.FirstOrDefault(x => x.IsAlmostEqualTo(pv1));
                    if (matcheditem != null)
                    {
                        indexlist.Add(xyzlist.IndexOf(matcheditem));
                    }
                    else
                    {
                        xyzlist.Add(pv1);
                        indexlist.Add(xyzlist.Count - 1);
                    }


                    var v2 = facet.V2;
                    var pv2 = pts[v2];
                    matcheditem = xyzlist.FirstOrDefault(x => x.IsAlmostEqualTo(pv2));
                    if (matcheditem != null)
                    {
                        indexlist.Add(xyzlist.IndexOf(matcheditem));
                    }
                    else
                    {
                        xyzlist.Add(pv2);
                        indexlist.Add(xyzlist.Count - 1);
                    }

                    var v3 = facet.V3;
                    var pv3 = pts[v3];
                    matcheditem = xyzlist.FirstOrDefault(x => x.IsAlmostEqualTo(pv3));
                    if (matcheditem != null)
                    {
                        indexlist.Add(xyzlist.IndexOf(matcheditem));
                    }
                    else
                    {
                        xyzlist.Add(pv3);
                        indexlist.Add(xyzlist.Count - 1);
                    }
                }

                XYZs[uuid] = xyzlist;
                Indexs[uuid] = indexlist;
            }
        }

        // <summary>
        /// This code marks the end of processing a face
        /// https://www.revitapidocs.com/2018/9a9f37ae-c8c2-7355-2c3b-f26762951f1d.htm
        /// </summary>
        /// <remarks>
        /// This method is invoked only if the custom exporter was set to include faces.
        /// </remarks>
        public void OnFaceEnd(FaceNode node)
        {
            // Note: This method is invoked even for faces that were skipped.
            Debug.WriteLine($"OnFaceEnd:{node.NodeName}");
        }

        public void OnElementEnd(ElementId elementId)
        {
            var elem = doc.GetElement(elementId);
            string uuid = elem.UniqueId;
            var currentelemmaterialuuid = string.Empty;
            if (elem.Category != null && elem.Category.Material != null)
            {
                currentelemmaterialuuid = elem.Category.Material.UniqueId;
            }
            else
            {
                currentelemmaterialuuid = orignmaterialuuid;
            }
            var elem_per_material = $"{uuid}-{currentelemmaterialuuid}";

            if (isrvt)
            {
                if (elem.Category == null || !Utils.HasSolid(elem) || elem is ElementType)
                {
                    return;
                }
            }

            if (XYZs.ContainsKey(elem_per_material))
            {
                var positionarray = new List<double>();
                var xyzlist = XYZs[elem_per_material];
                foreach (var xyz in xyzlist)
                {
                    var p = new PointInt(xyz, switch_coordinates);
                    positionarray.Add(scale_vertex * p.X);
                    positionarray.Add(scale_vertex * p.Y);
                    positionarray.Add(scale_vertex * p.Z);
                }

                var indexlist = Indexs[elem_per_material];

                var currentgeometry = new GeometryModel()
                {
                    uuid = currentobject.children.FirstOrDefault().uuid,
                    type = "BufferGeometry",
                };
                var geometrydata = new GeometryDataModel()
                {
                    attributes = new AttributeModel()
                    {
                        position = new GeometryBasicModel()
                        {
                            itemSize = 3,
                            type = "Float32Array",
                            array = positionarray,
                            normalized = false,
                        },
                    },
                    index = new IndexModel()
                    {
                        array = indexlist,
                        type = "Uint16Array",
                    },
                };

                currentgeometry.data = geometrydata;

                var muid = currentobject.children.FirstOrDefault().material;
                if (!geometries.ContainsKey(elem_per_material))
                {
                    geometries.Add(elem_per_material, currentgeometry);
                }
            }
            if (geometries.ContainsKey(elem_per_material) && 
                cacheobjects.ContainsKey(elem_per_material) &&
                !objects.ContainsKey(elem_per_material))
            {
                objects.Add(elem_per_material, cacheobjects[elem_per_material]);
            }
        }

        /// <summary>
        /// This method marks the start of processing a family instance 
        /// https://www.revitapidocs.com/2018/2db35bdb-8d14-a015-9bfb-9283f503edab.htm
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            // We can get particular information about the family instance and its type if we need to
            ElementId symbolId = node.GetSymbolId();
            FamilySymbol famSymbol = doc.GetElement(symbolId) as FamilySymbol;

            // Typically, an export context has to manage a stack of transformation
            // for all nested objects, such as instances, lights, links, etc.
            // A combined transformation needs to be applied to the incoming geometry
            // (providing all geometry is to be flattened in the resultant format.)
            tfStack.Push(tfStack.Peek().Multiply(node.GetTransform()));
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            Debug.WriteLine($"OnInstanceEnd:{node.NodeName}");
        }

        /// <summary>
        /// https://www.revitapidocs.com/2018/5df27a2c-ae84-2d1d-635e-107ec0525ebb.htm
        /// </summary>
        /// <param name="elementId"></param>
        public void OnViewEnd(ElementId elementId)
        {
            Debug.WriteLine($"OnViewEnd: Id: {elementId.IntegerValue}");
        }

        /// <summary>
        /// https://www.revitapidocs.com/2018/68714169-e994-41e3-f1c6-8f929b40565f.htm
        /// </summary>
        public void Finish()
        {
            container.geometries = geometries.Values.ToList();
            container.materials = materials.Values.ToList();
            container.obj.children = objects.Values.ToList();

            var setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            var result = JsonConvert.SerializeObject(container, setting);
            File.WriteAllText(filename, result);
        }

        /// <summary>
        /// https://www.revitapidocs.com/2018/31f0b662-81a1-89b8-ab2a-0de99af3b753.htm
        /// </summary>
        /// <returns></returns>

        public bool IsCanceled()
        {
            return false;
        }


        /// <summary>
        /// This method is called for instances of lights
        /// https://www.revitapidocs.com/2018/d56129ca-950b-34fc-89ac-f0fb2e7fe9f2.htm
        /// </summary>
        /// <remarks>
        /// The Light API can be used to get more information about each particular light
        /// </remarks>
        public void OnLight(LightNode node)
        {
            // Obtain local transform data of the light object.
            var lightTrf = node.GetTransform();

            // Note: 1. If your light coordinate system differs from the one in REvit, 
            //   The light's local transform should be adjusted to reflect the difference.

            // Note 2. This local transform describes the light source position and light direction data.
            //   It means the "TiltAngle" property of a spot light has already been accounted for.

            // If a stack of transformation is maintained by the context object,
            // the current combined transform will be multiplied by the local transform.
            var lightWorldTrf = tfStack.Peek().Multiply(lightTrf);
            Debug.WriteLine($"OnLight:{node.NodeName}");
        }


        /// <summary>
        /// This code demonstrates how to process instances of Revit links.
        /// https://www.revitapidocs.com/2018/40d99b4a-e6aa-42d7-18ff-b546d1a5154e.htm
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            // We can get more information about the family instance and its type if we need to
            var symbolId = node.GetSymbolId();
            var linkType = doc.GetElement(symbolId) as RevitLinkType;
            var linkDocumentName = linkType.Name;

            // Typically, an export context has to manage a stack of transformation
            // for all nested objects, such as instances, lights, links, etc.
            // A combined transformation needs to be applied to the incoming geometry
            // (providing all geometry is to be flattened in the resultant format.)
            tfStack.Push(tfStack.Peek().Multiply(node.GetTransform()));

            // We can either skip this link instance or proceed with rendering it
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// This method marks the end of processing a Revit link
        /// https://www.revitapidocs.com/2018/40d99b4a-e6aa-42d7-18ff-b546d1a5154e.htm
        /// </summary>
        /// <param name="node"></param>
        public void OnLinkEnd(LinkNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            // If we maintain a transformation stack, we need to remove the latest one from it.
            tfStack.Pop();
            Debug.WriteLine($"OnLinkEnd:{node.NodeName}");
        }

        /// <summary>
        /// This method is only called for photo-rendering export 
        /// (a custom exporter that implements IPhotoRenderContext ). 
        /// When an RPC object is encountered for a model context export 
        /// (a custom exporter that implements IModelExportContext ), 
        /// the RPC object will be provided as a polymesh (via OnPolymesh(PolymeshTopology) ).
        /// https://www.revitapidocs.com/2018/f84574d9-ef15-c317-c6dd-91304a0c174c.htm
        /// </summary>
        /// <param name="node"></param>
        public void OnRPC(RPCNode node)
        {
            Debug.WriteLine($"OnRPC:{node.NodeName}");
        }
    }
}
