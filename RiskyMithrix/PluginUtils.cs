using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix
{
    internal static class PluginUtils
    {
        internal static void SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, string value)
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).Completed += handle => SetAddressableEntityStateField_String_Completed(handle, fieldName, value);
        }

        private static void SetAddressableEntityStateField_String_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<EntityStateConfiguration> handle, string fieldName, string value)
        {
            EntityStateConfiguration esc = handle.Result;
            for (int i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
                    return;
                }
            }
            Debug.LogError("RiskyMithrix: " + esc + " does not have field " + fieldName);
        }

        internal static void SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, UnityEngine.Object newObject)
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).Completed += handle => SetAddressableEntityStateField_Object_Completed(handle, fieldName, newObject);
        }

        private static void SetAddressableEntityStateField_Object_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<EntityStateConfiguration> handle, string fieldName, Object newObject)
        {
            EntityStateConfiguration esc = handle.Result;
            for (int i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.objectValue = newObject;
                    return;
                }
            }
            Debug.LogError("RiskyMithrix: " + esc + " does not have field " + fieldName);
        }
    }
}
