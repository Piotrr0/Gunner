using System.Collections;
using UnityEngine;

namespace Utilities
{
    public static partial class HelperUtilities
    {
        public static bool ValidateCheckEmptyString(Object worldContextObject, string fieldName, string stringToCheck)
        {
            if (stringToCheck == "")
            {
                Debug.Log(fieldName + " is empty must contain a value in obejct" + worldContextObject.name.ToString());
                return true;
            }
            return false;
        }

        public static bool ValidateCheckEnumerableValues(Object worldContextObject, string fieldName, IEnumerable enumerableObjectsToCheck)
        {
            bool error = false;
            int count = 0;

            foreach (var item in enumerableObjectsToCheck)
            {
                if (item == null)
                {
                    Debug.Log(fieldName + " has null values in object" + worldContextObject.name.ToString());
                    error = true;
                }
                else
                {
                    count++;
                }
            }

            if (count == 0)
            {
                Debug.Log(fieldName + " has no values in object" + worldContextObject.name.ToString());
                error = true;
            }

            return error;
        }
    }
}