namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class UtilityExtensions
    {
        public static float Map(this float value, 
                                float originalMin, 
                                float originalMax, 
                                float newMin, 
                                float newMax)
        {
            return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
        }

        public static bool IsWithin(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }
        
        public static bool CheckIfTrue(string data)
        {
            if(data == "0")
            {
                return true;
            }
            else if (data == "1")
            {
                return false;
            }
            else { return false; }
        }
    }

}
