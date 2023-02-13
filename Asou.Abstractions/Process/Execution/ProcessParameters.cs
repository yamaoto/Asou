using System.Collections;
using System.Text.Json;
using Asou.Abstractions.Container;

namespace Asou.Abstractions.Process.Execution;

/// <summary>A collection of parameters to be passed to a process.</summary>
public class ProcessParameters : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, ValueStorage> _innerDictionary;

    /// <summary>
    ///     Constructor for ProcessParameters class
    /// </summary>
    public ProcessParameters()
    {
        _innerDictionary = new Dictionary<string, ValueStorage>();
    }

    /// <summary>
    ///     Constructor with mapping from Value Container Map
    /// </summary>
    /// <param name="parameters"></param>
    public ProcessParameters(Dictionary<string, ValueContainer> parameters)
    {
        _innerDictionary = parameters
            .ToDictionary(k => k.Key, v => new ValueStorage(v.Value.GetValue()));
    }

    public object? this[string key]
    {
        get
        {
            if (!_innerDictionary.TryGetValue(key, out var storage))
            {
                return null;
            }

            return storage.Value;
        }
        set
        {
            if (!_innerDictionary.TryGetValue(key, out var storage))
            {
                _innerDictionary.Add(key, new ValueStorage(value));
            }
            else
            {
                storage.Set(value);
            }
        }
    }

    public Dictionary<string, ValueContainer> ToValueContainerMap()
    {
        return _innerDictionary
            .ToDictionary(k => k.Key,
                v => new ValueContainer
                {
                    // TODO: Use ValueContainer Serializer
                    Value = v.Value.ValueType != null ? JsonSerializer.Serialize(v.Value.Value, v.Value.ValueType) : "",
                    Type = AsouTypes.Object,
                    ObjectType = v.Value.ValueType != null ? v.Value.ValueType.ToString() : ""
                });
    }

    private sealed class ValueStorage
    {
        public ValueStorage(object? value)
        {
            Set(value);
        }

        public Type? ValueType { get; set; }
        public object? Value { get; set; }

        public void Set(object? value)
        {
            Value = value;
            if (ValueType == null && value != null)
            {
                ValueType = value.GetType();
            }
        }
    }

    #region IDictionary

    public void Add(string key, object? value)
    {
        _innerDictionary.Add(key, new ValueStorage(value));
    }

    public bool Remove(string key)
    {
        return _innerDictionary.Remove(key);
    }

    public void Clear()
    {
        _innerDictionary.Clear();
    }

    #endregion

    #region IReadOnlyDictionary

    public IEnumerable<string> Keys => _innerDictionary.Keys;

    public IEnumerable<object?> Values => _innerDictionary.Values.Select(s => s.Value);

    public int Count => _innerDictionary.Count;

    public bool ContainsKey(string key)
    {
        return _innerDictionary.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        using var enumerator = _innerDictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var storage = enumerator.Current.Value;
            // new KeyValuePair allocates additional memory
            yield return new KeyValuePair<string, object?>(enumerator.Current.Key, storage.Value);
        }
    }

    public bool TryGetValue(string key, out object? value)
    {
        var result = _innerDictionary.TryGetValue(key, out var storage);
        value = storage?.Value;
        return result;
    }

    public bool TryGetStorageValue(string key, out object? value, out Type? type)
    {
        var result = _innerDictionary.TryGetValue(key, out var storage);
        value = storage?.Value;
        type = storage?.ValueType;
        return result;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        using var enumerator = _innerDictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var storage = enumerator.Current.Value;
            // new KeyValuePair allocates additional memory
            yield return new KeyValuePair<string, object?>(enumerator.Current.Key, storage.Value);
        }
    }

    #endregion
}
