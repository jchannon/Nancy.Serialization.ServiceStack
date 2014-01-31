﻿using Nancy.Extensions;

namespace Nancy.Serializers.Json.ServiceStack
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Nancy.ModelBinding;

    using global::ServiceStack.Text;

    public class ServiceStackBodyDeserializer : IBodyDeserializer
    {
        /// <summary>
        /// Whether the deserializer can deserialize the content type
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="context">Current <see cref="BindingContext"/>.</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanDeserialize(string contentType, BindingContext context)
        {
            return Helpers.IsJsonType(contentType);
        }

        /// <summary>
        /// Deserialize the request body to a model
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="bodyStream">Request body stream</param>
        /// <param name="context">Current context</param>
        /// <returns>Model instance</returns>
        public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
        {
            var deserializedObject = JsonSerializer.DeserializeFromStream(context.DestinationType, bodyStream);

            if (!context.DestinationType.IsCollection())
            {
                var existingInstance = false;
                foreach (var property in context.ValidModelProperties)
                {
                    var existingValue = property.GetValue(context.Model, null);

                    if (!IsDefaultValue(existingValue, property.PropertyType))
                    {
                        existingInstance = true;
                        break;
                    }
                }

                if (existingInstance)
                {
                    foreach (var property in context.ValidModelProperties)
                    {
                        var existingValue = property.GetValue(context.Model, null);

                        if (IsDefaultValue(existingValue, property.PropertyType))
                        {
                            CopyPropertyValue(property, deserializedObject, context.Model);
                        }
                    }

                    return context.Model;
                }
            }

            if (context.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Except(context.ValidModelProperties).Any())
            {
                return this.CreateObjectWithBlacklistExcluded(context, deserializedObject);
            }

            return deserializedObject;
        }

        private object CreateObjectWithBlacklistExcluded(BindingContext context, object deserializedObject)
        {
            var returnObject = Activator.CreateInstance(context.DestinationType);

            foreach (var property in context.ValidModelProperties)
            {
                this.CopyPropertyValue(property, deserializedObject, returnObject);
            }

            return returnObject;
        }

        private void CopyPropertyValue(PropertyInfo property, object sourceObject, object destinationObject)
        {
            property.SetValue(destinationObject, property.GetValue(sourceObject, null), null);
        }

        private static bool IsDefaultValue(object existingValue, Type propertyType)
        {
            return propertyType.IsValueType
                ? Equals(existingValue, Activator.CreateInstance(propertyType))
                : existingValue == null;
        }
    }
}