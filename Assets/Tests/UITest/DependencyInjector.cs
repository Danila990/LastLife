using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tests.UITest
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InjectAttribute : Attribute
    {
    }

    public static class DependencyInjector
    {
        private const BindingFlags FIELD_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.DeclaredOnly;

        public static readonly Dictionary<Type, object> components = new Dictionary<Type, object>();
        private static readonly object placeholder = new object();

        private static readonly Dictionary<Type, FieldInfo[]> cachedFields = new Dictionary<Type, FieldInfo[]>();

        private static readonly List<FieldInfo> injectFieldsBuffer = new List<FieldInfo>();

        public static void InjectObject(object target)
        {
            var fields = GetFields(target.GetType());
            for (var i = 0; i < fields.Length; i++)
                fields[i].SetValue(target, Resolve(fields[i].FieldType));
        }

        private static FieldInfo[] GetFields(Type type)
        {
            FieldInfo[] fields;
            if (!cachedFields.TryGetValue(type, out fields))
            {
                fields = GetInjectFields(type);
                cachedFields.Add(type, fields);
            }

            return fields;
        }

        public static void Inject(this MonoBehaviour target)
        {
            InjectObject(target);
        }

        private static FieldInfo[] GetInjectFields(Type type)
        {
            while (type != null)
            {
                var typeFields = type.GetFields(FIELD_FLAGS);
                for (var i = 0; i < typeFields.Length; i++)
                    if (HasInjectAttribute(typeFields[i]))
                        injectFieldsBuffer.Add(typeFields[i]);
                type = type.BaseType;
            }

            var resultFields = injectFieldsBuffer.ToArray();
            injectFieldsBuffer.Clear();
            return resultFields;
        }

        private static bool HasInjectAttribute(MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(InjectAttribute), true).Any();
        }

        public static T Resolve<T>()
        {
            return (T) Resolve(typeof(T));
        }

        private static object Resolve(Type type)
        {
            object component;

            if (components.TryGetValue(type, out component))
            {
                if (placeholder == component)
                    throw new Exception("Cyclic dependency detected in " + type);
            }
            else
            {
                components[type] = placeholder;
                component = components[type] = CreateComponent(type);
            }

            return component;
        }

        public static void ReplaceComponent<T>(T newComponent)
        {
            components[typeof(T)] = newComponent;
            foreach (var c in components.Values)
            foreach (var f in GetFields(c.GetType()))
                if (f.FieldType == typeof(T))
                    f.SetValue(c, newComponent);
        }

        public static void ClearCache()
        {
            foreach (var componentPair in components)
            {
                var cleanableComponent = componentPair.Value as IDisposable;
                if (cleanableComponent != null)
                    cleanableComponent.Dispose();
            }

            cachedFields.Clear();
            components.Clear();
            GC.Collect();
        }

        private static object CreateComponent(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}