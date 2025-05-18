using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Lando.EventWeaver.Editor
{
    [InitializeOnLoad]
    public static class EventWeaver
    {
        public const string Prefix = "[EventWeaver]";
        
        static EventWeaver()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
        }

        public static void OnAssemblyCompiled(string compiledAssemblyPath, CompilerMessage[] compilerMessages)
        {
            string fileName = Path.GetFileName(compiledAssemblyPath);
            if (fileName != "Assembly-CSharp.dll" && fileName != "Assembly-CSharp-Editor.dll")
                return;
            PatchAssembly(compiledAssemblyPath);
        }

        private static void PatchAssembly(string assemblyPath)
        {
            var resolver = new DefaultAssemblyResolver();
            string asmDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/ScriptAssemblies"));
            resolver.AddSearchDirectory(asmDir);

            string managedPath = Path.Combine(EditorApplication.applicationContentsPath, "Managed");
            resolver.AddSearchDirectory(managedPath);
            string enginePath = Path.Combine(managedPath, "UnityEngine");
            if (Directory.Exists(enginePath))
                resolver.AddSearchDirectory(enginePath);

            string pkgPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/com.Lando.EventWeaver/Runtime"));
            resolver.AddSearchDirectory(pkgPath);

            var readerParams = new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver };
            var module = ModuleDefinition.ReadModule(assemblyPath, readerParams);

            var eventRegistryType = ResolveEventRegistryType(module);
            if (eventRegistryType == null)
            {
                Debug.LogWarning("[Weaver] EventRegistry not found");
                module.Dispose();
                return;
            }

            var regDef   = eventRegistryType.Methods.FirstOrDefault(m => m.Name == "Register"   && m.HasGenericParameters && m.Parameters.Count == 1);
            var unregDef = eventRegistryType.Methods.FirstOrDefault(m => m.Name == "Unregister" && m.HasGenericParameters && m.Parameters.Count == 1);
            if (regDef == null || unregDef == null)
            {
                Debug.LogWarning("[Weaver] Register/Unregister methods not found");
                module.Dispose();
                return;
            }

            foreach (var type in module.Types.Where(t => !t.IsAbstract))
            {
                var listeners = GetAllEventListenerInterfaces(type);
                if (!listeners.Any())
                    continue;

                bool isMono = InheritsFrom(type, "MonoBehaviour");
                if (isMono)
                    InjectMonoBehaviour(type, listeners, regDef, unregDef, module);
                else
                    InjectPlainClass  (type, listeners, regDef, unregDef, module);
            }

            module.Write();
            module.Dispose();
        }
    
        private static System.Collections.Generic.List<GenericInstanceType> GetAllEventListenerInterfaces(TypeDefinition type)
        {
            var result = new System.Collections.Generic.List<GenericInstanceType>();
            var cur = type;
            while (cur != null)
            {
                var listeners = cur.Interfaces
                    .Select(i => i.InterfaceType)
                    .OfType<GenericInstanceType>()
                    .Where(g => g.ElementType.Name == "IEventListener`1");
                result.AddRange(listeners);
                try { cur = cur.BaseType?.Resolve(); }
                catch { break; }
            }
            return result;
        }

        private static TypeDefinition ResolveEventRegistryType(ModuleDefinition module)
        {
            var found = module.Types.FirstOrDefault(t => t.Name == "EventRegistry");
            if (found != null) return found;

            foreach (var ar in module.AssemblyReferences.Where(r => r.Name.StartsWith("Lando.EventWeaver")))
            {
                AssemblyDefinition asm;
                try { asm = module.AssemblyResolver.Resolve(ar); }
                catch { continue; }
                found = asm.MainModule.Types.FirstOrDefault(t => t.Name == "EventRegistry");
                if (found != null) return found;
            }
            return null;
        }

        private static bool InheritsFrom(TypeDefinition type, string baseName)
        {
            var cur = type;
            while (cur.BaseType != null)
            {
                if (cur.BaseType.Name == baseName) return true;
                cur = cur.BaseType.Resolve();
            }
            return false;
        }

        private static void InjectMonoBehaviour(
            TypeDefinition type,
            System.Collections.Generic.List<GenericInstanceType> listeners,
            MethodDefinition regDef,
            MethodDefinition unregDef,
            ModuleDefinition module)
        {
            var onEnable  = GetOrCreateOverride(type, "OnEnable",  module.TypeSystem.Void);
            var onDisable = GetOrCreateOverride(type, "OnDisable", module.TypeSystem.Void);

            InsertCalls(onEnable,  listeners, regDef,   module);
            InsertCalls(onDisable, listeners, unregDef, module);
        }

        private static void InjectPlainClass(
            TypeDefinition type,
            System.Collections.Generic.List<GenericInstanceType> listeners,
            MethodDefinition regDef,
            MethodDefinition unregDef,
            ModuleDefinition module)
        {
            var ctor = type.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic)
                       ?? CreateConstructor(type, module);
            InsertCalls(ctor, listeners, regDef, module);

            var fin = type.Methods.FirstOrDefault(m => m.Name == "Finalize" && m.IsVirtual)
                      ?? CreateFinalizer(type, module);
            InsertCalls(fin, listeners, unregDef, module);
        }

        private static MethodDefinition GetOrCreateOverride(
            TypeDefinition type,
            string name,
            TypeReference returnType)
        {
            var existing = type.Methods.FirstOrDefault(m => m.Name == name && !m.HasParameters);
            if (existing != null) return existing;

            var m = new MethodDefinition(
                name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                returnType);

            var il = m.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ret));
            type.Methods.Add(m);
            return m;
        }

        private static void InsertCalls(
            MethodDefinition method,
            System.Collections.Generic.List<GenericInstanceType> listeners,
            MethodDefinition genericDef,
            ModuleDefinition module)
        {
            var il = method.Body.GetILProcessor();
            var first = method.Body.Instructions.First(i => i.OpCode != OpCodes.Nop);

            foreach (var gi in listeners)
            {
                var gim = new GenericInstanceMethod(genericDef);
                gim.GenericArguments.Add(module.ImportReference(gi.GenericArguments[0]));
                var callRef = module.ImportReference(gim);

                il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
                il.InsertBefore(first, il.Create(OpCodes.Call,    callRef));
            }
        }

        private static MethodDefinition CreateConstructor(
            TypeDefinition type,
            ModuleDefinition module)
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                module.TypeSystem.Void);

            var il = ctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));

            var baseCtor = type.BaseType.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic);
            il.Append(il.Create(OpCodes.Call, module.ImportReference(baseCtor)));
            il.Append(il.Create(OpCodes.Ret));

            type.Methods.Add(ctor);
            return ctor;
        }

        private static MethodDefinition CreateFinalizer(
            TypeDefinition type,
            ModuleDefinition module)
        {
            var fin = new MethodDefinition(
                "Finalize",
                MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                module.TypeSystem.Void);

            var il = fin.Body.GetILProcessor();
            var ret = il.Create(OpCodes.Ret);

            il.Append(il.Create(OpCodes.Ldarg_0));
            var of = module.ImportReference(
                typeof(object).GetMethod("Finalize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            il.Append(il.Create(OpCodes.Callvirt, of));
            il.Append(ret);

            type.Methods.Add(fin);
            return fin;
        }
    }
}
