﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Concoct
{
    public static class TypeMixer<TWanted>
    {
        public static TWanted MixWith(object target) {
            var targetType = target.GetType();
            var proxies = AppDomain.CurrentDomain.DefineDynamicAssembly(Assembly.GetExecutingAssembly().GetName(), AssemblyBuilderAccess.Run);
            var module = proxies.DefineDynamicModule("Mixers");
            var type = DefineType(module, targetType);

            var targetField = type.DefineField("$0", targetType, FieldAttributes.InitOnly | FieldAttributes.Private);

            var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { targetType });
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, targetField);
            il.Emit(OpCodes.Ret);


            foreach (var wantedMethod in typeof(TWanted).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!wantedMethod.IsVirtual || wantedMethod.DeclaringType.Equals(typeof(object)))
                    continue;
                foreach (var targetMethod in targetType.GetMethods()) {
                    if (targetMethod.Name != wantedMethod.Name || targetMethod.ReturnType != wantedMethod.ReturnType)
                        continue;
                    Console.WriteLine("Mixing {0}", targetMethod.Name);
                    var impl = type.DefineMethod(wantedMethod.Name, 
                        MethodAttributes.HideBySig | MethodAttributes.Virtual | (wantedMethod.Attributes & MethodAttributes.MemberAccessMask), 
                        wantedMethod.ReturnType, 
                        wantedMethod.GetParameters().Select(x => x.ParameterType).ToArray());
                    il = impl.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, targetField);
                    il.Emit(OpCodes.Tailcall);
                    il.Emit(OpCodes.Callvirt, targetMethod);
                    il.Emit(OpCodes.Ret);
                }
            }

            var baked = type.CreateType();
            return (TWanted)baked.GetConstructor(new[] { targetType }).Invoke(new object[] { target });
        }

        static TypeBuilder DefineType(ModuleBuilder module, Type targetType) {
            return module.DefineType(targetType.FullName + "Mixer", TypeAttributes.Class, typeof(TWanted));
        }
    }
}