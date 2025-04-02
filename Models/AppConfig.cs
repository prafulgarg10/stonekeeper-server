using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Appconfig
{
    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
