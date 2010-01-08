using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Reflection;
using Pencil.Core;

namespace NMeter
{
    public class MethodMetrics
    {
        public byte[] Fingerprint;
        public int InstructionCount;

        public static MethodMetrics For<T>(Expression<Action<T>> action) { return For((action.Body as MethodCallExpression).Method); }

        public static MethodMetrics For(MethodInfo method) {
            var metrics = new MethodMetrics();
            var bytes = new MemoryStream();
            var writer = new StreamWriter(bytes);
            foreach(var item in Disassembler.Decode(method)) {
                if(!item.Opcode.Equals(Opcode.Nop))
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
    }
}