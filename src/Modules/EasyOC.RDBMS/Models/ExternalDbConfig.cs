﻿namespace EasyOC.RDBMS.Models;

public class ExternalDbConfig
{
    public string Name { get; set; }
    public string ConnectionConfigId { get; set; } = "";
    public bool UseShellDb { get; set; } = false;
}

