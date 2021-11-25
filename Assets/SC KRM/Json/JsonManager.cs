using Newtonsoft.Json;
using SCKRM.Resource;
using UnityEngine;

namespace SCKRM.Json
{
    public static class JsonManager
    {
        public static T JsonRead<T>(string path, bool pathExtensionUse = false)
        {
            string json;
            json = ResourceManager.GetText(path, pathExtensionUse);
            
            if (json != "")
                return JsonConvert.DeserializeObject<T>(json);
            else
                return default(T);
        }

        public static T JsonRead<T>(string path, string nameSpace) => JsonConvert.DeserializeObject<T>(ResourceManager.SearchText(path, nameSpace));



        public static T JsonToObject<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        public static string ObjectToJson(object value) => JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings() { });
        public static string ObjectToJson(params object[] value) => JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings() { });
    }

    public struct JVector2
    {
        public float x;
        public float y;

        public static JVector2 zero { get; } = new JVector2();

        public JVector2(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }

        public JVector2(float value) => x = y = value;
        public JVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator JVector2(JRect value) => new JVector2(value.x, value.y);

        public static implicit operator JVector2(Vector2 value) => new JVector2(value);
        public static implicit operator Vector2(JVector2 value) => new Vector2() { x = value.x, y = value.y };
    }

    public struct JVector3
    {
        public float x;
        public float y;
        public float z;

        public static JVector3 zero { get; } = new JVector3();

        public JVector3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public JVector3(float value) => x = y = z = value;

        public JVector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0;
        }

        public JVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator JVector2(JVector3 value) => new JVector2(value.x, value.y);

        public static implicit operator JVector3(JRect value) => new JVector3(value.x, value.y, value.width);

        public static implicit operator JVector3(Vector3 value) => new JVector3(value);
        public static implicit operator Vector3(JVector3 value) => new Vector3() { x = value.x, y = value.y, z = value.z };
    }
    public struct JVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static JVector4 zero { get; } = new JVector4();

        public JVector4(Vector4 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

        public JVector4(float value) => x = y = z = w = value;

        public JVector4(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0;
            w = 0;
        }

        public JVector4(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            w = 0;
        }

        public JVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator JVector2(JVector4 value) => new JVector2(value.x, value.y);
        public static implicit operator JVector3(JVector4 value) => new JVector3(value.x, value.y, value.z);

        public static implicit operator JVector4(JRect value) => new JVector4(value.x, value.y, value.width, value.height);
        public static implicit operator JVector4(Rect value) => new JVector4(value.x, value.y, value.width, value.height);

        public static implicit operator JVector4(Vector4 value) => new JVector4(value);
        public static implicit operator Vector4(JVector4 value) => new Vector4() { x = value.x, y = value.y, z = value.z, w = value.w };
    }

    public struct JRect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public static JRect zero { get; } = new JRect();

        public JRect(Rect value)
        {
            x = value.x;
            y = value.y;
            width = value.width;
            height = value.height;
        }

        public JRect(float value) => x = y = width = height = value;

        public JRect(float x, float y)
        {
            this.x = x;
            this.y = y;
            width = 0;
            height = 0;
        }

        public JRect(float x, float y, float width)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            height = 0;
        }

        public JRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public static implicit operator JRect(JVector2 value) => new JRect(value.x, value.y);
        public static implicit operator JRect(JVector3 value) => new JRect(value.x, value.y, value.z);
        public static implicit operator JRect(JVector4 value) => new JRect(value.x, value.y, value.z, value.w);

        public static implicit operator JRect(Vector2 value) => new JRect(value.x, value.y);
        public static implicit operator JRect(Vector3 value) => new JRect(value.x, value.y, value.z);
        public static implicit operator JRect(Vector4 value) => new JRect(value.x, value.y, value.z, value.w);

        public static implicit operator Vector2(JRect value) => new Vector2(value.x, value.y);
        public static implicit operator Vector3(JRect value) => new Vector3(value.x, value.y, value.width);
        public static implicit operator Vector4(JRect value) => new Vector4(value.x, value.y, value.width, value.height);

        public static implicit operator JRect(Rect value) => new JRect(value);
        public static implicit operator Rect(JRect value) => new Rect() { x = value.x, y = value.y, width = value.width, height = value.height };
    }
}