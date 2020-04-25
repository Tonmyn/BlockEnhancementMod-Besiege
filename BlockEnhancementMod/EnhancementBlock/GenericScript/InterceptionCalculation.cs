using Modding;
using Modding.Common;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    class InterceptionCalculation
    {
        /// <summary>
        /// Thanks for Forrest Smith's generous share of his fantastic work
        /// https://www.forrestthewoods.com/blog/solving_ballistic_trajectories/
        /// </summary>
        public static int SolveBallisticArc(Vector3 projPos, float projSpeed, Vector3 targetPos, Vector3 targetVelocity, float gravity, out Vector3 s0, out float time)
        {
            // Initialize output parameters
            s0 = Vector3.zero;
            time = 0;

            // Derivation 
            //
            //  For full derivation see: blog.forrestthewoods.com
            //  Here is an abbreviated version.
            //
            //  Four equations, four unknowns (solution.x, solution.y, solution.z, time):
            //
            //  (1) proj_pos.x + solution.x*time = target_pos.x + target_vel.x*time
            //  (2) proj_pos.y + solution.y*time + .5*G*t = target_pos.y + target_vel.y*time
            //  (3) proj_pos.z + solution.z*time = target_pos.z + target_vel.z*time
            //  (4) proj_speed^2 = solution.x^2 + solution.y^2 + solution.z^2
            //
            //  (5) Solve for solution.x and solution.z in equations (1) and (3)
            //  (6) Square solution.x and solution.z from (5)
            //  (7) Solve solution.y^2 by plugging (6) into (4)
            //  (8) Solve solution.y by rearranging (2)
            //  (9) Square (8)
            //  (10) Set (8) = (7). All solution.xyz terms should be gone. Only time remains.
            //  (11) Rearrange 10. It will be of the form a*^4 + b*t^3 + c*t^2 + d*t * e. This is a quartic.
            //  (12) Solve the quartic using SolveQuartic.
            //  (13) If there are no positive, real roots there is no solution.
            //  (14) Each positive, real root is one valid solution
            //  (15) Plug each time value into (1) (2) and (3) to calculate solution.xyz
            //  (16) The end.

            float G = gravity;

            float A = projPos.x;
            float B = projPos.y;
            float C = projPos.z;
            float M = targetPos.x;
            float N = targetPos.y;
            float O = targetPos.z;
            float P = targetVelocity.x;
            float Q = targetVelocity.y;
            float R = targetVelocity.z;
            float S = projSpeed;

            float H = M - A;
            float J = O - C;
            float K = N - B;
            float L = -.5f * G;

            // Quartic Coeffecients
            float c0 = L * L;
            float c1 = 2 * Q * L;
            float c2 = Q * Q + 2 * K * L - S * S + P * P + R * R;
            float c3 = 2 * K * Q + 2 * H * P + 2 * J * R;
            float c4 = K * K + H * H + J * J;

            // Solve quartic
            float[] times = new float[4];
            int numTimes = SolveQuartic(c0, c1, c2, c3, c4, out times[0], out times[1], out times[2], out times[3]);

            // Sort so faster collision is found first
            Array.Sort(times);

            // Plug quartic solutions into base equations
            // There should never be more than 2 positive, real roots.
            Vector3[] solutions = new Vector3[2];
            float[] timesOut = new float[2];
            int numSolutions = 0;

            for (int i = 0; i < numTimes && numSolutions < 2; ++i)
            {
                float t = times[i];
                if (t <= 0)
                    continue;

                timesOut[numSolutions] = t;
                solutions[numSolutions].x = ((H + P * t) / t);
                solutions[numSolutions].y = ((K + Q * t - L * t * t) / t);
                solutions[numSolutions].z = ((J + R * t) / t);
                ++numSolutions;
            }

            // Write out solutions
            if (numSolutions > 0)
            {
                time = timesOut[0];
                s0 = solutions[0];
            }

            return numSolutions;
        }

        public static int SolveBallisticArc(Vector3 projPos, float projSpeed, Vector3 targetPos, float gravity, out Vector3 s0, out float time)
        {
            // Handling these cases is up to your project's coding standards
            Debug.Assert(projPos != targetPos && projSpeed > 0 && gravity > 0, "fts.solve_ballistic_arc called with invalid data");

            // C# requires out variables be set
            s0 = Vector3.zero;
            time = 0;
            // Derivation
            //   (1) x = v*t*cos O
            //   (2) y = v*t*sin O - .5*g*t^2
            // 
            //   (3) t = x/(cos O*v)                                        [solve t from (1)]
            //   (4) y = v*x*sin O/(cos O * v) - .5*g*x^2/(cos^2 O*v^2)     [plug t into y=...]
            //   (5) y = x*tan O - g*x^2/(2*v^2*cos^2 O)                    [reduce; cos/sin = tan]
            //   (6) y = x*tan O - (g*x^2/(2*v^2))*(1+tan^2 O)              [reduce; 1+tan O = 1/cos^2 O]
            //   (7) 0 = ((-g*x^2)/(2*v^2))*tan^2 O + x*tan O - (g*x^2)/(2*v^2) - y    [re-arrange]
            //   Quadratic! a*p^2 + b*p + c where p = tan O
            //
            //   (8) let gxv = -g*x*x/(2*v*v)
            //   (9) p = (-x +- sqrt(x*x - 4gxv*(gxv - y)))/2*gxv           [quadratic formula]
            //   (10) p = (v^2 +- sqrt(v^4 - g(g*x^2 + 2*y*v^2)))/gx        [multiply top/bottom by -2*v*v/x; move 4*v^4/x^2 into root]
            //   (11) O = atan(p)

            Vector3 diff = targetPos - projPos;
            Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
            float groundDist = diffXZ.magnitude;

            float speed2 = projSpeed * projSpeed;
            float speed4 = projSpeed * projSpeed * projSpeed * projSpeed;
            float y = diff.y;
            float x = groundDist;
            float gx = gravity * x;

            float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

            // No solution
            if (root < 0)
                return 0;

            root = Mathf.Sqrt(root);

            float lowAng = Mathf.Atan2(speed2 - root, gx);
            float highAng = Mathf.Atan2(speed2 + root, gx);
            int numSolutions = lowAng != highAng ? 2 : 1;

            Vector3 groundDir = diffXZ.normalized;
            s0 = groundDir * Mathf.Cos(lowAng) * projSpeed + Vector3.up * Mathf.Sin(lowAng) * projSpeed;
            time = groundDist / Mathf.Cos(lowAng) / projSpeed;
            return numSolutions;
        }

        public static int SolveQuartic(float c0, float c1, float c2, float c3, float c4, out float s0, out float s1, out float s2, out float s3)
        {
            s0 = float.NaN;
            s1 = float.NaN;
            s2 = float.NaN;
            s3 = float.NaN;

            float[] coeffs = new float[4];
            float z, u, v, sub;
            float A, B, C, D;
            float sq_A, p, q, r;
            int num;

            /* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;
            D = c4 / c0;

            /*  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0 */
            sq_A = A * A;
            p = -3.0f / 8 * sq_A + B;
            q = 1.0f / 8 * sq_A * A - 1.0f / 2 * A * B + C;
            r = -3.0f / 256 * sq_A * sq_A + 1.0f / 16 * sq_A * B - 1.0f / 4 * A * C + D;

            if (IsZero(r))
            {
                /* no absolute term: y(y^3 + py + q) = 0 */

                coeffs[3] = q;
                coeffs[2] = p;
                coeffs[1] = 0;
                coeffs[0] = 1;

                num = SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);
            }
            else
            {
                /* solve the resolvent cubic ... */
                coeffs[3] = 1.0f / 2 * r * p - 1.0f / 8 * q * q;
                coeffs[2] = -r;
                coeffs[1] = -1.0f / 2 * p;
                coeffs[0] = 1;

                SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);

                /* ... and take the one real solution ... */
                z = s0;

                /* ... to build two quadric equations */
                u = z * z - r;
                v = 2 * z - p;

                if (IsZero(u))
                    u = 0;
                else if (u > 0)
                    u = Mathf.Sqrt(u);
                else
                    return 0;

                if (IsZero(v))
                    v = 0;
                else if (v > 0)
                    v = Mathf.Sqrt(v);
                else
                    return 0;

                coeffs[2] = z - u;
                coeffs[1] = q < 0 ? -v : v;
                coeffs[0] = 1;

                num = SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);

                coeffs[2] = z + u;
                coeffs[1] = q < 0 ? v : -v;
                coeffs[0] = 1;

                if (num == 0) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);
                if (num == 1) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s1, out s2);
                if (num == 2) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s2, out s3);
            }

            /* resubstitute */
            sub = 1.0f / 4 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;
            if (num > 3) s3 -= sub;

            return num;
        }

        public static int SolveCubic(float c0, float c1, float c2, float c3, out float s0, out float s1, out float s2)
        {
            s0 = float.NaN;
            s1 = float.NaN;
            s2 = float.NaN;

            int num;
            float sub;
            float A, B, C;
            float sqA, p, q;
            float cbP, D;

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;

            /*  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0 */
            sqA = A * A;
            p = 1.0f / 3f * (-1.0f / 3f * sqA + B);
            q = 1.0f / 2f * (2.0f / 27f * A * sqA - 1.0f / 3f * A * B + C);

            /* use Cardano's formula */
            cbP = p * p * p;
            D = q * q + cbP;

            if (IsZero(D))
            {
                if (IsZero(q)) /* one triple solution */
                {
                    s0 = 0;
                    num = 1;
                }
                else /* one single and one double solution */
                {
                    float u = Mathf.Pow(-q, 1.0f / 3.0f);
                    s0 = 2 * u;
                    s1 = -u;
                    num = 2;
                }
            }
            else if (D < 0) /* Casus irreducibilis: three real solutions */
            {
                float phi = 1.0f / 3f * Mathf.Acos(-q / Mathf.Sqrt(-cbP));
                float t = 2f * Mathf.Sqrt(-p);

                s0 = t * Mathf.Cos(phi);
                s1 = -t * Mathf.Cos(phi + Mathf.PI / 3);
                s2 = -t * Mathf.Cos(phi - Mathf.PI / 3);
                num = 3;
            }
            else /* one real solution */
            {
                float sqrt_D = Mathf.Sqrt(D);
                float u = Mathf.Pow(sqrt_D - q, 1.0f / 3.0f);
                float v = -Mathf.Pow(sqrt_D + q, 1.0f / 3.0f);

                s0 = u + v;
                num = 1;
            }

            /* resubstitute */
            sub = 1.0f / 3 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;

            return num;
        }

        public static int SolveQuadric(float c0, float c1, float c2, out float s0, out float s1)
        {
            s0 = float.NaN;
            s1 = float.NaN;

            float p, q, D;

            /* normal form: x^2 + px + q = 0 */
            p = c1 / (2 * c0);
            q = c2 / c0;

            D = p * p - q;

            if (IsZero(D))
            {
                s0 = -p;
                return 1;
            }
            else if (D < 0)
            {
                return 0;
            }
            else /* if (D > 0) */
            {
                float sqrtD = Mathf.Sqrt(D);

                s0 = sqrtD - p;
                s1 = -sqrtD - p;
                return 2;
            }
        }

        public static bool IsZero(double d)
        {
            const double eps = 1e-4;
            return d > -eps && d < eps;
        }

        public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
        {
            float velocitySquared = targetRelativeVelocity.sqrMagnitude;
            if (velocitySquared < 0.001f)
                return 0f;

            float a = velocitySquared - shotSpeed * shotSpeed;

            //handle similar velocities
            if (Mathf.Abs(a) < 0.001f)
            {
                float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
                return Mathf.Max(t, 0f); //don't shoot back in time
            }

            float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
            float c = targetRelativePosition.sqrMagnitude;
            float determinant = b * b - 4f * a * c;

            if (determinant > 0f)
            { //determinant > 0; two intercept paths (most common)
                float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                        t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
                if (t1 > 0f)
                {
                    if (t2 > 0f)
                        return Mathf.Min(t1, t2); //both are positive
                    else
                        return t1; //only t1 is positive
                }
                else
                    return Mathf.Max(t2, 0f); //don't shoot back in time
            }
            else if (determinant < 0f) //determinant < 0; no intercept path
                return 0f;
            else //determinant = 0; one intercept path, pretty much never happens
                return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
        }
    }
}