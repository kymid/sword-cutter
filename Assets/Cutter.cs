using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    [SerializeField] Transform _handle, _tip, _farblade;

    List<int> _positiveSideTriangles = new List<int>();
    List<Vector3> _positiveSideVertices = new List<Vector3>();
    List<Vector2> _positiveSideUvs = new List<Vector2>();
    List<Vector3> _positiveSideNormals = new List<Vector3>();

    List<int> _negativeSideTriangles = new List<int>();
    List<Vector3> _negativeSideVertices = new List<Vector3>();
    List<Vector2> _negativeSideUvs = new List<Vector2>();
    List<Vector3> _negativeSideNormals = new List<Vector3>();

    List<Vector3> _pointsAlongPlane = new List<Vector3>();
    Plane _plane;

    bool isSkinned;
    private void ClearLists()
    {
        _positiveSideTriangles = new List<int>();
        _positiveSideVertices = new List<Vector3>();
        _positiveSideUvs = new List<Vector2>();
        _positiveSideNormals = new List<Vector3>();

        _negativeSideTriangles = new List<int>();
        _negativeSideVertices = new List<Vector3>();
        _negativeSideUvs = new List<Vector2>();
        _negativeSideNormals = new List<Vector3>();

        _pointsAlongPlane = new List<Vector3>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("CanCut")) return;
        ClearLists();

        Vector3 handlePos = _handle.position;
        Vector3 tipPos = _tip.position;
        Vector3 farbladePos = _farblade.position;

        Vector3 normal = Vector3.Cross(farbladePos - handlePos, farbladePos - tipPos).normalized;

        Vector3 transformedNormal = ((Vector3)(collision.transform.localToWorldMatrix.transpose * normal)).normalized;
        Vector3 transformedPosition = collision.transform.InverseTransformPoint(tipPos);

        _plane = new Plane(transformedNormal, transformedPosition);

        var direction = Vector3.Dot(Vector3.up,transformedNormal);

        if (direction < 0)
            _plane = _plane.flipped;

        CutDown(_plane, collision.gameObject);
    }

    void CutDown(Plane plane, GameObject cuttedObject)
    {
        Mesh mesh = new Mesh();

        if (cuttedObject.TryGetComponent(out MeshFilter filter))
        {
            mesh = filter.mesh;
            isSkinned = false;
        }
        else if (cuttedObject.TryGetComponent(out SkinnedMeshRenderer renderer))
        {
            renderer.BakeMesh(mesh);
            isSkinned = true;
        }
        else return;    

        CutMetaData(plane,mesh, true, false, false, true);

        GameObject _positiveGameObject = CreateGameObject(cuttedObject, true);
        GameObject _negativeGameObject = CreateGameObject(cuttedObject, false);

        Destroy(cuttedObject);
    }

    GameObject CreateGameObject(GameObject cuttedObject, bool isPositive)
    {
        GameObject cuttedHalf = new GameObject();

        MeshFilter filter = cuttedHalf.AddComponent<MeshFilter>();
        MeshRenderer renderer = cuttedHalf.AddComponent<MeshRenderer>();

        Material[] parentMaterials;

        if (!isSkinned)
        parentMaterials = cuttedObject.GetComponent<MeshRenderer>().materials;
        else
        parentMaterials = cuttedObject.GetComponent<SkinnedMeshRenderer>().materials;

        renderer.materials = parentMaterials;

        cuttedHalf.transform.position = cuttedObject.transform.position;
        cuttedHalf.transform.rotation = cuttedObject.transform.rotation;
        cuttedHalf.transform.localScale = cuttedObject.transform.lossyScale;

        if(isPositive)
        {
            filter.mesh.vertices = _positiveSideVertices.ToArray();
            filter.mesh.triangles = _positiveSideTriangles.ToArray();
            filter.mesh.normals = _positiveSideNormals.ToArray();
            filter.mesh.uv = _positiveSideUvs.ToArray();
        }
        else
        {
            filter.mesh.vertices = _negativeSideVertices.ToArray();
            filter.mesh.triangles = _negativeSideTriangles.ToArray();
            filter.mesh.normals = _negativeSideNormals.ToArray();
            filter.mesh.uv = _negativeSideUvs.ToArray();
        }

        Rigidbody rb = cuttedHalf.AddComponent<Rigidbody>();
        float sideForce = UnityEngine.Random.Range(.4f, .5f);
        if (isPositive)
            sideForce *= -1;

        rb.AddForce(Vector3.forward * 800 + (Vector3.left * sideForce) * 600);

        MeshCollider collider = cuttedHalf.AddComponent<MeshCollider>();
        collider.convex = true;

        cuttedHalf.AddComponent<AutoDestroy>();

        return cuttedHalf;
    }
    void CutMetaData(Plane plane, Mesh mesh, bool isSolid,
                     bool createReverseTriangle, bool shareVertices, bool smoothVertices)
    {

        int[] meshTriangles = mesh.triangles;
        Vector3[] meshVerts = mesh.vertices;
        Vector2[] meshUvs = mesh.uv;
        Vector3[] meshNormals = mesh.normals;


        for(int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector3 vert1 = meshVerts[meshTriangles[i]];
            int vert1Index = Array.IndexOf(meshVerts, vert1);
            Vector2 uv1 = meshUvs[vert1Index];
            Vector3 normal1 = meshNormals[vert1Index];
            bool vert1ArePositive = plane.GetSide(vert1);

            Vector3 vert2 = meshVerts[meshTriangles[i+1]];
            int vert2Index = Array.IndexOf(meshVerts, vert2);
            Vector2 uv2 = meshUvs[vert2Index];
            Vector3 normal2 = meshNormals[vert2Index];
            bool vert2ArePositive = plane.GetSide(vert2);

            Vector3 vert3 = meshVerts[meshTriangles[i+2]];
            int vert3Index = Array.IndexOf(meshVerts, vert3);
            Vector2 uv3 = meshUvs[vert3Index];
            Vector3 normal3 = meshNormals[vert3Index];
            bool vert3ArePositive = plane.GetSide(vert3);


            if(vert1ArePositive == vert2ArePositive & vert2ArePositive == vert3ArePositive)
            {
                SetPositiveOrNegativeTriangles(vert1ArePositive, vert1, normal1, uv1, 
                    vert2, normal2, uv2, vert3, normal3, uv3, true, false);
            }
            else
            {
                Vector3 intersection1, intersection2;
                Vector2 intersectionUv1, intersectionUv2;

                if(vert1ArePositive == vert2ArePositive)
                {
                    intersection1 = GetPlaneIntersactionPointWithUv(vert2, uv2, vert3, uv3, out intersectionUv1);
                    intersection2 = GetPlaneIntersactionPointWithUv(vert3, uv3, vert1, uv1, out intersectionUv2);

                    SetPositiveOrNegativeTriangles(vert1ArePositive, vert1, null, uv1, vert2, null, uv2, intersection1, null, intersectionUv1, shareVertices, false);
                    SetPositiveOrNegativeTriangles(vert1ArePositive, vert1, null, uv1, intersection1, null, intersectionUv1, intersection2, null, intersectionUv2, shareVertices, false);
                    SetPositiveOrNegativeTriangles(!vert1ArePositive, intersection1, null, intersectionUv1, vert3, null, uv3, intersection2, null, intersectionUv2, shareVertices, false);
                }
                else if(vert1ArePositive == vert3ArePositive)
                {
                    intersection1 = GetPlaneIntersactionPointWithUv(vert1, uv1, vert2, uv2, out intersectionUv1);
                    intersection2 = GetPlaneIntersactionPointWithUv(vert2, uv2, vert3, uv3, out intersectionUv2);

                    SetPositiveOrNegativeTriangles(vert1ArePositive, vert1, null, uv1, intersection1, null, intersectionUv1, vert3, null, uv3, shareVertices, false);
                    SetPositiveOrNegativeTriangles(vert1ArePositive, intersection1, null, intersectionUv1, intersection2, null, intersectionUv2, vert3, null, uv3, shareVertices, false);
                    SetPositiveOrNegativeTriangles(!vert1ArePositive, intersection1, null, intersectionUv1, vert2, null, uv2, intersection2, null, intersectionUv2, shareVertices, false);
                }
                else
                {
                    intersection1 = GetPlaneIntersactionPointWithUv(vert1, uv1, vert2, uv2, out intersectionUv1);
                    intersection2 = GetPlaneIntersactionPointWithUv(vert1, uv1, vert3, uv3, out intersectionUv2);

                    SetPositiveOrNegativeTriangles(vert1ArePositive, vert1, null, uv1, intersection1, null, intersectionUv1, intersection2, null, intersectionUv2, shareVertices, false);
                    SetPositiveOrNegativeTriangles(!vert1ArePositive, intersection1, null, intersectionUv1, vert2, null, uv2, vert3, null, uv3, shareVertices, false);
                    SetPositiveOrNegativeTriangles(!vert1ArePositive, intersection1, null, intersectionUv1, vert3, null, uv3, intersection2, null, intersectionUv2, shareVertices, false);
                }

                _pointsAlongPlane.Add(intersection1);
                _pointsAlongPlane.Add(intersection2);
            }

        }

        if (isSolid)
            JoinPointsAlongPLane();

        //else if(createReverseTriangle)

        //if(smoothVertices)
    }

    private void JoinPointsAlongPLane()
    {
        Vector3 firstPoint = _pointsAlongPlane[0];
        Vector3 furthestPoint = firstPoint;

        float distance = 0;

        foreach(var point in _pointsAlongPlane)
        {
            float currentdistance = Vector3.Distance(firstPoint, point);
            if(currentdistance > distance)
            {
                distance = currentdistance;
                furthestPoint = point;
            }
        }

        Vector3 halfPoint = Vector3.Lerp(firstPoint, furthestPoint, .5f);
        
        for(int i = 0; i < _pointsAlongPlane.Count; i += 2)
        {
            Vector3 firstvert = _pointsAlongPlane[i];
            Vector3 secondvert = _pointsAlongPlane[i + 1];

            Vector3 normal = Vector3.Cross(secondvert - halfPoint, firstvert - halfPoint).normalized;

            var direction = Vector3.Dot(normal, _plane.normal);

            if(direction > 0)
            {
                SetPositiveOrNegativeTriangles(true, halfPoint, -normal,Vector2.zero,firstvert,-normal,Vector2.zero,secondvert,-normal,Vector2.zero, false,true);
                SetPositiveOrNegativeTriangles(false, halfPoint, normal, Vector2.zero, secondvert, normal, Vector2.zero, firstvert, normal, Vector2.zero, false, true);
            }
            else
            {
                SetPositiveOrNegativeTriangles(false, halfPoint, -normal, Vector2.zero, firstvert, -normal, Vector2.zero, secondvert, -normal, Vector2.zero, false, true);
                SetPositiveOrNegativeTriangles(true, halfPoint, normal, Vector2.zero, secondvert, normal, Vector2.zero, firstvert, normal, Vector2.zero, false, true);
            }
        }
    }

    private void SetPositiveOrNegativeTriangles(bool isPositive,
        Vector3 vert1, Vector3? normal1, Vector2 uv1, 
        Vector3 vert2, Vector3? normal2, Vector2 uv2, 
        Vector3 vert3, Vector3? normal3, Vector2 uv3,
        bool shareVertices, bool addFirst)
    {
        if (isPositive)
            AddTrianglesNormalsUvs(ref _positiveSideVertices, ref _positiveSideTriangles,
                                   ref _positiveSideNormals,ref _positiveSideUvs,
                                   vert1, normal1, uv1, vert2, normal2, uv2, 
                                   vert3, normal3, uv3, shareVertices, addFirst);
        else
            AddTrianglesNormalsUvs(ref _negativeSideVertices, ref _negativeSideTriangles,
                                   ref _negativeSideNormals, ref _negativeSideUvs,
                                   vert1, normal1, uv1, vert2, normal2, uv2,
                                   vert3, normal3, uv3, shareVertices, addFirst);
    }

    private void AddTrianglesNormalsUvs(
        ref List<Vector3> vertices, ref List<int> triangles,
        ref List<Vector3> normals, ref List<Vector2> uvs, 
        Vector3 vert1, Vector3? normal1, Vector2 uv1,
        Vector3 vert2, Vector3? normal2, Vector2 uv2,
        Vector3 vert3, Vector3? normal3, Vector2 uv3,
        bool shareVertices, bool addFirst)
    {

        if (addFirst)
        {
            for( int i = 0; i < triangles.Count; i ++)
                triangles[i] += 3;
        }

        int Tri1Index = vertices.IndexOf(vert1);
        int Tri2Index = vertices.IndexOf(vert2);
        int Tri3Index = vertices.IndexOf(vert3);

        if(Tri1Index > -1 & shareVertices) triangles.Add(Tri1Index);
        else
        {
            if (normal1 == null) normal1 = Vector3.Cross(vert2 - vert1, vert3 - vert1);

            int index = -1;
            if (addFirst) index = 0;
            AddVertNormalUv(ref vertices, ref triangles, ref normals, ref uvs, vert1,
                            (Vector3)normal1, uv1, index);
        }

        if (Tri2Index > -1 & shareVertices) triangles.Add(Tri2Index);
        else
        {
            if (normal2 == null) normal2 = Vector3.Cross(vert3 - vert2, vert1 - vert2);

            int index = -1;
            if (addFirst) index = 1;
            AddVertNormalUv(ref vertices, ref triangles, ref normals, ref uvs, vert2,
                            (Vector3)normal2, uv2, index);
        }

        if (Tri3Index > -1 & shareVertices) triangles.Add(Tri3Index);
        else
        {
            if (normal3 == null) normal3 = Vector3.Cross(vert1 - vert3, vert2 - vert3);

            int index = -1;
            if (addFirst) index = 2;
            AddVertNormalUv(ref vertices, ref triangles, ref normals, ref uvs, vert3,
                            (Vector3)normal3, uv3, index);
        }
    }

    private void AddVertNormalUv(ref List<Vector3> vertices, ref List<int> triangles,
                                 ref List<Vector3> normals, ref List<Vector2> uvs,
                                 Vector3 vert, Vector3 normal, Vector2 uv, int index)
    {
        if(index != -1)
        {
            vertices.Insert(index, vert);
            triangles.Insert(index, index);
            normals.Insert(index, normal);
            uvs.Insert(index, uv);
        }
        else
        {
            vertices.Add(vert);
            triangles.Add(vertices.IndexOf(vert));
            normals.Add(normal);
            uvs.Add(uv);
        }
    }

    private Vector3 GetPlaneIntersactionPointWithUv(Vector3 vert1, Vector2 uv1, 
        Vector3 vert2, Vector2 uv2, out Vector2 uv)
    {
        float distance = GetDistanseRelativeToPlane(vert1, vert2, out Vector3 pointOfIntersection);
        uv = Vector2.Lerp(vert1, vert2, distance);
        return pointOfIntersection;
    }

    private float GetDistanseRelativeToPlane(Vector3 vert1, Vector3 vert2, out Vector3 pointOfIntersection)
    {
        Ray ray = new Ray(vert1, vert2 - vert1);
        _plane.Raycast(ray, out float distance);
        pointOfIntersection = ray.GetPoint(distance);
        return distance;
    }
}
