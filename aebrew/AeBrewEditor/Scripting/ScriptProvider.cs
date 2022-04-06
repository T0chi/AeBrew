﻿using AeBrewCommon.Scripting;
using System;
using System.Reflection;

namespace AeBrewEditor.Scripting
{
    public class ScriptProvider<TScript> : MarshalByRefObject
        where TScript : Script
    {
        private readonly string identifier = Guid.NewGuid().ToString();
        private Type type;

        public void Initialize(string assemblyPath, string typeName)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            type = assembly.GetType(typeName, true, true);
        }

        public TScript CreateScript()
        {
            var script = (TScript)Activator.CreateInstance(type);
            script.Identifier = identifier;
            return script;
        }
    }
}
