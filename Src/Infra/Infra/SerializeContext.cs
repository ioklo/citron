using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using Citron.Collections;

namespace Citron.Infra;

public struct SerializeContext
{   
    JsonArray rootObjs; // reference와 최상위 오브젝트한개
    Dictionary<object, int> refIndices;
    JsonObject curObj;

    // top-level
    public static string Serialize<TClass>(TClass? inst)
        where TClass : class, ISerializable
    {
        var context = new SerializeContext();
        context.SerializeRef("root", inst);

        return context.curObj.ToJsonString(new JsonSerializerOptions() { WriteIndented = true });
    }

    public SerializeContext()
    {        
        rootObjs = new JsonArray();
        refIndices = new Dictionary<object, int>();
        curObj = new JsonObject();
        
        curObj.Add("refs", rootObjs);
    }

    int GetOrAddReference<TClass>(TClass o)
        where TClass : class, ISerializable
    {
        if (!refIndices.TryGetValue(o, out var index))
        {
            index = rootObjs.Count;
            refIndices.Add(o, index);

            var newObj = new JsonObject();
            rootObjs.Add(newObj);

            newObj.Add("$type", o.GetType().FullName);
            newObj.Add("$index", index);

            var savedObj = curObj;
            curObj = newObj;
            o.DoSerialize(ref this);
            curObj = savedObj;
        }

        return index;
    }

    // 클래스를 
    public void SerializeRef<TClass>(string name, TClass? inst)
        where TClass : class, ISerializable
    {
        if (inst == null)
        {
            curObj.Add(name, null);
        }
        else
        {
            var index = GetOrAddReference(inst);

            // curObj에는 $ref[241] 를 넣어주고
            curObj.Add(name, JsonValue.Create($"$refs[{index}]"));
        }
    }

    // Struct
    public void SerializeValue<TValue>(string name, TValue inst)
        where TValue : struct, ISerializable
    {
        SerializeValueRef(name, ref inst);
    }

    // Struct
    public void SerializeValueRef<TValue>(string name, ref TValue inst)
        where TValue : struct, ISerializable
    {
        var newObj = new JsonObject();
        curObj.Add(name, newObj); // 바로 inline

        newObj.Add("$type", JsonValue.Create(inst.GetType().FullName));

        var savedObj = curObj;
        curObj = newObj;
        inst.DoSerialize(ref this);
        curObj = savedObj;
    }

    // bool, int, string 거침없이 inline

    // bool version
    public void SerializeBool(string name, bool value)
    {
        curObj.Add(name, value);
    }

    // int version
    public void SerializeInt(string name, int value)
    {
        curObj.Add(name, value);
    }

    // string version
    public void SerializeString(string name, string value)
    {
        curObj.Add(name, value);
    }

    // ref list version
    public void SerializeRefList<TITem>(string name, List<TITem> list)
        where TITem : class, ISerializable
    {
        // name: [ "$refs[200]", ...  ] 이런게 들어있을 것
        var jsonArray = new JsonArray();
        curObj.Add(name, jsonArray);

        foreach(var item in list)
        {
            var index = GetOrAddReference(item);
            jsonArray.Add($"$refs[{index}]");
        }
    }

    // value, inline version
    public void SerializeValueList<TITem>(string name, List<TITem> list)
        where TITem : ISerializable
    {
        // name: [ "$refs[200]", ...  ] 이런게 들어있을 것
        var jsonArray = new JsonArray();
        curObj.Add(name, jsonArray);

        foreach (var item in list)
        {
            var newObj = new JsonObject();
            newObj.Add("$type", item.GetType().FullName);

            var savedObj = curObj;
            curObj = newObj;
            item.DoSerialize(ref this);
            curObj = savedObj;

            jsonArray.Add(newObj);
        }
    }

    public void SerializeRefArray<TItem>(string name, ImmutableArray<TItem> array)
        where TItem : class, ISerializable
    {
        // name: [ "$refs[200]", ...  ] 이런게 들어있을 것
        var jsonArray = new JsonArray();
        curObj.Add(name, jsonArray);

        foreach (var item in array)
        {
            var index = GetOrAddReference(item);
            jsonArray.Add($"$refs[{index}]");
        }
    }

    public void SerializeValueArray<TItem>(string name, ImmutableArray<TItem> array)
        where TItem : struct, ISerializable
    {
        // name: [ "$refs[200]", ...  ] 이런게 들어있을 것
        var jsonArray = new JsonArray();
        curObj.Add(name, jsonArray);

        foreach (var item in array)
        {
            var newObj = new JsonObject();
            newObj.Add("$type", item.GetType().FullName);

            var savedObj = curObj;
            curObj = newObj;
            item.DoSerialize(ref this);
            curObj = savedObj;

            jsonArray.Add(newObj);
        }
    }

    // key가 value라면, [ { key: ... value: ... } ... ] 로 인코딩한다
    public void SerializeDictValueKeyRefValue<TKey, TValue>(string name, Dictionary<TKey, TValue> dict)
        where TKey : struct, ISerializable, IComparable<TKey>
        where TValue : class, ISerializable
    {
        var newArray = new JsonArray();
        curObj.Add(name, newArray);

        // key를 먼저
        var list = new List<(TKey Key, JsonObject KeyObject, int ValueIndex)>();

        foreach (var (key, value) in dict)
        {   
            var valueIndex = GetOrAddReference(value);

            var keyObj = new JsonObject();
            keyObj.Add("$type", key.GetType().FullName);

            var savedObj = curObj;
            curObj = keyObj;
            key.DoSerialize(ref this);
            curObj = savedObj;

            list.Add((key, keyObj, valueIndex));
        }

        list.Sort((x, y) => x.Key.CompareTo(y.Key));

        foreach (var (key, keyObj, valueIndex) in list)
        {
            var itemObj = new JsonObject();
            itemObj.Add("key", keyObj);
            itemObj.Add("value", $"$refs[{valueIndex}]");

            newArray.Add(itemObj);
        }
    }

    // dict version
    public void SerializeDictRefKeyRefValue<TKey, TValue>(string name, Dictionary<TKey, TValue> dict)
        where TKey : class, ISerializable
        where TValue : class, ISerializable
    {
        var newObj = new JsonObject();
        curObj.Add(name, newObj);

        // key를 먼저
        var list = new List<(int KeyIndex, int ValueIndex)>();

        foreach(var (key, value) in dict)
        {
            var keyIndex = GetOrAddReference(key);
            var valueIndex = GetOrAddReference(value);

            list.Add((keyIndex, valueIndex));
        }

        list.Sort((x, y) => x.KeyIndex - y.KeyIndex);

        foreach(var (keyIndex, valueIndex) in list)        
            newObj.Add($"$refs[{keyIndex}]", $"$refs[{valueIndex}]");
    }    
}

public interface ISerializable
{
    void DoSerialize(ref SerializeContext context);
}