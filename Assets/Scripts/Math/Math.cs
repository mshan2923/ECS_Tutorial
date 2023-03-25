using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math
{
    /// <summary>
    /// Return To Degrees
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    public static float Dot(Vector3 A, Vector3 B)
    {
        return Mathf.Acos(Vector3.Dot(A, B)) * Mathf.Rad2Deg;
    }
    /// <summary>
    /// Closest On infinity Length Line
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="Target"></param>
    /// <returns></returns>
    public static Vector3 ClosePointOnDirection(Vector3 origin, Vector3 direction, Vector3 Target)
    {
        return origin + Vector3.Project(Target - origin, direction);
        // line_start + Vector3.Project(point - line_start, line_end - line_start);

        /*
        Vector3 lhs = Target - origin;
        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
        *///=====Other Way
    }

    // For finite lines:
    public static Vector3 ClosePointOnLine(Vector3 origin, Vector3 end, Vector3 target)
    {
        Vector3 direction = end - origin;
        float length = direction.magnitude;
        direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(target - origin, direction), 0f, length);
        return origin + direction * project_length;
    }
    public static bool InRange(float vaule, float min, float max, bool equalPass = true)
    {
        if (equalPass)
        {
            return vaule >= min && vaule <= max;
        }else
        {
            return vaule > min && vaule < max;
        }
    }
    public static bool InRange(Vector3 vaule, Vector3 min, Vector3 max, bool equalPass = true)
    {
        return InRange(vaule.x, min.x, max.x, equalPass)
            && InRange(vaule.y, min.y, max.y, equalPass)
            && InRange(vaule.z, min.z, max.z, equalPass);
    }
    public static Vector3 Direction2Rotation(Vector3 Dir)
    {
        /*
        Vector3 Lrot = Vector3.zero;

        Lrot.y = Mathf.Acos(Vector3.Normalize(new Vector3(Dir.x, 0, Dir.z)).x) * Mathf.Rad2Deg;
        if (Dir.z < 0)
        {
            Lrot.y = 360 - Lrot.y;
        }

        float XZLength = new Vector3(Dir.x, 0, Dir.z).magnitude;
        float YOffset = Mathf.Abs(1 / Mathf.Max(Mathf.Sin(Lrot.y * Mathf.Deg2Rad), Mathf.Cos(Lrot.y * Mathf.Deg2Rad)));

        //Lrot.z = Mathf.Atan(Dir.y) * Mathf.Rad2Deg;//이건 45 이상으로 안됨
        Lrot.z = Mathf.Atan((Dir.y * YOffset) / XZLength) * Mathf.Rad2Deg;

        return Lrot;*/

        return Quaternion.LookRotation(Dir).eulerAngles;
    }
    public static Vector3 Rotation2Direction(Vector3 Rot)
    {
        /*
        Vector3 temp = new Vector3(Cos2(Rot.y), (Sin2(Rot.z) / Cos2(Rot.z)), Sin2(Rot.y));
        float YawRate = Mathf.Min(Mathf.Abs(temp.x), Mathf.Abs(temp.z)) / Mathf.Max(Mathf.Abs(temp.x), Mathf.Abs(temp.z));

        if (Mathf.Abs(temp.x) > Mathf.Abs(temp.z))
        {
            return new Vector3((temp.x > 0 ? 1 : -1), temp.y, (temp.z > 0 ? YawRate : YawRate * -1));
        }
        else
        {
            return new Vector3((temp.x > 0 ? YawRate : YawRate * -1), temp.y, (temp.z > 0 ? 1 : -1));
        }*/

        return Quaternion.Euler(Rot) * Vector3.forward;
    }

    public static float Sin2(float Degreese)
    {
        return Mathf.Sin(Degreese * Mathf.Deg2Rad);
    }
    public static float Cos2(float Degreese)
    {
        return Mathf.Cos(Degreese * Mathf.Deg2Rad);
    }

    public static float Pow2(float vaule)
    {
        return vaule * vaule;
    }
    public static Vector2[] ContactTwoCircle(Vector2 Origin, float OriginRadius, Vector2 Target, float TargetRadius)
    {
        //https://blog.daum.net/chamtech/8891791

        Vector2[] result = new Vector2[2];

        float Dis = (Origin - Target).magnitude;

        if (Dis <= (OriginRadius + TargetRadius))
        {
            float gaping = Mathf.Acos((Pow2(OriginRadius) - Pow2(TargetRadius) + Pow2(Dis)) / (2 * OriginRadius * Dis)) * Mathf.Rad2Deg;
            //T1

            float Dot = Mathf.Atan2((Target.y - Origin.y), (Target.x - Origin.x)) * Mathf.Rad2Deg;//T2

            //Debug.Log("Angle : (" + gaping + " + " + Dot + ")");
            result[0].x = Origin.x + OriginRadius * Cos2(gaping + Dot);
            result[0].y = Origin.y + OriginRadius * Sin2(gaping + Dot);
            //Debug.Log(Origin + " + " + OriginRadius + " * (" + Cos2(gaping + Dot) + " , " + Sin2(gaping + Dot) + " )");
            //T3 , T4

            result[1].x = Origin.x + OriginRadius * Cos2(Dot - gaping);
            result[1].y = Origin.x + OriginRadius * Sin2(Dot - gaping);
            //T5 , T6

            return result;
        }else
        {
            Debug.Log("Not Contect / " + (OriginRadius + TargetRadius) + " / " + Dis);
            return null;
        }
    }
    /// <summary>
    /// LookDirection is Local / Get LookSize
    /// </summary>
    /// <param name="bound"></param>
    /// <param name="LookDirection"></param>
    /// <returns></returns>
    public static Rect Bound2Rect(Bounds bound, Quaternion LookDirection)
    {
        Rect result = new Rect();
        Vector3 Look = LookDirection.eulerAngles;
        Vector3 Size = bound.extents * 2;

        result.width = Size.x * Mathf.Abs(Cos2(Look.y)) + Size.z * Mathf.Abs(Sin2(Look.y));

        float Depth = Size.x * Mathf.Abs(Sin2(Look.y)) + Size.z * Mathf.Abs(Cos2(Look.y));
        //Debug.Log("X side Depth : " + Mathf.Abs(Sin2(Look.y)) + "Z side Depth : " + Mathf.Abs(Cos2(Look.y)));//정면인상태에서 Yaw 할때 Bound의 깊이

        result.height = Size.y * Mathf.Abs(Cos2(Look.x)) + Depth * Mathf.Abs(Sin2(Look.x));

        return result;
    }
    public static bool InArea(Bounds area, Vector3 Pos)
    {
        return InArea(area.center, area.extents, Pos);
    }
    public static bool InArea(Vector3 areaCenter, Vector3 areaExtent, Vector3 Pos)
    {
        return Math.InRange(Pos, areaCenter - (areaExtent), areaCenter + (areaExtent));
    }
    public static bool InCircle(Bounds area, Vector3 Pos)
    {
        // (x - c)^2 / a^2 + (y - d)^2 / b^2 + (z - e)^2 / c^2 = 1
        // a : area.Extents.x  * 0.5f / b : area.Extents.y  * 0.5f / c : area.Extents.z  * 0.5f

        //(Pos.x * Pos.x) / (area.extents.x * 0.5f * area.extents.x * 0.5f) + (Pos.z * Pos.z) / (area.extents.z * 0.5f * area.extents.z * 0.5f)

        return Math.Pow2(Pos.x - area.center.x) / Math.Pow2(area.extents.x) + Math.Pow2(Pos.z - area.center.z) / Math.Pow2(area.extents.z) <= 1;
    }
    /// <summary>
    /// Apply Origin Rotation
    /// </summary>
    public static bool InCircle(Bounds bounds, float Yaw, Vector3 Target)
    {
        return InCircle(bounds.center, bounds.extents, Yaw, Target);
    }
    /// <summary>
    /// Apply Origin Rotation
    /// </summary>
    public static bool InCircle(Vector3 Origin, Vector3 HalfSize, float Yaw, Vector3 Target)
    {
        var forwardDir = Quaternion.Euler(new Vector3(0, Yaw, 0)) * Vector3.forward;

        var BorderDis = Mathf.Lerp(HalfSize.x, HalfSize.z,
            Mathf.Abs(Vector3.Dot(forwardDir, (Target - Origin).normalized)));

        return (Target - Origin).sqrMagnitude <= Math.Pow2(BorderDis);
    }
    public static float CircleRadiusRate(Bounds area, Vector3 Pos)
    {
        return Math.Pow2(Pos.x - area.center.x) / Math.Pow2(area.extents.x) + Math.Pow2(Pos.z - area.center.z) / Math.Pow2(area.extents.z);
    }

    public static float AngleNormalize(float Angle)
    {
        var NorAngle = Mathf.Abs(Mathf.Abs(Angle) < 360 ? Angle : Angle % 360);

        if (NorAngle > 180)
        {
            NorAngle -= 360;
        }//Normalize Angle (-180 ~ 180)

        return NorAngle;
    }
    static float SideToDistance(float Angle, float MaxSizeRot, float HalfLength)
    {
        var temp = Mathf.Abs(Mathf.Abs(Angle) < 360 ? Angle : Angle % 360);

        if (temp > 180)
        {
            temp -= 360;
        }//Normalize Angle (-180 ~ 180)

        temp = Mathf.Abs(temp);//좌우 미러

        //float Rate = 0;

        if (temp < 90)
        {
            if (temp < MaxSizeRot)
            {
                //Rate = temp / MaxSizeRot;
                //return Mathf.Lerp(0, MaxSizeRot, Rate);
                return HalfLength / Math.Cos2(Mathf.Lerp(0, MaxSizeRot, (temp / MaxSizeRot)));
            }
            else
            {
                //Rate = 1 - ((temp - MaxSizeRot) / (90 - MaxSizeRot));
                //return Mathf.Lerp(0, 90 - MaxSizeRot, Rate);
                return HalfLength / Math.Cos2(Mathf.Lerp(0, 90 - MaxSizeRot,
                    (1 - ((temp - MaxSizeRot) / (90 - MaxSizeRot)))));
            }
        }
        else
        {
            if (temp < (180 - MaxSizeRot))
            {
                //Rate = ((temp - 90) / (90 - MaxSizeRot));
                //return Mathf.Lerp(0, MaxSizeRot, Rate);
                return HalfLength / Math.Cos2(Mathf.Lerp(0, 90 - MaxSizeRot, ((temp - 90) / (90 - MaxSizeRot))));
            }
            else
            {
                //Rate = (Mathf.Abs(temp - 180) / MaxSizeRot);
                //return Mathf.Lerp(0, 90 - MaxSizeRot, Rate);
                return HalfLength / Math.Cos2(Mathf.Lerp(0, MaxSizeRot, (Mathf.Abs(temp - 180) / MaxSizeRot)));
            }
        }
    }
    /// <summary>
    /// Distance For (Origin ~ ClosestPoint In Quad)
    /// </summary>
    public static float ClosestQuadToDistance(float Wide, float Height, float Rotation)
    {
        float MaxSizeRot = Mathf.Atan2(Wide * 0.5f, Height * 0.5f) * Mathf.Rad2Deg;

        var NorAngle = Mathf.Abs(Mathf.Abs(Rotation) < 360 ? Rotation : Rotation % 360);

        if (NorAngle > 180)
        {
            NorAngle -= 360;
        }//Normalize Angle (-180 ~ 180)

        NorAngle = Mathf.Abs(NorAngle);//좌우 미러

        if (NorAngle < MaxSizeRot || (180 - MaxSizeRot) < NorAngle)
        {
            return SideToDistance(NorAngle, MaxSizeRot, Height * 0.5f);
        }
        else
        {
            return SideToDistance(NorAngle, MaxSizeRot, Wide * 0.5f);
        }
    }
    /// <summary>
    /// Distance For (Origin ~ ClosestPoint In Quad)
    /// </summary>
    public static float ClosestQuadToDistance(Vector3 Size, Vector3 Origin, float Yaw, Vector3 Target)
    {
        float LookAngle = Math.Dot(YawToDirection(Yaw), (Target - Origin).normalized);
        return ClosestQuadToDistance(Size.x, Size.z, LookAngle);
    }
    public static Vector3 ClosestQuad(Vector3 Size, Vector3 Origin, float Yaw, Vector3 Target)
    {
        var CPoint = (Target - Origin).normalized * ClosestQuadToDistance(Size, Origin, Yaw, Target);

        return Origin + CPoint;
    }
    /// <summary>
    /// Apply Origin Rotation
    /// </summary>
    public static bool InQuad(Vector3 Size, Transform Origin, Vector3 Target)
    {
        //================================================ Rotation 은 Yaw만
        return Vector3.SqrMagnitude(Target - Origin.position) <=
            Math.Pow2(ClosestQuadToDistance(Size, Origin.position, Origin.rotation.eulerAngles.y, Target));
    }
    /// <summary>
    /// Apply Origin Rotation
    /// </summary>
    public static bool InQuad(Vector3 Size, Vector3 Origin, Quaternion Rotation, Vector3 Target)
    {
        return Vector3.SqrMagnitude(Target - Origin) <=
            Math.Pow2(ClosestQuadToDistance(Size, Origin, Rotation.eulerAngles.y, Target));
    }


    [System.Obsolete("Recommand use RotatedQuadSize")]
    public static Vector3 RotatedBound2D(Bounds bounds, float diagonal, float Yaw)
    {
        Vector3 RotatedBound = Vector3.zero;
        var YAngle = Quaternion.Euler(0, Yaw, 0);

        Vector3 MaxPos = (Quaternion.LookRotation(bounds.extents) * YAngle) * Vector3.forward * diagonal;

        //Debug.DrawLine(bounds.center - MaxPos, bounds.center + MaxPos, Color.green, Time.deltaTime);
        RotatedBound = new Vector3(Mathf.Abs(MaxPos.x),
            0,//Mathf.Abs(MaxPos.y),
            Mathf.Abs(MaxPos.z));

        MaxPos = (Quaternion.LookRotation(new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z * -1)) * YAngle) * Vector3.forward * diagonal;
        //Debug.DrawLine(bounds.center - MaxPos, bounds.center + MaxPos, Color.red, Time.deltaTime);

        RotatedBound = new Vector3(Mathf.Max(RotatedBound.x, Mathf.Abs(MaxPos.x)),
            0,//Mathf.Max(RotatedBound.y, Mathf.Abs(MaxPos.y)),
            Mathf.Max(RotatedBound.z, Mathf.Abs(MaxPos.z)));

        return RotatedBound;
    }
    public static Vector2 RotatedQuadSize(Vector2 Extents, float Yaw)
    {
        Quaternion rotate = Quaternion.AngleAxis(Yaw, Vector3.up);

        Vector3 FR = rotate * (Vector3.right * Extents.x) + rotate * (Vector3.forward * Extents.y);
        Vector3 BL = rotate * (Vector3.left * Extents.x) + rotate * (Vector3.back * Extents.y);

        Vector3 FL = rotate * (Vector3.left * Extents.x) + rotate * (Vector3.forward * Extents.y);
        Vector3 BR = rotate * (Vector3.right * Extents.x) + rotate * (Vector3.back * Extents.y);

        return new Vector2(Mathf.Max(Mathf.Abs(FR.x - BL.x), Mathf.Abs(FL.x - BR.x)),
                            Mathf.Max(Mathf.Abs(FR.z - BL.z), Mathf.Abs(FL.z - BR.z))) * 0.5f;
    }
    public static Vector3 RotatedQuadSize(Vector3 Extents, float Yaw)
    {
        Quaternion rotate = Quaternion.AngleAxis(Yaw, Vector3.up);

        Vector3 FR = rotate * (Vector3.right * Extents.x) + rotate * (Vector3.forward * Extents.z);
        Vector3 BL = rotate * (Vector3.left * Extents.x) + rotate * (Vector3.back * Extents.z);

        Vector3 FL = rotate * (Vector3.left * Extents.x) + rotate * (Vector3.forward * Extents.z);
        Vector3 BR = rotate * (Vector3.right * Extents.x) + rotate * (Vector3.back * Extents.z);

        return new Vector3(Mathf.Max(Mathf.Abs(FR.x - BL.x), Mathf.Abs(FL.x - BR.x)), 0,
                            Mathf.Max(Mathf.Abs(FR.z - BL.z), Mathf.Abs(FL.z - BR.z))) * 0.5f;
    }
    public static Vector3 Snap(Vector3 pos, float gridSize)
    {
        float multiplied = 1 / gridSize;
        return new Vector3(Mathf.Round(pos.x * multiplied),
            Mathf.Round(pos.y * multiplied), Mathf.Round(pos.z * multiplied)) * gridSize;
    }
    public static Vector3 YawToDirection(float Yaw)
    {
        return new Vector3(Math.Sin2(Yaw), 0, Math.Cos2(Yaw));
    }

    public static Vector3[] GetCloestLines(Vector3 Point1, Vector3 Vec1, Vector3 Point2, Vector3 Vec2, float intersectLength = 0.01f)
    {
        Vector3 Cloest1 = Vector3.zero;
        Vector3 Cloest2 = Vector3.zero;

        {
            Vector3 Vec3 = Point2 - Point1;
            Vector3 crossVec1and2 = Vector3.Cross(Vec1, Vec2);
            Vector3 crossVec3and2 = Vector3.Cross(Vec3, Vec2);

            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            Cloest1 = Point1 + (Vec1 * s);
        }
        {
            Vector3 vec3 = Point1 - Point2;
            Vector3 cross1N2 = Vector3.Cross(Vec2, Vec1);
            Vector3 cross3N1 = Vector3.Cross(vec3, Vec1);

            float t = Vector3.Dot(cross3N1, cross1N2) / cross1N2.sqrMagnitude;
            Cloest2 = Point2 + Vec2 * t;
        }

        if ((Cloest1 - Cloest2).sqrMagnitude < (intersectLength * intersectLength))
        {
            return new Vector3[] { Cloest1 };
        }
        else
        {
            return new Vector3[] { Cloest1, Cloest2 };
        }
    }
    public static float GetCloestLinesDistance(Vector3 Point1, Vector3 Vec1, Vector3 Point2, Vector3 Vec2, float intersectLength = 0.01f, bool sqr = true)
    {
        Vector3 Cloest1 = Vector3.zero;
        Vector3 Cloest2 = Vector3.zero;

        {
            Vector3 Vec3 = Point2 - Point1;
            Vector3 crossVec1and2 = Vector3.Cross(Vec1, Vec2);
            Vector3 crossVec3and2 = Vector3.Cross(Vec3, Vec2);

            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            Cloest1 = Point1 + (Vec1 * s);
        }
        {
            Vector3 vec3 = Point1 - Point2;
            Vector3 cross1N2 = Vector3.Cross(Vec2, Vec1);
            Vector3 cross3N1 = Vector3.Cross(vec3, Vec1);

            float t = Vector3.Dot(cross3N1, cross1N2) / cross1N2.sqrMagnitude;
            Cloest2 = Point2 + Vec2 * t;
        }

        if ((Cloest1 - Cloest2).sqrMagnitude < (intersectLength * intersectLength))
        {
            return 0;
        }
        else
        {
            return sqr ? (Cloest1 - Cloest2).sqrMagnitude : (Cloest1 - Cloest2).magnitude;
        }
    }
    /// <summary>
    /// 두백터의 충돌 반사각
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetCollisionReflect(Quaternion Target, Quaternion Other, float TargetMass, float OtherMass)
    {
        var AddVecForward = (Target * Vector3.forward * TargetMass + Other * Vector3.forward * OtherMass).normalized;
        var AddVecUp = (Target * Vector3.up * TargetMass + Other * Vector3.up * OtherMass).normalized;

        var AddVecRot = Quaternion.LookRotation(AddVecForward, AddVecUp);

        return Quaternion.LookRotation(Vector3.Reflect(Target * Vector3.forward, AddVecRot * Vector3.right)) * Vector3.forward * (TargetMass / (TargetMass + OtherMass));
    }// 충돌을 했을때 기준 방향
    public static bool GetSphereNormal(Vector3 TargetPos, Vector3 ProjectPos, Vector3 ProjectDir, float targetRadius, out Quaternion Normal)
    {
        var dis = (TargetPos - ProjectPos).magnitude;
        var ProjectDot = Vector3.Dot(ProjectDir, (ProjectPos - TargetPos).normalized);
        var ProjectRad = Mathf.Acos(ProjectDot);

        var OthoLength = dis * Mathf.Sin(ProjectRad);
        if (OthoLength > targetRadius)
        {
            Normal = Quaternion.identity;
            return false;
        }

        var ProjectLength = dis * Mathf.Cos(ProjectRad);
        var ProjectForwardPos = TargetPos + ProjectDir * ProjectLength;
        var ProjectUp = (ProjectForwardPos + (ProjectPos - ProjectForwardPos).normalized * OthoLength) - ProjectForwardPos;

        var RotationAxis = Quaternion.LookRotation(ProjectDir * -1, ProjectUp);
        var CollisionRad = Mathf.Asin(OthoLength / targetRadius);

        Normal = RotationAxis * Quaternion.AngleAxis(CollisionRad * Mathf.Rad2Deg * -1, Vector3.right);
        return true;
    }
}
