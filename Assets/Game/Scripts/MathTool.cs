using UnityEngine;

public static class MathTool
{
    public static bool SolveCircle(Vector2 cA, float rA, Vector2 cB, float rB, float rC, out Vector2 p1, out Vector2 p2)
    {
        var dA = rA + rC;
        var dB = rB + rC;

        var AB = cB - cA;
        var d = AB.magnitude;
        if (d <= 0.0001f)
        {
            p1 = p2 = Vector2.zero;
            return false;
        }

        var a = (dA * dA - dB * dB + d * d) / (2f * d);
        var hSq = dA * dA - a * a;

        if (hSq < 0)
        {
            p1 = p2 = Vector2.zero;
            return false;
        }

        var h = Mathf.Sqrt(hSq);
        var P = cA + a * AB / d;
        var perp = new Vector2(-AB.y, AB.x) / d;

        p1 = P + perp * h;
        p2 = P - perp * h;

        return true;
    }

    public static float GetGearAngleCorrection(int teethA, int teethB, float thetaA, float thetaB, Vector2 direction)
    {
        float alpha = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float stepA = 360f / teethA;
        float stepB = 360f / teethB;

        float contactA = alpha;
        float contactB = alpha + 180f;

        float phaseA = (thetaA - contactA) / stepA;
        float phaseB = (thetaB - contactB) / stepB;

        float targetPhaseB = -phaseA + 0.5f;
        float deltaPhase = RepeatCentered(targetPhaseB - phaseB);

        return deltaPhase * stepB;
    }

    public static float RepeatCentered(float x)
    {
        x = x - Mathf.Floor(x); // [0,1)
        if (x > 0.5f) x -= 1f;  // [-0.5, 0.5]
        return x;
    }

    public static Vector2 Aim(float start, float target, float velocity, float gravity, float height)
    {
        var vy = Mathf.Sqrt(2f * gravity * height);
        var time = (vy / gravity) * 2f;

        for (int i = 0; i < 3; i++)
        {
            var futureX = target + velocity * time;
            var vx = (futureX - start) / time;

            var newTime = (futureX - start) / vx;
            time = newTime;
        }

        var finalFutureX = target + velocity * time;
        var finalVx = (finalFutureX - start) / time;

        return new Vector2(finalVx, vy);
    }
}