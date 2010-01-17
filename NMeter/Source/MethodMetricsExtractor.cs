using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using Pencil.Core;
using Xlnt.Stuff;

namespace NMeter
{
    public class MethodMetricsExtractor
    {
        ITypeLoader typeLoader = new DefaultTypeLoader();
        HashAlgorithm hash = MD5.Create();
        DefaultFormatter formatter = new DefaultFormatter();

        public MethodMetrics ComputeMetrics(MethodInfo method) {
            var attributeData = CustomAttributeData.GetCustomAttributes(method);
            var metrics = new MethodMetrics {
                Signature = formatter.Format(method),
                ParameterCount = method.GetParameters().Length,
                IsGenerated = IsGenerated(method),
                IsStatic = method.IsStatic
            };
            ComputeInstructionMetrics(method, metrics);
            return metrics;
        }

        private void ComputeInstructionMetrics(MethodInfo method, MethodMetrics metrics) {
            var bytes = new MemoryStream();
            var writer = new StreamWriter(bytes);
            foreach(var item in Disassembler.Decode(typeLoader, method)) {
                if(item.Opcode.Equals(Opcode.Nop))
                    continue;
                metrics.InstructionCount += 1;
                writer.WriteLine(item);
            }
            writer.Flush();
            bytes.Position = 0;
            metrics.Fingerprint = hash.ComputeHash(bytes);
        }

        static bool IsGenerated(MethodInfo method) {
            return IsGenerated(CustomAttributeData.GetCustomAttributes(method))
                || (method.DeclaringType != null
                    && IsGenerated(CustomAttributeData.GetCustomAttributes(method.DeclaringType)));
        }

        static bool IsGenerated(IEnumerable<CustomAttributeData> attributeData) {
            return attributeData.Any(x => x.Constructor.DeclaringType.Name == "CompilerGeneratedAttribute");
        }
    }
}
