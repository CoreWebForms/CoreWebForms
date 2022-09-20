# WebForms on ASP.NET Core

# ViewState serialization

In .NET Framework, ViewState was serialized with BinaryFormatter. This is not available and will not be supported on .NET 8+. So, to replace, this, we will encode values with known types since we are the only consumer and producer of the serialized state (it is not expected to be decoded anywhere else).

The format is a binary stream that is encoded with `BinaryReader`/`BinaryWriter` with the following scheme:

```
+-----------------+--------------+---------------+--------------+-------------------+---------------+
|                 |              |               |              |                   |               |
| ControlCount    | ControlName  | PropertyCount | PropertyName | PropertyValueType | PropertyValue |
|                 |              |               |              |                   |               |
+-----------------+--------------+---------------+--------------+-------------------+---------------+
                  |                              |                                                  |
                  | Repeat for all controls      | Repeat for all properties                        |
                  |                              |                                                  |
                  |                              +--------------------------------------------------+
                  |                                                                                 |
                  +---------------------------------------------------------------------------------+
```

**Note**:

- `PropertyValueType`: Is a collection of known types for values that have been written. This is tracked globally and is an index that can be used to deserialize a value correctly.
- `PropertyValue`: This is a serialized form of the property value. Currently, this uses a `TypeDescriptor` to read and write from a string.
