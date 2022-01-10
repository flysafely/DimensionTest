using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace LinearDimension
{
    [Transaction(TransactionMode.Manual)]
    public class LinearDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Wall wall = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element)) as Wall;
            if (wall != null)
            {
                ReferenceArray refArry = new ReferenceArray();
                Line wallLine =(wall.Location as LocationCurve).Curve as Line;
                XYZ wallDir = ((wall.Location as LocationCurve).Curve as Line).Direction;
                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.DetailLevel = ViewDetailLevel.Fine;
                GeometryElement gelem = wall.get_Geometry(opt);
                foreach (GeometryObject gobj in gelem)
                {
                    if (gobj is Solid)
                    {
                        Solid solid = gobj as Solid;
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace)
                            {
                                XYZ faceDir =face.ComputeNormal(new UV());
                                if (faceDir.IsAlmostEqualTo(wallDir)||faceDir.IsAlmostEqualTo(-wallDir))
                                {
                                    refArry.Append(face.Reference);
                                }
                            }
                        }
                    }
                }
                Transaction trans = new Transaction(doc, "trans");
                trans.Start();
                doc.Create.NewDimension(doc.ActiveView, wallLine, refArry);
                doc.Regenerate();
                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}