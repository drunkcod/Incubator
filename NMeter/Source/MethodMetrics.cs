using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using Pencil.Core;
using System.Runtime.CompilerServices;

namespace NMeter
{
    public class MethodMetrics
    {
        public byte[] Fingerprint;
        public int InstructionCount;
        public int ParameterCount;
        public bool IsGenerated;

        public static MethodMetrics For(MethodInfo method) {
            var attributeData = CustomAttributeData.GetCustomAttributes(method);
            var metrics = new MethodMetrics { 
                ParameterCount = method.GetParameters().Length,
                IsGenerated = CheckIsGenerated(method)
            };
            var bytes = new MemoryStream();
            var writer = new StreamWriter(bytes);
            foreach(var item in Disassembler.Decode(method)) {
                if(item.Opcode.Equals(Opcode.Nop))
                    continue;
                metrics.InstructionCount += 1;
                writer.WriteLine(item);
            }
            writer.Flush();
            bytes.Position = 0;
            using(var hash = MD5.Create()){
                metrics.Fingerprint = hash.ComputeHash(bytes);
                return metrics;
            }
        }

        static bool CheckIsGenerated(MethodInfo method) {
            return CheckIsGenerated(CustomAttributeData.GetCustomAttributes(method))
                || (method.DeclaringType != null 
                    && CheckIsGenerated(CustomAttributeData.GetCustomAttributes(method.DeclaringType)));
        }

        static bool CheckIsGenerated(IEnumerable<CustomAttributeData> attributeData) {
            return attributeData.Any(x => x.Constructor.DeclaringType.Name == "CompilerGeneratedAttribute");
        }
    }
}