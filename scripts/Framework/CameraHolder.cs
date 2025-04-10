using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class CameraHolder : Node2D
{
    private Camera2D _camera;
    public Camera2D Camera;
    public Godot.Collections.Array<Godot.Collections.Dictionary> CameraProperties;

    public virtual Camera2D GetCamera()
    {
        return null;
    }

    public virtual Godot.Collections.Array<Godot.Collections.Dictionary> GetCameraProperties()
        {
           var node_properties = ClassDB.ClassGetPropertyList("Node2D");
           if (CameraProperties.Count == 0)
           {
                if (Camera != null)
                {
                    var properties = Camera.GetPropertyList();
                    foreach (var property in properties)
                    {
                        if (!node_properties.Contains(property))
                        {
                            property["usage"] = (long)property["usage"] & (long)PropertyUsageFlags.Storage;
                            CameraProperties.Add(property);
                        }
                    }
                }
           }
           return CameraProperties;
        }

    public CameraHolder GetCameraHolder(String property)
    {
        var node_properties = ClassDB.ClassGetPropertyList("Node2D");
        foreach (var dict in node_properties)
        {
            if (dict.ContainsKey(property))
                return null;
        }

        if (Camera != null)
            return (CameraHolder)Camera.Get(property);

        return null;   
    }

    public bool SetCameraHolder(String property, String value)
    {
        var node_properties = ClassDB.ClassGetPropertyList("Node2D");
        foreach (var dict in node_properties)
        {
            if (dict.ContainsKey(property))
                return false;
        }

        if (Camera != null)
        {
            var cam_properties = ClassDB.ClassGetPropertyList("Camera2D");
            foreach (var dict in cam_properties)
            {
                if (dict.ContainsKey(property))
                {
                    Camera.Set(property, value);
                    return true;
                }
            }
        }
        return false;
    }
}
