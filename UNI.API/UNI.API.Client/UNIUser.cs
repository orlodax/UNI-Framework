﻿using UNI.Core.Library;

namespace UNI.API.Client;

public class UNIUser : BaseModel
{
    public static string? Username { get; set; }

    public static string? Password { get; set; }

    public static UNIToken? Token { get; set; }
}

public class UNIBlazorUser : BaseModel
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    public UNIToken? Token { get; set; }
}
