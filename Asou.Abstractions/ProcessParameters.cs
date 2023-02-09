using System.Collections;

namespace Asou.Abstractions;

/// <summary>A collection of parameters to be passed to a process.</summary>
public class ProcessParameters : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _innerDictionary;

    /// <summary>
    ///     Constructor for ProcessParameters class
    /// </summary>
    public ProcessParameters()
    {
        _innerDictionary = new Dictionary<string, object?>();
    }

    public object? this[string key]
    {
        get => _innerDictionary[key];
        set => _innerDictionary[key] = value;
    }

    #region IDictionary

    public void Add(string key, object? value)
    {
        _innerDictionary.Add(key, value);
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

    public IEnumerable<object?> Values => _innerDictionary.Values;

    public int Count => _innerDictionary.Count;

    public bool ContainsKey(string key)
    {
        return _innerDictionary.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _innerDictionary.GetEnumerator();
    }

    public bool TryGetValue(string key, out object? value)
    {
        return _innerDictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_innerDictionary).GetEnumerator();
    }

    #endregion
}