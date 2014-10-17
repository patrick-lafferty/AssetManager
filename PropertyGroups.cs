using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

namespace Assets
{
    /*
    Shader parameters map to hlsl variables, so we need to know
    waht type of hlsl variable it is.

    scalar - float x, int counter, bool flag
    vector2 - float2 texCoords
    vector3 - float3 position
    vector4 - float4 orientation
    array - float4[] coordinates
    */
    public enum ParameterType
    {
        Scalar, Vector2, Vector3, Vector4, Array
    }

    public enum PodType
    {
        Float, Int, Bool, Unknown
    }

    /*
    Utility class that converts c#'s pod types to parameter podtypes,
    and serializes pod values
    */
    public static class TypeHelper
    {
        public static PodType GetPodType<T>()
        {
            if (typeof(T) == typeof(float))
            {
                return PodType.Float;
            }
            else if (typeof(T) == typeof(int))
            {
                return PodType.Int;
            }
            else if (typeof(T) == typeof(bool))
            {
                return PodType.Bool;
            }
            else
            {
                return PodType.Unknown;
            }
        }

        public static void WriteValue<T>(Object value, BinaryWriter writer)
        {
            if (typeof(T) == typeof(float))
            {
                writer.Write((float)value);
            }
            else if (typeof(T) == typeof(int))
            {
                writer.Write((int)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                writer.Write((bool)value);
            }            
        }
    }

    /*
    A shader parameter maps 1:1 to a variable in hlsl sourcecode. When
    you set a shader parameter in Glitch, the engine basically for each
    parameter looks up a corresponding variable in all of the 
    material's shaders and sets its value if the name matches.
    */        
    public abstract class Parameter
    {
        string name;        
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (OnChange != null) OnChange();
            }
        }

        string type;        
        public string Type 
        { 
            get { return this.ToString(); }
            private set
            {
                type = value;
            }
        }
                
        public ParameterType ParameterType { get; set; }
                
        public PodType PodType {get; set;}

        public abstract void WriteoutValue(BinaryWriter writer);
        public abstract void WriteoutValue(System.Text.StringBuilder builder);
                
        public virtual Action OnChange { get; set; }

