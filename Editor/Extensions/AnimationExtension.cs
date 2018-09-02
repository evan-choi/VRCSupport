using UnityEditor;
using UnityEngine;

static class AnimationExtension
{
    public static string GetFriendlyName(this EditorCurveBinding binding)
    {
        string propertyName = binding.propertyName;
        
        if (propertyName.StartsWith("m_"))
            propertyName = propertyName.Substring(2);

        if (binding.type != typeof(GameObject))
        {
            if (propertyName.StartsWith("Local"))
            {
                propertyName = propertyName.Substring(5);
            }
        }

        return string.Format("{0} : {1}.{2}", 
            binding.path, 
            binding.type.Name, 
            propertyName);
    }
}
