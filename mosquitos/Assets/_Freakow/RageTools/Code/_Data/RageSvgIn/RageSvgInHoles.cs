using UnityEngine;
using System.Collections.Generic;

public class RageSvgInHoles{

    public static void ProcessHoles(RageSvgObject pathData) {
        if (pathData.Paths.Count < 2) return;
        int j = 0;
        while (j < pathData.Paths.Count - 1) {
            var mainSpline = pathData.Paths[j].Spline;
            var subPaths = new Dictionary<int, ISpline>();
            for (int i = j+1; i < pathData.Paths.Count; i++) {
                var subSpline = pathData.Paths[i].Spline;
                if (mainSpline == subSpline) continue;
                subPaths.Add(i, subSpline);
            }
            var toRemove = ProcessHoles(mainSpline,subPaths);
            for (int i = toRemove.Count - 1; i >= 0; i--) {
                //pathData.Paths[toRemove[i]].Spline.GameObject.name = i + "_Hole_" + toRemove[i];
                pathData.Paths[toRemove[i]].Spline.GameObject.SmartDestroy();
                pathData.Paths.RemoveAt(toRemove[i]);
            }
            j++;
        }
    }

    private static List<int> ProcessHoles(ISpline mainSpline, Dictionary<int, ISpline> subSplines) {
        List<ISpline> holes = new List<ISpline>();
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, ISpline> path in subSplines) {
            if (!IsInsideSpline(mainSpline, path.Value)) continue;
            if (IsInsideSplines(subSplines, path.Value)) continue;
            holes.Add(path.Value);
            toRemove.Add(path.Key);
        }
        while (holes.Count > 0) {
            int holeIndex = FindHoleClosestToEdge(mainSpline, holes);
            MakeHole(mainSpline, holes[holeIndex]);
            holes.RemoveAt(holeIndex);
        }
        return toRemove;
    }

    private static bool IsInsideSplines(Dictionary<int, ISpline> subSplines, ISpline spline) {
        foreach (KeyValuePair<int, ISpline> path in subSplines) {
            if (path.Value == spline) continue;
            if (!IsInsideSpline(path.Value, spline)) continue;
            return true;
        }
        return false;
    }

    private static int FindHoleClosestToEdge(ISpline mainSpline, List<ISpline> holes) {
        int holeIndex = 0;
        float holeDistance = 0;
        for (int i = 0; i < holes.Count; i++) {
            float cutPointSplinePosition = 0;
            float holeCutPointSplinePosition = 0;
            Vector3 cutPointPosition = mainSpline.GetPointAt(0).Position;
            Vector3 holeCutPointPosition = holes[i].GetPointAt(0).Position;
            float cutPointDistance = 0;

            GetCutPointData(mainSpline, holes[i], ref cutPointSplinePosition, ref holeCutPointSplinePosition, ref cutPointPosition, ref holeCutPointPosition, ref cutPointDistance);

            if (i==0){
                holeDistance = cutPointDistance;
                continue;
            }

            if (cutPointDistance >= holeDistance) continue;

            holeIndex = i;
            holeDistance = cutPointDistance;
        }
        return holeIndex;
    }

    private static bool IsInsideSpline(ISpline mainSpline, ISpline subSpline) {
        if (mainSpline.PointsCount < 3) return false;
        if (subSpline.PointsCount < 3) return false;
        bool pointsInSpline = true;
        for (int i = 0; i < subSpline.PointsCount; i++){
            var point = subSpline.GetPointAt(i).Position;
            pointsInSpline = SplineContainsPoint(mainSpline, point);
            if (!pointsInSpline) break;
        }
        if (!pointsInSpline) return false;
        return true;
    }

    private static void MakeHole(ISpline mainSpline, ISpline subSpline) {
        float cutPointSplinePosition = 0;
        float holeCutPointSplinePosition = 0;
        Vector3 cutPointPosition = mainSpline.GetPointAt(0).Position;
        Vector3 holeCutPointPosition = subSpline.GetPointAt(0).Position;
        float cutPointDistance = 0;

        GetCutPointData(mainSpline, subSpline, ref cutPointSplinePosition, ref holeCutPointSplinePosition, ref cutPointPosition, ref holeCutPointPosition, ref cutPointDistance);

        int startCutPointIdx = mainSpline.Rs.spline.GetNearestSplinePointIndex(cutPointSplinePosition);
        if (Vector2.Distance(mainSpline.GetPointAt(startCutPointIdx).Position, cutPointPosition) > 0.1f)
            startCutPointIdx = mainSpline.Rs.AddPoint(cutPointSplinePosition);
        var startCutPoint = mainSpline.GetPointAt(startCutPointIdx);

        var endCutPointIdx = mainSpline.AddPointLocal(startCutPointIdx + 1, startCutPoint.PositionLocal, Vector3.zero, startCutPoint.OutTangentLocal, 1.0f, false).Index;

        startCutPoint.OutTangentLocal = Vector3.zero;

        int holeCutPointIdx = subSpline.Rs.spline.GetNearestSplinePointIndex(holeCutPointSplinePosition);
        if (Vector2.Distance(subSpline.GetPointAt(holeCutPointIdx).Position, holeCutPointPosition) > 0.1)
            holeCutPointIdx = subSpline.Rs.AddPoint(holeCutPointSplinePosition);

        if ((holeCutPointIdx != 0) && (holeCutPointIdx != subSpline.PointsCount - 1))
            while (holeCutPointIdx != 0) {
                subSpline.GetPointAt(subSpline.PointsCount - 1).OutTangentLocal = subSpline.GetPointAt(0).OutTangentLocal;
                subSpline.RemovePointAt(0);
                var firstHolePoint = subSpline.GetPointAt(0);
                subSpline.AddPointLocal(subSpline.PointsCount, firstHolePoint.PositionLocal, firstHolePoint.InTangentLocal, firstHolePoint.OutTangentLocal, 1.0f, false);
                holeCutPointIdx--;
            }

        float distanceOffset = 0.01f;
        Vector3 fowardVector = subSpline.PointsAreInClockWiseOrder() ? Vector3.forward : Vector3.back;
        Vector3 vectorOffset = Vector3.Cross(fowardVector, subSpline.GetPointAt(0).Position - startCutPoint.Position);
        vectorOffset = distanceOffset * vectorOffset.normalized;
        subSpline.GetPointAt(0).Position += vectorOffset;
        startCutPoint.Position += vectorOffset;

        for (int i = 0; i < subSpline.PointsCount; i++) {
            var holePoint = subSpline.GetPointAt(i);
            if (i == subSpline.PointsCount - 1)
                mainSpline.AddPointLocal(endCutPointIdx, holePoint.PositionLocal, holePoint.InTangentLocal, Vector3.zero, 1.0f, false);
            else
                if (i == 0)
                    mainSpline.AddPointLocal(endCutPointIdx, holePoint.PositionLocal, Vector3.zero, holePoint.OutTangentLocal, 1.0f, false);
                else
                    mainSpline.AddPointLocal(endCutPointIdx, holePoint.PositionLocal, holePoint.InTangentLocal, holePoint.OutTangentLocal, 1.0f, false);
            endCutPointIdx++;
        }

    }

    private static void GetCutPointData(ISpline mainSpline, ISpline subSpline, ref float cutPointSplinePosition, ref float holeCutPointSplinePosition, ref Vector3 cutPointPosition, ref Vector3 holeCutPointPosition, ref float cutPointDistance) {
        int mainSplineSubdiv = mainSpline.PointsCount > 10 ? mainSpline.PointsCount : 10;
        int holeSplineSubdiv = subSpline.PointsCount > 10 ? subSpline.PointsCount : 10;

        float pointStep = 1 / (float)mainSplineSubdiv;
        float holePointStep = 1 / (float)holeSplineSubdiv;

        for (float i = 0; i < 1; i += pointStep) {
            Vector3 splinePointPosition = mainSpline.Rs.spline.GetPoint(i);
            for (float j = 0; j < 1; j += holePointStep) {
                Vector3 holePointPosition = subSpline.Rs.spline.GetPoint(j);
                var currentDist = Vector2.Distance(splinePointPosition, holePointPosition);
                if ((Mathf.Approximately(i, 0)) && (Mathf.Approximately(j, 0))) cutPointDistance = currentDist;
                if (cutPointDistance <= currentDist) continue;
                cutPointDistance = currentDist;
                cutPointSplinePosition = i;
                holeCutPointSplinePosition = j;
                cutPointPosition = splinePointPosition;
                holeCutPointPosition = holePointPosition;
            }
        }
    }

    private static bool SplineContainsPoint(ISpline spline, Vector2 point) {
        var j = spline.PointsCount - 1;
        var oddNodes = false;

        for (var i = 0; i < spline.PointsCount; i++) {
            var iPoint = spline.GetPointAt(i).Position;
            var jPoint = spline.GetPointAt(j).Position;
            if (iPoint.y < point.y && jPoint.y >= point.y || jPoint.y < point.y && iPoint.y >= point.y)
                if (iPoint.x + (point.y - iPoint.y) / (jPoint.y - iPoint.y) * (jPoint.x - iPoint.x) < point.x)
                    oddNodes = !oddNodes;
            j = i;
        }

        return oddNodes;
    }
}