        static Type[] GetKnownTypes()
        {
            return typeof(Parameter).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Parameter)))
                .SelectMany(x => new Type[] {typeof(float), typeof(int), typeof(bool) },
                    (generic, pod) => generic.MakeGenericType(pod))
                .ToArray();
        }
    }

    /*
    A shader parameter that maps to an hlsl float, int, or bool.
    */
    public class ScalarParameter<T> : Parameter where T : struct
    {
        public ScalarParameter()
        {
            ParameterType = ParameterType.Scalar;

            PodType = TypeHelper.GetPodType<T>();
        }

        T value;
        public T Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (OnChange != null) OnChange();
            }
        }

        public override string ToString()
        {
            if (PodType == Assets.PodType.Float)
            {
                return "float";
            }
            else if (PodType == Assets.PodType.Int)
            {
                return "int";
            }
            else
            {
                return "bool";
            }
        }

        public override void WriteoutValue(BinaryWriter writer)
        {
            TypeHelper.WriteValue<T>(Value, writer);
        }

        public override void WriteoutValue(System.Text.StringBuilder builder)
        {
            builder.Append(Value.ToString());
        }
    }

    /*
    A shader parameter that maps to an hlsl float2, int2, or bool2.
    */
    public class Vector2Parameter<T> : Parameter
    {
        public Vector2Parameter()
        {
            ParameterType = Assets.ParameterType.Vector2;

            PodType = TypeHelper.GetPodType<T>();
        }

        T x;
        public T X
        {
            get { return x; }
            set
            {
                x = value;
                if (OnChange != null) OnChange();
            }
        }

        T y;
        public T Y
        {
            get { return y; }
            set
            {
                y = value;
                if (OnChange != null) OnChange();
            }
        }

        public override string ToString()
        {
            if (PodType == Assets.PodType.Float)
            {
                return "float2";
            }
            else if (PodType == Assets.PodType.Int)
            {
                return "int2";
            }
            else
            {
                return "bool2";
            }
        }

        public override void WriteoutValue(BinaryWriter writer)
        {
            TypeHelper.WriteValue<T>(X, writer);
            TypeHelper.WriteValue<T>(Y, writer);
        }

        public override void WriteoutValue(System.Text.StringBuilder builder)
        {
            builder.Append(X.ToString());
            builder.Append("*");
            builder.Append(Y.ToString());
        }
    }

    /*
    A shader parameter that maps to an hlsl float3, int3, or bool3.
    */
    public class Vector3Parameter<T> : Parameter
    {
        public Vector3Parameter()
        {
            ParameterType = Assets.ParameterType.Vector3;

            PodType = TypeHelper.GetPodType<T>();
        }

        T x;
        public T X
        {
            get { return x; }
            set
            {
                x = value;
                if (OnChange != null) OnChange();
            }
        }

        T y;
        public T Y
        {
            get { return y; }
            set
            {
                y = value;
                if (OnChange != null) OnChange();
            }
        }

        T z;
        public T Z
        {
            get { return z; }
            set
            {
                z = value;
                if (OnChange != null) OnChange();
            }
        }

        public override string ToString()
        {
            if (PodType == Assets.PodType.Float)
            {
                return "float3";
            }
            else if (PodType == Assets.PodType.Int)
            {
                return "int3";
            }
            else
            {
                return "bool3";
            }
        }

        public override void WriteoutValue(BinaryWriter writer)
        {
            TypeHelper.WriteValue<T>(X, writer);
            TypeHelper.WriteValue<T>(Y, writer);
            TypeHelper.WriteValue<T>(Z, writer);
        }

        public override void WriteoutValue(System.Text.StringBuilder builder)
        {
            builder.Append(X.ToString());
            builder.Append("*");
            builder.Append(Y.ToString());
            builder.Append("*");
            builder.Append(Z.ToString());
        }
    }

    /*
    A shader parameter that maps to an hlsl float4, int4, or bool4.
    */
    public class Vector4Parameter<T> : Parameter
    {
        public Vector4Parameter()
        {
            ParameterType = Assets.ParameterType.Vector4;

            PodType = TypeHelper.GetPodType<T>();
        }

        T x;
        public T X
        {
            get { return x; }
            set
            {
                x = value;
                if (OnChange != null) OnChange();
            }
        }

        T y;
        public T Y
        {
            get { return y; }
            set
            {
                y = value;
                if (OnChange != null) OnChange();
            }
        }

        T z;
        public T Z
        {
            get { return z; }
            set
            {
                z = value;
                if (OnChange != null) OnChange();
            }
        }

        T w;
        public T W
        {
            get { return w; }
            set
            {
                w = value;
                if (OnChange != null) OnChange();
            }
        }

        public override string ToString()
        {
            if (PodType == Assets.PodType.Float)
            {
                return "float4";
            }
            else if (PodType == Assets.PodType.Int)
            {
                return "int4";
            }
            else
            {
                return "bool4";
            }
        }

        public override void WriteoutValue(BinaryWriter writer)
        {
            TypeHelper.WriteValue<T>(X, writer);
            TypeHelper.WriteValue<T>(Y, writer);
            TypeHelper.WriteValue<T>(Z, writer);
            TypeHelper.WriteValue<T>(W, writer);
        }

        public override void WriteoutValue(System.Text.StringBuilder builder)
        {
            builder.Append(X.ToString());
            builder.Append("*");
            builder.Append(Y.ToString());
            builder.Append("*");
            builder.Append(Z.ToString());
            builder.Append("*");
            builder.Append(W.ToString());
        }
    }

    
    /*
    This is a cheap hack so that the material editor can add elements
    to any array, since array parameters are generic and all stored
    in a lsit of base class parameter, the editor has no way of 
    knowing what T it is, and so wouldn't be able to call
    ArrayParameter<T>.AddElement<T>.
    */
    public interface ArrayParam
    {
        void AddElement(Object o);
        void RemoveSelectedElement();
    }

    /*
    A shader parameter that maps to an hlsl float4[], int4[], or bool4[].

    Note: due to constant buffer memory issues, you can only have an
    array of vector4s, you cannot have an array of scalars,
    vector2, or vector3s.
    */
    public class ArrayParameter<T> : Parameter, ArrayParam
    {
        public ArrayParameter()
        {
            Elements = new ObservableCollection<Vector4Parameter<T>>();
            ParameterType = Assets.ParameterType.Array;

            PodType = TypeHelper.GetPodType<T>();
        }

        public ObservableCollection<Vector4Parameter<T>> Elements { get; set; }
        public Vector4Parameter<T> SelectedElement { get; set; }

        public void AddElement(Object p)
        {
            var parameter = p as Vector4Parameter<T>;

            if (parameter != null)
            {
                parameter.OnChange = OnChange;
                Elements.Add(parameter);
            }
        }

        public void RemoveSelectedElement()
        {
            if (SelectedElement != null)
            {
                Elements.Remove(SelectedElement);
            }
        }

        public override string ToString()
        {
            if (PodType == Assets.PodType.Float)
            {
                return "float4 array";
            }
            else if (PodType == Assets.PodType.Int)
            {
                return "int4 array";
            }
            else
            {
                return "bool4 array";
            }
        }

        Action onChange;        
        public override Action OnChange
        {
            get { return onChange; }
            set
            {
                onChange = value;
                
                if (Elements == null)
                    return;

                foreach (var element in Elements)
                {
                    element.OnChange = value;
                }
            }
        }

        public override void WriteoutValue(BinaryWriter writer)
        {
            writer.Write((short)Elements.Count);

            foreach (var element in Elements)
            {
                element.WriteoutValue(writer);
            }
        }

        public override void WriteoutValue(System.Text.StringBuilder builder)
        {
            foreach(var element in Elements)
            {
                element.WriteoutValue(builder);
                builder.Append("$");
            }
        }
    }

    /*
    A parameter group maps 1:1 with a constant buffer in hlsl.
    Note that while memder order in a constant buffer matters,
    it doesn't matter for parameter groups, since members
    are matched by their names, not their indices.
    */
    
    public class ParameterGroup
    {
        public ParameterGroup()
        {
            Parameters = new ObservableCollection<Parameter>();
        }

        string name;
        
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (OnChange != null) OnChange();
            }
        }

        Action onChange;
        
        public Action OnChange
        {
            get { return onChange; }
            set
            {
                onChange = value;
                foreach (var parameter in Parameters)
                {
                    parameter.OnChange = value;
                }
            }        
        }

        
        public ObservableCollection<Parameter> Parameters { get; set; }

        
        public Parameter SelectedParameter { get; set; }
    }
}