namespace UnityEngine.Experimental.PostProcessing
{
    public static class ColorUtilities
    {
        // An analytical model of chromaticity of the standard illuminant, by Judd et al.
        // http://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D
        // Slightly modifed to adjust it with the D65 white point (x=0.31271, y=0.32902).
        public static float StandardIlluminantY(float x)
        {
            return 2.87f * x - 3f * x * x - 0.27509507f;
        }

        // CIE xy chromaticity to CAT02 LMS.
        // http://en.wikipedia.org/wiki/LMS_color_space#CAT02
        public static Vector3 CIExyToLMS(float x, float y)
        {
            float Y = 1f;
            float X = Y * x / y;
            float Z = Y * (1f - x - y) / y;

            float L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
            float M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
            float S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;

            return new Vector3(L, M, S);
        }

        public static Vector3 ComputeColorBalance(float temperature, float tint)
        {
            // Range ~[-1.67;1.67] works best
            float t1 = temperature / 60f;
            float t2 = tint / 60f;

            // Get the CIE xy chromaticity of the reference white point.
            // Note: 0.31271 = x value on the D65 white point
            float x = 0.31271f - t1 * (t1 < 0f ? 0.1f : 0.05f);
            float y = StandardIlluminantY(x) + t2 * 0.05f;

            // Calculate the coefficients in the LMS space.
            var w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point
            var w2 = CIExyToLMS(x, y);
            return new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
        }

        // Alpha/w is offset
        public static Vector3 ColorToLift(Vector4 color)
        {
            // Shadows
            var S = new Vector3(color.x, color.y, color.z);
            float lumLift = S.x * 0.2126f + S.y * 0.7152f + S.z * 0.0722f;
            S = new Vector3(S.x - lumLift, S.y - lumLift, S.z - lumLift);

            float liftOffset = color.w;
            return new Vector3(S.x + liftOffset, S.y + liftOffset, S.z + liftOffset);
        }

        // Alpha/w is offset
        public static Vector3 ColorToInverseGamma(Vector4 color)
        {
            // Midtones
            var M = new Vector3(color.x, color.y, color.z);
            float lumGamma = M.x * 0.2126f + M.y * 0.7152f + M.z * 0.0722f;
            M = new Vector3(M.x - lumGamma, M.y - lumGamma, M.z - lumGamma);

            float gammaOffset = color.w + 1f;
            return new Vector3(
                1f / Mathf.Max(M.x + gammaOffset, 1e-03f),
                1f / Mathf.Max(M.y + gammaOffset, 1e-03f),
                1f / Mathf.Max(M.z + gammaOffset, 1e-03f)
            );
        }

        // Alpha/w is offset
        public static Vector3 ColorToGain(Vector4 color)
        {
            // Highlights
            var H = new Vector3(color.x, color.y, color.z);
            float lumGain = H.x * 0.2126f + H.y * 0.7152f + H.z * 0.0722f;
            H = new Vector3(H.x - lumGain, H.y - lumGain, H.z - lumGain);

            float gainOffset = color.w + 1f;
            return new Vector3(H.x + gainOffset, H.y + gainOffset, H.z + gainOffset);
        }

        // Alexa LogC converters (El 1000)
        // See http://www.vocas.nl/webfm_send/964
        public static float LogCToLinear(float x)
        {
            const float cut = 0.011361f;
            const float a = 5.555556f;
            const float b = 0.047996f;
            const float c = 0.244161f;
            const float d = 0.386036f;
            const float e = 5.301883f;
            const float f = 0.092819f;

            return (x > e * cut + f)
                ? (Mathf.Pow(10f, (x - d) / c) - b) / a
                : (x - f) / e;
        }

        public static float LinearToLogC(float x)
        {
            const float cut = 0.011361f;
            const float a = 5.555556f;
            const float b = 0.047996f;
            const float c = 0.244161f;
            const float d = 0.386036f;
            const float e = 5.301883f;
            const float f = 0.092819f;

            return (x > cut)
                ? c * Mathf.Log10(a * x + b) + d
                : e * x + f;
        }

        public static uint ToHex(Color c)
        {
            return ((uint)(c.a * 255) << 24)
                 | ((uint)(c.r * 255) << 16)
                 | ((uint)(c.g * 255) <<  8)
                 | ((uint)(c.b * 255));
        }

        public static Color ToRGBA(uint hex)
        {
            return new Color(
                ((hex >> 16) & 0xff) / 255f, // r
                ((hex >>  8) & 0xff) / 255f, // g
                ((hex      ) & 0xff) / 255f, // b
                ((hex >> 24) & 0xff) / 255f  // a
            );
        }
    }
}
