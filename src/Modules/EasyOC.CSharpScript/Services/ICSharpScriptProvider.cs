﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyOC.CSharpScript.Services;

public interface ICSharpScriptProvider
{
    AssemblyCSharpBuilder GetAssemblyCSharpBuilder(bool useGlobalSharedBuilder = true);
    Task<Type> CreateTypeAsync(string fullName, string cSharpScripts, IEnumerable<string> usings = default);
    Task<Type> GetOrCreateAsync(string fullName, string cSharpScripts, IEnumerable<string> usings = default);
}
